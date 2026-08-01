using System;
using UnityEngine;

namespace PocketForge.Content
{
    public enum AchievementMetric
    {
        TotalOresMined,
        HighestCompletedChapter,
        FacilityLevelTotal,
        MinerLevel,
        ResearchLevelTotal,
        EquipmentAcquired
    }

    public enum AchievementRewardType
    {
        Credits,
        Gems,
        BlueprintCores
    }

    [Serializable]
    public sealed class AchievementTierDefinition
    {
        [SerializeField, Min(1)] private long target = 1;
        [SerializeField] private AchievementRewardType rewardType;
        [SerializeField, Min(1)] private long rewardAmount = 1;

        public long Target => Math.Max(1L, target);
        public AchievementRewardType RewardType => rewardType;
        public long RewardAmount => Math.Max(1L, rewardAmount);

        public static AchievementTierDefinition Create(
            long requiredProgress,
            AchievementRewardType type,
            long amount)
        {
            return new AchievementTierDefinition
            {
                target = Math.Max(1L, requiredProgress),
                rewardType = type,
                rewardAmount = Math.Max(1L, amount)
            };
        }
    }

    [Serializable]
    public sealed class AchievementDefinition
    {
        [SerializeField] private string achievementId = string.Empty;
        [SerializeField] private string nameLocalizationKey = string.Empty;
        [SerializeField] private AchievementMetric metric;
        [SerializeField] private AchievementTierDefinition[] tiers =
            Array.Empty<AchievementTierDefinition>();

        public string AchievementId => achievementId;
        public string NameLocalizationKey => nameLocalizationKey;
        public AchievementMetric Metric => metric;
        public AchievementTierDefinition[] Tiers => tiers ?? Array.Empty<AchievementTierDefinition>();

        public static AchievementDefinition[] CreateRuntimeDefaults()
        {
            return new[]
            {
                Create("mine_ores", "achievement_mine_ores", AchievementMetric.TotalOresMined,
                    AchievementTierDefinition.Create(10, AchievementRewardType.Credits, 100),
                    AchievementTierDefinition.Create(100, AchievementRewardType.Credits, 500),
                    AchievementTierDefinition.Create(1000, AchievementRewardType.Credits, 2000)),
                Create("clear_chapters", "achievement_clear_chapters", AchievementMetric.HighestCompletedChapter,
                    AchievementTierDefinition.Create(1, AchievementRewardType.Gems, 1),
                    AchievementTierDefinition.Create(2, AchievementRewardType.Gems, 2),
                    AchievementTierDefinition.Create(3, AchievementRewardType.Gems, 3)),
                Create("upgrade_facilities", "achievement_upgrade_facilities", AchievementMetric.FacilityLevelTotal,
                    AchievementTierDefinition.Create(5, AchievementRewardType.Credits, 150),
                    AchievementTierDefinition.Create(20, AchievementRewardType.Credits, 750),
                    AchievementTierDefinition.Create(50, AchievementRewardType.Credits, 3000)),
                Create("raise_miner", "achievement_raise_miner", AchievementMetric.MinerLevel,
                    AchievementTierDefinition.Create(3, AchievementRewardType.Gems, 1),
                    AchievementTierDefinition.Create(5, AchievementRewardType.Gems, 1),
                    AchievementTierDefinition.Create(7, AchievementRewardType.Gems, 2)),
                Create("complete_research", "achievement_complete_research", AchievementMetric.ResearchLevelTotal,
                    AchievementTierDefinition.Create(1, AchievementRewardType.BlueprintCores, 1),
                    AchievementTierDefinition.Create(4, AchievementRewardType.BlueprintCores, 2),
                    AchievementTierDefinition.Create(8, AchievementRewardType.BlueprintCores, 3)),
                Create("collect_equipment", "achievement_collect_equipment", AchievementMetric.EquipmentAcquired,
                    AchievementTierDefinition.Create(1, AchievementRewardType.Credits, 250),
                    AchievementTierDefinition.Create(4, AchievementRewardType.Credits, 1000),
                    AchievementTierDefinition.Create(12, AchievementRewardType.Credits, 5000))
            };
        }

        private static AchievementDefinition Create(
            string id,
            string localizationKey,
            AchievementMetric achievementMetric,
            params AchievementTierDefinition[] tierDefinitions)
        {
            return new AchievementDefinition
            {
                achievementId = id,
                nameLocalizationKey = localizationKey,
                metric = achievementMetric,
                tiers = tierDefinitions ?? Array.Empty<AchievementTierDefinition>()
            };
        }
    }
}
