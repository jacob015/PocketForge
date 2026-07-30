using System;
using UnityEngine;

namespace PocketForge.Content
{
    [Serializable]
    public sealed class ResearchNodeDefinition
    {
        [SerializeField] private string nodeId = "core_output";
        [SerializeField] private string nameLocalizationKey = "research_core_output";
        [SerializeField] private string prerequisiteNodeId = string.Empty;
        [SerializeField, Min(0)] private int prerequisiteLevel;
        [SerializeField, Min(1)] private int maxLevel = 5;
        [SerializeField, Min(1)] private int baseCost = 1;
        [SerializeField, Min(0)] private int costStep = 1;
        [SerializeField, Min(0f)] private float powerBonusPerLevel = 0.05f;

        public string NodeId => nodeId;
        public string NameLocalizationKey => nameLocalizationKey;
        public string PrerequisiteNodeId => prerequisiteNodeId;
        public int PrerequisiteLevel => Mathf.Max(0, prerequisiteLevel);
        public int MaxLevel => Mathf.Max(1, maxLevel);
        public float PowerBonusPerLevel => Mathf.Max(0f, powerBonusPerLevel);

        public long GetCost(int currentLevel)
        {
            var safeLevel = Math.Max(0, currentLevel);
            return Math.Max(1L, (long)baseCost + (long)costStep * safeLevel);
        }

        public static ResearchNodeDefinition[] CreateRuntimeDefaults()
        {
            return new[]
            {
                Create("core_output", "research_core_output", string.Empty, 0, 5, 1, 1, 0.05f),
                Create("precision_tools", "research_precision_tools", "core_output", 2, 4, 2, 2, 0.07f),
                Create("deep_automation", "research_deep_automation", "precision_tools", 2, 3, 4, 4, 0.10f)
            };
        }

        private static ResearchNodeDefinition Create(
            string id,
            string localizationKey,
            string prerequisiteId,
            int requiredLevel,
            int maximumLevel,
            int initialCost,
            int costIncrease,
            float bonusPerLevel)
        {
            return new ResearchNodeDefinition
            {
                nodeId = id,
                nameLocalizationKey = localizationKey,
                prerequisiteNodeId = prerequisiteId,
                prerequisiteLevel = requiredLevel,
                maxLevel = maximumLevel,
                baseCost = initialCost,
                costStep = costIncrease,
                powerBonusPerLevel = bonusPerLevel
            };
        }
    }
}
