using System.Linq;
using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Localization;
using PocketForge.Mining;
using PocketForge.Presentation;
using PocketForge.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PocketForge.Tests.Editor
{
    public sealed class MineHudResponsiveLayoutTests
    {
        private MineHudView view;

        [SetUp]
        public void SetUp()
        {
            view = MineHudView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            if (view != null)
            {
                Object.DestroyImmediate(view.gameObject);
            }

            var eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                Object.DestroyImmediate(eventSystem.gameObject);
            }
        }

        [Test]
        public void CanvasScaler_KeepsPortraitHudAtStableWidth()
        {
            var scaler = view.GetComponent<CanvasScaler>();

            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1080f, 1920f)));
            Assert.That(scaler.matchWidthOrHeight, Is.Zero);
        }

        [TestCase(1920f)]
        [TestCase(2340f)]
        [TestCase(2400f)]
        public void MainHudRegions_StayOrderedInsideCommonPortraitHeights(float virtualHeight)
        {
            var topBar = VerticalRange("TopSurface", virtualHeight);
            var rewardedAd = VerticalRange("RewardedAdButton", virtualHeight);
            var progress = VerticalRange("ProgressBackground", virtualHeight);
            var mine = VerticalRange("MineButton", virtualHeight);
            var cards = VerticalRange("PickaxeButton", virtualHeight);

            Assert.That(topBar.Max, Is.LessThanOrEqualTo(virtualHeight));
            Assert.That(rewardedAd.Max, Is.LessThanOrEqualTo(topBar.Min), "The ad pill must sit below the resource bar.");
            Assert.That(mine.Max, Is.LessThanOrEqualTo(progress.Min + 0.01f), "The mine button must not cover the ore gauge.");
            Assert.That(cards.Max, Is.LessThanOrEqualTo(mine.Min + 0.01f), "Upgrade cards must not cover the mine button.");
            Assert.That(cards.Min, Is.GreaterThanOrEqualTo(0f));
        }

        [Test]
        public void MainHudRegions_StayInsideReferenceWidthWithoutOverlap()
        {
            const float width = 1080f;
            var topBar = HorizontalRange("TopSurface", width);
            var settings = HorizontalRange("SettingsButton", width);
            var pickaxe = HorizontalRange("PickaxeButton", width);
            var drill = HorizontalRange("DrillButton", width);
            var robot = HorizontalRange("RobotButton", width);

            Assert.That(topBar.Min, Is.GreaterThanOrEqualTo(0f));
            Assert.That(topBar.Max, Is.LessThanOrEqualTo(settings.Min));
            Assert.That(settings.Max, Is.LessThanOrEqualTo(width));
            Assert.That(pickaxe.Min, Is.GreaterThanOrEqualTo(0f));
            Assert.That(pickaxe.Max, Is.LessThanOrEqualTo(drill.Min));
            Assert.That(drill.Max, Is.LessThanOrEqualTo(robot.Min));
            Assert.That(robot.Max, Is.LessThanOrEqualTo(width));
        }

        [Test]
        public void ApprovedSnapshotChrome_UsesMeasured1080WideGeometry()
        {
            AssertRect("TopSurface", new Vector2(-61f, -123f), new Vector2(826f, 140f));
            AssertRect("MinerRankButton", new Vector2(-321f, -123f), new Vector2(260f, 94f));
            AssertRect("SettingsButton", new Vector2(-92f, -123f), new Vector2(116f, 116f));
            AssertRect("RewardedAdButton", new Vector2(263f, -254f), new Vector2(400f, 88f));
            AssertRect("ProgressBackground", new Vector2(0f, 29f), new Vector2(638f, 78f));
            AssertRect("MineButton", new Vector2(0f, 14f), new Vector2(504f, 232f));
            AssertRect("PickaxeButton", new Vector2(-320f, -6f), new Vector2(300f, 460f));
            AssertRect("SettingsCard", new Vector2(0f, 53f), new Vector2(900f, 1344f));
            AssertRect("CloseSettingsButton", new Vector2(0f, 66f), new Vector2(326f, 88f));
        }

        [Test]
        public void MiningFeedback_UsesTextOnlyCasualPresentation()
        {
            var surface = FindRect("ActionFeedbackSurface").GetComponent<Image>();
            var feedback = FindRect("ActionFeedback").GetComponent<Text>();

            view.ShowFeedback("+10 C", Color.yellow);

            Assert.That(surface.gameObject.activeSelf, Is.False);
            Assert.That(surface.raycastTarget, Is.False);
            Assert.That(feedback.gameObject.activeSelf, Is.True);
            Assert.That(feedback.fontSize, Is.EqualTo(46));
            Assert.That(feedback.GetComponent<Outline>(), Is.Not.Null);
            Assert.That(feedback.GetComponent<CasualFeedbackText>(), Is.Not.Null);
        }

        [Test]
        public void FinalUpgradeIcons_PreserveTheirSourceAspectRatios()
        {
            view.SetTheme(null, null, null, null, null);

            AssertTextureAspect("PickaxeIcon");
            AssertTextureAspect("DrillIcon");
            AssertTextureAspect("RobotIcon");
        }

        [Test]
        public void UpgradeCardDetails_HaveSeparateNonOverlappingRows()
        {
            var card = FindRect("PickaxeButton");
            var pip = FindChildRect(card, "LevelPip1");
            var level = FindChildRect(card, "Label");
            var cost = FindChildRect(card, "CostText");
            var action = FindChildRect(card, "UpgradeAction");

            Assert.That(VerticalRange(pip).Min, Is.GreaterThanOrEqualTo(VerticalRange(level).Max));
            Assert.That(VerticalRange(level).Min, Is.GreaterThanOrEqualTo(VerticalRange(cost).Max));
            Assert.That(VerticalRange(cost).Min, Is.GreaterThanOrEqualTo(VerticalRange(action).Max));
        }

        [Test]
        public void RemovedOreBadge_IsNotCreated()
        {
            Assert.That(view.GetComponentsInChildren<RectTransform>(true).Any(rect => rect.name == "OreBadge"), Is.False);
        }

        [Test]
        public void BossStage_ShowsLocalizedCountdownAboveProgressBar()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
                state.Ore.BossTimeRemaining = 12.1f;

                view.Render(state, service);

                var status = FindRect("ChapterStatus").GetComponent<Text>();
                Assert.That(status.text, Does.Contain(LanguageService.Get("boss").ToUpper()));
                Assert.That(status.text, Does.Contain("00:13"));
                Assert.That(status.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(0f, 66f)));
                Assert.That(status.rectTransform.sizeDelta, Is.EqualTo(new Vector2(600f, 52f)));
                Assert.That(status.GetComponent<Button>(), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void GeneratedOreHealthBarAndPips_AreAppliedByFinalSkin()
        {
            view.SetTheme(null, null, null, null, null);

            var frame = FindRect("ProgressBackground").GetComponent<Image>();
            var track = FindRect("ProgressTrack").GetComponent<Image>();
            var fill = FindRect("ProgressFill").GetComponent<Image>();
            var pip = FindChildRect(FindRect("PickaxeButton"), "LevelPip1").GetComponent<Image>();

            Assert.That(frame.sprite.texture.name, Is.EqualTo("HudOreHealthFrame"));
            Assert.That(track.sprite.texture.name, Is.EqualTo("HudOreHealthTrack"));
            Assert.That(fill.sprite.texture.name, Is.EqualTo("HudOreHealthFill"));
            Assert.That(pip.sprite.texture.name, Is.EqualTo("HudLevelPip"));
            Assert.That(frame.type, Is.EqualTo(Image.Type.Simple));
            Assert.That(track.type, Is.EqualTo(Image.Type.Simple));
            Assert.That(fill.type, Is.EqualTo(Image.Type.Simple));
            Assert.That(pip.type, Is.EqualTo(Image.Type.Simple));
            Assert.That(frame.preserveAspect, Is.False);
            Assert.That(track.preserveAspect, Is.False);
            Assert.That(fill.preserveAspect, Is.False);
            Assert.That(pip.preserveAspect, Is.False);
        }

        [Test]
        public void FinalHudChrome_UsesStretchedSimpleImages()
        {
            view.SetTheme(null, null, null, null, null);

            var hudNames = new[]
            {
                "TopSurface",
                "MinerRankButton",
                "SettingsButton",
                "RewardedAdButton",
                "ProgressBackground",
                "ProgressTrack",
                "ProgressFill",
                "MineButton",
                "PickaxeButton",
                "DrillButton",
                "RobotButton"
            };

            foreach (var name in hudNames)
            {
                AssertStretchedSimple(FindRect(name).GetComponent<Image>(), name);
            }

            foreach (var surface in view.GetComponentsInChildren<Image>(true).Where(image => image.name == "UpgradeAction"))
            {
                AssertStretchedSimple(surface, surface.name);
            }

            AssertStretchedSimple(FindRect("SettingsCard").GetComponent<Image>(), "SettingsCard");
        }

        [Test]
        public void FinalSettingsChrome_UsesSimpleImagesAndSmallerIcons()
        {
            view.SetTheme(null, null, null, null, null);

            var settingsCard = FindRect("SettingsCard");
            var slicedImages = settingsCard
                .GetComponentsInChildren<Image>(true)
                .Where(image => image.type == Image.Type.Sliced)
                .Select(image => image.name)
                .ToArray();

            Assert.That(slicedImages, Is.Empty, $"Settings UI still contains sliced images: {string.Join(", ", slicedImages)}");
            AssertIconSizes(settingsCard, "SettingIcon", new Vector2(60f, 60f), 4);
            AssertIconSizes(settingsCard, "MuteIcon", new Vector2(52f, 52f), 2);
            AssertIconSizes(settingsCard, "FlagIcon", new Vector2(114f, 68f), 4);
            AssertIconSizes(settingsCard, "RemoveAdsIcon", new Vector2(60f, 60f), 1);
            AssertIconSizes(settingsCard, "RestorePurchasesIcon", new Vector2(60f, 60f), 1);
            AssertIconSizes(settingsCard, "CloseIcon", new Vector2(64f, 64f), 1);
            Assert.That(FindRect("SettingsIcon").sizeDelta, Is.EqualTo(new Vector2(100f, 100f)));
        }

        private (float Min, float Max) VerticalRange(string name, float parentHeight)
        {
            var rect = FindRect(name);
            var center = rect.anchorMin.y * parentHeight + rect.anchoredPosition.y;
            return (center - rect.sizeDelta.y * 0.5f, center + rect.sizeDelta.y * 0.5f);
        }

        private (float Min, float Max) HorizontalRange(string name, float parentWidth)
        {
            var rect = FindRect(name);
            var center = rect.anchorMin.x * parentWidth + rect.anchoredPosition.x;
            return (center - rect.sizeDelta.x * 0.5f, center + rect.sizeDelta.x * 0.5f);
        }

        private RectTransform FindRect(string name)
        {
            return view.GetComponentsInChildren<RectTransform>(true).Single(rect => rect.name == name);
        }

        private static void AssertStretchedSimple(Image image, string name)
        {
            Assert.That(image.type, Is.EqualTo(Image.Type.Simple), $"{name} must not use 9-slice rendering.");
            Assert.That(image.preserveAspect, Is.False, $"{name} must fill its approved RectTransform.");
        }

        private static void AssertIconSizes(RectTransform parent, string name, Vector2 expectedSize, int expectedCount)
        {
            var icons = parent
                .GetComponentsInChildren<RectTransform>(true)
                .Where(rect => rect.name == name)
                .ToArray();

            Assert.That(icons, Has.Length.EqualTo(expectedCount));
            foreach (var icon in icons)
            {
                Assert.That(icon.sizeDelta, Is.EqualTo(expectedSize));
            }
        }

        [Test]
        public void UnusedTopLeftCounterAndDuplicateRewardButton_AreNotCreated()
        {
            var names = view.GetComponentsInChildren<RectTransform>(true)
                .Select(rect => rect.name)
                .ToArray();

            Assert.That(names, Does.Not.Contain("HeaderCoin"));
            Assert.That(names, Does.Not.Contain("HeaderCounterSlot"));
            Assert.That(names, Does.Not.Contain("CreditsRewardButton"));
        }

        [Test]
        public void MinerRankDisplay_UsesReservedHeaderSpaceWithoutMovingExistingCounters()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData
                {
                    minerLevel = 3,
                    minerExperience = 12
                }, 1f);

                view.Render(state, service);

                Assert.That(FindRect("MinerRank").GetComponent<Text>().text, Does.Contain("3"));
                Assert.That(FindRect("MinerExperience").GetComponent<Text>().text, Does.Contain("12"));
                AssertRect("CreditsCurrencyIcon", new Vector2(-52f, -123f), new Vector2(52f, 52f));
                AssertRect("DepthCurrencyIcon", new Vector2(151f, -123f), new Vector2(54f, 54f));
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void FirstBossClear_ShowsBlockingRewardPanelUntilContinue()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
                state.Ore.Health = 1f;
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();

                FindRect("MineButton").GetComponent<Button>().onClick.Invoke();

                var backdrop = FindRect("ChapterCompleteBackdrop");
                Assert.That(backdrop.gameObject.activeSelf, Is.True);
                Assert.That(backdrop.GetComponent<Image>().raycastTarget, Is.True);
                Assert.That(backdrop.GetSiblingIndex(), Is.EqualTo(view.transform.childCount - 1));
                Assert.That(FindRect("ChapterCompleteTitle").GetComponent<Text>().text, Does.Contain("1"));
                Assert.That(FindRect("ChapterCompleteCreditsValue").GetComponent<Text>().text, Is.EqualTo("+200"));
                Assert.That(FindRect("ChapterCompleteGemsValue").GetComponent<Text>().text, Is.EqualTo("+5"));
                Assert.That(state.Player.stage, Is.EqualTo(11));

                FindRect("ChapterCompleteContinueButton").GetComponent<Button>().onClick.Invoke();
                Assert.That(backdrop.gameObject.activeSelf, Is.False);
                Assert.That(state.Player.stage, Is.EqualTo(11));
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void ChapterCompletePanel_LocalizesEverySupportedLanguage()
        {
            var originalLanguage = LanguageService.Current;
            try
            {
                foreach (var language in new[]
                         {
                             SupportedLanguage.Korean,
                             SupportedLanguage.English,
                             SupportedLanguage.Japanese,
                             SupportedLanguage.ChineseSimplified
                         })
                {
                    LanguageService.SetLanguage(language);
                    view.ShowChapterComplete(2, 300, 7);

                    var title = FindRect("ChapterCompleteTitle").GetComponent<Text>().text;
                    var reward = FindRect("ChapterCompleteRewardLabel").GetComponent<Text>().text;
                    var continueText = FindRect("ChapterCompleteContinueButton").GetComponentInChildren<Text>().text;
                    Assert.That(title, Does.Contain("2"));
                    Assert.That(title, Does.Not.Contain("chapter_clear"));
                    Assert.That(reward, Does.Not.Contain("first_clear_reward"));
                    Assert.That(continueText, Does.Not.Contain("continue"));
                }
            }
            finally
            {
                LanguageService.SetLanguage(originalLanguage);
            }
        }

        [Test]
        public void ChapterStatus_OpensBlockingSelectionAndPreservesFurthestProgressOnRetry()
        {
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                stage = 15,
                furthestStage = 15,
                highestCompletedChapter = 1
            }, 1f);
            var presenter = new MineHudPresenter(view, service, state);
            presenter.Render();

            FindRect("ChapterStatus").GetComponent<Button>().onClick.Invoke();

            var backdrop = FindRect("ChapterSelectionBackdrop");
            Assert.That(backdrop.gameObject.activeSelf, Is.True);
            Assert.That(backdrop.GetComponent<Image>().raycastTarget, Is.True);
            Assert.That(backdrop.GetSiblingIndex(), Is.EqualTo(view.transform.childCount - 1));
            var chapterOneAction = FindChildRect(FindRect("ChapterRow1"), "ChapterActionButton").GetComponent<Button>();
            var chapterTwoAction = FindChildRect(FindRect("ChapterRow2"), "ChapterActionButton").GetComponent<Button>();
            var chapterThreeAction = FindChildRect(FindRect("ChapterRow3"), "ChapterActionButton").GetComponent<Button>();
            Assert.That(chapterOneAction.interactable, Is.True);
            Assert.That(chapterTwoAction.interactable, Is.False);
            Assert.That(chapterThreeAction.interactable, Is.False);

            chapterOneAction.onClick.Invoke();

            Assert.That(backdrop.gameObject.activeSelf, Is.False);
            Assert.That(state.Player.stage, Is.EqualTo(1));
            Assert.That(state.Player.furthestStage, Is.EqualTo(15));

            FindRect("ChapterStatus").GetComponent<Button>().onClick.Invoke();
            chapterTwoAction = FindChildRect(FindRect("ChapterRow2"), "ChapterActionButton").GetComponent<Button>();
            Assert.That(chapterTwoAction.interactable, Is.True);
            Assert.That(chapterTwoAction.GetComponentInChildren<Text>().text, Is.EqualTo(LanguageService.Get("resume").ToUpper()));

            chapterTwoAction.onClick.Invoke();

            Assert.That(state.Player.stage, Is.EqualTo(15));
            Assert.That(state.Player.furthestStage, Is.EqualTo(15));
            Assert.That(backdrop.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void BossFailure_StatusAndCurrentChapterActionReturnToBoss()
        {
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                stage = 10,
                furthestStage = 10
            }, 1f);
            var failure = service.Tick(state, state.Ore.Chapter.BossTimeLimitSeconds, 1f);
            Assert.That(failure.BossFailed, Is.True);

            var presenter = new MineHudPresenter(view, service, state);
            presenter.Render();

            var status = FindRect("ChapterStatus").GetComponent<Text>().text;
            Assert.That(status, Does.Contain(LanguageService.Get("boss_ready").ToUpper()));
            Assert.That(status, Does.Contain("0.5/5.5"));

            FindRect("ChapterStatus").GetComponent<Button>().onClick.Invoke();
            var chapterOneAction = FindChildRect(
                    FindRect("ChapterRow1"),
                    "ChapterActionButton")
                .GetComponent<Button>();
            Assert.That(chapterOneAction.interactable, Is.True);
            Assert.That(
                chapterOneAction.GetComponentInChildren<Text>().text,
                Is.EqualTo(LanguageService.Get("challenge").ToUpper()));

            chapterOneAction.onClick.Invoke();

            Assert.That(state.Player.stage, Is.EqualTo(10));
            Assert.That(state.Player.furthestStage, Is.EqualTo(10));
            Assert.That(state.Ore.IsBoss, Is.True);
        }

        [Test]
        public void OfflineProgress_UsesExistingPanelAndLocalizesDetailedSummary()
        {
            var result = new OfflineProgressResult(
                true,
                3720,
                3720,
                9,
                123,
                456);
            var originalLanguage = LanguageService.Current;
            try
            {
                foreach (var language in new[]
                         {
                             SupportedLanguage.Korean,
                             SupportedLanguage.English,
                             SupportedLanguage.Japanese,
                             SupportedLanguage.ChineseSimplified
                         })
                {
                    LanguageService.SetLanguage(language);
                    view.ShowOfflineReward(result);

                    var surface = FindRect("OfflineRewardSurface");
                    var text = FindRect("OfflineReward").GetComponent<Text>();
                    Assert.That(surface.sizeDelta, Is.EqualTo(new Vector2(410f, 82f)));
                    Assert.That(text.rectTransform.sizeDelta, Is.EqualTo(new Vector2(380f, 58f)));
                    Assert.That(text.text, Does.Contain("123"));
                    Assert.That(text.text, Does.Contain("+456 C"));
                    Assert.That(text.text, Does.Contain("\n"));
                    Assert.That(text.text, Does.Not.Contain("offline_"));
                }
            }
            finally
            {
                LanguageService.SetLanguage(originalLanguage);
            }
        }

        [Test]
        public void ChapterSelectionPanel_LocalizesEverySupportedLanguage()
        {
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<MiningContentCatalog>(
                "Assets/PocketForge/Content/MiningContentCatalog.asset");
            var service = new MiningGameService(catalog);
            var state = service.CreateInitialState(new GameSaveData
            {
                stage = 15,
                furthestStage = 15,
                highestCompletedChapter = 1
            }, 1f);
            var originalLanguage = LanguageService.Current;
            try
            {
                foreach (var language in new[]
                         {
                             SupportedLanguage.Korean,
                             SupportedLanguage.English,
                             SupportedLanguage.Japanese,
                             SupportedLanguage.ChineseSimplified
                         })
                {
                    LanguageService.SetLanguage(language);
                    view.ShowChapterSelection(service.GetChapterSelectionOptions(state));

                    var title = FindRect("ChapterSelectionTitle").GetComponent<Text>().text;
                    var stageRange = FindChildRect(FindRect("ChapterRow1"), "StageRange").GetComponent<Text>().text;
                    var action = FindChildRect(FindRect("ChapterRow1"), "ChapterActionButton").GetComponentInChildren<Text>().text;
                    Assert.That(title, Does.Not.Contain("chapter_select"));
                    Assert.That(stageRange, Does.Not.Contain("stage_range"));
                    Assert.That(action, Does.Not.Contain("retry"));
                }
            }
            finally
            {
                LanguageService.SetLanguage(originalLanguage);
            }
        }

        [Test]
        public void MinerRankButton_OpensResearchAndPurchasesFirstNodeAtLevelFour()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData
                {
                    minerLevel = 4,
                    highestRewardedMinerLevel = 4,
                    blueprintCores = 1
                }, 1f);
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();

                FindRect("MinerRankButton").GetComponent<Button>().onClick.Invoke();

                var backdrop = FindRect("ResearchBackdrop");
                var firstRow = FindRect("ResearchRow1");
                var purchase = FindChildRect(firstRow, "ResearchPurchaseButton").GetComponent<Button>();
                Assert.That(backdrop.gameObject.activeSelf, Is.True);
                Assert.That(backdrop.GetSiblingIndex(), Is.EqualTo(view.transform.childCount - 1));
                Assert.That(purchase.interactable, Is.True);

                purchase.onClick.Invoke();

                Assert.That(state.Player.blueprintCores, Is.Zero);
                Assert.That(
                    FindChildRect(firstRow, "ResearchLevel").GetComponent<Text>().text,
                    Does.Contain("1/5"));
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        private static RectTransform FindChildRect(RectTransform parent, string name)
        {
            return parent.GetComponentsInChildren<RectTransform>(true).Single(rect => rect.name == name);
        }

        private static (float Min, float Max) VerticalRange(RectTransform rect)
        {
            return (rect.anchoredPosition.y - rect.sizeDelta.y * 0.5f, rect.anchoredPosition.y + rect.sizeDelta.y * 0.5f);
        }

        private void AssertRect(string name, Vector2 position, Vector2 size)
        {
            var rect = FindRect(name);
            Assert.That(rect.anchoredPosition, Is.EqualTo(position), $"{name} position drifted from the approved snapshot.");
            Assert.That(rect.sizeDelta, Is.EqualTo(size), $"{name} size drifted from the approved snapshot.");
        }

        private void AssertTextureAspect(string name)
        {
            var icon = FindRect(name).GetComponent<RawImage>();
            var expected = icon.texture.width * icon.uvRect.width / (icon.texture.height * icon.uvRect.height);
            var actual = icon.rectTransform.rect.width / icon.rectTransform.rect.height;
            Assert.That(actual, Is.EqualTo(expected).Within(0.01f), $"{name} is visually stretched.");
        }
    }
}
