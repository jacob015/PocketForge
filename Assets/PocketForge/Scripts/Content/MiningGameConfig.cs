using PocketForge.Economy;
using UnityEngine;

namespace PocketForge.Content
{
    [CreateAssetMenu(menuName = "Pocket Forge/Mining Game Config", fileName = "MiningGameConfig")]
    public sealed class MiningGameConfig : ScriptableObject
    {
        [Header("Ore")]
        [SerializeField, Min(1f)] private float baseOreDurability = 10f;
        [SerializeField, Min(0f)] private float durabilityPerStage = 5f;
        [SerializeField, Range(0f, 1f)] private float rareOreChance = 0.08f;
        [SerializeField, Min(1)] private int normalOreRewardMultiplier = 2;
        [SerializeField, Min(1)] private int rareOreRewardMultiplier = 5;

        [Header("Upgrades")]
        [SerializeField, Min(1)] private int pickaxeBaseCost = 10;
        [SerializeField, Min(1)] private int drillBaseCost = 25;
        [SerializeField, Min(1)] private int robotBaseCost = 50;
        [SerializeField, Min(1f)] private float upgradeCostGrowth = 1.65f;
        [SerializeField, Min(0f)] private float drillPowerPerLevel = 0.5f;
        [SerializeField, Min(0f)] private float robotRewardBonusPerLevel = 0.1f;

        public float BaseOreDurability => baseOreDurability;
        public float DurabilityPerStage => durabilityPerStage;
        public float RareOreChance => rareOreChance;
        public int NormalOreRewardMultiplier => normalOreRewardMultiplier;
        public int RareOreRewardMultiplier => rareOreRewardMultiplier;
        public float UpgradeCostGrowth => upgradeCostGrowth;
        public float DrillPowerPerLevel => drillPowerPerLevel;
        public float RobotRewardBonusPerLevel => robotRewardBonusPerLevel;

        public int GetUpgradeBaseCost(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.Pickaxe => pickaxeBaseCost,
                UpgradeType.Drill => drillBaseCost,
                UpgradeType.Robot => robotBaseCost,
                _ => pickaxeBaseCost
            };
        }

        public static MiningGameConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<MiningGameConfig>();
            config.name = "Runtime Mining Game Config";
            return config;
        }
    }
}
