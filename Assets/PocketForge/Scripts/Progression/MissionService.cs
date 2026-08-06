using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PocketForge.Content;
using PocketForge.Mining;
using PocketForge.Save;

namespace PocketForge.Progression
{
    public enum MissionClaimStatus
    {
        Success,
        FeatureLocked,
        UnknownMission,
        RequirementNotMet,
        AlreadyClaimed,
        CompletionNotReady,
        CompletionAlreadyClaimed
    }

    public readonly struct MissionState
    {
        public MissionState(MissionDefinition definition, long progress, bool claimed)
        {
            Definition = definition;
            Progress = Math.Max(0L, progress);
            Claimed = claimed;
        }

        public MissionDefinition Definition { get; }
        public long Progress { get; }
        public bool Claimed { get; }
        public bool CanClaim => !Claimed && Progress >= Definition.Target;
    }

    public readonly struct MissionBoardState
    {
        public MissionBoardState(
            MissionPeriod period,
            string periodKey,
            long refreshAtUnixSeconds,
            IReadOnlyList<MissionState> missions,
            bool completionRewardClaimed,
            MissionRewardType completionRewardType,
            long completionRewardAmount)
        {
            Period = period;
            PeriodKey = periodKey ?? string.Empty;
            RefreshAtUnixSeconds = Math.Max(0L, refreshAtUnixSeconds);
            Missions = missions ?? Array.Empty<MissionState>();
            CompletionRewardClaimed = completionRewardClaimed;
            CompletionRewardType = completionRewardType;
            CompletionRewardAmount = Math.Max(1L, completionRewardAmount);
        }

        public MissionPeriod Period { get; }
        public string PeriodKey { get; }
        public long RefreshAtUnixSeconds { get; }
        public IReadOnlyList<MissionState> Missions { get; }
        public bool CompletionRewardClaimed { get; }
        public MissionRewardType CompletionRewardType { get; }
        public long CompletionRewardAmount { get; }
        public int ClaimedCount => Missions.Count(state => state.Claimed);
        public int ClaimableCount => Missions.Count(state => state.CanClaim);
        public bool CanClaimCompletion => !CompletionRewardClaimed &&
                                          Missions.Count > 0 &&
                                          Missions.All(state => state.Claimed);
    }

    public readonly struct MissionClaimResult
    {
        public MissionClaimResult(
            MissionClaimStatus status,
            MissionRewardType rewardType = MissionRewardType.Credits,
            long rewardAmount = 0L,
            EquipmentItemData rewardEquipment = null)
        {
            Status = status;
            RewardType = rewardType;
            RewardAmount = Math.Max(0L, rewardAmount);
            RewardEquipment = rewardEquipment;
        }

        public MissionClaimStatus Status { get; }
        public MissionRewardType RewardType { get; }
        public long RewardAmount { get; }
        public EquipmentItemData RewardEquipment { get; }
    }

    public sealed class MissionService
    {
        private readonly MissionDefinition[] definitions;
        private readonly Dictionary<string, MissionDefinition> definitionsById;
        private readonly EquipmentService equipmentService;

        public MissionService(MiningContentCatalog catalog, EquipmentService equipmentService)
        {
            this.equipmentService = equipmentService;
            definitions = catalog.GetMissions().ToArray();
            definitionsById = definitions.ToDictionary(
                definition => definition.MissionId,
                StringComparer.Ordinal);
        }

        public bool Refresh(GameSaveData player, long nowUtcUnixSeconds, bool featureUnlocked)
        {
            if (!featureUnlocked || nowUtcUnixSeconds <= 0L)
            {
                return false;
            }

            var changed = false;
            var effectiveNow = Math.Max(player.lastObservedMissionUnixSeconds, nowUtcUnixSeconds);
            if (effectiveNow != player.lastObservedMissionUnixSeconds)
            {
                player.lastObservedMissionUnixSeconds = effectiveNow;
            }

            var snapshot = CaptureProgress(player);
            var dailyKey = GetPeriodKey(MissionPeriod.Daily, effectiveNow);
            var weeklyKey = GetPeriodKey(MissionPeriod.Weekly, effectiveNow);
            changed |= RefreshPeriod(ref player.dailyMissions, dailyKey, snapshot);
            changed |= RefreshPeriod(ref player.weeklyMissions, weeklyKey, snapshot);
            return changed;
        }

        public MissionBoardState GetBoard(
            GameSaveData player,
            MissionPeriod period,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return new MissionBoardState(
                    period,
                    string.Empty,
                    0L,
                    Array.Empty<MissionState>(),
                    false,
                    GetCompletionRewardType(period),
                    GetCompletionRewardAmount(period));
            }

