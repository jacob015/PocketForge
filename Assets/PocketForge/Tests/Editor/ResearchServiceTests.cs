using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class ResearchServiceTests
    {
        [Test]
        public void Purchase_EnforcesUnlockPrerequisiteCostAndMaximumLevel()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new ResearchService(catalog);
            var player = new GameSaveData
            {
                blueprintCores = 100
            };

            Assert.That(
                service.TryPurchase(player, "core_output", false),
                Is.EqualTo(ResearchPurchaseStatus.FeatureLocked));
            Assert.That(
                service.TryPurchase(player, "precision_tools", true),
                Is.EqualTo(ResearchPurchaseStatus.PrerequisiteMissing));

            Assert.That(
                service.TryPurchase(player, "core_output", true),
                Is.EqualTo(ResearchPurchaseStatus.Success));
            Assert.That(
                service.TryPurchase(player, "core_output", true),
                Is.EqualTo(ResearchPurchaseStatus.Success));
            Assert.That(player.blueprintCores, Is.EqualTo(97));
            Assert.That(
                service.TryPurchase(player, "precision_tools", true),
                Is.EqualTo(ResearchPurchaseStatus.Success));
            Assert.That(player.blueprintCores, Is.EqualTo(95));
            Assert.That(service.GetPowerMultiplier(player), Is.EqualTo(1.17f).Within(0.0001f));

            service.TryPurchase(player, "core_output", true);
            service.TryPurchase(player, "core_output", true);
            service.TryPurchase(player, "core_output", true);
            Assert.That(
                service.TryPurchase(player, "core_output", true),
                Is.EqualTo(ResearchPurchaseStatus.MaxLevel));
            Assert.That(service.GetLevel(player, "core_output"), Is.EqualTo(5));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Purchase_DoesNotMutateProgressWhenCoresAreInsufficient()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new ResearchService(catalog);
            var player = new GameSaveData();

            var result = service.TryPurchase(player, "core_output", true);

            Assert.That(result, Is.EqualTo(ResearchPurchaseStatus.InsufficientCores));
            Assert.That(player.blueprintCores, Is.Zero);
            Assert.That(service.GetLevel(player, "core_output"), Is.Zero);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void SaveMigration_NormalizesDuplicateResearchEntries()
        {
            var player = GameSaveMigrator.Normalize(new GameSaveData
            {
                version = 7,
                blueprintCores = -1,
                researchProgress = new[]
                {
                    new ResearchProgressData { nodeId = "core_output", level = 1 },
                    new ResearchProgressData { nodeId = "core_output", level = 3 },
                    new ResearchProgressData { nodeId = string.Empty, level = 5 }
                }
            });

            Assert.That(player.version, Is.EqualTo(GameSaveMigrator.CurrentVersion));
            Assert.That(player.blueprintCores, Is.Zero);
            Assert.That(player.researchProgress, Has.Length.EqualTo(1));
            Assert.That(player.researchProgress[0].nodeId, Is.EqualTo("core_output"));
            Assert.That(player.researchProgress[0].level, Is.EqualTo(3));
        }
    }
}
