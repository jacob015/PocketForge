using System;
using System.Linq;
using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Economy;
using PocketForge.Mining;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEditor;

namespace PocketForge.Tests.Editor
{
    /// <summary>
    /// Task 14-2 meta economy regression: free/ad/gem/starter rewards are pinned
    /// against the deterministic chapter-1 credit production, and the repeatable
    /// weekly blueprint-core supply (boss rewards excluded) is capped so the
    /// research tree keeps a multi-week lifetime.
    /// </summary>
    public sealed class MetaEconomyBalanceTests
    {
        private const string CatalogPath = "Assets/PocketForge/Content/MiningContentCatalog.asset";
        private const long MaxWeeklyRepeatableCores = 13L;

        [Test]
        public void Simulator_ReportsChapterOneCreditProductionAndSpending()
        {
            var catalog = LoadCatalog();
            var active = new ProgressionBalanceSimulator(catalog)
                .SimulateFirstChapter(BalanceSimulationMode.Active);
            var idle = new ProgressionBalanceSimulator(catalog)
                .SimulateFirstChapter(BalanceSimulationMode.Idle);

            Assert.That(active.Completed, Is.True);
            Assert.That(idle.Completed, Is.True);
            Assert.That(active.TotalCreditsEarned, Is.InRange(450L, 700L));
            Assert.That(idle.TotalCreditsEarned, Is.InRange(700L, 1000L));
            Assert.That(active.TotalCreditsSpent, Is.GreaterThan(0L));
            Assert.That(idle.TotalCreditsSpent, Is.GreaterThan(0L));
            Assert.That(
                active.FinalPlayer.credits,
                Is.EqualTo(active.TotalCreditsEarned - active.TotalCreditsSpent));
            Assert.That(
                idle.FinalPlayer.credits,
                Is.EqualTo(idle.TotalCreditsEarned - idle.TotalCreditsSpent));
        }

        [Test]
        public void FreeAndAdDailyCredits_StayAroundOneActiveChapterProduction()
        {
            var catalog = LoadCatalog();
            var production = ActiveChapterProduction(catalog);
            var products = catalog.GetShopProducts();
            var dailyFree = products.First(product => product.Kind == ShopProductKind.DailyFree);
            var rewarded = products.First(product => product.Kind == ShopProductKind.RewardedAd);
            var freeAndAdCredits =
                dailyFree.RewardCredits * Math.Max(1, dailyFree.DailyLimit) +
                rewarded.RewardCredits * Math.Max(1, rewarded.DailyLimit);

            Assert.That(dailyFree.RewardBlueprintCores, Is.Zero);
            Assert.That(rewarded.RewardBlueprintCores, Is.Zero);
            Assert.That(
                freeAndAdCredits / (double)production,
                Is.InRange(0.5, 1.5));
        }

        [Test]
        public void GemAndStarterCredits_AreCappedRelativeToChapterProduction()
        {
            var catalog = LoadCatalog();
            var production = ActiveChapterProduction(catalog);
            var products = catalog.GetShopProducts();
            var gemCredits = products.First(product =>
                product.Kind == ShopProductKind.GemExchange && product.RewardCredits > 0L);
            var starter = products.First(product => product.ProductId == "starter_pack");

            Assert.That(gemCredits.RewardCredits, Is.LessThanOrEqualTo((long)(production * 1.25)));
            Assert.That(starter.RewardCredits, Is.LessThanOrEqualTo(production * 4L));
        }

        [Test]
        public void WeeklyRepeatableCoreSupply_ExcludingBoss_IsCapped()
        {
            var weeklyCores = CalculateWeeklyRepeatableCores(LoadCatalog());

            Assert.That(weeklyCores, Is.GreaterThan(0L));
            Assert.That(weeklyCores, Is.LessThanOrEqualTo(MaxWeeklyRepeatableCores));
        }

        [Test]
        public void ResearchLifetime_SpansMultipleWeeksOfRepeatableCores()
        {
            var catalog = LoadCatalog();
            var totalResearchCost = catalog.GetResearchNodes().Sum(TotalNodeCost);
            var weeklyCores = CalculateWeeklyRepeatableCores(catalog);

            Assert.That(
                totalResearchCost / (double)weeklyCores,
                Is.InRange(4.0, 5.5));
        }

        private static long ActiveChapterProduction(MiningContentCatalog catalog)
        {
            var result = new ProgressionBalanceSimulator(catalog)
                .SimulateFirstChapter(BalanceSimulationMode.Active);
            Assert.That(result.Completed, Is.True);
            return result.TotalCreditsEarned;
        }

        private static long CalculateWeeklyRepeatableCores(MiningContentCatalog catalog)
        {
            var weeklyCores = 0L;
            var missions = catalog.GetMissions();
            weeklyCores += missions
                .Where(mission => mission.Period == MissionPeriod.Daily &&
                                  mission.RewardType == MissionRewardType.BlueprintCores)
                .Sum(mission => mission.RewardAmount) * 7L;
            weeklyCores += missions
                .Where(mission => mission.Period == MissionPeriod.Weekly &&
                                  mission.RewardType == MissionRewardType.BlueprintCores)
                .Sum(mission => mission.RewardAmount);

            var missionService = new MissionService(catalog, new EquipmentService(catalog));
            var player = new GameSaveData();
            var now = new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            var dailyBoard = missionService.GetBoard(player, MissionPeriod.Daily, now, true);
            if (dailyBoard.CompletionRewardType == MissionRewardType.BlueprintCores)
            {
                weeklyCores += dailyBoard.CompletionRewardAmount * 7L;
            }

            var weeklyBoard = missionService.GetBoard(player, MissionPeriod.Weekly, now, true);
            if (weeklyBoard.CompletionRewardType == MissionRewardType.BlueprintCores)
            {
                weeklyCores += weeklyBoard.CompletionRewardAmount;
            }

            weeklyCores += catalog.GetShopProducts()
                .Where(product => product.Kind == ShopProductKind.DailyFree ||
                                  product.Kind == ShopProductKind.RewardedAd)
                .Sum(product => product.RewardBlueprintCores * Math.Max(1, product.DailyLimit)) * 7L;

            foreach (var miningEvent in catalog.GetMiningEvents())
            {
                weeklyCores += miningEvent.RewardTiers
                    .Where(tier => tier.RewardType == EventRewardType.BlueprintCores)
                    .Sum(tier => tier.RewardAmount);
                if (miningEvent.ExchangeRewardType == EventRewardType.BlueprintCores)
                {
                    weeklyCores += miningEvent.ExchangeRewardAmount * miningEvent.ExchangeLimit;
                }
            }

            return weeklyCores;
        }

        private static long TotalNodeCost(ResearchNodeDefinition node)
        {
            var total = 0L;
            for (var level = 0; level < node.MaxLevel; level++)
            {
                total += node.GetCost(level);
            }

            return total;
        }

        private static MiningContentCatalog LoadCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(CatalogPath);
            Assert.That(catalog, Is.Not.Null);
            return catalog;
        }
    }
}
