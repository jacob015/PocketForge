using System;

namespace PocketForge.Progression
{
    public enum ProgressionFeature
    {
        Equipment,
        Museum,
        Research,
        Missions,
        Shop,
        Events
    }

    public static class ProgressionFeatureExtensions
    {
        public static string LocalizationKey(this ProgressionFeature feature)
        {
            return feature switch
            {
                ProgressionFeature.Equipment => "feature_equipment",
                ProgressionFeature.Museum => "feature_museum",
                ProgressionFeature.Research => "feature_research",
                ProgressionFeature.Missions => "feature_missions",
                ProgressionFeature.Shop => "feature_shop",
                _ => "feature_events"
            };
        }
    }

    [Serializable]
    public sealed class FeatureUnlockDefinition
    {
        public ProgressionFeature feature;
        public int requiredLevel = 2;

        public ProgressionFeature Feature => feature;
        public int RequiredLevel => Math.Max(2, requiredLevel);

        public static FeatureUnlockDefinition[] CreateRuntimeDefaults()
        {
            return new[]
            {
                Create(ProgressionFeature.Equipment, 2),
                Create(ProgressionFeature.Museum, 3),
                Create(ProgressionFeature.Research, 4),
                Create(ProgressionFeature.Missions, 5),
                Create(ProgressionFeature.Shop, 6),
                Create(ProgressionFeature.Events, 7)
            };
        }

        private static FeatureUnlockDefinition Create(ProgressionFeature feature, int level)
        {
            return new FeatureUnlockDefinition
            {
                feature = feature,
                requiredLevel = level
            };
        }
    }
}