            Refresh(player, nowUtcUnixSeconds, true);
            var periodData = GetPeriodData(player, period);
            var current = CaptureProgress(player);
            var claimed = new HashSet<string>(
                periodData.claimedMissionIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            var states = definitions
                .Where(definition => definition.Period == period)
                .Select(definition => new MissionState(
                    definition,
                    GetProgressDelta(current, periodData.baseline, definition.Metric),
                    claimed.Contains(definition.MissionId)))
                .ToArray();

            var effectiveNow = Math.Max(player.lastObservedMissionUnixSeconds, nowUtcUnixSeconds);
            return new MissionBoardState(
                period,
                periodData.periodKey,
                GetNextRefreshUnixSeconds(period, effectiveNow),
                states,
                periodData.completionRewardClaimed,
                GetCompletionRewardType(period),
                GetCompletionRewardAmount(period));
        }

        public MissionClaimResult Claim(
            GameSaveData player,
            string missionId,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return new MissionClaimResult(MissionClaimStatus.FeatureLocked);
            }

            if (!definitionsById.TryGetValue(missionId ?? string.Empty, out var definition))
            {
                return new MissionClaimResult(MissionClaimStatus.UnknownMission);
            }

            var board = GetBoard(player, definition.Period, nowUtcUnixSeconds, true);
            var state = board.Missions.First(candidate => candidate.Definition.MissionId == missionId);
            if (state.Claimed)
            {
                return new MissionClaimResult(MissionClaimStatus.AlreadyClaimed);
            }

            if (!state.CanClaim)
            {
                return new MissionClaimResult(MissionClaimStatus.RequirementNotMet);
            }

            var reward = GrantReward(
                player,
                definition.RewardType,
                definition.RewardAmount);
            AddClaim(GetPeriodData(player, definition.Period), definition.MissionId);
            return reward;
        }

        public MissionClaimResult ClaimCompletion(
            GameSaveData player,
            MissionPeriod period,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return new MissionClaimResult(MissionClaimStatus.FeatureLocked);
            }

            var board = GetBoard(player, period, nowUtcUnixSeconds, true);
            if (board.CompletionRewardClaimed)
            {
                return new MissionClaimResult(MissionClaimStatus.CompletionAlreadyClaimed);
            }

            if (!board.CanClaimCompletion)
            {
                return new MissionClaimResult(MissionClaimStatus.CompletionNotReady);
            }

