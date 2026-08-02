using System.Linq;

namespace PocketForge.Save
{
    public static class GameSaveMigrator
    {
        public const int CurrentVersion = 12;

        public static GameSaveData Normalize(GameSaveData data)
        {
            data ??= new GameSaveData();
            data.credits = System.Math.Max(0, data.credits);
            data.gems = System.Math.Max(0, data.gems);
            data.blueprintCores = System.Math.Max(0, data.blueprintCores);
            data.researchProgress = NormalizeResearchProgress(data.researchProgress);
            data.equipmentInventory = NormalizeEquipmentInventory(data.equipmentInventory);
            data.equippedEquipment = NormalizeEquippedEquipment(
                data.equippedEquipment,
                data.equipmentInventory);
            data.equipmentRewardSequence = System.Math.Max(0, data.equipmentRewardSequence);
            data.oreCollection = NormalizeOreCollection(data.oreCollection);
            data.achievementClaims = NormalizeAchievementClaims(data.achievementClaims);
            data.dailyMissions = NormalizeMissionPeriod(data.dailyMissions);
            data.weeklyMissions = NormalizeMissionPeriod(data.weeklyMissions);
            data.lastObservedMissionUnixSeconds = System.Math.Max(
                0L,
                data.lastObservedMissionUnixSeconds);
            data.dailyShop = NormalizeDailyShop(data.dailyShop);
            data.lastObservedShopUnixSeconds = System.Math.Max(
                0L,
                data.lastObservedShopUnixSeconds);
            data.miningEvent = NormalizeMiningEvent(data.miningEvent);
            data.lastObservedEventUnixSeconds = System.Math.Max(
                0L,
                data.lastObservedEventUnixSeconds);
            data.bossesDefeated = System.Math.Max(
                System.Math.Max(0L, data.bossesDefeated),
                data.highestCompletedChapter);
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

        private static DailyShopData NormalizeDailyShop(DailyShopData shop)
        {
            shop ??= new DailyShopData();
            shop.periodKey ??= string.Empty;
            shop.claimedProductIds = (shop.claimedProductIds ?? System.Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(System.StringComparer.Ordinal)
                .ToArray();
            shop.claimCounts = (shop.claimCounts ?? System.Array.Empty<ShopClaimCountData>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.productId))
                .GroupBy(entry => entry.productId, System.StringComparer.Ordinal)
                .Select(group => new ShopClaimCountData
                {
                    productId = group.Key,
                    count = group.Max(entry => System.Math.Max(0, entry.count))
                })
                .ToArray();
            return shop;
        }

        private static MiningEventProgressData NormalizeMiningEvent(MiningEventProgressData progress)
        {
            progress ??= new MiningEventProgressData();
            progress.eventId ??= string.Empty;
            progress.periodKey ??= string.Empty;
            progress.baselineOresMined = System.Math.Max(0L, progress.baselineOresMined);
            progress.earnedTokens = System.Math.Max(0L, progress.earnedTokens);
            progress.tokenBalance = System.Math.Min(
                progress.earnedTokens,
                System.Math.Max(0L, progress.tokenBalance));
            progress.claimedTierIds = (progress.claimedTierIds ?? System.Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(System.StringComparer.Ordinal)
                .ToArray();
            progress.exchangePurchases = System.Math.Max(0, progress.exchangePurchases);
            return progress;
        }

        private static MissionPeriodData NormalizeMissionPeriod(MissionPeriodData period)
        {
            period ??= new MissionPeriodData();
            period.periodKey ??= string.Empty;
            period.baseline ??= new MissionProgressSnapshotData();
            period.baseline.oresMined = System.Math.Max(0L, period.baseline.oresMined);
            period.baseline.facilityUpgrades = System.Math.Max(0L, period.baseline.facilityUpgrades);
            period.baseline.researchCompleted = System.Math.Max(0L, period.baseline.researchCompleted);
            period.baseline.bossesDefeated = System.Math.Max(0L, period.baseline.bossesDefeated);
            period.baseline.equipmentAcquired = System.Math.Max(0L, period.baseline.equipmentAcquired);

            var uniqueClaims = new System.Collections.Generic.HashSet<string>(
                System.StringComparer.Ordinal);
            foreach (var missionId in period.claimedMissionIds ?? System.Array.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(missionId))
                {
                    uniqueClaims.Add(missionId);
                }
            }

            period.claimedMissionIds = new string[uniqueClaims.Count];
            uniqueClaims.CopyTo(period.claimedMissionIds);
            return period;
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

        private static OreCollectionData[] NormalizeOreCollection(OreCollectionData[] entries)
        {
            if (entries == null || entries.Length == 0)
            {
                return System.Array.Empty<OreCollectionData>();
            }

            var normalized = new System.Collections.Generic.Dictionary<string, long>();
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.contentId))
                {
                    continue;
                }

                var count = System.Math.Max(0L, entry.minedCount);
                if (!normalized.TryGetValue(entry.contentId, out var existing) || count > existing)
                {
                    normalized[entry.contentId] = count;
                }
            }

