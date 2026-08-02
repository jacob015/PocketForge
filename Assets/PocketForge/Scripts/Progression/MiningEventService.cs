using System;
using System.Collections.Generic;
using System.Linq;
using PocketForge.Content;
using PocketForge.Save;

namespace PocketForge.Progression
{
    public enum EventActionStatus
    {
        Success,
        FeatureLocked,
        NoActiveEvent,
        UnknownReward,
        RequirementNotMet,
        AlreadyClaimed,
        InsufficientTokens,
        ExchangeLimitReached
    }

    public readonly struct EventRewardState
    {
        public EventRewardState(EventRewardTierDefinition definition, bool claimed)
        {
            Definition = definition;
            Claimed = claimed;
        }

        public EventRewardTierDefinition Definition { get; }
        public bool Claimed { get; }
    }

    public readonly struct MiningEventBoardState
    {
        public MiningEventBoardState(
            MiningEventDefinition definition,
            string periodKey,
            long refreshAtUnixSeconds,
            long oresMined,
            long earnedTokens,
            long tokenBalance,
            int exchangePurchases,
            IReadOnlyList<EventRewardState> rewards)
        {
            Definition = definition;
            PeriodKey = periodKey ?? string.Empty;
            RefreshAtUnixSeconds = Math.Max(0L, refreshAtUnixSeconds);
            OresMined = Math.Max(0L, oresMined);
            EarnedTokens = Math.Max(0L, earnedTokens);
            TokenBalance = Math.Max(0L, tokenBalance);
            ExchangePurchases = Math.Max(0, exchangePurchases);
            Rewards = rewards ?? Array.Empty<EventRewardState>();
        }

        public MiningEventDefinition Definition { get; }
        public string PeriodKey { get; }
        public long RefreshAtUnixSeconds { get; }
        public long OresMined { get; }
        public long EarnedTokens { get; }
        public long TokenBalance { get; }
        public int ExchangePurchases { get; }
        public IReadOnlyList<EventRewardState> Rewards { get; }
    }

    public readonly struct EventActionResult
    {
        public EventActionResult(
            EventActionStatus status,
            EventRewardType rewardType = EventRewardType.Credits,
            long rewardAmount = 0L)
        {
            Status = status;
            RewardType = rewardType;
            RewardAmount = Math.Max(0L, rewardAmount);
        }

        public EventActionStatus Status { get; }
        public EventRewardType RewardType { get; }
        public long RewardAmount { get; }
    }

    public sealed class MiningEventService
    {
        private readonly MiningEventDefinition[] definitions;

        public MiningEventService(MiningContentCatalog catalog)
        {
            definitions = catalog.GetMiningEvents().ToArray();
        }

        public bool Refresh(GameSaveData player, long nowUtcUnixSeconds, bool featureUnlocked)
        {
            if (!featureUnlocked || nowUtcUnixSeconds <= 0L || definitions.Length == 0)
            {
                return false;
            }

            var effectiveNow = Math.Max(player.lastObservedEventUnixSeconds, nowUtcUnixSeconds);
            player.lastObservedEventUnixSeconds = effectiveNow;
            var definition = definitions[0];
            var periodKey = MissionService.GetPeriodKey(MissionPeriod.Weekly, effectiveNow);
            player.miningEvent ??= new MiningEventProgressData();
            if (player.miningEvent.periodKey == periodKey &&
                player.miningEvent.eventId == definition.EventId)
            {
                return SyncProgress(player, definition);
            }

            player.miningEvent = new MiningEventProgressData
            {
                eventId = definition.EventId,
                periodKey = periodKey,
                baselineOresMined = GetTotalOresMined(player),
                earnedTokens = 0L,
                tokenBalance = 0L,
                claimedTierIds = Array.Empty<string>(),
                exchangePurchases = 0
            };
            return true;
        }

