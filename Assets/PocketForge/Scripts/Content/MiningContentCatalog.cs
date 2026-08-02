using System;
using System.Collections.Generic;
using System.Linq;
using PocketForge.Economy;
using PocketForge.Progression;
using UnityEngine;

namespace PocketForge.Content
{
    [CreateAssetMenu(menuName = "Pocket Forge/Content/Mining Content Catalog", fileName = "MiningContentCatalog")]
    public sealed class MiningContentCatalog : ScriptableObject
    {
        [SerializeField] private OreDefinition[] ores = Array.Empty<OreDefinition>();
        [SerializeField] private UpgradeDefinition[] upgrades = Array.Empty<UpgradeDefinition>();
        [SerializeField] private ChapterDefinition[] chapters = Array.Empty<ChapterDefinition>();
        [SerializeField] private ResearchNodeDefinition[] researchNodes = Array.Empty<ResearchNodeDefinition>();
        [SerializeField] private EquipmentDefinition[] equipment = Array.Empty<EquipmentDefinition>();
        [SerializeField] private AchievementDefinition[] achievements = Array.Empty<AchievementDefinition>();
        [SerializeField] private MissionDefinition[] missions = Array.Empty<MissionDefinition>();
        [SerializeField] private ShopProductDefinition[] shopProducts = Array.Empty<ShopProductDefinition>();
        [SerializeField] private MiningEventDefinition[] miningEvents = Array.Empty<MiningEventDefinition>();
        [Header("Collection")]
        [SerializeField, Min(0f)] private float collectionDiscoveryPowerBonus = 0.01f;
        [SerializeField] private int[] collectionMilestones = { 25, 100, 500 };
        [SerializeField, Min(0f)] private float collectionMilestonePowerBonus = 0.01f;
        [SerializeField, Min(60)] private int maxOfflineRewardSeconds = 14400;
        [SerializeField, Min(1)] private int rewardedAdRewardMultiplier = 5;
        [SerializeField, Min(0.01f)] private float baseAutoPowerPerSecond = 0.5f;
        [SerializeField, Min(0.01f)] private float baseTapDamage = 1f;
        [SerializeField, Min(0.001f)] private float baseTapAssistSeconds = 0.05f;
        [SerializeField, Min(0f)] private float tapAssistSecondsPerPickaxeLevel = 0.005f;
        [SerializeField, Min(0.001f)] private float maxTapAssistSeconds = 0.1f;
        [SerializeField, Min(1f)] private float referenceTapsPerSecond = 5f;
        [Header("Miner Progression")]
        [SerializeField, Min(1)] private int baseExperienceToLevel = 20;
        [SerializeField, Min(0)] private int experienceGrowthPerLevel = 15;
        [SerializeField, Min(1)] private int normalOreExperiencePerChapter = 1;
        [SerializeField, Min(1)] private int bossExperiencePerChapter = 10;
        [SerializeField, Min(0f)] private float minerRankPowerBonusPerLevel = 0.02f;
        [SerializeField, Min(0)] private int levelRewardCreditsPerLevel = 25;
        [SerializeField, Min(1)] private int milestoneLevelInterval = 5;
        [SerializeField, Min(0)] private int milestoneRewardGems = 1;
        [SerializeField] private FeatureUnlockDefinition[] featureUnlocks = Array.Empty<FeatureUnlockDefinition>();

        private static readonly ChapterDefinition RuntimeDefaultChapter = ChapterDefinition.CreateRuntimeDefault();
        private static readonly ResearchNodeDefinition[] RuntimeDefaultResearchNodes =
            ResearchNodeDefinition.CreateRuntimeDefaults();
        private static readonly EquipmentDefinition[] RuntimeDefaultEquipment =
            EquipmentDefinition.CreateRuntimeDefaults();
        private static readonly AchievementDefinition[] RuntimeDefaultAchievements =
            AchievementDefinition.CreateRuntimeDefaults();
        private static readonly MissionDefinition[] RuntimeDefaultMissions =
            MissionDefinition.CreateRuntimeDefaults();
        private static readonly ShopProductDefinition[] RuntimeDefaultShopProducts =
            ShopProductDefinition.CreateRuntimeDefaults();
        private static readonly MiningEventDefinition[] RuntimeDefaultMiningEvents =
            MiningEventDefinition.CreateRuntimeDefaults();

