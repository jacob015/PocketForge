using PocketForge.Economy;
using UnityEngine;

namespace PocketForge.Content
{
    [CreateAssetMenu(menuName = "Pocket Forge/Content/Upgrade Definition", fileName = "UpgradeDefinition")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private UpgradeType type;
        [SerializeField, Min(1)] private int baseCost = 10;
        [SerializeField, Min(1f)] private float costGrowth = 1.65f;
        [SerializeField, Min(0f)] private float effectPerLevel = 1f;

        public UpgradeType Type => type;
        public float EffectPerLevel => effectPerLevel;

        public long GetCost(int currentLevel)
        {
            var value = baseCost * System.Math.Pow(costGrowth, System.Math.Max(0, currentLevel));
            if (double.IsInfinity(value) || value >= long.MaxValue)
            {
                return long.MaxValue;
            }

            return System.Math.Max(1L, (long)System.Math.Ceiling(value));
        }

        public static UpgradeDefinition CreateRuntimeDefault(UpgradeType type, int baseCost, float effectPerLevel)
        {
            var definition = CreateInstance<UpgradeDefinition>();
            definition.name = $"Runtime {type} Upgrade";
            definition.type = type;
            definition.baseCost = baseCost;
            definition.effectPerLevel = effectPerLevel;
            return definition;
        }
    }
}
