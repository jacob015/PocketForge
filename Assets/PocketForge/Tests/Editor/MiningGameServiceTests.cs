using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Economy;
using PocketForge.Mining;
using PocketForge.Save;
using UnityEditor;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class MiningGameServiceTests
    {
        [Test]
        public void MiningOre_GrantsRewardAndCreatesNextStage()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData(), 1f);
            state.Ore.Health = 1f;

            var result = service.Mine(state, 1f);

            Assert.IsTrue(result.OreBroken);
            Assert.AreEqual(2, state.Player.stage);
            Assert.AreEqual(2, state.Player.furthestStage);
            Assert.AreEqual(2, state.Player.credits);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Upgrade_UsesConfiguredCostAndIncreasesLevel()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var data = new GameSaveData { credits = 10 };
            var state = service.CreateInitialState(data, 1f);

            var result = service.TryUpgrade(state, UpgradeType.Pickaxe);

            Assert.IsTrue(result.PurchaseSucceeded);
            Assert.AreEqual(1, state.Player.pickaxeLevel);
            Assert.AreEqual(0, state.Player.credits);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void ContentCatalog_SelectsCrystalOreAtStageTen()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");

            Assert.IsNotNull(catalog);
            Assert.AreEqual("crystal", catalog.GetOreForStage(10).ContentId);
            Assert.AreEqual(55f, catalog.GetOreForStage(10).GetDurability(10));
        }

        [Test]
        public void ContentCatalog_SelectsIronAndGoldAtTheirMilestones()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");

            Assert.IsNotNull(catalog);
            Assert.AreEqual("copper", catalog.GetOreForStage(3).ContentId);
            Assert.AreEqual("iron", catalog.GetOreForStage(4).ContentId);
            Assert.AreEqual("gold", catalog.GetOreForStage(7).ContentId);
            Assert.AreEqual("crystal", catalog.GetOreForStage(10).ContentId);
        }

        [Test]
        public void RuntimeChapter_MarksEveryTenthStageAsBoss()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);

            var normalState = service.CreateInitialState(new GameSaveData { stage = 9 }, 1f);
            var bossState = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);

            Assert.IsFalse(normalState.Ore.IsBoss);
            Assert.IsTrue(bossState.Ore.IsBoss);
            Assert.AreEqual(165f, bossState.Ore.Durability);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void FirstBossClear_GrantsChapterRewardOnlyOnce()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
            state.Ore.Health = 1f;

            var result = service.Mine(state, 1f);

            Assert.IsTrue(result.ChapterCompleted);
            Assert.IsTrue(result.FirstChapterClear);
            Assert.AreEqual(200, result.RewardCredits);
            Assert.AreEqual(5, result.RewardGems);
            Assert.AreEqual(1, result.CompletedChapterNumber);
            Assert.AreEqual(1, state.Player.highestCompletedChapter);
            Assert.AreEqual(5, state.Player.gems);

            var replay = service.CreateInitialState(new GameSaveData
            {
                stage = 10,
                highestCompletedChapter = 1
            }, 1f);
            replay.Ore.Health = 1f;
            var replayResult = service.Mine(replay, 1f);

            Assert.IsTrue(replayResult.ChapterCompleted);
            Assert.IsFalse(replayResult.FirstChapterClear);
            Assert.AreEqual(100, replayResult.RewardCredits);
            Assert.AreEqual(0, replayResult.RewardGems);
            Assert.AreEqual(1, replayResult.CompletedChapterNumber);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void BossTimer_CountsDownWhileBaseAutomationDamagesBoss()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);

            var result = service.Tick(state, 5f, 1f);

            Assert.IsTrue(result.StateChanged);
            Assert.IsFalse(result.BossFailed);
            Assert.AreEqual(25f, state.Ore.BossTimeRemaining);
            Assert.AreEqual(state.Ore.Durability - 2.5f, state.Ore.Health, 0.001f);
            Assert.AreEqual(10, state.Player.stage);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void BossTimer_BaseAutomationRefreshesHudWhileDamagingBoss()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);

            var withinSameSecond = service.Tick(state, 0.1f, 1f);
            var nextDisplayedSecond = service.Tick(state, 1f, 1f);

            Assert.IsTrue(withinSameSecond.StateChanged);
            Assert.IsTrue(nextDisplayedSecond.StateChanged);
            Assert.AreEqual(28.9f, state.Ore.BossTimeRemaining, 0.001f);
            Assert.AreEqual(state.Ore.Durability - 0.55f, state.Ore.Health, 0.001f);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void BossTimer_ExpiresAndReturnsToRepeatableFarmStage()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);

            var result = service.Tick(state, state.Ore.Chapter.BossTimeLimitSeconds, 1f);

            Assert.IsTrue(result.StateChanged);
            Assert.IsTrue(result.BossFailed);
            Assert.IsFalse(result.OreBroken);
            Assert.AreEqual(0, result.RewardCredits);
            Assert.AreEqual(9, state.Player.stage);
            Assert.AreEqual(10, state.Player.furthestStage);
            Assert.AreEqual(0, state.Player.credits);
            Assert.IsFalse(state.Ore.IsBoss);
            Assert.AreEqual(state.Ore.Durability, state.Ore.Health);
            Assert.AreEqual(0f, state.Ore.BossTimeRemaining);

            state.Ore.Health = 1f;
            var farmResult = service.Mine(state, 1f);
            Assert.IsTrue(farmResult.OreBroken);
            Assert.AreEqual(9, state.Player.stage);
            Assert.AreEqual(10, state.Player.furthestStage);

            var challenge = service.GetChapterSelectionOptions(state)[0];
            Assert.IsTrue(challenge.IsCurrent);
            Assert.IsTrue(challenge.IsBossChallenge);
            Assert.AreEqual(10, challenge.TargetStage);

            var challengeResult = service.SelectChapter(state, 1, 1f);
            Assert.IsTrue(challengeResult.StateChanged);
            Assert.AreEqual(10, state.Player.stage);
            Assert.IsTrue(state.Ore.IsBoss);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void BossTimer_FinalAutomationTickCanCompleteBossBeforeFailure()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
            state.Ore.Health = 0.01f;
            state.Ore.BossTimeRemaining = 0.1f;

            var result = service.Tick(state, 0.1f, 1f);

            Assert.IsTrue(result.OreBroken);
            Assert.IsFalse(result.BossFailed);
            Assert.IsTrue(result.ChapterCompleted);
            Assert.AreEqual(11, state.Player.stage);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void TapInput_IsLimitedToConfiguredReferenceRate()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData(), 1f);

            var firstTap = service.Mine(state, 1f);
            var healthAfterFirstTap = state.Ore.Health;
            var blockedTap = service.Mine(state, 1f);
            service.Tick(state, catalog.TapCooldownSeconds, 1f);
            var nextTap = service.Mine(state, 1f);

            Assert.IsTrue(firstTap.StateChanged);
            Assert.IsFalse(blockedTap.StateChanged);
            Assert.IsTrue(nextTap.StateChanged);
            Assert.That(state.Ore.Health, Is.LessThan(healthAfterFirstTap));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void NormalOre_HasNoBossTimer()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData { stage = 9 }, 1f);

            var result = service.Tick(state, 5f, 1f);

            Assert.IsTrue(result.StateChanged);
            Assert.IsFalse(result.BossFailed);
            Assert.AreEqual(state.Ore.Durability - 2.5f, state.Ore.Health, 0.001f);
            Assert.AreEqual(0f, state.Ore.BossTimeRemaining);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void ChapterSelection_ReportsClearedCurrentAndLockedChapters()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                stage = 15,
                furthestStage = 15,
                highestCompletedChapter = 1
            }, 1f);

            var options = service.GetChapterSelectionOptions(state);

            Assert.That(options, Has.Count.EqualTo(3));
            Assert.That(options[0].ChapterNumber, Is.EqualTo(1));
            Assert.That(options[0].IsCleared, Is.True);
            Assert.That(options[0].IsCurrent, Is.False);
            Assert.That(options[0].IsLocked, Is.False);
            Assert.That(options[1].ChapterNumber, Is.EqualTo(2));
            Assert.That(options[1].IsCurrent, Is.True);
            Assert.That(options[1].TargetStage, Is.EqualTo(15));
            Assert.That(options[2].ChapterNumber, Is.EqualTo(3));
            Assert.That(options[2].IsLocked, Is.True);
        }

        [Test]
        public void ChapterSelection_RetriesClearedChapterAndReturnsToFurthestStage()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                stage = 15,
                furthestStage = 15,
                highestCompletedChapter = 1
            }, 1f);

            var retryResult = service.SelectChapter(state, 1, 1f);

            Assert.That(retryResult.StateChanged, Is.True);
            Assert.That(state.Player.stage, Is.EqualTo(1));
            Assert.That(state.Player.furthestStage, Is.EqualTo(15));
            Assert.That(state.Ore.Chapter.ChapterNumber, Is.EqualTo(1));

            var resumeResult = service.SelectChapter(state, 2, 1f);

            Assert.That(resumeResult.StateChanged, Is.True);
            Assert.That(state.Player.stage, Is.EqualTo(15));
            Assert.That(state.Player.furthestStage, Is.EqualTo(15));
            Assert.That(state.Ore.Chapter.ChapterNumber, Is.EqualTo(2));
        }

        [Test]
        public void ChapterSelection_RejectsLockedOrCurrentChapter()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                stage = 15,
                furthestStage = 15,
                highestCompletedChapter = 1
            }, 1f);

            var lockedResult = service.SelectChapter(state, 3, 1f);
            var currentResult = service.SelectChapter(state, 2, 1f);

            Assert.That(lockedResult.StateChanged, Is.False);
            Assert.That(currentResult.StateChanged, Is.False);
            Assert.That(state.Player.stage, Is.EqualTo(15));
            Assert.That(state.Ore.Chapter.ChapterNumber, Is.EqualTo(2));
        }

        [Test]
        public void OfflineProgress_UsesTotalAutoPowerAndCapsElapsedTime()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                drillLevel = 1,
                lastSavedUnixSeconds = 100
            }, 1f);

            var result = service.ClaimOfflineProgress(
                state,
                100 + catalog.MaxOfflineRewardSeconds * 2L);

            Assert.That(result.CheckpointAdvanced, Is.True);
            Assert.That(result.ElapsedSeconds, Is.EqualTo(catalog.MaxOfflineRewardSeconds * 2L));
            Assert.That(result.RewardedSeconds, Is.EqualTo(catalog.MaxOfflineRewardSeconds));
            Assert.That(result.FarmStage, Is.EqualTo(1));
            Assert.That(result.ProcessedOres, Is.EqualTo(1440));
            Assert.That(result.Progression.ExperienceGained, Is.EqualTo(1440));
            Assert.That(result.Progression.CurrentLevel, Is.EqualTo(14));
            Assert.That(result.RewardCredits, Is.EqualTo(5480));
            Assert.That(state.Player.credits, Is.EqualTo(result.RewardCredits));
        }

        [Test]
        public void OfflineProgress_BaseAutomationWorksWithoutDrill()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                lastSavedUnixSeconds = 100
            }, 1f);

            var powerBeforeClaim = service.GetMiningPower(state).AutoPowerPerSecond;

            var result = service.ClaimOfflineProgress(state, 120);

            Assert.That(powerBeforeClaim, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(service.GetMiningPower(state).AutoPowerPerSecond, Is.EqualTo(0.505f).Within(0.0001f));
            Assert.That(result.ProcessedOres, Is.EqualTo(1));
            Assert.That(result.RewardCredits, Is.EqualTo(2));
            Assert.That(state.Player.credits, Is.EqualTo(2));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void OfflineProgress_BossUsesPreviousNormalStageWithoutAdvancingProgress()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                stage = 10,
                furthestStage = 10,
                lastSavedUnixSeconds = 100
            }, 1f);

            var result = service.ClaimOfflineProgress(state, 3700);

            Assert.That(result.FarmStage, Is.EqualTo(9));
            Assert.That(state.Player.stage, Is.EqualTo(10));
            Assert.That(state.Player.furthestStage, Is.EqualTo(10));
            Assert.That(state.Player.highestCompletedChapter, Is.Zero);
            Assert.That(state.Ore.IsBoss, Is.True);
        }

        [Test]
        public void OfflineProgress_ReplayUsesHighestUnlockedNormalStage()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                stage = 1,
                furthestStage = 25,
                highestCompletedChapter = 2,
                lastSavedUnixSeconds = 100
            }, 1f);

            var result = service.ClaimOfflineProgress(state, 200);

            Assert.That(result.FarmStage, Is.EqualTo(24));
            Assert.That(state.Player.stage, Is.EqualTo(1));
            Assert.That(state.Player.furthestStage, Is.EqualTo(25));
        }

        [Test]
        public void OfflineProgress_MissingTimestampInitializesWithoutWindfall()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData(), 1f);

            var result = service.ClaimOfflineProgress(state, 1000);

            Assert.That(result.CheckpointAdvanced, Is.True);
            Assert.That(result.ElapsedSeconds, Is.Zero);
            Assert.That(result.ProcessedOres, Is.Zero);
            Assert.That(result.RewardCredits, Is.Zero);
            Assert.That(state.Player.credits, Is.Zero);
            Assert.That(state.Player.lastSavedUnixSeconds, Is.EqualTo(1000));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void OfflineProgress_DuplicateOrBackwardClockCannotClaimAgain()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                lastSavedUnixSeconds = 100
            }, 1f);

            var first = service.ClaimOfflineProgress(state, 120);
            var creditsAfterFirstClaim = state.Player.credits;
            var duplicate = service.ClaimOfflineProgress(state, 120);
            var backward = service.ClaimOfflineProgress(state, 90);

            Assert.That(first.RewardCredits, Is.GreaterThan(0));
            Assert.That(duplicate.CheckpointAdvanced, Is.False);
            Assert.That(duplicate.RewardCredits, Is.Zero);
            Assert.That(backward.CheckpointAdvanced, Is.False);
            Assert.That(backward.RewardCredits, Is.Zero);
            Assert.That(state.Player.credits, Is.EqualTo(creditsAfterFirstClaim));
            Assert.That(state.Player.lastSavedUnixSeconds, Is.EqualTo(120));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void RewardedAd_GrantsConfiguredCreditsWithoutChangingStage()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData(), 1f);

            var result = service.GrantRewardedAdCredits(state);

            Assert.IsTrue(result.StateChanged);
            Assert.AreEqual(10, result.RewardCredits);
            Assert.AreEqual(10, state.Player.credits);
            Assert.AreEqual(1, state.Player.stage);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void SaveMigrator_NormalizesLegacyInvalidValues()
        {
            var data = GameSaveMigrator.Normalize(new GameSaveData
            {
                version = 1,
                credits = -1,
                gems = -1,
                stage = 0,
                furthestStage = -1,
                highestCompletedChapter = -1,
                pickaxeLevel = -2
                , minerLevel = -1
                , minerExperience = -1
                , highestRewardedMinerLevel = -1
                , lastSavedUnixSeconds = -1
            });

            Assert.AreEqual(GameSaveMigrator.CurrentVersion, data.version);
            Assert.AreEqual(0, data.credits);
            Assert.AreEqual(0, data.gems);
            Assert.AreEqual(1, data.stage);
            Assert.AreEqual(1, data.furthestStage);
            Assert.AreEqual(0, data.highestCompletedChapter);
            Assert.AreEqual(0, data.pickaxeLevel);
            Assert.AreEqual(1, data.minerLevel);
            Assert.AreEqual(0, data.minerExperience);
            Assert.AreEqual(1, data.highestRewardedMinerLevel);
            Assert.AreEqual(0, data.lastSavedUnixSeconds);
        }

        [Test]
        public void BreakingNormalAndBossOre_GrantsChapterScaledExperience()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var normalState = service.CreateInitialState(new GameSaveData { stage = 1 }, 1f);
            normalState.Ore.Health = 0.01f;
            var bossState = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
            bossState.Ore.Health = 0.01f;

            var normal = service.Mine(normalState, 1f);
            var boss = service.Mine(bossState, 1f);

            Assert.That(normal.Progression.ExperienceGained, Is.EqualTo(1));
            Assert.That(normalState.Player.minerExperience, Is.EqualTo(1));
            Assert.That(boss.Progression.ExperienceGained, Is.EqualTo(10));
            Assert.That(bossState.Player.minerExperience, Is.EqualTo(10));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void MinerRankModifier_ScalesAutoTapAndOfflinePower()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var baseState = service.CreateInitialState(new GameSaveData
            {
                lastSavedUnixSeconds = 100
            }, 1f);
            var rankedState = service.CreateInitialState(new GameSaveData
            {
                minerLevel = 6,
                highestRewardedMinerLevel = 6,
                lastSavedUnixSeconds = 100
            }, 1f);

            var basePower = service.GetMiningPower(baseState);
            var rankedPower = service.GetMiningPower(rankedState);
            var baseOffline = service.ClaimOfflineProgress(baseState, 1100);
            var rankedOffline = service.ClaimOfflineProgress(rankedState, 1100);

            Assert.That(rankedPower.AutoPowerPerSecond, Is.EqualTo(basePower.AutoPowerPerSecond * 1.1f).Within(0.0001f));
            Assert.That(rankedPower.TapDamage, Is.EqualTo(basePower.TapDamage * 1.1f).Within(0.0001f));
            Assert.That(rankedOffline.ProcessedOres, Is.GreaterThan(baseOffline.ProcessedOres));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void BossKills_GrantFirstAndRepeatBlueprintCoreRewards()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var player = new GameSaveData { stage = 10 };
            var firstState = service.CreateInitialState(player, 1f);
            firstState.Ore.Health = 0.01f;

            var first = service.Mine(firstState, 1f);
            player.stage = 10;
            var repeatState = service.CreateInitialState(player, 1f);
            repeatState.Ore.Health = 0.01f;
            var repeat = service.Mine(repeatState, 1f);

            Assert.That(first.FirstChapterClear, Is.True);
            Assert.That(first.RewardBlueprintCores, Is.EqualTo(3));
            Assert.That(repeat.FirstChapterClear, Is.False);
            Assert.That(repeat.RewardBlueprintCores, Is.EqualTo(1));
            Assert.That(player.blueprintCores, Is.EqualTo(4));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void ResearchModifier_ScalesTapAutoAndOfflineThroughSharedPowerBoundary()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var baseState = service.CreateInitialState(new GameSaveData
            {
                minerLevel = 4,
                highestRewardedMinerLevel = 4,
                lastSavedUnixSeconds = 100
            }, 1f);
            var researchedState = service.CreateInitialState(new GameSaveData
            {
                minerLevel = 4,
                highestRewardedMinerLevel = 4,
                lastSavedUnixSeconds = 100,
                researchProgress = new[]
                {
                    new ResearchProgressData { nodeId = "core_output", level = 1 }
                }
            }, 1f);

            var basePower = service.GetMiningPower(baseState);
            var researchedPower = service.GetMiningPower(researchedState);
            var baseOffline = service.ClaimOfflineProgress(baseState, 4100);
            var researchedOffline = service.ClaimOfflineProgress(researchedState, 4100);

            Assert.That(
                researchedPower.AutoPowerPerSecond,
                Is.EqualTo(basePower.AutoPowerPerSecond * 1.05f).Within(0.0001f));
            Assert.That(
                researchedPower.TapDamage,
                Is.EqualTo(basePower.TapDamage * 1.05f).Within(0.0001f));
            Assert.That(researchedOffline.ProcessedOres, Is.GreaterThan(baseOffline.ProcessedOres));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Credits_CanGrowPastLegacyIntLimit()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                credits = (long)int.MaxValue + 1000L
            }, 1f);

            var result = service.GrantRewardedAdCredits(state);

            Assert.That(result.RewardCredits, Is.GreaterThan(0));
            Assert.That(state.Player.credits, Is.GreaterThan(int.MaxValue));
            Object.DestroyImmediate(catalog);
        }
    }
}