            var result = new OreCollectionData[normalized.Count];
            var index = 0;
            foreach (var entry in normalized)
            {
                result[index++] = new OreCollectionData
                {
                    contentId = entry.Key,
                    minedCount = entry.Value
                };
            }

            return result;
        }

        private static AchievementClaimData[] NormalizeAchievementClaims(
            AchievementClaimData[] entries)
        {
            if (entries == null || entries.Length == 0)
            {
                return System.Array.Empty<AchievementClaimData>();
            }

            var normalized = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.achievementId))
                {
                    continue;
                }

                var claimedTiers = System.Math.Max(0, entry.claimedTiers);
                if (!normalized.TryGetValue(entry.achievementId, out var existing) ||
                    claimedTiers > existing)
                {
                    normalized[entry.achievementId] = claimedTiers;
                }
            }

            var result = new AchievementClaimData[normalized.Count];
            var index = 0;
            foreach (var entry in normalized)
            {
                result[index++] = new AchievementClaimData
                {
                    achievementId = entry.Key,
                    claimedTiers = entry.Value
                };
            }

            return result;
        }

        private static EquipmentItemData[] NormalizeEquipmentInventory(
            EquipmentItemData[] inventory)
        {
            if (inventory == null || inventory.Length == 0)
            {
                return System.Array.Empty<EquipmentItemData>();
            }

            var normalized = new System.Collections.Generic.Dictionary<string, EquipmentItemData>();
            foreach (var item in inventory)
            {
                if (item == null ||
                    string.IsNullOrWhiteSpace(item.instanceId) ||
                    string.IsNullOrWhiteSpace(item.definitionId))
                {
                    continue;
                }

                var rarity = System.Math.Max(0, System.Math.Min(3, item.rarity));
                if (!normalized.TryGetValue(item.instanceId, out var existing) || rarity > existing.rarity)
                {
                    normalized[item.instanceId] = new EquipmentItemData
                    {
                        instanceId = item.instanceId,
                        definitionId = item.definitionId,
                        rarity = rarity
                    };
                }
            }

            var result = new EquipmentItemData[normalized.Count];
            var index = 0;
            foreach (var entry in normalized)
            {
                result[index++] = entry.Value;
            }

            return result;
        }

        private static EquippedItemData[] NormalizeEquippedEquipment(
            EquippedItemData[] equipped,
            EquipmentItemData[] inventory)
        {
            if (equipped == null || equipped.Length == 0 || inventory.Length == 0)
            {
                return System.Array.Empty<EquippedItemData>();
            }

            var knownItems = new System.Collections.Generic.HashSet<string>();
            foreach (var item in inventory)
            {
                knownItems.Add(item.instanceId);
            }

            var normalized = new System.Collections.Generic.Dictionary<int, EquippedItemData>();
            foreach (var item in equipped)
            {
                if (item == null ||
                    item.slot < 0 ||
                    item.slot > 3 ||
                    string.IsNullOrWhiteSpace(item.instanceId) ||
                    !knownItems.Contains(item.instanceId) ||
                    normalized.ContainsKey(item.slot))
                {
                    continue;
                }

                normalized[item.slot] = new EquippedItemData
                {
                    slot = item.slot,
                    instanceId = item.instanceId
                };
            }

            var result = new EquippedItemData[normalized.Count];
            var index = 0;
            foreach (var entry in normalized)
            {
                result[index++] = entry.Value;
            }

            return result;
        }
    }
}
