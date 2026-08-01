using System;
using UnityEngine;

namespace PocketForge.Content
{
    public enum EquipmentSlot
    {
        Pickaxe,
        Drill,
        Robot,
        Charm
    }

    public enum EquipmentRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public sealed class EquipmentDefinition
    {
        [SerializeField] private string definitionId = string.Empty;
        [SerializeField] private string localizationKey = string.Empty;
        [SerializeField] private EquipmentSlot slot;
        [SerializeField, Min(0f)] private float basePowerBonus = 0.05f;

        public string DefinitionId => definitionId;
        public string LocalizationKey => localizationKey;
        public EquipmentSlot Slot => slot;
        public float BasePowerBonus => Mathf.Max(0f, basePowerBonus);

        public float GetPowerBonus(EquipmentRarity rarity)
        {
            var scale = rarity switch
            {
                EquipmentRarity.Rare => 2f,
                EquipmentRarity.Epic => 4f,
                EquipmentRarity.Legendary => 8f,
                _ => 1f
            };
            return BasePowerBonus * scale;
        }

        public static EquipmentDefinition[] CreateRuntimeDefaults()
        {
            return new[]
            {
                Create("rugged_pickaxe", "equipment_rugged_pickaxe", EquipmentSlot.Pickaxe, 0.05f),
                Create("core_drill", "equipment_core_drill", EquipmentSlot.Drill, 0.06f),
                Create("forge_bot", "equipment_forge_bot", EquipmentSlot.Robot, 0.05f),
                Create("lucky_crystal", "equipment_lucky_crystal", EquipmentSlot.Charm, 0.04f)
            };
        }

        private static EquipmentDefinition Create(
            string id,
            string key,
            EquipmentSlot equipmentSlot,
            float bonus)
        {
            return new EquipmentDefinition
            {
                definitionId = id,
                localizationKey = key,
                slot = equipmentSlot,
                basePowerBonus = bonus
            };
        }
    }
}
