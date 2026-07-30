using PocketForge.Content;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Mining
{
    public readonly struct MiningPowerModifiers
    {
        public MiningPowerModifiers(
            float minerRankMultiplier,
            float equipmentMultiplier,
            float researchMultiplier,
            float collectionMultiplier,
            float temporaryMultiplier)
        {
            MinerRankMultiplier = Mathf.Max(0f, minerRankMultiplier);
            EquipmentMultiplier = Mathf.Max(0f, equipmentMultiplier);
            ResearchMultiplier = Mathf.Max(0f, researchMultiplier);
            CollectionMultiplier = Mathf.Max(0f, collectionMultiplier);
            TemporaryMultiplier = Mathf.Max(0f, temporaryMultiplier);
        }

        public float MinerRankMultiplier { get; }
        public float EquipmentMultiplier { get; }
        public float ResearchMultiplier { get; }
        public float CollectionMultiplier { get; }
        public float TemporaryMultiplier { get; }

        public float PermanentMultiplier =>
            MinerRankMultiplier *
            EquipmentMultiplier *
            ResearchMultiplier *
            CollectionMultiplier;

        public static MiningPowerModifiers Identity => new(1f, 1f, 1f, 1f, 1f);
    }

    public readonly struct MiningPowerSnapshot
    {
        public MiningPowerSnapshot(
            float drillPower,
            float robotMultiplier,
            float permanentMultiplier,
            float temporaryMultiplier,
            float autoPowerPerSecond,
            float tapDamage,
            float activePowerPerSecond,
            float tapCooldownSeconds)
        {
            DrillPower = drillPower;
            RobotMultiplier = robotMultiplier;
            PermanentMultiplier = permanentMultiplier;
            TemporaryMultiplier = temporaryMultiplier;
            AutoPowerPerSecond = autoPowerPerSecond;
            TapDamage = tapDamage;
            ActivePowerPerSecond = activePowerPerSecond;
            TapCooldownSeconds = tapCooldownSeconds;
        }

        public float DrillPower { get; }
        public float RobotMultiplier { get; }
        public float PermanentMultiplier { get; }
        public float TemporaryMultiplier { get; }
        public float AutoPowerPerSecond { get; }
        public float TapDamage { get; }
        public float ActivePowerPerSecond { get; }
        public float TapCooldownSeconds { get; }
    }

    public sealed class MiningPowerService
    {
        private readonly MiningContentCatalog catalog;

        public MiningPowerService(MiningContentCatalog catalog)
        {
            this.catalog = catalog;
        }

        public MiningPowerSnapshot Calculate(GameSaveData player) =>
            Calculate(player, MiningPowerModifiers.Identity);

        public MiningPowerSnapshot Calculate(
            GameSaveData player,
            MiningPowerModifiers modifiers)
        {
            var drillDefinition = catalog.GetUpgrade(Economy.UpgradeType.Drill);
            var pickaxeDefinition = catalog.GetUpgrade(Economy.UpgradeType.Pickaxe);
            var robotDefinition = catalog.GetUpgrade(Economy.UpgradeType.Robot);
            var drillPower = catalog.BaseAutoPowerPerSecond +
                             Mathf.Max(0, player.drillLevel) * drillDefinition.EffectPerLevel;
            var robotMultiplier = 1f +
                                  Mathf.Max(0, player.robotLevel) * robotDefinition.EffectPerLevel;
            var autoPower = drillPower *
                            robotMultiplier *
                            modifiers.PermanentMultiplier *
                            modifiers.TemporaryMultiplier;
            var baseTapDamage = (catalog.BaseTapDamage +
                                 Mathf.Sqrt(Mathf.Max(0, player.pickaxeLevel) * pickaxeDefinition.EffectPerLevel)) *
                                modifiers.PermanentMultiplier *
                                modifiers.TemporaryMultiplier;
            var tapAssistSeconds = Mathf.Min(
                catalog.MaxTapAssistSeconds,
                catalog.BaseTapAssistSeconds +
                Mathf.Max(0, player.pickaxeLevel) * catalog.TapAssistSecondsPerPickaxeLevel);
            var tapDamage = Mathf.Max(baseTapDamage, autoPower * tapAssistSeconds);
            var activePower = autoPower + tapDamage * catalog.ReferenceTapsPerSecond;

            return new MiningPowerSnapshot(
                drillPower,
                robotMultiplier,
                modifiers.PermanentMultiplier,
                modifiers.TemporaryMultiplier,
                autoPower,
                tapDamage,
                activePower,
                catalog.TapCooldownSeconds);
        }

        public static float GetBossRecommendedPower(OreState ore)
        {
            if (ore == null || !ore.IsBoss || ore.Chapter.BossTimeLimitSeconds <= 0f)
            {
                return 0f;
            }

            return ore.Durability / ore.Chapter.BossTimeLimitSeconds;
        }
    }
}
