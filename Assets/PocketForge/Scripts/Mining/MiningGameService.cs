using PocketForge.Content;
using PocketForge.Economy;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Mining
{
    public readonly struct MiningGameResult
    {
        public MiningGameResult(bool stateChanged, bool oreBroken, bool purchaseSucceeded)
        {
            StateChanged = stateChanged;
            OreBroken = oreBroken;
            PurchaseSucceeded = purchaseSucceeded;
        }

        public bool StateChanged { get; }
        public bool OreBroken { get; }
        public bool PurchaseSucceeded { get; }
    }

    public sealed class MiningGameService
    {
        private readonly MiningContentCatalog catalog;

        public MiningGameService(MiningContentCatalog catalog)
        {
            this.catalog = catalog;
        }

        public MiningGameState CreateInitialState(GameSaveData saveData, float rareRoll)
        {
            return new MiningGameState(saveData, CreateOre(saveData.stage, rareRoll));
        }

        public MiningGameResult Mine(MiningGameState state, float rareRoll)
        {
            return DamageOre(state, GetTapPower(state.Player.pickaxeLevel), rareRoll);
        }

        public MiningGameResult Tick(MiningGameState state, float deltaTime, float rareRoll)
        {
            var autoPower = GetAutoPowerPerSecond(state.Player.drillLevel);
            return autoPower <= 0f
                ? new MiningGameResult(false, false, false)
                : DamageOre(state, autoPower * deltaTime, rareRoll);
        }

        public MiningGameResult TryUpgrade(MiningGameState state, UpgradeType type)
        {
            var level = GetLevel(state.Player, type);
            var cost = GetUpgradeCost(type, level);
            if (state.Player.credits < cost)
            {
                return new MiningGameResult(false, false, false);
            }

            state.Player.credits -= cost;
            SetLevel(state.Player, type, level + 1);
            return new MiningGameResult(true, false, true);
        }

        public int GetUpgradeCost(UpgradeType type, int currentLevel)
        {
            return catalog.GetUpgrade(type).GetCost(currentLevel);
        }

        public float GetTapPower(int pickaxeLevel) =>
            1f + pickaxeLevel * catalog.GetUpgrade(UpgradeType.Pickaxe).EffectPerLevel;

        public float GetAutoPowerPerSecond(int drillLevel) => drillLevel * catalog.GetUpgrade(UpgradeType.Drill).EffectPerLevel;

        public float GetRewardMultiplier(int robotLevel) => 1f + robotLevel * catalog.GetUpgrade(UpgradeType.Robot).EffectPerLevel;

        public int GetOreReward(int stage, bool isRare, int robotLevel)
        {
            var definition = catalog.GetOreForStage(stage);
            var multiplier = isRare ? definition.RareRewardMultiplier : definition.NormalRewardMultiplier;
            return Mathf.CeilToInt(stage * multiplier * GetRewardMultiplier(robotLevel));
        }

        public int ApplyOfflineReward(MiningGameState state, long elapsedSeconds)
        {
            if (elapsedSeconds <= 0 || state.Player.drillLevel <= 0)
            {
                return 0;
            }

            var cappedSeconds = Mathf.Min(elapsedSeconds, catalog.MaxOfflineRewardSeconds);
            var damage = GetAutoPowerPerSecond(state.Player.drillLevel) * cappedSeconds;
            var ore = catalog.GetOreForStage(state.Player.stage);
            var creditsPerDamage = GetOreReward(state.Player.stage, false, state.Player.robotLevel) / ore.GetDurability(state.Player.stage);
            var reward = Mathf.FloorToInt(damage * creditsPerDamage);
            state.Player.credits += reward;
            return reward;
        }

        private MiningGameResult DamageOre(MiningGameState state, float amount, float rareRoll)
        {
            if (amount <= 0f)
            {
                return new MiningGameResult(false, false, false);
            }

            state.Ore.Health -= amount;
            if (state.Ore.Health > 0f)
            {
                return new MiningGameResult(true, false, false);
            }

            state.Player.credits += GetOreReward(state.Player.stage, state.Ore.IsRare, state.Player.robotLevel);
            state.Player.stage++;
            state.ReplaceOre(CreateOre(state.Player.stage, rareRoll));
            return new MiningGameResult(true, true, false);
        }

        private OreState CreateOre(int stage, float rareRoll)
        {
            var definition = catalog.GetOreForStage(stage);
            var durability = definition.GetDurability(stage);
            return new OreState(definition, durability, rareRoll < definition.RareChance);
        }

        private static int GetLevel(GameSaveData data, UpgradeType type)
        {
            return type switch
            {
                UpgradeType.Pickaxe => data.pickaxeLevel,
                UpgradeType.Drill => data.drillLevel,
                UpgradeType.Robot => data.robotLevel,
                _ => 0
            };
        }

        private static void SetLevel(GameSaveData data, UpgradeType type, int level)
        {
            switch (type)
            {
                case UpgradeType.Pickaxe: data.pickaxeLevel = level; break;
                case UpgradeType.Drill: data.drillLevel = level; break;
                case UpgradeType.Robot: data.robotLevel = level; break;
            }
        }
    }
}
