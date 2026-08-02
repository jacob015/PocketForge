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
    public sealed class MissionProgressSnapshotData
    {
        public long oresMined;
        public long facilityUpgrades;
        public long researchCompleted;
        public long bossesDefeated;
        public long equipmentAcquired;
    }

    [Serializable]
    public sealed class MissionPeriodData
    {
        public string periodKey = string.Empty;
        public MissionProgressSnapshotData baseline = new();
        public string[] claimedMissionIds = Array.Empty<string>();
        public bool completionRewardClaimed;
    }

    [Serializable]
    public sealed class ShopClaimCountData
    {
        public string productId = string.Empty;
        public int count;
    }

    [Serializable]
    public sealed class DailyShopData
    {
        public string periodKey = string.Empty;
        public string[] claimedProductIds = Array.Empty<string>();
        public ShopClaimCountData[] claimCounts = Array.Empty<ShopClaimCountData>();
    }

    [Serializable]
    public sealed class MiningEventProgressData
    {
        public string eventId = string.Empty;
        public string periodKey = string.Empty;
        public long baselineOresMined;
        public long earnedTokens;
        public long tokenBalance;
        public string[] claimedTierIds = Array.Empty<string>();
        public int exchangePurchases;
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
        public MissionPeriodData dailyMissions = new();
        public MissionPeriodData weeklyMissions = new();
        public long lastObservedMissionUnixSeconds;
        public DailyShopData dailyShop = new();
        public long lastObservedShopUnixSeconds;
        public bool starterPackPurchased;
        public MiningEventProgressData miningEvent = new();
        public long lastObservedEventUnixSeconds;
        public long bossesDefeated;
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