            var reward = GrantReward(
                player,
                board.CompletionRewardType,
                board.CompletionRewardAmount);
            GetPeriodData(player, period).completionRewardClaimed = true;
            return reward;
        }

        public static string GetPeriodKey(MissionPeriod period, long utcUnixSeconds)
        {
            var utc = DateTimeOffset.FromUnixTimeSeconds(Math.Max(0L, utcUnixSeconds)).UtcDateTime;
            if (period == MissionPeriod.Weekly)
            {
                var daysSinceMonday = ((int)utc.DayOfWeek + 6) % 7;
                utc = utc.Date.AddDays(-daysSinceMonday);
            }

            return utc.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        }

        public static long GetNextRefreshUnixSeconds(MissionPeriod period, long utcUnixSeconds)
        {
            var utc = DateTimeOffset.FromUnixTimeSeconds(Math.Max(0L, utcUnixSeconds)).UtcDateTime;
            DateTime next;
            if (period == MissionPeriod.Daily)
            {
                next = utc.Date.AddDays(1);
            }
            else
            {
                var daysSinceMonday = ((int)utc.DayOfWeek + 6) % 7;
                next = utc.Date.AddDays(7 - daysSinceMonday);
            }

            return new DateTimeOffset(DateTime.SpecifyKind(next, DateTimeKind.Utc))
                .ToUnixTimeSeconds();
        }

        private MissionClaimResult GrantReward(
            GameSaveData player,
            MissionRewardType rewardType,
            long amount)
        {
            amount = Math.Max(1L, amount);
            EquipmentItemData equipment = null;
            switch (rewardType)
            {
                case MissionRewardType.Gems:
                    player.gems = SaturatingAdd(player.gems, amount);
                    break;
                case MissionRewardType.BlueprintCores:
                    player.blueprintCores = SaturatingAdd(player.blueprintCores, amount);
                    break;
                case MissionRewardType.Equipment:
                    equipment = equipmentService.GrantBossReward(
                        player,
                        Math.Max(1, player.highestCompletedChapter));
                    break;
                default:
                    player.credits = SaturatingAdd(player.credits, amount);
                    break;
            }

            return new MissionClaimResult(
                MissionClaimStatus.Success,
                rewardType,
                amount,
                equipment);
        }

        private static bool RefreshPeriod(
            ref MissionPeriodData period,
            string periodKey,
            MissionProgressSnapshotData snapshot)
        {
            period ??= new MissionPeriodData();
            if (period.periodKey == periodKey)
            {
                return false;
            }

            period = new MissionPeriodData
            {
                periodKey = periodKey,
                baseline = CloneSnapshot(snapshot),
                claimedMissionIds = Array.Empty<string>(),
                completionRewardClaimed = false
            };
            return true;
        }

        private static MissionPeriodData GetPeriodData(GameSaveData player, MissionPeriod period)
        {
            if (period == MissionPeriod.Daily)
            {
                return player.dailyMissions ??= new MissionPeriodData();
            }

            return player.weeklyMissions ??= new MissionPeriodData();
        }

        private static void AddClaim(MissionPeriodData period, string missionId)
        {
            var claims = new HashSet<string>(
                period.claimedMissionIds ?? Array.Empty<string>(),
                StringComparer.Ordinal)
            {
                missionId
            };
            period.claimedMissionIds = claims.ToArray();
        }

        private static MissionProgressSnapshotData CaptureProgress(GameSaveData player)
        {
            return new MissionProgressSnapshotData
            {
                oresMined = SaturatingSum(
                    (player.oreCollection ?? Array.Empty<OreCollectionData>())
                    .Where(entry => entry != null)
                    .Select(entry => Math.Max(0L, entry.minedCount))),
                facilityUpgrades = SaturatingSum(new[]
                {
                    (long)Math.Max(0, player.pickaxeLevel),
                    Math.Max(0, player.drillLevel),
                    Math.Max(0, player.robotLevel)
                }),
                researchCompleted = SaturatingSum(
                    (player.researchProgress ?? Array.Empty<ResearchProgressData>())
                    .Where(entry => entry != null)
                    .Select(entry => (long)Math.Max(0, entry.level))),
                bossesDefeated = Math.Max(0L, player.bossesDefeated),
                equipmentAcquired = Math.Max(0, player.equipmentRewardSequence)
            };
        }

        private static long GetProgressDelta(
            MissionProgressSnapshotData current,
            MissionProgressSnapshotData baseline,
            MissionMetric metric)
        {
            baseline ??= new MissionProgressSnapshotData();
            var currentValue = GetMetricValue(current, metric);
            var baselineValue = GetMetricValue(baseline, metric);
            return Math.Max(0L, currentValue - Math.Min(currentValue, baselineValue));
        }

        private static long GetMetricValue(MissionProgressSnapshotData snapshot, MissionMetric metric)
        {
            snapshot ??= new MissionProgressSnapshotData();
            return metric switch
            {
                MissionMetric.FacilityUpgrades => Math.Max(0L, snapshot.facilityUpgrades),
                MissionMetric.ResearchCompleted => Math.Max(0L, snapshot.researchCompleted),
                MissionMetric.BossesDefeated => Math.Max(0L, snapshot.bossesDefeated),
                MissionMetric.EquipmentAcquired => Math.Max(0L, snapshot.equipmentAcquired),
                _ => Math.Max(0L, snapshot.oresMined)
            };
        }

        private static MissionProgressSnapshotData CloneSnapshot(MissionProgressSnapshotData source)
        {
            return new MissionProgressSnapshotData
            {
                oresMined = source.oresMined,
                facilityUpgrades = source.facilityUpgrades,
                researchCompleted = source.researchCompleted,
                bossesDefeated = source.bossesDefeated,
                equipmentAcquired = source.equipmentAcquired
            };
        }

        private static MissionRewardType GetCompletionRewardType(MissionPeriod period) =>
            period == MissionPeriod.Daily
                ? MissionRewardType.BlueprintCores
                : MissionRewardType.Equipment;

        private static long GetCompletionRewardAmount(MissionPeriod period) => 1L;

        private static long SaturatingAdd(long left, long right)
        {
            left = Math.Max(0L, left);
            right = Math.Max(0L, right);
            return left > long.MaxValue - right ? long.MaxValue : left + right;
        }

        private static long SaturatingSum(IEnumerable<long> values)
        {
            var result = 0L;
            foreach (var value in values)
            {
                result = SaturatingAdd(result, value);
            }

            return result;
        }
    }
}