        public int MaxOfflineRewardSeconds => maxOfflineRewardSeconds;
        public int RewardedAdRewardMultiplier => rewardedAdRewardMultiplier;
        public float BaseAutoPowerPerSecond => baseAutoPowerPerSecond;
        public float BaseTapDamage => baseTapDamage;
        public float BaseTapAssistSeconds => baseTapAssistSeconds;
        public float TapAssistSecondsPerPickaxeLevel => tapAssistSecondsPerPickaxeLevel;
        public float MaxTapAssistSeconds => maxTapAssistSeconds;
        public float ReferenceTapsPerSecond => referenceTapsPerSecond;
        public float TapCooldownSeconds => 1f / Mathf.Max(1f, referenceTapsPerSecond);
        public int BaseExperienceToLevel => baseExperienceToLevel;
        public int ExperienceGrowthPerLevel => experienceGrowthPerLevel;
        public int NormalOreExperiencePerChapter => normalOreExperiencePerChapter;
        public int BossExperiencePerChapter => bossExperiencePerChapter;
        public float MinerRankPowerBonusPerLevel => minerRankPowerBonusPerLevel;
        public int LevelRewardCreditsPerLevel => levelRewardCreditsPerLevel;
        public int MilestoneLevelInterval => milestoneLevelInterval;
        public int MilestoneRewardGems => milestoneRewardGems;
        public float CollectionDiscoveryPowerBonus => Mathf.Max(0f, collectionDiscoveryPowerBonus);
        public float CollectionMilestonePowerBonus => Mathf.Max(0f, collectionMilestonePowerBonus);

        public IReadOnlyList<int> GetCollectionMilestones()
        {
            var configured = collectionMilestones
                .Where(value => value > 0)
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            return configured.Length > 0 ? configured : new[] { 25, 100, 500 };
        }

        public IReadOnlyList<OreDefinition> GetOreDefinitions()
        {
            var configured = ores
                .Where(candidate => candidate != null && !string.IsNullOrWhiteSpace(candidate.ContentId))
                .GroupBy(candidate => candidate.ContentId, StringComparer.Ordinal)
                .Select(group => group.OrderBy(candidate => candidate.StartStage).First())
                .OrderBy(candidate => candidate.StartStage)
                .ToArray();
            return configured.Length > 0
                ? configured
                : new[] { OreDefinition.CreateRuntimeDefault() };
        }

        public OreDefinition GetOreForStage(int stage)
        {
            var ore = ores
                .Where(candidate => candidate != null && candidate.StartStage <= stage)
                .OrderByDescending(candidate => candidate.StartStage)
                .FirstOrDefault();

            return ore != null ? ore : OreDefinition.CreateRuntimeDefault();
        }

        public UpgradeDefinition GetUpgrade(UpgradeType type)
        {
            var upgrade = upgrades.FirstOrDefault(candidate => candidate != null && candidate.Type == type);
            return upgrade != null ? upgrade : CreateRuntimeUpgrade(type);
        }

        public ChapterDefinition GetChapterForStage(int stage)
        {
            var chapter = chapters
                .Where(candidate => candidate != null && candidate.StartStage <= stage)
                .OrderByDescending(candidate => candidate.StartStage)
                .FirstOrDefault();

            return chapter ?? RuntimeDefaultChapter;
        }

        public IReadOnlyList<ChapterDefinition> GetChapters()
        {
            return chapters
                .Where(candidate => candidate != null)
                .OrderBy(candidate => candidate.ChapterNumber)
                .ToArray();
        }

        public IReadOnlyList<FeatureUnlockDefinition> GetFeatureUnlocks()
        {
            var configured = featureUnlocks
                .Where(candidate => candidate != null && candidate.RequiredLevel > 1)
                .OrderBy(candidate => candidate.RequiredLevel)
                .ToArray();
            return configured.Length > 0
                ? configured
                : FeatureUnlockDefinition.CreateRuntimeDefaults();
        }

        public IReadOnlyList<ResearchNodeDefinition> GetResearchNodes()
        {
            var configured = researchNodes
                .Where(candidate => candidate != null && !string.IsNullOrWhiteSpace(candidate.NodeId))
                .ToArray();
            return configured.Length > 0 ? configured : RuntimeDefaultResearchNodes;
        }