        public MiningEventBoardState GetBoard(
            GameSaveData player,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            if (!featureUnlocked || definitions.Length == 0)
            {
                return new MiningEventBoardState(
                    null, string.Empty, 0L, 0L, 0L, 0L, 0,
                    Array.Empty<EventRewardState>());
            }

            Refresh(player, nowUtcUnixSeconds, true);
            var definition = definitions[0];
            var progress = player.miningEvent;
            var claimed = new HashSet<string>(
                progress.claimedTierIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            var rewards = definition.RewardTiers
                .Select(tier => new EventRewardState(tier, claimed.Contains(tier.TierId)))
                .ToArray();
            var effectiveNow = Math.Max(player.lastObservedEventUnixSeconds, nowUtcUnixSeconds);
            return new MiningEventBoardState(
                definition,
                progress.periodKey,
                MissionService.GetNextRefreshUnixSeconds(MissionPeriod.Weekly, effectiveNow),
                Math.Max(0L, GetTotalOresMined(player) - progress.baselineOresMined),
                progress.earnedTokens,
                progress.tokenBalance,
                progress.exchangePurchases,
                rewards);
        }

        public EventActionResult ClaimReward(
            GameSaveData player,
            string tierId,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            var board = GetBoard(player, nowUtcUnixSeconds, featureUnlocked);
            if (!featureUnlocked)
            {
                return new EventActionResult(EventActionStatus.FeatureLocked);
            }

            if (board.Definition == null)
            {
                return new EventActionResult(EventActionStatus.NoActiveEvent);
            }

            var tier = board.Definition.RewardTiers.FirstOrDefault(candidate =>
                candidate.TierId == (tierId ?? string.Empty));
            if (tier == null)
            {
                return new EventActionResult(EventActionStatus.UnknownReward);
            }

            if (board.Rewards.First(state => state.Definition.TierId == tier.TierId).Claimed)
            {
                return new EventActionResult(EventActionStatus.AlreadyClaimed);
            }

            if (board.EarnedTokens < tier.RequiredTokens)
            {
                return new EventActionResult(EventActionStatus.RequirementNotMet);
            }

            player.miningEvent.claimedTierIds = (player.miningEvent.claimedTierIds ?? Array.Empty<string>())
                .Append(tier.TierId)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            GrantReward(player, tier.RewardType, tier.RewardAmount);
            return new EventActionResult(EventActionStatus.Success, tier.RewardType, tier.RewardAmount);
        }

        public EventActionResult PurchaseExchange(
            GameSaveData player,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            var board = GetBoard(player, nowUtcUnixSeconds, featureUnlocked);
            if (!featureUnlocked)
            {
                return new EventActionResult(EventActionStatus.FeatureLocked);
            }

            if (board.Definition == null)
            {
                return new EventActionResult(EventActionStatus.NoActiveEvent);
            }

            if (player.miningEvent.exchangePurchases >= board.Definition.ExchangeLimit)
            {
                return new EventActionResult(EventActionStatus.ExchangeLimitReached);
            }

            if (player.miningEvent.tokenBalance < board.Definition.ExchangeCostTokens)
            {
                return new EventActionResult(EventActionStatus.InsufficientTokens);
            }

            player.miningEvent.tokenBalance -= board.Definition.ExchangeCostTokens;
            player.miningEvent.exchangePurchases++;
            GrantReward(
                player,
                board.Definition.ExchangeRewardType,
                board.Definition.ExchangeRewardAmount);
            return new EventActionResult(
                EventActionStatus.Success,
                board.Definition.ExchangeRewardType,
                board.Definition.ExchangeRewardAmount);
        }

        private static bool SyncProgress(GameSaveData player, MiningEventDefinition definition)
        {
            var progress = player.miningEvent;
            var ores = Math.Max(0L, GetTotalOresMined(player) - progress.baselineOresMined);
            var earned = ores / definition.OresPerToken;
            if (earned <= progress.earnedTokens)
            {
                return false;
            }

            var delta = earned - progress.earnedTokens;
            progress.earnedTokens = earned;
            progress.tokenBalance = SaturatingAdd(progress.tokenBalance, delta);
            return true;
        }

        private static void GrantReward(GameSaveData player, EventRewardType type, long amount)
        {
            switch (type)
            {
                case EventRewardType.Gems:
                    player.gems = SaturatingAdd(player.gems, amount);
                    break;
                case EventRewardType.BlueprintCores:
                    player.blueprintCores = SaturatingAdd(player.blueprintCores, amount);
                    break;
                default:
                    player.credits = SaturatingAdd(player.credits, amount);
                    break;
            }
        }

        private static long GetTotalOresMined(GameSaveData player)
        {
            var total = 0L;
            foreach (var entry in player.oreCollection ?? Array.Empty<OreCollectionData>())
            {
                total = SaturatingAdd(total, entry?.minedCount ?? 0L);
            }

            return total;
        }

        private static long SaturatingAdd(long left, long right)
        {
            left = Math.Max(0L, left);
            right = Math.Max(0L, right);
            return left > long.MaxValue - right ? long.MaxValue : left + right;
        }
    }
}
