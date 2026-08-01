using System;
using System.Collections.Generic;
using System.Linq;
using PocketForge.Content;
using PocketForge.Save;

namespace PocketForge.Progression
{
    public enum AchievementClaimStatus
    {
        Success,
        FeatureLocked,
        UnknownAchievement,
        RequirementNotMet,
        AlreadyCompleted
    }

    public readonly struct AchievementState
    {
        public AchievementState(
            AchievementDefinition definition,
            long progress,
            int claimedTiers)
        {
            Definition = definition;
            Progress = Math.Max(0L, progress);
            ClaimedTiers = Math.Clamp(claimedTiers, 0, definition.Tiers.Length);
        }

        public AchievementDefinition Definition { get; }
        public long Progress { get; }
        public int ClaimedTiers { get; }
        public bool IsCompleted => ClaimedTiers >= Definition.Tiers.Length;
        public AchievementTierDefinition NextTier => IsCompleted ? null : Definition.Tiers[ClaimedTiers];
        public bool CanClaim => !IsCompleted && Progress >= NextTier.Target;
    }

    public readonly struct AchievementClaimResult
    {
        public AchievementClaimResult(
            AchievementClaimStatus status,
            AchievementRewardType rewardType = AchievementRewardType.Credits,
            long rewardAmount = 0L)
        {
            Status = status;
            RewardType = rewardType;
            RewardAmount = Math.Max(0L, rewardAmount);
        }

        public AchievementClaimStatus Status { get; }
        public AchievementRewardType RewardType { get; }
        public long RewardAmount { get; }
    }

    public sealed class AchievementService
    {
        private readonly CollectionService collectionService;
        private readonly AchievementDefinition[] definitions;
        private readonly Dictionary<string, AchievementDefinition> definitionsById;

        public AchievementService(MiningContentCatalog catalog, CollectionService collectionService)
        {
            this.collectionService = collectionService;
            definitions = catalog.GetAchievements().ToArray();
            definitionsById = definitions.ToDictionary(
                definition => definition.AchievementId,
                StringComparer.Ordinal);
        }

        public void SanitizeAgainstCatalog(GameSaveData player)
        {
            player.achievementClaims = (player.achievementClaims ?? Array.Empty<AchievementClaimData>())
                .Where(entry => entry != null && definitionsById.ContainsKey(entry.achievementId))
                .GroupBy(entry => entry.achievementId, StringComparer.Ordinal)
                .Select(group =>
                {
                    var definition = definitionsById[group.Key];
                    return new AchievementClaimData
                    {
                        achievementId = group.Key,
                        claimedTiers = Math.Clamp(
                            group.Max(entry => entry.claimedTiers),
                            0,
                            definition.Tiers.Length)
                    };
                })
                .ToArray();
        }

        public IReadOnlyList<AchievementState> GetStates(GameSaveData player)
        {
            return definitions
                .Select(definition => new AchievementState(
                    definition,
                    GetProgress(player, definition.Metric),
                    GetClaimedTiers(player, definition.AchievementId)))
                .ToArray();
        }

        public AchievementClaimResult Claim(
            GameSaveData player,
            string achievementId,
            bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return new AchievementClaimResult(AchievementClaimStatus.FeatureLocked);
            }

            if (!definitionsById.TryGetValue(achievementId, out var definition))
            {
                return new AchievementClaimResult(AchievementClaimStatus.UnknownAchievement);
            }

            SanitizeAgainstCatalog(player);
            var claimedTiers = GetClaimedTiers(player, achievementId);
            if (claimedTiers >= definition.Tiers.Length)
            {
                return new AchievementClaimResult(AchievementClaimStatus.AlreadyCompleted);
            }

            var tier = definition.Tiers[claimedTiers];
            if (GetProgress(player, definition.Metric) < tier.Target)
            {
                return new AchievementClaimResult(AchievementClaimStatus.RequirementNotMet);
            }

            GrantReward(player, tier.RewardType, tier.RewardAmount);
            SetClaimedTiers(player, achievementId, claimedTiers + 1);
            return new AchievementClaimResult(
                AchievementClaimStatus.Success,
                tier.RewardType,
                tier.RewardAmount);
        }

        public long GetProgress(GameSaveData player, AchievementMetric metric)
        {
            return metric switch
            {
                AchievementMetric.TotalOresMined => collectionService.GetTotalMined(player),
                AchievementMetric.HighestCompletedChapter => Math.Max(0, player.highestCompletedChapter),
                AchievementMetric.FacilityLevelTotal => Math.Max(0L,
                    (long)player.pickaxeLevel + player.drillLevel + player.robotLevel),
                AchievementMetric.MinerLevel => Math.Max(1, player.minerLevel),
                AchievementMetric.ResearchLevelTotal => (player.researchProgress ??
                        Array.Empty<ResearchProgressData>())
                    .Where(entry => entry != null)
                    .Sum(entry => (long)Math.Max(0, entry.level)),
                AchievementMetric.EquipmentAcquired => Math.Max(
                    Math.Max(0, player.equipmentRewardSequence),
                    (player.equipmentInventory ?? Array.Empty<EquipmentItemData>()).Length),
                _ => 0L
            };
        }

        private static int GetClaimedTiers(GameSaveData player, string achievementId)
        {
            return (player.achievementClaims ?? Array.Empty<AchievementClaimData>())
                .Where(entry => entry != null && entry.achievementId == achievementId)
                .Select(entry => Math.Max(0, entry.claimedTiers))
                .DefaultIfEmpty(0)
                .Max();
        }

        private static void SetClaimedTiers(
            GameSaveData player,
            string achievementId,
            int claimedTiers)
        {
            var entries = (player.achievementClaims ?? Array.Empty<AchievementClaimData>())
                .Where(entry => entry != null && entry.achievementId != achievementId)
                .ToList();
            entries.Add(new AchievementClaimData
            {
                achievementId = achievementId,
                claimedTiers = Math.Max(0, claimedTiers)
            });
            player.achievementClaims = entries.ToArray();
        }

        private static void GrantReward(
            GameSaveData player,
            AchievementRewardType rewardType,
            long amount)
        {
            switch (rewardType)
            {
                case AchievementRewardType.Gems:
                    player.gems = SaturatingAdd(player.gems, amount);
                    break;
                case AchievementRewardType.BlueprintCores:
                    player.blueprintCores = SaturatingAdd(player.blueprintCores, amount);
                    break;
                default:
                    player.credits = SaturatingAdd(player.credits, amount);
                    break;
            }
        }

        private static long SaturatingAdd(long left, long right)
        {
            left = Math.Max(0L, left);
            right = Math.Max(0L, right);
            return left > long.MaxValue - right ? long.MaxValue : left + right;
        }
    }
}
