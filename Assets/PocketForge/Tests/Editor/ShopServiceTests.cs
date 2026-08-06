using System;
using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class ShopServiceTests
    {
        private MiningContentCatalog catalog;
        private ShopService service;

        [SetUp]
        public void SetUp()
        {
            catalog = MiningContentCatalog.CreateRuntimeDefault();
            service = new ShopService(catalog);
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(catalog);

        [Test]
        public void DailyFree_ClaimsOnceAndResetsAtUtcMidnight()
        {
            var data = new GameSaveData();
            var firstDay = new DateTimeOffset(2026, 8, 2, 23, 50, 0, TimeSpan.Zero).ToUnixTimeSeconds();

            Assert.That(service.ClaimDaily(data, "daily_supply", firstDay, true).Status,
                Is.EqualTo(ShopActionStatus.Success));
            Assert.That(service.ClaimDaily(data, "daily_supply", firstDay, true).Status,
                Is.EqualTo(ShopActionStatus.DailyLimitReached));
            Assert.That(service.ClaimDaily(data, "daily_supply", firstDay + 600, true).Status,
                Is.EqualTo(ShopActionStatus.Success));
            Assert.That(data.credits, Is.EqualTo(250));
            Assert.That(data.blueprintCores, Is.Zero);
        }

        [Test]
        public void ClockRollback_DoesNotReopenPreviousDailyOffer()
        {
            var data = new GameSaveData();
            var dayTwo = new DateTimeOffset(2026, 8, 3, 1, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            service.ClaimDaily(data, "daily_supply", dayTwo, true);

            var result = service.ClaimDaily(data, "daily_supply", dayTwo - 86400, true);

            Assert.That(result.Status, Is.EqualTo(ShopActionStatus.DailyLimitReached));
        }

        [Test]
        public void RewardedAndGemProducts_EnforceLimitAndCost()
        {
            var data = new GameSaveData { gems = 5 };
            const long now = 1785680000;
            for (var index = 0; index < 3; index++)
            {
                Assert.That(service.GrantRewarded(data, "rewarded_supply", now, true).Status,
                    Is.EqualTo(ShopActionStatus.Success));
            }

            Assert.That(service.GrantRewarded(data, "rewarded_supply", now, true).Status,
                Is.EqualTo(ShopActionStatus.DailyLimitReached));
            Assert.That(service.PurchaseWithGems(data, "gem_credit_crate", now, true).Status,
                Is.EqualTo(ShopActionStatus.Success));
            Assert.That(data.gems, Is.Zero);
            Assert.That(data.credits, Is.EqualTo(1200));
            Assert.That(service.PurchaseWithGems(data, "gem_credit_crate", now, true).Status,
                Is.EqualTo(ShopActionStatus.InsufficientGems));
        }

        [Test]
        public void StarterPack_GrantsExactlyOnce()
        {
            var data = new GameSaveData();

            Assert.That(service.GrantStarterPack(data).Status, Is.EqualTo(ShopActionStatus.Success));
            Assert.That(service.GrantStarterPack(data).Status, Is.EqualTo(ShopActionStatus.AlreadyOwned));
            Assert.That(data.credits, Is.EqualTo(2000));
            Assert.That(data.gems, Is.EqualTo(20));
            Assert.That(data.blueprintCores, Is.EqualTo(6));
        }
    }
}
