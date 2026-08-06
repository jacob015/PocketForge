using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Mining;
using PocketForge.Save;
using UnityEditor;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class MiningPowerServiceTests
    {
        [Test]
        public void BaseFacilities_ProvideIdleAndActivePowerFromTheStart()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningPowerService(catalog);

            var power = service.Calculate(new GameSaveData());

            Assert.That(power.AutoPowerPerSecond, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(power.TapDamage, Is.EqualTo(1f).Within(0.001f));
            Assert.That(power.ActivePowerPerSecond, Is.EqualTo(5.5f).Within(0.001f));
            Assert.That(power.TapCooldownSeconds, Is.EqualTo(0.2f).Within(0.001f));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void FacilityLevels_ContributeThroughSeparatePowerFactors()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningPowerService(catalog);

            var power = service.Calculate(new GameSaveData
            {
                pickaxeLevel = 4,
                drillLevel = 4,
                robotLevel = 5
            });

            Assert.That(power.DrillPower, Is.EqualTo(2.5f).Within(0.001f));
            Assert.That(power.RobotMultiplier, Is.EqualTo(1.5f).Within(0.001f));
            Assert.That(power.AutoPowerPerSecond, Is.EqualTo(3.75f).Within(0.001f));
            Assert.That(power.TapDamage, Is.EqualTo(3f).Within(0.001f));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void FutureMetaModifiers_ScalePowerWithoutChangingSavedSourceLevels()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningPowerService(catalog);
            var player = new GameSaveData { drillLevel = 3 };
            var basePower = service.Calculate(player);

            var modifiedPower = service.Calculate(
                player,
                new MiningPowerModifiers(1.2f, 1.5f, 1.1f, 1.05f, 1.25f));

            Assert.That(modifiedPower.AutoPowerPerSecond, Is.GreaterThan(basePower.AutoPowerPerSecond));
            Assert.That(player.drillLevel, Is.EqualTo(3));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void ExistingChapterData_ProducesIncreasingBossPowerGates()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var gameService = new MiningGameService(catalog);
            var chapterOne = gameService.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
            var chapterTwo = gameService.CreateInitialState(new GameSaveData { stage = 20 }, 1f);
            var chapterThree = gameService.CreateInitialState(new GameSaveData { stage = 30 }, 1f);

            Assert.That(gameService.GetBossRecommendedPower(chapterOne), Is.EqualTo(4f).Within(0.01f));
            Assert.That(gameService.GetBossRecommendedPower(chapterTwo), Is.EqualTo(11f).Within(0.01f));
            Assert.That(gameService.GetBossRecommendedPower(chapterThree), Is.EqualTo(20.967f).Within(0.01f));
        }

        [Test]
        public void BaseActivePower_CannotReachFirstOrLaterBosses()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var powerService = new MiningPowerService(catalog);
            var gameService = new MiningGameService(catalog);
            var basePower = powerService.Calculate(new GameSaveData());
            var chapterOne = gameService.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
            var chapterTwo = gameService.CreateInitialState(new GameSaveData { stage = 20 }, 1f);

            Assert.That(
                basePower.ActivePowerPerSecond,
                Is.LessThan(gameService.GetBossRecommendedPower(chapterOne)));
            Assert.That(
                basePower.ActivePowerPerSecond,
                Is.LessThan(gameService.GetBossRecommendedPower(chapterTwo)));
        }
    }
}
