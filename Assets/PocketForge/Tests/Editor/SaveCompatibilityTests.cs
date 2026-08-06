using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Mining;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    /// <summary>
    /// An update must never strand an existing player. A save written by any shipped
    /// version is missing every field added after it, so JsonUtility fills those with
    /// nulls and zeros; loading such a save has to produce a playable state.
    /// </summary>
    public sealed class SaveCompatibilityTests
    {
        [Test]
        public void EverySavedVersion_LoadsIntoAPlayableState(
            [NUnit.Framework.Range(1, GameSaveMigrator.CurrentVersion)] int version)
        {
            // Only the version field survives from the oldest saves; this is the worst
            // case an update can meet.
            var data = JsonUtility.FromJson<GameSaveData>($"{{\"version\":{version}}}");

            var migrated = GameSaveMigrator.Normalize(data);

            Assert.That(migrated.version, Is.EqualTo(GameSaveMigrator.CurrentVersion));
            Assert.That(migrated.stage, Is.GreaterThanOrEqualTo(1));
            Assert.That(migrated.furthestStage, Is.GreaterThanOrEqualTo(migrated.stage));
            Assert.That(migrated.minerLevel, Is.GreaterThanOrEqualTo(1));
            Assert.That(migrated.highestRewardedMinerLevel, Is.InRange(1, migrated.minerLevel));
            Assert.That(migrated.credits, Is.GreaterThanOrEqualTo(0));
            Assert.That(migrated.gems, Is.GreaterThanOrEqualTo(0));
            Assert.That(migrated.blueprintCores, Is.GreaterThanOrEqualTo(0));
            Assert.That(migrated.researchProgress, Is.Not.Null);
            Assert.That(migrated.equipmentInventory, Is.Not.Null);
            Assert.That(migrated.equippedEquipment, Is.Not.Null);
            Assert.That(migrated.oreCollection, Is.Not.Null);
            Assert.That(migrated.achievementClaims, Is.Not.Null);
            Assert.That(migrated.dailyMissions, Is.Not.Null);
            Assert.That(migrated.weeklyMissions, Is.Not.Null);
            Assert.That(migrated.dailyShop, Is.Not.Null);
            Assert.That(migrated.miningEvent, Is.Not.Null);

            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(migrated, 1f);

                Assert.That(state.Ore, Is.Not.Null);
                Assert.That(state.Ore.Durability, Is.GreaterThan(0f));
                Assert.That(service.GetMiningPower(state).AutoPowerPerSecond, Is.GreaterThan(0f));
                Assert.That(service.Tick(state, 1f, 1f).StateChanged, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void CorruptedSave_FallsBackInsteadOfThrowing()
        {
            var data = JsonUtility.FromJson<GameSaveData>(
                "{\"version\":12,\"stage\":-40,\"furthestStage\":-99,\"credits\":-5," +
                "\"gems\":-1,\"blueprintCores\":-7,\"minerLevel\":0," +
                "\"highestRewardedMinerLevel\":99,\"pickaxeLevel\":-3," +
                "\"lastSavedUnixSeconds\":-1000,\"bossesDefeated\":-4," +
                "\"highestCompletedChapter\":-2}");

            var migrated = GameSaveMigrator.Normalize(data);

            Assert.That(migrated.stage, Is.EqualTo(1));
            Assert.That(migrated.furthestStage, Is.EqualTo(1));
            Assert.That(migrated.credits, Is.Zero);
            Assert.That(migrated.gems, Is.Zero);
            Assert.That(migrated.blueprintCores, Is.Zero);
            Assert.That(migrated.minerLevel, Is.EqualTo(1));
            Assert.That(migrated.highestRewardedMinerLevel, Is.EqualTo(1));
            Assert.That(migrated.pickaxeLevel, Is.Zero);
            Assert.That(migrated.lastSavedUnixSeconds, Is.Zero);
            Assert.That(migrated.bossesDefeated, Is.Zero);
            Assert.That(migrated.highestCompletedChapter, Is.Zero);
        }

        [Test]
        public void NormalizeIsIdempotent()
        {
            var once = GameSaveMigrator.Normalize(JsonUtility.FromJson<GameSaveData>(
                "{\"version\":3,\"stage\":17,\"credits\":250,\"minerLevel\":4}"));
            var twice = GameSaveMigrator.Normalize(JsonUtility.FromJson<GameSaveData>(
                JsonUtility.ToJson(once)));

            Assert.That(JsonUtility.ToJson(twice), Is.EqualTo(JsonUtility.ToJson(once)));
        }
    }
}
