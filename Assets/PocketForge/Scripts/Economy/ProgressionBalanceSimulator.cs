using PocketForge.Content;
using PocketForge.Mining;
using PocketForge.Save;

namespace PocketForge.Economy
{
    public enum BalanceSimulationMode
    {
        Idle,
        Active
    }

    public readonly struct ProgressionBalanceResult
    {
        public ProgressionBalanceResult(
            bool completed,
            float elapsedSeconds,
            float firstUpgradeSeconds,
            float firstBossSeconds,
            int oresMined,
            int bossFailures,
            int upgradePurchases,
            GameSaveData finalPlayer,
            MiningPowerSnapshot finalPower,
            float bossRecommendedPower)
        {
            Completed = completed;
            ElapsedSeconds = elapsedSeconds;
            FirstUpgradeSeconds = firstUpgradeSeconds;
            FirstBossSeconds = firstBossSeconds;
            OresMined = oresMined;
            BossFailures = bossFailures;
            UpgradePurchases = upgradePurchases;
            FinalPlayer = finalPlayer;
            FinalPower = finalPower;
            BossRecommendedPower = bossRecommendedPower;
        }

        public bool Completed { get; }
        public float ElapsedSeconds { get; }
        public float FirstUpgradeSeconds { get; }
        public float FirstBossSeconds { get; }
        public int OresMined { get; }
        public int BossFailures { get; }
        public int UpgradePurchases { get; }
        public GameSaveData FinalPlayer { get; }
        public MiningPowerSnapshot FinalPower { get; }
        public float BossRecommendedPower { get; }
    }

    /// <summary>
    /// Replays the real mining and upgrade rules with a deterministic no-rare drop sequence.
    /// It is intentionally presentation-free so balance targets can be regression-tested.
    /// </summary>
    public sealed class ProgressionBalanceSimulator
    {
        private const float SimulationStepSeconds = 0.2f;
        private const float BossRetrySafetyMultiplier = 1.01f;

        private readonly MiningGameService gameService;

        public ProgressionBalanceSimulator(MiningContentCatalog catalog)
        {
            gameService = new MiningGameService(catalog);
        }

        public ProgressionBalanceResult SimulateFirstChapter(
            BalanceSimulationMode mode,
            float maximumSeconds = 1800f)
        {
            var state = gameService.CreateInitialState(new GameSaveData(), 1f);
            var elapsed = 0f;
            var firstUpgradeSeconds = -1f;
            var firstBossSeconds = -1f;
            var oresMined = 0;
            var bossFailures = 0;
            var upgradePurchases = 0;
            var bossRecommendedPower = 0f;

            while (elapsed < maximumSeconds)
            {
                upgradePurchases += PurchaseEfficientUpgrades(state, mode, elapsed, ref firstUpgradeSeconds);
                RetryBossWhenReady(state, mode);

                if (state.Ore.IsBoss)
                {
                    if (firstBossSeconds < 0f)
                    {
                        firstBossSeconds = elapsed;
                    }

                    bossRecommendedPower = gameService.GetBossRecommendedPower(state);
                }

                if (mode == BalanceSimulationMode.Active && state.Ore.CanTap)
                {
                    var mineResult = gameService.Mine(state, 1f);
                    RecordResult(mineResult, ref oresMined, ref bossFailures);
                    if (mineResult.ChapterCompleted)
                    {
                        return CreateResult(
                            true,
                            elapsed,
                            firstUpgradeSeconds,
                            firstBossSeconds,
                            oresMined,
                            bossFailures,
                            upgradePurchases,
                            state,
                            bossRecommendedPower);
                    }
                }

                var tickResult = gameService.Tick(state, SimulationStepSeconds, 1f);
                elapsed += SimulationStepSeconds;
                RecordResult(tickResult, ref oresMined, ref bossFailures);
                if (tickResult.ChapterCompleted)
                {
                    return CreateResult(
                        true,
                        elapsed,
                        firstUpgradeSeconds,
                        firstBossSeconds,
                        oresMined,
                        bossFailures,
                        upgradePurchases,
                        state,
                        bossRecommendedPower);
                }
            }

            return CreateResult(
                false,
                elapsed,
                firstUpgradeSeconds,
                firstBossSeconds,
                oresMined,
                bossFailures,
                upgradePurchases,
                state,
                bossRecommendedPower);
        }

