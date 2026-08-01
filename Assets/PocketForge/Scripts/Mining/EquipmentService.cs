using System;
using System.Collections.Generic;
using System.Linq;
using PocketForge.Content;
using PocketForge.Save;

namespace PocketForge.Mining
{
    public enum EquipmentActionStatus
    {
        Success,
        FeatureLocked,
        ItemNotFound,
        InvalidSlot,
        AlreadyEquipped,
        NotEquipped,
        NeedMoreDuplicates,
        MaxRarity,
        InvalidDefinition
    }

    public readonly struct EquipmentItemState
    {
        public EquipmentItemState(
            EquipmentItemData item,
            EquipmentDefinition definition,
            bool isEquipped)
        {
            Item = item;
            Definition = definition;
            IsEquipped = isEquipped;
        }

        public EquipmentItemData Item { get; }
        public EquipmentDefinition Definition { get; }
        public EquipmentRarity Rarity => (EquipmentRarity)Item.rarity;
        public bool IsEquipped { get; }
        public float PowerBonus => Definition.GetPowerBonus(Rarity);
    }

    public sealed class EquipmentService
    {
        private const int FusionMaterialCount = 3;
        private readonly MiningContentCatalog catalog;

        public EquipmentService(MiningContentCatalog catalog)
        {
            this.catalog = catalog;
        }

        public void SanitizeAgainstCatalog(GameSaveData player)
        {
            var validItems = (player.equipmentInventory ?? Array.Empty<EquipmentItemData>())
                .Where(item => item != null &&
                               !string.IsNullOrWhiteSpace(item.instanceId) &&
                               catalog.GetEquipment(item.definitionId) != null)
                .GroupBy(item => item.instanceId, StringComparer.Ordinal)
                .Select(group => group.OrderByDescending(item => item.rarity).First())
                .ToArray();
            foreach (var item in validItems)
            {
                item.rarity = Math.Max(0, Math.Min((int)EquipmentRarity.Legendary, item.rarity));
            }
            player.equipmentInventory = validItems;

            var itemById = validItems.ToDictionary(item => item.instanceId, item => item);
            var validSlots = new HashSet<EquipmentSlot>();
            var equipped = new List<EquippedItemData>();
            foreach (var entry in player.equippedEquipment ?? Array.Empty<EquippedItemData>())
            {
                if (entry == null ||
                    !Enum.IsDefined(typeof(EquipmentSlot), entry.slot) ||
                    !itemById.TryGetValue(entry.instanceId, out var item))
                {
                    continue;
                }

                var definition = catalog.GetEquipment(item.definitionId);
                var slot = (EquipmentSlot)entry.slot;
                if (definition == null || definition.Slot != slot || !validSlots.Add(slot))
                {
                    continue;
                }

                equipped.Add(new EquippedItemData { slot = entry.slot, instanceId = entry.instanceId });
            }

            player.equippedEquipment = equipped.ToArray();
        }

        public IReadOnlyList<EquipmentItemState> GetInventoryStates(GameSaveData player)
        {
            SanitizeAgainstCatalog(player);
            var equippedIds = new HashSet<string>(
                player.equippedEquipment.Select(entry => entry.instanceId));
            return player.equipmentInventory
                .Select(item => new EquipmentItemState(
                    item,
                    catalog.GetEquipment(item.definitionId),
                    equippedIds.Contains(item.instanceId)))
                .OrderBy(state => state.Definition.Slot)
                .ThenByDescending(state => state.Rarity)
                .ThenBy(state => state.Item.instanceId, StringComparer.Ordinal)
                .ToArray();
        }

        public EquipmentItemState? GetEquipped(GameSaveData player, EquipmentSlot slot)
        {
            SanitizeAgainstCatalog(player);
            var equipped = player.equippedEquipment.FirstOrDefault(entry => entry.slot == (int)slot);
            if (equipped == null)
            {
                return null;
            }

            var item = player.equipmentInventory.First(candidate => candidate.instanceId == equipped.instanceId);
            return new EquipmentItemState(item, catalog.GetEquipment(item.definitionId), true);
        }

        public EquipmentActionStatus TryEquip(GameSaveData player, string instanceId, bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return EquipmentActionStatus.FeatureLocked;
            }

            SanitizeAgainstCatalog(player);
            var item = player.equipmentInventory.FirstOrDefault(candidate => candidate.instanceId == instanceId);
            if (item == null)
            {
                return EquipmentActionStatus.ItemNotFound;
            }

            var definition = catalog.GetEquipment(item.definitionId);
            if (definition == null)
            {
                return EquipmentActionStatus.InvalidDefinition;
            }

            var equipped = player.equippedEquipment.ToList();
            var current = equipped.FirstOrDefault(entry => entry.slot == (int)definition.Slot);
            if (current != null && current.instanceId == instanceId)
            {
                return EquipmentActionStatus.AlreadyEquipped;
            }

