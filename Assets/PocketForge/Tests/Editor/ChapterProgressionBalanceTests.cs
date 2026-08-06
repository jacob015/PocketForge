using System.Linq;
using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Economy;
using UnityEditor;

namespace PocketForge.Tests.Editor
{
    /// <summary>
    /// Task 14-3 mid-game regression: chapters 2 and 3 must keep growing in length
    /// without the idle path stalling, and the boss gate must step up smoothly rather
    /// than spiking. Chapter 1's own targets stay pinned by
    /// <see cref="ProgressionBalanceSimulatorTests"/>.
    /// </summary>
    public sealed class ChapterProgressionBalanceTests
    {
        private const string CatalogPath = "Assets/PocketForge/Content/MiningContentCatalog.asset";
        private const int LastTunedChapter = 3;

        [Test]
        public void ActiveRun_ClearsThreeChaptersWithChaptersGettingLonger()
        {
            var result = Simulate(BalanceSimulationMode.Active);

            Assert.That(result.Completed, Is.True);
            Assert.That(result.Chapters, Has.Count.EqualTo(LastTunedChapter));
            AssertDurationsIncrease(result);
            Assert.That(Minutes(result, 2), Is.InRange(9f, 16f));
            Assert.That(Minutes(result, 3), Is.InRange(11f, 22f));
        }

        [Test]
        public void IdleRun_ClearsThreeChaptersWithoutStalling()
        {
            var result = Simulate(BalanceSimulationMode.Idle);

            Assert.That(result.Completed, Is.True, "The no-tap path must still finish chapter 3.");
            Assert.That(result.Chapters, Has.Count.EqualTo(LastTunedChapter));
            AssertDurationsIncrease(result);
            Assert.That(Minutes(result, 2), Is.InRange(20f, 34f));
            // Kept inside the 4 hour offline cap so one away session can cover a chapter.
            Assert.That(Minutes(result, 3), Is.InRange(40f, 70f));
        }

        [Test]
        public void EveryChapterKeepsItsBossGate()
        {
            foreach (var mode in new[] { BalanceSimulationMode.Active, BalanceSimulationMode.Idle })
            {
                var result = Simulate(mode);
                foreach (var chapter in result.Chapters)
                {
                    Assert.That(
                        chapter.BossFailures,
                        Is.GreaterThanOrEqualTo(1),
                        $"{mode} chapter {chapter.ChapterNumber} cleared its boss without ever being gated.");
                    Assert.That(
                        chapter.PowerAtClear,
                        Is.GreaterThanOrEqualTo(chapter.BossRecommendedPower),
                        $"{mode} chapter {chapter.ChapterNumber} cleared below the recommended power.");
                }
            }
        }

        [Test]
        public void BossRecommendedPower_StepsUpSmoothlyBetweenChapters()
        {
            var chapters = Simulate(BalanceSimulationMode.Active).Chapters;

            for (var index = 1; index < chapters.Count; index++)
            {
                var previous = chapters[index - 1].BossRecommendedPower;
                var current = chapters[index].BossRecommendedPower;
                Assert.That(previous, Is.GreaterThan(0f));
                Assert.That(
                    current / previous,
                    Is.InRange(1.5f, 3.0f),
                    $"Chapter {chapters[index].ChapterNumber} boss requirement jumps too far from the previous one.");
            }
        }

        [Test]
        public void StageDurability_NeverCollapsesInsideAChapter()
        {
            var catalog = LoadCatalog();

            for (var stage = 2; stage <= 30; stage++)
            {
                var chapter = catalog.GetChapterForStage(stage);
                if (chapter.IsBossStage(stage) || chapter.IsBossStage(stage - 1))
                {
                    continue;
                }

                var previous = catalog.GetOreForStage(stage - 1).GetDurability(stage - 1);
                var current = catalog.GetOreForStage(stage).GetDurability(stage);
                Assert.That(
                    current,
                    Is.GreaterThanOrEqualTo(previous),
                    $"Stage {stage} is weaker than stage {stage - 1}.");
            }
        }

        [Test]
        public void ChapterRewardMultiplier_GrowsWithTheCostCurve()
        {
            var chapters = LoadCatalog().GetChapters().OrderBy(c => c.ChapterNumber).ToArray();

            Assert.That(chapters[0].RewardMultiplier, Is.EqualTo(1f), "Chapter 1 stays the balance baseline.");
            for (var index = 1; index < chapters.Length; index++)
            {
                Assert.That(
                    chapters[index].RewardMultiplier,
                    Is.GreaterThan(chapters[index - 1].RewardMultiplier),
                    $"Chapter {chapters[index].ChapterNumber} must out-pay the previous chapter.");
            }
        }

        [Test]
        public void InterstitialSettings_ProtectTheFirstSessionAndSpaceOutAds()
        {
            var catalog = LoadCatalog();
            var activeFirstChapterSeconds =
                Simulate(BalanceSimulationMode.Active).Chapters[0].DurationSeconds;

            Assert.That(
                catalog.InterstitialGraceSeconds,
                Is.GreaterThanOrEqualTo(activeFirstChapterSeconds),
                "A new player should reach the first boss before any interstitial.");
            Assert.That(catalog.InterstitialCooldownSeconds, Is.GreaterThanOrEqualTo(240f));
            Assert.That(catalog.InterstitialOreBreakInterval, Is.GreaterThanOrEqualTo(5));
        }

        private static void AssertDurationsIncrease(ProgressionBalanceResult result)
        {
            for (var index = 1; index < result.Chapters.Count; index++)
            {
                Assert.That(
                    result.Chapters[index].DurationSeconds,
                    Is.GreaterThan(result.Chapters[index - 1].DurationSeconds),
                    $"Chapter {result.Chapters[index].ChapterNumber} is shorter than the one before it.");
            }
        }

        private static float Minutes(ProgressionBalanceResult result, int chapterNumber) =>
            result.Chapters.First(c => c.ChapterNumber == chapterNumber).DurationSeconds / 60f;

        private static ProgressionBalanceResult Simulate(BalanceSimulationMode mode) =>
            new ProgressionBalanceSimulator(LoadCatalog())
                .SimulateChapters(mode, LastTunedChapter, 12000f);

        private static MiningContentCatalog LoadCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(CatalogPath);
            Assert.That(catalog, Is.Not.Null);
            return catalog;
        }
    }
}
