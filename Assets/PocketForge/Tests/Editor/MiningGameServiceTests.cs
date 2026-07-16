using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Economy;
using PocketForge.Mining;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class MiningGameServiceTests
    {
        [Test]
        public void MiningOre_GrantsRewardAndCreatesNextStage()
        {
            var config = MiningGameConfig.CreateRuntimeDefault();
            var service = new MiningGameService(config);
            var state = service.CreateInitialState(new GameSaveData(), 1f);

            var result = service.Mine(state, 1f);
            for (var i = 0; i < 9; i++)
            {
                result = service.Mine(state, 1f);
            }

            Assert.IsTrue(result.OreBroken);
            Assert.AreEqual(2, state.Player.stage);
            Assert.AreEqual(2, state.Player.credits);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Upgrade_UsesConfiguredCostAndIncreasesLevel()
        {
            var config = MiningGameConfig.CreateRuntimeDefault();
            var service = new MiningGameService(config);
            var data = new GameSaveData { credits = 10 };
            var state = service.CreateInitialState(data, 1f);

            var result = service.TryUpgrade(state, UpgradeType.Pickaxe);

            Assert.IsTrue(result.PurchaseSucceeded);
            Assert.AreEqual(1, state.Player.pickaxeLevel);
            Assert.AreEqual(0, state.Player.credits);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SaveMigrator_NormalizesLegacyInvalidValues()
        {
            var data = GameSaveMigrator.Normalize(new GameSaveData
            {
                version = 1,
                credits = -1,
                stage = 0,
                pickaxeLevel = -2
            });

            Assert.AreEqual(GameSaveMigrator.CurrentVersion, data.version);
            Assert.AreEqual(0, data.credits);
            Assert.AreEqual(1, data.stage);
            Assert.AreEqual(0, data.pickaxeLevel);
        }
    }
}