            equipped.RemoveAll(entry => entry.slot == (int)definition.Slot || entry.instanceId == instanceId);
            equipped.Add(new EquippedItemData
            {
                slot = (int)definition.Slot,
                instanceId = instanceId
            });
            player.equippedEquipment = equipped.ToArray();
            return EquipmentActionStatus.Success;
        }

        public EquipmentActionStatus TryUnequip(GameSaveData player, EquipmentSlot slot, bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return EquipmentActionStatus.FeatureLocked;
            }

            var equipped = (player.equippedEquipment ?? Array.Empty<EquippedItemData>()).ToList();
            var removed = equipped.RemoveAll(entry => entry != null && entry.slot == (int)slot);
            if (removed == 0)
            {
                return EquipmentActionStatus.NotEquipped;
            }

            player.equippedEquipment = equipped.ToArray();
            return EquipmentActionStatus.Success;
        }

        public EquipmentActionStatus TryFuse(
            GameSaveData player,
            string definitionId,
            EquipmentRarity rarity,
            bool featureUnlocked,
            string resultInstanceId = null)
        {
            if (!featureUnlocked)
            {
                return EquipmentActionStatus.FeatureLocked;
            }

            if (rarity >= EquipmentRarity.Legendary)
            {
                return EquipmentActionStatus.MaxRarity;
            }

            if (catalog.GetEquipment(definitionId) == null)
            {
                return EquipmentActionStatus.InvalidDefinition;
            }

            SanitizeAgainstCatalog(player);
            var equippedIds = new HashSet<string>(player.equippedEquipment.Select(entry => entry.instanceId));
            var materials = player.equipmentInventory
                .Where(item => item.definitionId == definitionId &&
                               item.rarity == (int)rarity &&
                               !equippedIds.Contains(item.instanceId))
                .OrderBy(item => item.instanceId, StringComparer.Ordinal)
                .Take(FusionMaterialCount)
                .ToArray();
            if (materials.Length < FusionMaterialCount)
            {
                return EquipmentActionStatus.NeedMoreDuplicates;
            }

            var materialIds = new HashSet<string>(materials.Select(item => item.instanceId));
            var inventory = player.equipmentInventory
                .Where(item => !materialIds.Contains(item.instanceId))
                .ToList();
            inventory.Add(new EquipmentItemData
            {
                instanceId = string.IsNullOrWhiteSpace(resultInstanceId)
                    ? Guid.NewGuid().ToString("N")
                    : resultInstanceId,
                definitionId = definitionId,
                rarity = (int)rarity + 1
            });
            player.equipmentInventory = inventory.ToArray();
            return EquipmentActionStatus.Success;
        }

        public EquipmentActionStatus AutoEquip(GameSaveData player, bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return EquipmentActionStatus.FeatureLocked;
            }

            SanitizeAgainstCatalog(player);
            var equipped = new List<EquippedItemData>();
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                var best = player.equipmentInventory
                    .Where(item => catalog.GetEquipment(item.definitionId)?.Slot == slot)
                    .OrderByDescending(item => catalog.GetEquipment(item.definitionId)
                        .GetPowerBonus((EquipmentRarity)item.rarity))
                    .ThenBy(item => item.instanceId, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (best != null)
                {
                    equipped.Add(new EquippedItemData { slot = (int)slot, instanceId = best.instanceId });
                }
            }

            player.equippedEquipment = equipped.ToArray();
            return EquipmentActionStatus.Success;
        }

        public EquipmentItemData GrantBossReward(
            GameSaveData player,
            int chapterNumber,
            string instanceId = null)
        {
            var definitions = catalog.GetEquipmentDefinitions();
            if (definitions.Count == 0)
            {
                return null;
            }

            var sequence = Math.Max(0, player.equipmentRewardSequence);
            var definition = definitions[sequence % definitions.Count];
            var rarity = chapterNumber >= 3 ? EquipmentRarity.Rare : EquipmentRarity.Common;
            var item = new EquipmentItemData
            {
                instanceId = string.IsNullOrWhiteSpace(instanceId)
                    ? Guid.NewGuid().ToString("N")
                    : instanceId,
                definitionId = definition.DefinitionId,
                rarity = (int)rarity
            };

            var inventory = (player.equipmentInventory ?? Array.Empty<EquipmentItemData>()).ToList();
            inventory.Add(item);
            player.equipmentInventory = inventory.ToArray();
            player.equipmentRewardSequence = sequence + 1;
            return item;
        }

        public float GetPowerMultiplier(GameSaveData player)
        {
            SanitizeAgainstCatalog(player);
            var bonus = 0f;
            foreach (var equipped in player.equippedEquipment)
            {
                var item = player.equipmentInventory.First(candidate => candidate.instanceId == equipped.instanceId);
                var definition = catalog.GetEquipment(item.definitionId);
                bonus += definition.GetPowerBonus((EquipmentRarity)item.rarity);
            }

            return 1f + bonus;
        }
    }
}
