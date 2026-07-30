namespace PocketForge.Save
{
    public static class GameSaveMigrator
    {
        public const int CurrentVersion = 8;

        public static GameSaveData Normalize(GameSaveData data)
        {
            data ??= new GameSaveData();
            data.credits = System.Math.Max(0, data.credits);
            data.gems = System.Math.Max(0, data.gems);
            data.blueprintCores = System.Math.Max(0, data.blueprintCores);
            data.researchProgress = NormalizeResearchProgress(data.researchProgress);
            data.stage = System.Math.Max(1, data.stage);
            data.furthestStage = System.Math.Max(data.stage, data.furthestStage);
            data.highestCompletedChapter = System.Math.Max(0, data.highestCompletedChapter);
            data.pickaxeLevel = System.Math.Max(0, data.pickaxeLevel);
            data.drillLevel = System.Math.Max(0, data.drillLevel);
            data.robotLevel = System.Math.Max(0, data.robotLevel);
            data.minerLevel = System.Math.Max(1, data.minerLevel);
            data.minerExperience = System.Math.Max(0, data.minerExperience);
            data.highestRewardedMinerLevel = System.Math.Max(
                1,
                System.Math.Min(data.minerLevel, data.highestRewardedMinerLevel));
            data.lastSavedUnixSeconds = System.Math.Max(0, data.lastSavedUnixSeconds);
            data.version = CurrentVersion;
            return data;
        }

        private static ResearchProgressData[] NormalizeResearchProgress(
            ResearchProgressData[] progressEntries)
        {
            if (progressEntries == null || progressEntries.Length == 0)
            {
                return System.Array.Empty<ResearchProgressData>();
            }

            var normalized = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var progress in progressEntries)
            {
                if (progress == null || string.IsNullOrWhiteSpace(progress.nodeId))
                {
                    continue;
                }

                var level = System.Math.Max(0, progress.level);
                if (!normalized.TryGetValue(progress.nodeId, out var existing) || level > existing)
                {
                    normalized[progress.nodeId] = level;
                }
            }

            var result = new ResearchProgressData[normalized.Count];
            var index = 0;
            foreach (var entry in normalized)
            {
                result[index++] = new ResearchProgressData
                {
                    nodeId = entry.Key,
                    level = entry.Value
                };
            }

            return result;
        }
    }
}
