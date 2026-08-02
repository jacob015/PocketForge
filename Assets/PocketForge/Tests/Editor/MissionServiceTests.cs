using System;
using System.Linq;
using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Mining;
using PocketForge.Localization;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PocketForge.Tests.Editor
{
    public sealed class MissionServiceTests
    {
        private MiningContentCatalog catalog;
        private MissionService service;

        [SetUp]
        public void SetUp()
        {
            catalog = MiningContentCatalog.CreateRuntimeDefault();
            service = new MissionService(catalog, new EquipmentService(catalog));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void MissionBoard_RequiresMinerLevelFiveUnlock()
        {
            var player = new GameSaveData { minerLevel = 4 };
            var board = service.GetBoard(player, MissionPeriod.Daily, Utc(2026, 8, 3, 12), false);

            Assert.That(board.Missions, Is.Empty);
            Assert.That(player.dailyMissions.periodKey, Is.Empty);
        }

        [Test]
        public void DailyMission_UsesPeriodBaselineAndCannotPayTwice()
        {
            var now = Utc(2026, 8, 3, 12);
            var player = CreateUnlockedPlayer();
            service.Refresh(player, now, true);
            player.oreCollection[0].minedCount = 20;

            var first = service.Claim(player, "daily_mine", now, true);
            var duplicate = service.Claim(player, "daily_mine", now, true);

            Assert.That(first.Status, Is.EqualTo(MissionClaimStatus.Success));
            Assert.That(first.RewardAmount, Is.EqualTo(120));
            Assert.That(duplicate.Status, Is.EqualTo(MissionClaimStatus.AlreadyClaimed));
            Assert.That(player.credits, Is.EqualTo(120));
            Assert.That(player.dailyMissions.claimedMissionIds, Is.EquivalentTo(new[] { "daily_mine" }));
        }

        [Test]
        public void DailyRefresh_DoesNotResetWeeklyProgressAndClockRollbackDoesNotReopenOldPeriod()
        {
            var monday = Utc(2026, 8, 3, 12);
            var tuesday = Utc(2026, 8, 4, 1);
            var rolledBackSunday = Utc(2026, 8, 2, 8);
            var player = CreateUnlockedPlayer();
            service.Refresh(player, monday, true);
            player.oreCollection[0].minedCount = 25;

            var dailyBefore = service.GetBoard(player, MissionPeriod.Daily, monday, true);
            var weeklyBefore = service.GetBoard(player, MissionPeriod.Weekly, monday, true);
            var rollback = service.GetBoard(player, MissionPeriod.Daily, rolledBackSunday, true);
            var dailyAfter = service.GetBoard(player, MissionPeriod.Daily, tuesday, true);
            var weeklyAfter = service.GetBoard(player, MissionPeriod.Weekly, tuesday, true);

            Assert.That(dailyBefore.Missions.First().Progress, Is.EqualTo(25));
            Assert.That(weeklyBefore.Missions.First().Progress, Is.EqualTo(25));
            Assert.That(rollback.PeriodKey, Is.EqualTo(dailyBefore.PeriodKey));
            Assert.That(rollback.Missions.First().Progress, Is.EqualTo(25));
            Assert.That(dailyAfter.PeriodKey, Is.Not.EqualTo(dailyBefore.PeriodKey));
            Assert.That(dailyAfter.Missions.First().Progress, Is.Zero);
            Assert.That(weeklyAfter.PeriodKey, Is.EqualTo(weeklyBefore.PeriodKey));
            Assert.That(weeklyAfter.Missions.First().Progress, Is.EqualTo(25));
        }

        [Test]
        public void CompletionReward_RequiresEveryMissionClaimAndPaysOnce()
        {
            var now = Utc(2026, 8, 3, 12);
            var player = CreateUnlockedPlayer();
            service.Refresh(player, now, true);
            player.oreCollection[0].minedCount = 20;
            player.pickaxeLevel = 1;
            player.researchProgress = new[]
            {
                new ResearchProgressData { nodeId = "core_output", level = 1 }
            };
            player.bossesDefeated = 1;

            Assert.That(
                service.ClaimCompletion(player, MissionPeriod.Daily, now, true).Status,
                Is.EqualTo(MissionClaimStatus.CompletionNotReady));
            foreach (var id in new[] { "daily_mine", "daily_upgrade", "daily_research", "daily_boss" })
            {
                Assert.That(service.Claim(player, id, now, true).Status, Is.EqualTo(MissionClaimStatus.Success));
            }

            var completion = service.ClaimCompletion(player, MissionPeriod.Daily, now, true);
            var duplicate = service.ClaimCompletion(player, MissionPeriod.Daily, now, true);

            Assert.That(completion.Status, Is.EqualTo(MissionClaimStatus.Success));
            Assert.That(completion.RewardType, Is.EqualTo(MissionRewardType.BlueprintCores));
            Assert.That(player.blueprintCores, Is.EqualTo(3));
            Assert.That(duplicate.Status, Is.EqualTo(MissionClaimStatus.CompletionAlreadyClaimed));
        }

        [Test]
        public void WeeklyCompletion_UsesExistingEquipmentRewardPipeline()
        {
            var now = Utc(2026, 8, 3, 12);
            var player = CreateUnlockedPlayer();
            service.Refresh(player, now, true);
            player.oreCollection[0].minedCount = 300;
            player.pickaxeLevel = 8;
            player.bossesDefeated = 3;
            player.equipmentRewardSequence = 2;
            foreach (var id in new[] { "weekly_mine", "weekly_upgrade", "weekly_boss", "weekly_equipment" })
            {
                Assert.That(service.Claim(player, id, now, true).Status, Is.EqualTo(MissionClaimStatus.Success));
            }

            var completion = service.ClaimCompletion(player, MissionPeriod.Weekly, now, true);

            Assert.That(completion.Status, Is.EqualTo(MissionClaimStatus.Success));
            Assert.That(completion.RewardType, Is.EqualTo(MissionRewardType.Equipment));
            Assert.That(completion.RewardEquipment, Is.Not.Null);
            Assert.That(player.equipmentInventory, Has.Length.EqualTo(1));
            Assert.That(player.equipmentRewardSequence, Is.EqualTo(3));
        }

        [Test]
        public void SaveMigrationV11_NormalizesMissionStateAndBossCounter()
        {
            var player = GameSaveMigrator.Normalize(new GameSaveData
            {
                version = 10,
                highestCompletedChapter = 2,
                bossesDefeated = -5,
                lastObservedMissionUnixSeconds = -1,
                dailyMissions = new MissionPeriodData
                {
                    periodKey = null,
                    baseline = new MissionProgressSnapshotData { oresMined = -10 },
                    claimedMissionIds = new[] { "daily_mine", "daily_mine", "" }
                }
            });

            Assert.That(player.version, Is.EqualTo(GameSaveMigrator.CurrentVersion));
            Assert.That(player.bossesDefeated, Is.EqualTo(2));
            Assert.That(player.lastObservedMissionUnixSeconds, Is.Zero);
            Assert.That(player.dailyMissions.periodKey, Is.Empty);
            Assert.That(player.dailyMissions.baseline.oresMined, Is.Zero);
            Assert.That(player.dailyMissions.claimedMissionIds, Is.EquivalentTo(new[] { "daily_mine" }));
        }

        [Test]
        public void MissionLocalization_ProvidesEveryRuntimeKeyInFourLanguages()
        {
            var previous = LanguageService.Current;
            var keys = new[]
            {
                "daily", "weekly", "mission_summary", "mission_refresh_in",
                "mission_completion_reward", "mission_mine_ores",
                "mission_upgrade_facilities", "mission_complete_research",
                "mission_defeat_bosses", "mission_collect_equipment",
                "missions_locked", "mission_incomplete", "mission_already_claimed",
                "mission_completion_not_ready", "mission_completion_claimed",
                "missions_unavailable"
            };

            try
            {
                foreach (SupportedLanguage language in Enum.GetValues(typeof(SupportedLanguage)))
                {
                    LanguageService.SetLanguage(language);
                    foreach (var key in keys)
                    {
                        Assert.That(LanguageService.Get(key), Is.Not.EqualTo(key), $"{language}: {key}");
                    }
                }
            }
            finally
            {
                LanguageService.SetLanguage(previous);
            }
        }

        private static GameSaveData CreateUnlockedPlayer()
        {
            return new GameSaveData
            {
                minerLevel = 5,
                highestRewardedMinerLevel = 5,
                oreCollection = new[]
                {
                    new OreCollectionData { contentId = "copper", minedCount = 0 }
                }
            };
        }

        private static long Utc(int year, int month, int day, int hour)
        {
            return new DateTimeOffset(year, month, day, hour, 0, 0, TimeSpan.Zero)
                .ToUnixTimeSeconds();
        }
    }
}
