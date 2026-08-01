namespace PocketForge.Save
{
    public static class GameSaveMigrator
    {
        public const int CurrentVersion = 9;

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
