using System;
using UnityEngine;

namespace PocketForge.Content
{
    public enum MissionPeriod
    {
        Daily,
        Weekly
    }

    public enum MissionMetric
    {
        OresMined,
        FacilityUpgrades,
        ResearchCompleted,
        BossesDefeated,
        EquipmentAcquired
    }

    public enum MissionRewardType
    {
        Credits,
        Gems,
        BlueprintCores,
        Equipment
    }

    [Serializable]
    public sealed class MissionDefinition
    {
        [SerializeField] private string missionId = string.Empty;
        [SerializeField] private string nameLocalizationKey = string.Empty;
        [SerializeField] private MissionPeriod period;
        [SerializeField] private MissionMetric metric;
        [SerializeField, Min(1)] private long target = 1;
        [SerializeField] private MissionRewardType rewardType;
        [SerializeField, Min(1)] private long rewardAmount = 1;

        public string MissionId => missionId;
        public string NameLocalizationKey => nameLocalizationKey;
        public MissionPeriod Period => period;
        public MissionMetric Metric => metric;
        public long Target => Math.Max(1L, target);
        public MissionRewardType RewardType => rewardType;
        public long RewardAmount => Math.Max(1L, rewardAmount);

        public static MissionDefinition[] CreateRuntimeDefaults()
        {
            return new[]
            {
                Create("daily_mine", "mission_mine_ores", MissionPeriod.Daily,
                    MissionMetric.OresMined, 20, MissionRewardType.Credits, 100),
                Create("daily_upgrade", "mission_upgrade_facilities", MissionPeriod.Daily,
                    MissionMetric.FacilityUpgrades, 1, MissionRewardType.Credits, 75),
                Create("daily_research", "mission_complete_research", MissionPeriod.Daily,
                    MissionMetric.ResearchCompleted, 1, MissionRewardType.Credits, 125),
                Create("daily_boss", "mission_defeat_bosses", MissionPeriod.Daily,
                    MissionMetric.BossesDefeated, 1, MissionRewardType.Gems, 1),
                Create("weekly_mine", "mission_mine_ores", MissionPeriod.Weekly,
                    MissionMetric.OresMined, 300, MissionRewardType.Credits, 600),
                Create("weekly_upgrade", "mission_upgrade_facilities", MissionPeriod.Weekly,
                    MissionMetric.FacilityUpgrades, 8, MissionRewardType.BlueprintCores, 2),
                Create("weekly_boss", "mission_defeat_bosses", MissionPeriod.Weekly,
                    MissionMetric.BossesDefeated, 3, MissionRewardType.Gems, 3),
                Create("weekly_equipment", "mission_collect_equipment", MissionPeriod.Weekly,
                    MissionMetric.EquipmentAcquired, 2, MissionRewardType.Credits, 800)
            };
        }

        private static MissionDefinition Create(
            string id,
            string localizationKey,
            MissionPeriod missionPeriod,
            MissionMetric missionMetric,
            long requiredProgress,
            MissionRewardType type,
            long amount)
        {
            return new MissionDefinition
            {
                missionId = id,
                nameLocalizationKey = localizationKey,
                period = missionPeriod,
                metric = missionMetric,
                target = Math.Max(1L, requiredProgress),
                rewardType = type,
                rewardAmount = Math.Max(1L, amount)
            };
        }
    }
}
