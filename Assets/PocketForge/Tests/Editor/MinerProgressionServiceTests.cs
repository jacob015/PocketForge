using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class MinerProgressionServiceTests
    {
        [Test]
        public void ExperienceCurveAndOreSources_UseCatalogValues()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MinerProgressionService(catalog);

            Assert.That(service.GetRequiredExperienceForNextLevel(1), Is.EqualTo(20));
            Assert.That(service.GetRequiredExperienceForNextLevel(2), Is.EqualTo(35));
            Assert.That(service.GetOreExperience(1, false), Is.EqualTo(1));
            Assert.That(service.GetOreExperience(3, false), Is.EqualTo(3));
            Assert.That(service.GetOreExperience(3, true), Is.EqualTo(30));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GrantExperience_CanCrossMultipleLevelsAndUnlockFeatures()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MinerProgressionService(catalog);
            var player = new GameSaveData();

            var result = service.GrantExperience(player, 55);

            Assert.That(player.minerLevel, Is.EqualTo(3));
            Assert.That(player.minerExperience, Is.Zero);
            Assert.That(player.highestRewardedMinerLevel, Is.EqualTo(3));
            Assert.That(player.credits, Is.EqualTo(125));
            Assert.That(result.LevelsGained, Is.EqualTo(2));
            Assert.That(result.RewardCredits, Is.EqualTo(125));
            Assert.That(result.UnlockedFeatures, Is.EquivalentTo(new[]
            {
                ProgressionFeature.Equipment,
                ProgressionFeature.Museum
            }));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void LevelRewards_AreGrantedOnceAndMilestonesAwardGems()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MinerProgressionService(catalog);
            var player = new GameSaveData();

            var first = service.GrantExperience(player, 170);
            var creditsAfterFirstGrant = player.credits;
            var gemsAfterFirstGrant = player.gems;
            var duplicate = service.GrantExperience(player, 0);

            Assert.That(player.minerLevel, Is.EqualTo(5));
            Assert.That(first.RewardCredits, Is.EqualTo(350));
            Assert.That(first.RewardGems, Is.EqualTo(1));
            Assert.That(creditsAfterFirstGrant, Is.EqualTo(350));
            Assert.That(gemsAfterFirstGrant, Is.EqualTo(1));
            Assert.That(duplicate.DidLevelUp, Is.False);
            Assert.That(duplicate.RewardCredits, Is.Zero);
            Assert.That(duplicate.RewardGems, Is.Zero);
            Assert.That(player.credits, Is.EqualTo(creditsAfterFirstGrant));
            Assert.That(player.gems, Is.EqualTo(gemsAfterFirstGrant));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void UnlockQueries_ReportCurrentAndNextFeature()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MinerProgressionService(catalog);

            Assert.That(service.IsUnlocked(1, ProgressionFeature.Equipment), Is.False);
            Assert.That(service.IsUnlocked(2, ProgressionFeature.Equipment), Is.True);
            Assert.That(service.TryGetNextUnlock(3, out var next), Is.True);
            Assert.That(next.Feature, Is.EqualTo(ProgressionFeature.Research));
            Assert.That(next.RequiredLevel, Is.EqualTo(4));
            Assert.That(service.TryGetNextUnlock(7, out _), Is.False);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void RankMultiplier_IncreasesByTwoPercentPerLevel()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MinerProgressionService(catalog);

            Assert.That(service.GetRankPowerMultiplier(1), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(service.GetRankPowerMultiplier(6), Is.EqualTo(1.1f).Within(0.0001f));
            Object.DestroyImmediate(catalog);
        }
    }
}
