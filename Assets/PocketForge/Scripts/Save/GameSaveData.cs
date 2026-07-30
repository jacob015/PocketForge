using System;

namespace PocketForge.Save
{
    [Serializable]
    public sealed class ResearchProgressData
    {
        public string nodeId = string.Empty;
        public int level;
    }

    [Serializable]
    public sealed class GameSaveData
    {
        public int version = GameSaveMigrator.CurrentVersion;
        public long credits;
        public long gems;
        public long blueprintCores;
        public ResearchProgressData[] researchProgress = Array.Empty<ResearchProgressData>();
        public int stage = 1;
        public int furthestStage = 1;
        public int highestCompletedChapter;
        public int pickaxeLevel;
        public int drillLevel;
        public int robotLevel;
        public int minerLevel = 1;
        public int minerExperience;
        public int highestRewardedMinerLevel = 1;
        public bool adsRemoved;
        public long lastSavedUnixSeconds;
    }
}
