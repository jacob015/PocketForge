using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Mining;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class CollectionServiceTests
    {
        [TestCase(0L, 1f)]
        [TestCase(1L, 1.01f)]
        [TestCase(25L, 1.02f)]
        [TestCase(100L, 1.03f)]
        [TestCase(500L, 1.04f)]
        public void CollectionMilestones_ApplyDiscoveryAndThreePermanentBonuses(
            long minedCount,
            float expectedMultiplier)
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new CollectionService(catalog);
            var player = new GameSaveData();
            service.RecordMinedOre(player, "copper", minedCount);

            Assert.That(service.GetPowerMultiplier(player), Is.EqualTo(expectedMultiplier).Within(0.0001f));
            Assert.That(service.GetStates(player)[0].MinedCount, Is.EqualTo(minedCount));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void CollectionSanitizer_RemovesUnknownAndKeepsHighestDuplicateCount()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new CollectionService(catalog);
            var player = new GameSaveData
            {
                oreCollection = new[]
                {
                    new OreCollectionData { contentId = "copper", minedCount = 3 },
                    new OreCollectionData { contentId = "copper", minedCount = 9 },
                    new OreCollectionData { contentId = "unknown", minedCount = 99 }
                }
            };

            service.SanitizeAgainstCatalog(player);

            Assert.That(player.oreCollection, Has.Length.EqualTo(1));
            Assert.That(player.oreCollection[0].contentId, Is.EqualTo("copper"));
            Assert.That(player.oreCollection[0].minedCount, Is.EqualTo(9));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void MiningGameService_OnlineAndOfflineOreBreaksUpdateCollection()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var game = new MiningGameService(catalog);
            var state = game.CreateInitialState(new GameSaveData { lastSavedUnixSeconds = 100 }, 1f);
            state.Ore.Health = 0.01f;

            var online = game.Mine(state, 1f);
            var offline = game.ClaimOfflineProgress(state, 4100);
            var collection = game.GetCollectionStates(state)[0];

            Assert.That(online.OreBroken, Is.True);
            Assert.That(offline.ProcessedOres, Is.GreaterThan(0));
            Assert.That(collection.MinedCount, Is.EqualTo(1L + offline.ProcessedOres));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void CollectionMultiplier_ScalesTapAutoAndFutureOfflinePower()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var game = new MiningGameService(catalog);
            var baseState = game.CreateInitialState(new GameSaveData { lastSavedUnixSeconds = 100 }, 1f);
            var collectedState = game.CreateInitialState(new GameSaveData
            {
                lastSavedUnixSeconds = 100,
                oreCollection = new[]
                {
                    new OreCollectionData { contentId = "copper", minedCount = 500 }
                }
            }, 1f);

            var basePower = game.GetMiningPower(baseState);
            var collectedPower = game.GetMiningPower(collectedState);
            var baseOffline = game.ClaimOfflineProgress(baseState, 4100);
            var collectedOffline = game.ClaimOfflineProgress(collectedState, 4100);

            Assert.That(collectedPower.AutoPowerPerSecond, Is.EqualTo(basePower.AutoPowerPerSecond * 1.04f).Within(0.0001f));
            Assert.That(collectedPower.TapDamage, Is.EqualTo(basePower.TapDamage * 1.04f).Within(0.0001f));
            Assert.That(collectedOffline.ProcessedOres, Is.GreaterThan(baseOffline.ProcessedOres));
            Object.DestroyImmediate(catalog);
        }
    }
}
