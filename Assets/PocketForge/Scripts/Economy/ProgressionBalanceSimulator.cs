using System;
using System.Collections.Generic;
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

    /// <summary>
    /// One chapter's slice of a run, so the growth curve can be compared chapter to
    /// chapter instead of only as a single total.
    /// </summary>
    public readonly struct ChapterProgressResult
    {
        public ChapterProgressResult(
            int chapterNumber,
            float durationSeconds,
            float completedAtSeconds,
            int bossFailures,
            int oresMined,
            long creditsEarned,
            float bossRecommendedPower,
            float powerAtClear)
        {
            ChapterNumber = chapterNumber;
            DurationSeconds = durationSeconds;
            CompletedAtSeconds = completedAtSeconds;
            BossFailures = bossFailures;
            OresMined = oresMined;
            CreditsEarned = creditsEarned;
            BossRecommendedPower = bossRecommendedPower;
            PowerAtClear = powerAtClear;
        }

        public int ChapterNumber { get; }
        public float DurationSeconds { get; }
        public float CompletedAtSeconds { get; }
        public int BossFailures { get; }
        public int OresMined { get; }
        public long CreditsEarned { get; }
        public float BossRecommendedPower { get; }
        public float PowerAtClear { get; }
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
            long totalCreditsEarned,
            long totalCreditsSpent,
            GameSaveData finalPlayer,
            MiningPowerSnapshot finalPower,
            float bossRecommendedPower,
            IReadOnlyList<ChapterProgressResult> chapters)
        {
            Completed = completed;
            ElapsedSeconds = elapsedSeconds;
            FirstUpgradeSeconds = firstUpgradeSeconds;
            FirstBossSeconds = firstBossSeconds;
            OresMined = oresMined;
            BossFailures = bossFailures;
            UpgradePurchases = upgradePurchases;
            TotalCreditsEarned = totalCreditsEarned;
            TotalCreditsSpent = totalCreditsSpent;
            FinalPlayer = finalPlayer;
            FinalPower = finalPower;
            BossRecommendedPower = bossRecommendedPower;
            Chapters = chapters ?? Array.Empty<ChapterProgressResult>();
        }

        public bool Completed { get; }
        public float ElapsedSeconds { get; }
        public float FirstUpgradeSeconds { get; }
        public float FirstBossSeconds { get; }
        public int OresMined { get; }
        public int BossFailures { get; }
        public int UpgradePurchases { get; }
        public long TotalCreditsEarned { get; }
        public long TotalCreditsSpent { get; }
        public GameSaveData FinalPlayer { get; }
        public MiningPowerSnapshot FinalPower { get; }
        public float BossRecommendedPower { get; }
        public IReadOnlyList<ChapterProgressResult> Chapters { get; }
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
            float maximumSeconds = 1800f) =>
            SimulateChapters(mode, 1, maximumSeconds);

        /// <summary>
        /// Runs a fresh save from stage 1 until <paramref name="throughChapter"/> is
        /// cleared, recording each chapter separately.
        /// </summary>
        public ProgressionBalanceResult SimulateChapters(
            BalanceSimulationMode mode,
            int throughChapter,
            float maximumSeconds = 1800f)
        {
            throughChapter = Math.Max(1, throughChapter);
            var state = gameService.CreateInitialState(new GameSaveData(), 1f);
            var elapsed = 0f;
            var firstUpgradeSeconds = -1f;
            var firstBossSeconds = -1f;
            var oresMined = 0;
            var bossFailures = 0;
            var upgradePurchases = 0;
            var totalCreditsEarned = 0L;
            var totalCreditsSpent = 0L;
            var bossRecommendedPower = 0f;

            var chapters = new List<ChapterProgressResult>();
            var chapterStartSeconds = 0f;
            var chapterStartCredits = 0L;
            var chapterStartOres = 0;
            var chapterBossFailures = 0;
            var chapterRecommendedPower = 0f;

            while (elapsed < maximumSeconds)
            {
                upgradePurchases += PurchaseEfficientUpgrades(
                    state, mode, elapsed, ref firstUpgradeSeconds, ref totalCreditsSpent);
                RetryBossWhenReady(state, mode);

                if (state.Ore.IsBoss)
                {
                    if (firstBossSeconds < 0f)
                    {
                        firstBossSeconds = elapsed;
                    }

                    bossRecommendedPower = gameService.GetBossRecommendedPower(state);
                    chapterRecommendedPower = bossRecommendedPower;
                }

                var clearedChapter = state.Ore.Chapter.ChapterNumber;
                var chapterCompleted = false;

                if (mode == BalanceSimulationMode.Active && state.Ore.CanTap)
                {
                    var creditsBeforeMine = state.Player.credits;
                    var mineResult = gameService.Mine(state, 1f);
                    totalCreditsEarned += Math.Max(0L, state.Player.credits - creditsBeforeMine);
                    RecordResult(mineResult, ref oresMined, ref bossFailures, ref chapterBossFailures);
                    chapterCompleted = mineResult.ChapterCompleted;
                }

                if (!chapterCompleted)
                {
                    var creditsBeforeTick = state.Player.credits;
                    var tickResult = gameService.Tick(state, SimulationStepSeconds, 1f);
                    elapsed += SimulationStepSeconds;
                    totalCreditsEarned += Math.Max(0L, state.Player.credits - creditsBeforeTick);
                    RecordResult(tickResult, ref oresMined, ref bossFailures, ref chapterBossFailures);
                    chapterCompleted = tickResult.ChapterCompleted;
                }

                if (!chapterCompleted)
                {
                    continue;
                }

                chapters.Add(new ChapterProgressResult(
                    clearedChapter,
                    elapsed - chapterStartSeconds,
                    elapsed,
                    chapterBossFailures,
                    oresMined - chapterStartOres,
                    totalCreditsEarned - chapterStartCredits,
                    chapterRecommendedPower,
                    GetRelevantPower(gameService.GetMiningPower(state), mode)));

                if (clearedChapter >= throughChapter)
                {
                    return CreateResult(
                        true,
                        elapsed,
                        firstUpgradeSeconds,
                        firstBossSeconds,
                        oresMined,
                        bossFailures,
                        upgradePurchases,
                        totalCreditsEarned,
                        totalCreditsSpent,
                        state,
                        bossRecommendedPower,
                        chapters);
                }

                chapterStartSeconds = elapsed;
                chapterStartCredits = totalCreditsEarned;
                chapterStartOres = oresMined;
                chapterBossFailures = 0;
                chapterRecommendedPower = 0f;
            }

            return CreateResult(
                false,
                elapsed,
                firstUpgradeSeconds,
                firstBossSeconds,
                oresMined,
                bossFailures,
                upgradePurchases,
                totalCreditsEarned,
                totalCreditsSpent,
                state,
                bossRecommendedPower,
                chapters);
        }

        private int PurchaseEfficientUpgrades(
            MiningGameState state,
            BalanceSimulationMode mode,
            float elapsed,
            ref float firstUpgradeSeconds,
            ref long totalCreditsSpent)
        {
            var purchases = 0;
            while (TrySelectUpgrade(state, mode, out var type))
            {
                var creditsBeforePurchase = state.Player.credits;
                var result = gameService.TryUpgrade(state, type);
                if (!result.PurchaseSucceeded)
                {
                    break;
                }

                totalCreditsSpent += Math.Max(0L, creditsBeforePurchase - state.Player.credits);
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

            gameService.SelectChapter(state, state.Ore.Chapter.ChapterNumber, 1f);
        }

        private ProgressionBalanceResult CreateResult(
            bool completed,
            float elapsed,
            float firstUpgradeSeconds,
            float firstBossSeconds,
            int oresMined,
            int bossFailures,
            int upgradePurchases,
            long totalCreditsEarned,
            long totalCreditsSpent,
            MiningGameState state,
            float bossRecommendedPower,
            IReadOnlyList<ChapterProgressResult> chapters)
        {
            return new ProgressionBalanceResult(
                completed,
                elapsed,
                firstUpgradeSeconds,
                firstBossSeconds,
                oresMined,
                bossFailures,
                upgradePurchases,
                totalCreditsEarned,
                totalCreditsSpent,
                state.Player,
                gameService.GetMiningPower(state),
                bossRecommendedPower,
                chapters);
        }

        private static void RecordResult(
            MiningGameResult result,
            ref int oresMined,
            ref int bossFailures,
            ref int chapterBossFailures)
        {
            if (result.OreBroken)
            {
                oresMined++;
            }

            if (result.BossFailed)
            {
                bossFailures++;
                chapterBossFailures++;
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
