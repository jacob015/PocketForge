using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Economy;
using PocketForge.Mining;
using PocketForge.Save;
using UnityEditor;

namespace PocketForge.Tests.Editor
{
    public sealed class ProgressionBalanceSimulatorTests
    {
        private const string CatalogPath = "Assets/PocketForge/Content/MiningContentCatalog.asset";

        [Test]
        public void ActiveFirstChapter_MatchesShortSessionTarget()
        {
            var result = CreateSimulator().SimulateFirstChapter(BalanceSimulationMode.Active);

            Assert.That(result.Completed, Is.True);
            Assert.That(result.FirstUpgradeSeconds, Is.InRange(20f, 40f));
            Assert.That(result.ElapsedSeconds, Is.InRange(8f * 60f, 12f * 60f));
            Assert.That(result.BossFailures, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.UpgradePurchases, Is.GreaterThanOrEqualTo(5));
            Assert.That(
                result.FinalPower.ActivePowerPerSecond,
                Is.GreaterThanOrEqualTo(result.BossRecommendedPower));
        }

        [Test]
        public void IdleFirstChapter_MatchesReturnSessionTarget()
        {
            var result = CreateSimulator().SimulateFirstChapter(BalanceSimulationMode.Idle);

            Assert.That(result.Completed, Is.True);
            Assert.That(result.ElapsedSeconds, Is.InRange(15f * 60f, 25f * 60f));
            Assert.That(result.BossFailures, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.FinalPlayer.drillLevel, Is.GreaterThan(0));
            Assert.That(
                result.FinalPower.AutoPowerPerSecond,
                Is.GreaterThanOrEqualTo(result.BossRecommendedPower));
        }

        [Test]
        public void FreshPlayer_TappingAcceleratesButCannotClearBossWithoutGrowth()
        {
            var catalog = LoadCatalog();
            var powerService = new MiningPowerService(catalog);
            var gameService = new MiningGameService(catalog);
            var player = new GameSaveData();
            var basePower = powerService.Calculate(player);
            var boss = gameService.CreateInitialState(new GameSaveData { stage = 10 }, 1f);

            Assert.That(basePower.ActivePowerPerSecond, Is.GreaterThan(basePower.AutoPowerPerSecond));
            Assert.That(basePower.ActivePowerPerSecond, Is.LessThanOrEqualTo(basePower.AutoPowerPerSecond * 1.4f));
            Assert.That(basePower.ActivePowerPerSecond, Is.LessThan(gameService.GetBossRecommendedPower(boss)));
        }

        private static ProgressionBalanceSimulator CreateSimulator() =>
            new(LoadCatalog());

        private static MiningContentCatalog LoadCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(CatalogPath);
            Assert.That(catalog, Is.Not.Null);
            return catalog;
        }
    }
}