        private int PurchaseEfficientUpgrades(
            MiningGameState state,
            BalanceSimulationMode mode,
            float elapsed,
            ref float firstUpgradeSeconds)
        {
            var purchases = 0;
            while (TrySelectUpgrade(state, mode, out var type))
            {
                var result = gameService.TryUpgrade(state, type);
                if (!result.PurchaseSucceeded)
                {
                    break;
                }

                purchases++;
                if (firstUpgradeSeconds < 0f)
                {
                    firstUpgradeSeconds = elapsed;
                }
            }

            return purchases;
        }

        private bool TrySelectUpgrade(
            MiningGameState state,
            BalanceSimulationMode mode,
            out UpgradeType selectedType)
        {
            selectedType = UpgradeType.Pickaxe;
            var bestEfficiency = 0f;
            var found = false;
            var baseline = GetRelevantPower(gameService.GetMiningPower(state), mode);

            foreach (var type in new[] { UpgradeType.Pickaxe, UpgradeType.Drill, UpgradeType.Robot })
            {
                var level = GetLevel(state.Player, type);
                var cost = gameService.GetUpgradeCost(type, level);
                if (cost <= 0L || cost > state.Player.credits)
                {
                    continue;
                }

                SetLevel(state.Player, type, level + 1);
                var upgraded = GetRelevantPower(gameService.GetMiningPower(state), mode);
                SetLevel(state.Player, type, level);
                var efficiency = (upgraded - baseline) / cost;
                if (efficiency <= bestEfficiency)
                {
                    continue;
                }

                bestEfficiency = efficiency;
                selectedType = type;
                found = true;
            }

            return found;
        }

        private void RetryBossWhenReady(MiningGameState state, BalanceSimulationMode mode)
        {
            if (state.Ore.IsBoss || !gameService.IsBossChallengeReady(state))
            {
                return;
            }

            var recommendedPower = gameService.GetBossRecommendedPower(state);
            var currentPower = GetRelevantPower(gameService.GetMiningPower(state), mode);
            if (recommendedPower <= 0f || currentPower < recommendedPower * BossRetrySafetyMultiplier)
            {
                return;
            }

            gameService.SelectChapter(state, 1, 1f);
        }

        private ProgressionBalanceResult CreateResult(
            bool completed,
            float elapsed,
            float firstUpgradeSeconds,
            float firstBossSeconds,
            int oresMined,
            int bossFailures,
            int upgradePurchases,
            MiningGameState state,
            float bossRecommendedPower)
        {
            return new ProgressionBalanceResult(
                completed,
                elapsed,
                firstUpgradeSeconds,
                firstBossSeconds,
                oresMined,
                bossFailures,
                upgradePurchases,
                state.Player,
                gameService.GetMiningPower(state),
                bossRecommendedPower);
        }

        private static void RecordResult(
            MiningGameResult result,
            ref int oresMined,
            ref int bossFailures)
        {
            if (result.OreBroken)
            {
                oresMined++;
            }

            if (result.BossFailed)
            {
                bossFailures++;
            }
        }

        private static float GetRelevantPower(
            MiningPowerSnapshot power,
            BalanceSimulationMode mode) =>
            mode == BalanceSimulationMode.Active
                ? power.ActivePowerPerSecond
                : power.AutoPowerPerSecond;

        private static int GetLevel(GameSaveData player, UpgradeType type)
        {
            return type switch
            {
                UpgradeType.Pickaxe => player.pickaxeLevel,
                UpgradeType.Drill => player.drillLevel,
                UpgradeType.Robot => player.robotLevel,
                _ => 0
            };
        }

        private static void SetLevel(GameSaveData player, UpgradeType type, int value)
        {
            switch (type)
            {
                case UpgradeType.Pickaxe:
                    player.pickaxeLevel = value;
                    break;
                case UpgradeType.Drill:
                    player.drillLevel = value;
                    break;
                case UpgradeType.Robot:
                    player.robotLevel = value;
                    break;
            }
        }
    }
}