        public ResearchNodeDefinition GetResearchNode(string nodeId)
        {
            return GetResearchNodes().FirstOrDefault(candidate => candidate.NodeId == nodeId);
        }

        public IReadOnlyList<EquipmentDefinition> GetEquipmentDefinitions()
        {
            var configured = equipment
                .Where(candidate => candidate != null && !string.IsNullOrWhiteSpace(candidate.DefinitionId))
                .ToArray();
            return configured.Length > 0 ? configured : RuntimeDefaultEquipment;
        }

        public EquipmentDefinition GetEquipment(string definitionId)
        {
            return GetEquipmentDefinitions()
                .FirstOrDefault(candidate => candidate.DefinitionId == definitionId);
        }

        public IReadOnlyList<AchievementDefinition> GetAchievements()
        {
            var configured = achievements
                .Where(candidate => candidate != null &&
                                    !string.IsNullOrWhiteSpace(candidate.AchievementId) &&
                                    candidate.Tiers.Length > 0)
                .GroupBy(candidate => candidate.AchievementId, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();
            return configured.Length > 0 ? configured : RuntimeDefaultAchievements;
        }

        public AchievementDefinition GetAchievement(string achievementId)
        {
            return GetAchievements()
                .FirstOrDefault(candidate => candidate.AchievementId == achievementId);
        }

        public IReadOnlyList<MissionDefinition> GetMissions()
        {
            var configured = missions
                .Where(candidate => candidate != null &&
                                    !string.IsNullOrWhiteSpace(candidate.MissionId))
                .GroupBy(candidate => candidate.MissionId, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();
            return configured.Length > 0 ? configured : RuntimeDefaultMissions;
        }

        public IReadOnlyList<ShopProductDefinition> GetShopProducts()
        {
            var configured = shopProducts
                .Where(candidate => candidate != null &&
                                    !string.IsNullOrWhiteSpace(candidate.ProductId))
                .GroupBy(candidate => candidate.ProductId, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();
            return configured.Length > 0 ? configured : RuntimeDefaultShopProducts;
        }

        public IReadOnlyList<MiningEventDefinition> GetMiningEvents()
        {
            var configured = miningEvents
                .Where(candidate => candidate != null &&
                                    !string.IsNullOrWhiteSpace(candidate.EventId))
                .GroupBy(candidate => candidate.EventId, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();
            return configured.Length > 0 ? configured : RuntimeDefaultMiningEvents;
        }

        public static MiningContentCatalog CreateRuntimeDefault()
        {
            var catalog = CreateInstance<MiningContentCatalog>();
            catalog.name = "Runtime Mining Content Catalog";
            catalog.ores = new[] { OreDefinition.CreateRuntimeDefault() };
            catalog.upgrades = new[]
            {
                UpgradeDefinition.CreateRuntimeDefault(UpgradeType.Pickaxe, 10, 1f),
                UpgradeDefinition.CreateRuntimeDefault(UpgradeType.Drill, 25, 0.5f),
                UpgradeDefinition.CreateRuntimeDefault(UpgradeType.Robot, 50, 0.1f)
            };
            catalog.chapters = new[] { ChapterDefinition.CreateRuntimeDefault() };
            catalog.researchNodes = ResearchNodeDefinition.CreateRuntimeDefaults();
            catalog.equipment = EquipmentDefinition.CreateRuntimeDefaults();
            catalog.achievements = AchievementDefinition.CreateRuntimeDefaults();
            catalog.missions = MissionDefinition.CreateRuntimeDefaults();
            catalog.shopProducts = ShopProductDefinition.CreateRuntimeDefaults();
            catalog.miningEvents = MiningEventDefinition.CreateRuntimeDefaults();
            return catalog;
        }

        private static UpgradeDefinition CreateRuntimeUpgrade(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.Drill => UpgradeDefinition.CreateRuntimeDefault(type, 25, 0.5f),
                UpgradeType.Robot => UpgradeDefinition.CreateRuntimeDefault(type, 50, 0.1f),
                _ => UpgradeDefinition.CreateRuntimeDefault(type, 10, 1f)
            };
        }
    }
}
