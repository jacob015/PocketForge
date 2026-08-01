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
    public sealed class EquipmentItemData
    {
        public string instanceId = string.Empty;
        public string definitionId = string.Empty;
        public int rarity;
    }

    [Serializable]
    public sealed class EquippedItemData
    {
        public int slot;
        public string instanceId = string.Empty;
    }

    [Serializable]
    public sealed class OreCollectionData
    {
        public string contentId = string.Empty;
        public long minedCount;
    }

    [Serializable]
    public sealed class AchievementClaimData
    {
        public string achievementId = string.Empty;
        public int claimedTiers;
    }

    [Serializable]
    public sealed class GameSaveData
    {
        public int version = GameSaveMigrator.CurrentVersion;
        public long credits;
        public long gems;
        public long blueprintCores;
        public ResearchProgressData[] researchProgress = Array.Empty<ResearchProgressData>();
        public EquipmentItemData[] equipmentInventory = Array.Empty<EquipmentItemData>();
        public EquippedItemData[] equippedEquipment = Array.Empty<EquippedItemData>();
        public int equipmentRewardSequence;
        public OreCollectionData[] oreCollection = Array.Empty<OreCollectionData>();
        public AchievementClaimData[] achievementClaims = Array.Empty<AchievementClaimData>();
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
