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
            var chapter = VerticalRange("ChapterInformationPanel", virtualHeight);
            var bossWarning = VerticalRange("BossWarningPanel", virtualHeight);
            var progress = VerticalRange("ProgressBackground", virtualHeight);
            var mine = VerticalRange("MineButton", virtualHeight);
            var cards = VerticalRange("PickaxeButton", virtualHeight);
            var navigation = VerticalRange("BottomNavigationBar", virtualHeight);

            Assert.That(topBar.Max, Is.LessThanOrEqualTo(virtualHeight));
            Assert.That(chapter.Max, Is.LessThanOrEqualTo(topBar.Min), "Chapter information must sit below the resource bar.");
            Assert.That(bossWarning.Max, Is.LessThanOrEqualTo(chapter.Min), "Boss warning must sit below chapter information.");
            Assert.That(mine.Max, Is.LessThanOrEqualTo(progress.Min + 0.01f), "The mine button must not cover the ore gauge.");
            Assert.That(cards.Max, Is.LessThanOrEqualTo(mine.Min + 0.01f), "Upgrade cards must not cover the mine button.");
            Assert.That(cards.Min, Is.GreaterThanOrEqualTo(navigation.Max - 0.01f));
            Assert.That(navigation.Min, Is.GreaterThanOrEqualTo(0f));
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
            Assert.That(settings.Max, Is.LessThanOrEqualTo(width));
            Assert.That(settings.Min, Is.GreaterThanOrEqualTo(topBar.Min));
            Assert.That(settings.Max, Is.LessThanOrEqualTo(topBar.Max + 0.01f));
            Assert.That(pickaxe.Min, Is.GreaterThanOrEqualTo(0f));
            Assert.That(pickaxe.Max, Is.LessThanOrEqualTo(drill.Min));
            Assert.That(drill.Max, Is.LessThanOrEqualTo(robot.Min));
            Assert.That(robot.Max, Is.LessThanOrEqualTo(width));
        }

        [Test]
        public void ApprovedSnapshotChrome_UsesMeasured1080WideGeometry()
        {
            AssertRect("TopSurface", new Vector2(0f, -104f), new Vector2(1040f, 142f));
            AssertRect("MinerRankButton", new Vector2(-324f, -104f), new Vector2(270f, 104f));
            AssertRect("SettingsButton", new Vector2(471f, -104f), new Vector2(94f, 94f));
            AssertRect("RewardedAdButton", new Vector2(33f, -104f), new Vector2(48f, 48f));
            AssertRect("ChapterInformationPanel", new Vector2(-178f, -316f), new Vector2(650f, 180f));
            AssertRect("PowerComparisonPanel", new Vector2(354f, -316f), new Vector2(294f, 180f));
            AssertRect("ProgressBackground", new Vector2(0f, 20f), new Vector2(560f, 86f));
            AssertRect("MineButton", new Vector2(0f, 660f), new Vector2(480f, 236f));
            AssertRect("PickaxeButton", new Vector2(-327f, 365f), new Vector2(302f, 340f));
            AssertRect("BottomNavigationBar", new Vector2(0f, 90f), new Vector2(1044f, 178f));
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

            Assert.That(pip.gameObject.activeSelf, Is.False);
            Assert.That(VerticalRange(level).Min, Is.GreaterThanOrEqualTo(VerticalRange(cost).Max));
            Assert.That(cost.anchoredPosition.y, Is.EqualTo(action.anchoredPosition.y));
            Assert.That(cost.anchoredPosition.x, Is.LessThan(action.anchoredPosition.x));
        }

        [Test]
        public void RemovedOreBadge_IsNotCreated()
        {
            Assert.That(view.GetComponentsInChildren<RectTransform>(true).Any(rect => rect.name == "OreBadge"), Is.False);
        }

        [Test]
        public void BossStage_ShowsLocalizedCountdownInWarningPanel()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
                state.Ore.BossTimeRemaining = 12.1f;

                view.Render(state, service);

                var status = FindRect("BossWarning").GetComponent<Text>();
                Assert.That(status.text, Does.Contain(LanguageService.Get("boss").ToUpper()));
                Assert.That(status.text, Does.Contain("00:13"));
                Assert.That(FindRect("ChapterInformationPanel").GetComponent<Button>(), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void V5OreHealthBar_UsesSeparateFrameAndFilledSprite()
        {
            view.SetTheme(null, null, null, null, null);

            var frame = FindRect("ProgressBackground").GetComponent<Image>();
            var track = FindRect("ProgressTrack").GetComponent<Image>();
            var fill = FindRect("ProgressFill").GetComponent<Image>();

            Assert.That(frame.sprite.texture.name, Is.EqualTo("12_OreHealthFrame"));
            Assert.That(track.sprite, Is.Null);
            Assert.That(fill.sprite.texture.name, Is.EqualTo("13_OreHealthFill"));
            Assert.That(frame.type, Is.EqualTo(Image.Type.Simple));
            Assert.That(track.type, Is.EqualTo(Image.Type.Simple));
            Assert.That(fill.type, Is.EqualTo(Image.Type.Filled));
            Assert.That(frame.preserveAspect, Is.True);
            Assert.That(track.preserveAspect, Is.False);
            Assert.That(fill.preserveAspect, Is.False);
        }

        [Test]
        public void V5HudChrome_UsesSimpleImagesWithoutSlicing()
        {
            view.SetTheme(null, null, null, null, null);

            var hudNames = new[]
            {
                "TopSurface",
                "SettingsButton",
                "ChapterInformationPanel",
                "PowerComparisonPanel",
                "BossWarningPanel",
                "ProgressBackground",
                "MineButton",
                "PickaxeButton",
                "DrillButton",
                "RobotButton",
                "BottomNavigationBar"
            };

            foreach (var name in hudNames)
            {
                AssertSimple(FindRect(name).GetComponent<Image>(), name);
            }

            AssertSimple(FindRect("SettingsCard").GetComponent<Image>(), "SettingsCard");
        }

        [Test]
        public void V5AssetSet_ContainsAllThirtyEightProductionSprites()
        {
            for (var index = 1; index <= 38; index++)
            {
                var prefix = index.ToString("00") + "_";
                var matches = Resources.LoadAll<Texture2D>("PocketForge/UI/V5")
                    .Where(texture => texture.name.StartsWith(prefix))
                    .ToArray();

                Assert.That(matches, Has.Length.EqualTo(1), $"V5 asset {prefix} is missing or duplicated.");
            }
        }

        [Test]
        public void V5RuntimeImages_AvoidSlicedRendering()
        {
            view.SetTheme(null, null, null, null, null);

            var v5Images = view.GetComponentsInChildren<Image>(true)
                .Where(image => image.sprite != null &&
                                char.IsDigit(image.sprite.texture.name[0]) &&
                                image.sprite.texture.name.Contains("_"))
                .ToArray();

            Assert.That(v5Images, Is.Not.Empty);
            foreach (var image in v5Images)
            {
                Assert.That(
                    image.type is Image.Type.Simple or Image.Type.Filled,
                    Is.True,
                    $"{image.name} unexpectedly uses {image.type}.");
            }
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
            Assert.That(FindRect("SettingsIcon").sizeDelta, Is.EqualTo(new Vector2(82f, 82f)));
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

        private static void AssertSimple(Image image, string name)
        {
            Assert.That(image.type, Is.EqualTo(Image.Type.Simple), $"{name} must not use 9-slice rendering.");
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
        public void MinerRankDisplay_UsesV5HeaderResourceSlots()
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
                AssertRect("CreditsCurrencyIcon", new Vector2(-116f, -104f), new Vector2(54f, 54f));
                AssertRect("DepthCurrencyIcon", new Vector2(92f, -104f), new Vector2(54f, 54f));
                Assert.That(FindRect("BlueprintCoreIcon"), Is.Not.Null);
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

            FindRect("ChapterInformationPanel").GetComponent<Button>().onClick.Invoke();

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

            FindRect("ChapterInformationPanel").GetComponent<Button>().onClick.Invoke();
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

            var status = FindRect("BossWarning").GetComponent<Text>().text;
            Assert.That(status, Does.Contain(LanguageService.Get("boss_ready").ToUpper()));

            Assert.That(FindRect("PowerValue").GetComponent<Text>().text, Does.Contain("5.5"));
            FindRect("ChapterInformationPanel").GetComponent<Button>().onClick.Invoke();
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
                    Assert.That(surface.sizeDelta, Is.EqualTo(new Vector2(230f, 236f)));
                    Assert.That(text.rectTransform.sizeDelta, Is.EqualTo(new Vector2(192f, 64f)));
                    Assert.That(text.text, Does.Contain("+456"));
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

        [Test]
        public void EquipmentNavigation_OpensSafeAreaModalAndEquipsSelectedItemAtLevelTwo()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData
                {
                    minerLevel = 2,
                    highestRewardedMinerLevel = 2,
                    equipmentInventory = new[]
                    {
                        new EquipmentItemData
                        {
                            instanceId = "pickaxe-item",
                            definitionId = "rugged_pickaxe",
                            rarity = (int)EquipmentRarity.Rare
                        }
                    }
                }, 1f);
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();

                FindRect("EquipmentNavigation").GetComponent<Button>().onClick.Invoke();

                var backdrop = FindRect("EquipmentBackdrop");
                var card = FindRect("EquipmentCard");
                var previous = FindRect("EquipmentPrevious");
                var firstRow = FindRect("EquipmentInventoryRow1");
                var primary = FindRect("EquipmentPrimary").GetComponent<Button>();
                Assert.That(backdrop.gameObject.activeSelf, Is.True);
                Assert.That(backdrop.GetSiblingIndex(), Is.EqualTo(view.transform.childCount - 1));
                Assert.That(card.sizeDelta, Is.EqualTo(new Vector2(920f, 1380f)));
                Assert.That(previous.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)));
                Assert.That(previous.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.5f)));
                Assert.That(firstRow.gameObject.activeSelf, Is.True);
                Assert.That(primary.interactable, Is.True);

                primary.onClick.Invoke();

                Assert.That(state.Player.equippedEquipment, Has.Length.EqualTo(1));
                Assert.That(state.Player.equippedEquipment[0].instanceId, Is.EqualTo("pickaxe-item"));
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void MuseumNavigation_OpensCollectionTabsAndClaimsAchievementAtLevelThree()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData
                {
                    minerLevel = 3,
                    highestRewardedMinerLevel = 3,
                    oreCollection = new[]
                    {
                        new OreCollectionData { contentId = "copper", minedCount = 10 }
                    }
                }, 1f);
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();
                view.SetTheme(null, null, null, null, null);

                FindRect("MuseumNavigation").GetComponent<Button>().onClick.Invoke();

                var backdrop = FindRect("CollectionBackdrop");
                var card = FindRect("CollectionCard");
                var museumRow = FindRect("MuseumRow1");
                Assert.That(backdrop.gameObject.activeSelf, Is.True);
                Assert.That(backdrop.GetSiblingIndex(), Is.EqualTo(view.transform.childCount - 1));
                Assert.That(card.sizeDelta, Is.EqualTo(new Vector2(920f, 1380f)));
                Assert.That(museumRow.gameObject.activeSelf, Is.True);
                Assert.That(museumRow.GetComponent<Image>().type, Is.EqualTo(Image.Type.Simple));

                FindRect("AchievementsTab").GetComponent<Button>().onClick.Invoke();
                var achievementRow = FindRect("AchievementRow1");
                var claim = FindChildRect(achievementRow, "ClaimAchievement").GetComponent<Button>();
                Assert.That(achievementRow.gameObject.activeSelf, Is.True);
                Assert.That(claim.interactable, Is.True);

                claim.onClick.Invoke();

                Assert.That(state.Player.credits, Is.EqualTo(100));
                Assert.That(state.Player.achievementClaims, Has.Length.EqualTo(1));
                Assert.That(state.Player.achievementClaims[0].claimedTiers, Is.EqualTo(1));
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
