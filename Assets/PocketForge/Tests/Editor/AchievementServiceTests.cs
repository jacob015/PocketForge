using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class AchievementServiceTests
    {
        [Test]
        public void Claim_RequiresMuseumUnlockAndGrantsEachEligibleTierOnce()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var collection = new CollectionService(catalog);
            var service = new AchievementService(catalog, collection);
            var player = new GameSaveData
            {
                minerLevel = 3,
                oreCollection = new[]
                {
                    new OreCollectionData { contentId = "copper", minedCount = 100 }
                }
            };

            Assert.That(
                service.Claim(player, "mine_ores", false).Status,
                Is.EqualTo(AchievementClaimStatus.FeatureLocked));
            var first = service.Claim(player, "mine_ores", true);
            var second = service.Claim(player, "mine_ores", true);
            var third = service.Claim(player, "mine_ores", true);

            Assert.That(first.Status, Is.EqualTo(AchievementClaimStatus.Success));
            Assert.That(second.Status, Is.EqualTo(AchievementClaimStatus.Success));
            Assert.That(third.Status, Is.EqualTo(AchievementClaimStatus.RequirementNotMet));
            Assert.That(player.credits, Is.EqualTo(375));
            Assert.That(player.achievementClaims[0].claimedTiers, Is.EqualTo(2));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void AchievementMetrics_ReuseExistingProgressWithoutDuplicateCounters()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var collection = new CollectionService(catalog);
            var service = new AchievementService(catalog, collection);
            var player = new GameSaveData
            {
                highestCompletedChapter = 2,
                pickaxeLevel = 3,
                drillLevel = 4,
                robotLevel = 5,
                minerLevel = 7,
                equipmentRewardSequence = 6,
                researchProgress = new[]
                {
                    new ResearchProgressData { nodeId = "a", level = 2 },
                    new ResearchProgressData { nodeId = "b", level = 3 }
                }
            };

            Assert.That(service.GetProgress(player, AchievementMetric.HighestCompletedChapter), Is.EqualTo(2));
            Assert.That(service.GetProgress(player, AchievementMetric.FacilityLevelTotal), Is.EqualTo(12));
            Assert.That(service.GetProgress(player, AchievementMetric.MinerLevel), Is.EqualTo(7));
            Assert.That(service.GetProgress(player, AchievementMetric.ResearchLevelTotal), Is.EqualTo(5));
            Assert.That(service.GetProgress(player, AchievementMetric.EquipmentAcquired), Is.EqualTo(6));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Claim_GrantsGemAndCoreRewardsThroughExistingCurrencies()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var collection = new CollectionService(catalog);
            var service = new AchievementService(catalog, collection);
            var player = new GameSaveData
            {
                minerLevel = 3,
                highestCompletedChapter = 1,
                researchProgress = new[]
                {
                    new ResearchProgressData { nodeId = "core_output", level = 1 }
                }
            };

            var chapter = service.Claim(player, "clear_chapters", true);
            var research = service.Claim(player, "complete_research", true);

            Assert.That(chapter.RewardType, Is.EqualTo(AchievementRewardType.Gems));
            Assert.That(research.RewardType, Is.EqualTo(AchievementRewardType.BlueprintCores));
            Assert.That(player.gems, Is.EqualTo(1));
            Assert.That(player.blueprintCores, Is.EqualTo(1));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void SaveMigrationV10_NormalizesCollectionAndAchievementClaims()
        {
            var player = GameSaveMigrator.Normalize(new GameSaveData
            {
                version = 9,
                oreCollection = new[]
                {
                    new OreCollectionData { contentId = "copper", minedCount = -3 },
                    new OreCollectionData { contentId = "copper", minedCount = 40 },
                    new OreCollectionData { contentId = string.Empty, minedCount = 99 }
                },
                achievementClaims = new[]
                {
                    new AchievementClaimData { achievementId = "mine_ores", claimedTiers = -1 },
                    new AchievementClaimData { achievementId = "mine_ores", claimedTiers = 2 },
                    new AchievementClaimData { achievementId = string.Empty, claimedTiers = 9 }
                }
            });

            Assert.That(player.version, Is.EqualTo(GameSaveMigrator.CurrentVersion));
            Assert.That(player.oreCollection, Has.Length.EqualTo(1));
            Assert.That(player.oreCollection[0].minedCount, Is.EqualTo(40));
            Assert.That(player.achievementClaims, Has.Length.EqualTo(1));
            Assert.That(player.achievementClaims[0].claimedTiers, Is.EqualTo(2));
        }
    }
}
