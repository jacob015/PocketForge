using System;
using System.Linq;
using PocketForge.Economy;
using UnityEngine;

namespace PocketForge.Content
{
    [CreateAssetMenu(menuName = "Pocket Forge/Content/Mining Content Catalog", fileName = "MiningContentCatalog")]
    public sealed class MiningContentCatalog : ScriptableObject
    {
        [SerializeField] private OreDefinition[] ores = Array.Empty<OreDefinition>();
        [SerializeField] private UpgradeDefinition[] upgrades = Array.Empty<UpgradeDefinition>();
        [SerializeField, Min(60)] private int maxOfflineRewardSeconds = 14400;

        public int MaxOfflineRewardSeconds => maxOfflineRewardSeconds;

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
