using System;
using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class MiningEventServiceTests
    {
        private MiningContentCatalog catalog;
        private MiningEventService service;

        [SetUp]
        public void SetUp()
        {
            catalog = MiningContentCatalog.CreateRuntimeDefault();
            service = new MiningEventService(catalog);
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(catalog);

        [Test]
        public void Mining_EarnsTokensClaimsTierAndSpendsOnlyBalance()
        {
            var data = CreateData(100);
            const long now = 1785680000;
            service.Refresh(data, now, true);
            data.oreCollection[0].minedCount += 150;

            var board = service.GetBoard(data, now, true);
            Assert.That(board.EarnedTokens, Is.EqualTo(30));
            Assert.That(board.TokenBalance, Is.EqualTo(30));
            Assert.That(service.ClaimReward(data, "crystal_rush_10", now, true).Status,
                Is.EqualTo(EventActionStatus.Success));
            Assert.That(service.PurchaseExchange(data, now, true).Status,
                Is.EqualTo(EventActionStatus.Success));

            board = service.GetBoard(data, now, true);
            Assert.That(board.EarnedTokens, Is.EqualTo(30));
            Assert.That(board.TokenBalance, Is.EqualTo(10));
            Assert.That(data.credits, Is.EqualTo(250));
            Assert.That(data.blueprintCores, Is.EqualTo(1));
        }

        [Test]
        public void RewardsAndExchange_CannotBeDuplicatedBeyondLimits()
        {
            var data = CreateData(0);
            const long now = 1785680000;
            service.Refresh(data, now, true);
            data.oreCollection[0].minedCount = 500;

            Assert.That(service.ClaimReward(data, "crystal_rush_10", now, true).Status,
                Is.EqualTo(EventActionStatus.Success));
            Assert.That(service.ClaimReward(data, "crystal_rush_10", now, true).Status,
                Is.EqualTo(EventActionStatus.AlreadyClaimed));
            Assert.That(service.PurchaseExchange(data, now, true).Status,
                Is.EqualTo(EventActionStatus.Success));
            Assert.That(service.PurchaseExchange(data, now, true).Status,
                Is.EqualTo(EventActionStatus.Success));
            Assert.That(service.PurchaseExchange(data, now, true).Status,
                Is.EqualTo(EventActionStatus.ExchangeLimitReached));
        }

        [Test]
        public void WeeklyResetAndClockRollback_DoNotReopenOldPeriod()
        {
            var monday = new DateTimeOffset(2026, 8, 3, 1, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            var data = CreateData(0);
            service.Refresh(data, monday, true);
            data.oreCollection[0].minedCount = 50;
            service.GetBoard(data, monday, true);
            var key = data.miningEvent.periodKey;

            service.Refresh(data, monday - 86400, true);
            Assert.That(data.miningEvent.periodKey, Is.EqualTo(key));
            Assert.That(data.miningEvent.earnedTokens, Is.EqualTo(10));

            service.Refresh(data, monday + 7 * 86400, true);
            Assert.That(data.miningEvent.periodKey, Is.Not.EqualTo(key));
            Assert.That(data.miningEvent.earnedTokens, Is.Zero);
            Assert.That(data.miningEvent.baselineOresMined, Is.EqualTo(50));
        }

        private static GameSaveData CreateData(long ores)
        {
            return new GameSaveData
            {
                oreCollection = new[]
                {
                    new OreCollectionData { contentId = "ore_copper", minedCount = ores }
                }
            };
        }
    }
}
