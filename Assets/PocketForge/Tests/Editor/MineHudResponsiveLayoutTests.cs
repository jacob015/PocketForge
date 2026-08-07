using System;
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
using Object = UnityEngine.Object;

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
            var bossAction = VerticalRange("BossChallengeButton", virtualHeight);
            var cards = VerticalRange("PickaxeButton", virtualHeight);
            var navigation = VerticalRange("BottomNavigationBar", virtualHeight);

            Assert.That(topBar.Max, Is.LessThanOrEqualTo(virtualHeight));
            Assert.That(chapter.Max, Is.LessThanOrEqualTo(topBar.Min), "Chapter information must sit below the resource bar.");
            Assert.That(bossWarning.Max, Is.LessThanOrEqualTo(chapter.Min), "Boss warning must sit below chapter information.");
            Assert.That(mine.Max, Is.LessThanOrEqualTo(progress.Min + 0.01f), "The mine button must not cover the ore gauge.");
            Assert.That(bossAction.Max, Is.LessThanOrEqualTo(mine.Min), "Boss action needs its own row below the mine shortcuts.");
            Assert.That(cards.Max, Is.LessThanOrEqualTo(bossAction.Min), "Upgrade cards must not cover the boss action row.");
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
            AssertRect("TopSurface", new Vector2(0f, -189f), new Vector2(972f, 162f));
            AssertRect("PortraitFrame", new Vector2(-379.5f, -191.5f), new Vector2(144f, 144f));
            AssertRect("MinerRankButton", new Vector2(-231.9f, -185f), new Vector2(184f, 104f));
            AssertRect("SettingsButton", new Vector2(403f, -189f), new Vector2(84f, 84f));
            AssertRect("RewardedAdButton", new Vector2(308f, -190f), new Vector2(100f, 62f));
            AssertRect("ChapterInformationPanel", new Vector2(-168f, -394f), new Vector2(636f, 188f));
            AssertRect("PowerComparisonPanel", new Vector2(353f, -394f), new Vector2(265f, 188f));
            AssertRect("ProgressBackground", new Vector2(0f, 1014f), new Vector2(560f, 86f));
            AssertRect("MineButton", new Vector2(0f, 843f), new Vector2(450f, 186f));
            AssertRect("BossChallengeButton", new Vector2(0f, 650f), new Vector2(322f, 90f));
            AssertRect("PickaxeButton", new Vector2(-327f, 420f), new Vector2(302f, 330f));
            AssertRect("BottomNavigationBar", new Vector2(0f, 120f), new Vector2(948f, 160f));
            AssertRect("EquipmentNavigation", new Vector2(-296f, -28f), new Vector2(144f, 144f));
            AssertRect("ResearchNavigation", new Vector2(-162f, -28f), new Vector2(144f, 144f));
            AssertRect("HomeNavigation", new Vector2(0f, -13f), new Vector2(144f, 144f));
            AssertRect("MuseumNavigation", new Vector2(162f, -32f), new Vector2(144f, 144f));
            AssertRect("MissionsNavigation", new Vector2(455f, 1529f), new Vector2(144f, 144f));
            AssertRect("ShopNavigation", new Vector2(291f, -28f), new Vector2(144f, 144f));
            AssertRect("ProgressTrack", new Vector2(-1f, 0.5f), new Vector2(449f, 54f));
            AssertRect("ProgressFill", Vector2.zero, new Vector2(449f, 54f));
            AssertRect("MineIcon", Vector2.zero, new Vector2(78.912f, 85.248f));
            AssertMissing("MineActionText");
            AssertMissing("SelectedNavigationTab");
            AssertMissing("feature_equipment");
            AssertMissing("feature_research");
            AssertMissing("home");
            AssertMissing("feature_museum");
            AssertMissing("feature_missions");
            AssertMissing("feature_shop");
            AssertRect("SettingsCard", new Vector2(0f, 53f), new Vector2(900f, 1344f));
            AssertRect("CloseSettingsButton", new Vector2(0f, 66f), new Vector2(326f, 88f));
        }

        [Test]
        public void HeaderPanels_DrawAtTheSameHeightSoTheirEdgesAlign()
        {
            view.SetTheme(null, null, null, null, null);

            var chapter = DrawnSize("ChapterInformationPanel");
            var power = DrawnSize("PowerComparisonPanel");

            Assert.That(power.y, Is.EqualTo(chapter.y).Within(4f));
            Assert.That(
                FindRect("PowerComparisonPanel").anchoredPosition.y,
                Is.EqualTo(FindRect("ChapterInformationPanel").anchoredPosition.y));
        }

        [Test]
        public void UpgradeCards_FillTheBakedGaugePillWithTheirCurrentContribution()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData { drillLevel = 2 }, 1f);

                view.Render(state, service);

                foreach (var card in new[] { "PickaxeButton", "DrillButton", "RobotButton" })
                {
                    var effect = FindChildRect(FindRect(card), "UpgradeEffect");
                    var text = effect.GetComponent<Text>();
                    Assert.That(text.text, Is.Not.Empty, card);
                    AssertNoOverlap(
                        effect,
                        FindChildRect(FindRect(card), "Label"),
                        $"{card} effect row must not enter the level row.");
                    AssertNoOverlap(
                        effect,
                        FindChildRect(FindRect(card), "CostText"),
                        $"{card} effect row must not enter the cost row.");
                }
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void EveryVisibleLabel_StaysAtOrAboveTheMinimumReadableSize()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var previous = LanguageService.Current;
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(CreateFullyUnlockedPlayer(), 1f);
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();
                view.SetTheme(null, null, null, null, null);

                // Every modal is opened so its labels count as visible for this sweep.
                foreach (var entry in new[]
                         {
                             "EquipmentNavigation", "ResearchNavigation",
                             "MuseumNavigation", "MissionsNavigation", "ShopNavigation"
                         })
                {
                    FindRect(entry).GetComponent<Button>().onClick.Invoke();
                }

                foreach (SupportedLanguage language in Enum.GetValues(typeof(SupportedLanguage)))
                {
                    LanguageService.SetLanguage(language);
                    view.Render(state, service);
                    Canvas.ForceUpdateCanvases();

                    foreach (var label in view.GetComponentsInChildren<Text>(true))
                    {
                        if (!label.gameObject.activeInHierarchy ||
                            string.IsNullOrWhiteSpace(label.text))
                        {
                            continue;
                        }

                        // With a graphics device the generator reports the size actually
                        // rasterised. Under -nographics (CI) no glyphs are generated and
                        // that value is meaningless, so fall back to the smallest size the
                        // label is *configured* to allow — the invariant that holds
                        // regardless of environment.
                        var resolved = label.resizeTextForBestFit
                            ? (HasGraphicsDevice
                                ? label.cachedTextGenerator.fontSizeUsedForBestFit
                                : label.resizeTextMinSize)
                            : label.fontSize;
                        if (resolved <= 0)
                        {
                            continue;
                        }

                        Assert.That(
                            resolved,
                            Is.GreaterThanOrEqualTo(MineHudView.MinimumReadableFontSize),
                            $"{language} {label.transform.parent.name}/{label.name} resolved to {resolved}.");
                    }
                }
            }
            finally
            {
                LanguageService.SetLanguage(previous);
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void WeeklyMissionRewardIcons_ShareOneDrawnAreaDespiteDifferentAspects()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(CreateFullyUnlockedPlayer(), 1f);
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();
                view.SetTheme(null, null, null, null, null);

                FindRect("MissionsNavigation").GetComponent<Button>().onClick.Invoke();
                FindRect("WeeklyMissionsTab").GetComponent<Button>().onClick.Invoke();

                var areas = new System.Collections.Generic.List<float>();
                var aspects = new System.Collections.Generic.List<float>();
                for (var index = 0; index < 4; index++)
                {
                    var icon = FindChildRect(FindRect($"MissionRow{index}"), "MissionRewardIcon");
                    var size = icon.sizeDelta;
                    areas.Add(size.x * size.y);
                    aspects.Add(size.x / size.y);
                }

                Assert.That(areas.Max() - areas.Min(), Is.LessThan(2f), "Reward icons must share one drawn area.");
                Assert.That(
                    aspects.Max() - aspects.Min(),
                    Is.GreaterThan(0.2f),
                    "This board must mix icon aspects, otherwise the rule is untested.");
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        private static GameSaveData CreateFullyUnlockedPlayer()
        {
            return new GameSaveData
            {
                minerLevel = 7,
                highestRewardedMinerLevel = 7,
                highestCompletedChapter = 2,
                pickaxeLevel = 4,
                drillLevel = 3,
                robotLevel = 2,
                credits = 5000,
                gems = 30,
                blueprintCores = 6
            };
        }

        private static bool HasGraphicsDevice =>
            SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;

        private Vector2 DrawnSize(string name)
        {
            var rect = FindRect(name);
            var image = rect.GetComponent<Image>();
            Assert.That(image, Is.Not.Null, name);
            Assert.That(image.sprite, Is.Not.Null, name);
            if (!image.preserveAspect)
            {
                return rect.sizeDelta;
            }

            var artAspect = image.sprite.rect.width / image.sprite.rect.height;
            var rectAspect = rect.sizeDelta.x / Mathf.Max(1f, rect.sizeDelta.y);
            return rectAspect < artAspect
                ? new Vector2(rect.sizeDelta.x, rect.sizeDelta.x / artAspect)
                : new Vector2(rect.sizeDelta.y * artAspect, rect.sizeDelta.y);
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
            Assert.That(cost.anchoredPosition.y, Is.EqualTo(-120.7f).Within(0.01f));
            Assert.That(Mathf.Abs(cost.anchoredPosition.y - action.anchoredPosition.y), Is.LessThan(8f));
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
        public void V5AssetSet_ContainsEveryProductionSprite()
        {
            for (var index = 1; index <= 38; index++)
            {
                // 29_SelectedNavigationTab was retired with the selected-tab indicator
                // itself, which AssertMissing("SelectedNavigationTab") already pins.
                if (index == 29)
                {
                    continue;
                }

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
        public void Task13GasShotAssets_AreCompleteAndUseMeasuredRuntimeClassifications()
        {
            var textures = Resources.LoadAll<Texture2D>("PocketForge/UI/Task13");

            // Only the surfaces the runtime actually draws are required. The pre-rework
            // originals they replaced (…CardBase, …ComparisonTray, …SummaryCard) were
            // dropped from Resources so they stop shipping; ResourceBudgetTests keeps
            // that pruning honest.
            var required = new[]
            {
                "UiEquipmentModalBody",
                "UiEquipmentSlotCardBase",
                "UiEquipmentInventoryCardClean",
                "UiTask13HorizontalPanelClean",
                "UiCollectionModalBody",
                "UiMuseumExhibitCardClean",
                "UiAchievementSummaryCard",
                "UiAchievementRowBase",
                "UiAchievementProgressTrack",
                "UiAchievementProgressFill",
                "ButtonAchievementClaimRuntime",
                "BadgeAchievementComplete",
                "IconAchievementEquipment"
            };

            foreach (var assetName in required)
            {
                Assert.That(
                    textures.Any(texture => texture.name == assetName),
                    Is.True,
                    $"Task 13 asset {assetName} is missing.");
            }

            view.SetTheme(null, null, null, null, null);
            foreach (var slicedName in new[]
            {
                "EquipmentCard",
                "EquipmentSlotPickaxe",
                "EquipmentInventoryRow1",
                "EquipmentCompareSurface",
                "EquipmentPrimary",
                "EquipmentFuse",
                "EquipmentAutoEquip",
                "CollectionCard",
                "CollectionSummarySurface",
                "MuseumNextRewardSurface",
                "MuseumTab",
                "AchievementsTab",
                "ResearchCard",
                "ResearchRow1"
            })
            {
                AssertValidSliced(FindRect(slicedName).GetComponent<Image>(), slicedName);
            }
            AssertValidSliced(
                FindChildRect(FindRect("ResearchRow1"), "ResearchPurchaseButton").GetComponent<Image>(),
                "ResearchPurchaseButton");
            AssertValidSliced(
                FindChildRect(FindRect("EquipmentInventoryRow1"), "RarityOverlay").GetComponent<Image>(),
                "RarityOverlay");
            AssertValidSliced(
                FindChildRect(FindRect("EquipmentInventoryRow1"), "SelectionOverlay").GetComponent<Image>(),
                "SelectionOverlay");

            AssertSimple(FindRect("EquipmentTitleSurface").GetComponent<Image>(), "EquipmentTitleSurface");
            AssertSimple(FindRect("CollectionTitleSurface").GetComponent<Image>(), "CollectionTitleSurface");
            Assert.That(
                FindRect("EquipmentCompareSurface").GetComponent<Image>().sprite.texture.name,
                Is.Not.EqualTo("UiEquipmentComparisonTray"),
                "The baked comparison tray must be replaced by a separated background and dividers.");
            Assert.That(FindRect("ProgressFill").GetComponent<Image>().type, Is.EqualTo(Image.Type.Filled));
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
            Assert.That(FindRect("SettingsIcon").sizeDelta, Is.EqualTo(new Vector2(74f, 74f)));
        }

        [Test]
        public void CorrectedHeaderComparisonResearchAndNavigationZones_DoNotOverlap()
        {
            view.SetTheme(null, null, null, null, null);

            AssertNoOverlap(
                FindRect("EquipmentTitleSurface"),
                FindRect("EquipmentCapacitySurface"),
                "Equipment capacity must sit below, not over, the title plaque.");
            AssertNoOverlap(
                FindRect("EquipmentCompare"),
                FindRect("EquipmentCompareDividerLeft"),
                "Comparison value must not touch the left divider.");
            AssertNoOverlap(
                FindRect("EquipmentCompare"),
                FindRect("EquipmentCompareDividerRight"),
                "Comparison value must not touch the right divider.");
            AssertNoOverlap(
                FindChildRect(FindRect("ResearchRow1"), "ResearchName"),
                FindChildRect(FindRect("ResearchRow1"), "ResearchPurchaseButton"),
                "Research text zone must not enter the purchase button.");
            AssertNoWorldOverlap(
                FindRect("ResearchSummary"),
                FindRect("ResearchRow1"),
                "Research rows must start below the summary and core header zones.");

            var navigation = FindRect("BottomNavigationBar");
            var first = LocalRect(FindChildRect(navigation, "EquipmentNavigation"));
            var last = LocalRect(FindChildRect(navigation, "ShopNavigation"));
            Assert.That(first.xMin, Is.GreaterThanOrEqualTo(navigation.rect.xMin));
            Assert.That(last.xMax, Is.LessThanOrEqualTo(navigation.rect.xMax));
        }

        [Test]
        public void CorrectedModalColumns_RemainSeparatedInEverySupportedLanguage()
        {
            var originalLanguage = LanguageService.Current;
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog);
                var state = service.CreateInitialState(new GameSaveData
                {
                    minerLevel = 63,
                    highestRewardedMinerLevel = 63,
                    blueprintCores = 20
                }, 1f);
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();
                view.SetTheme(null, null, null, null, null);
                FindRect("EquipmentNavigation").GetComponent<Button>().onClick.Invoke();
                FindRect("MuseumNavigation").GetComponent<Button>().onClick.Invoke();
                FindRect("AchievementsTab").GetComponent<Button>().onClick.Invoke();
                FindRect("ResearchNavigation").GetComponent<Button>().onClick.Invoke();

                foreach (var language in new[]
                         {
                             SupportedLanguage.Korean,
                             SupportedLanguage.English,
                             SupportedLanguage.Japanese,
                             SupportedLanguage.ChineseSimplified
                         })
                {
                    LanguageService.SetLanguage(language);
                    Canvas.ForceUpdateCanvases();
                    AssertNoOverlap(
                        FindChildRect(FindRect("AchievementRow1"), "IconFrame"),
                        FindChildRect(FindRect("AchievementRow1"), "AchievementName"),
                        $"Achievement icon/title overlap in {language}.");
                    AssertNoOverlap(
                        FindChildRect(FindRect("AchievementRow1"), "AchievementReward"),
                        FindChildRect(FindRect("AchievementRow1"), "ClaimAchievement"),
                        $"Achievement reward/action overlap in {language}.");
                    AssertNoOverlap(
                        FindChildRect(FindRect("ResearchRow1"), "ResearchName"),
                        FindChildRect(FindRect("ResearchRow1"), "ResearchPurchaseButton"),
                        $"Research text/action overlap in {language}.");
                }
            }
            finally
            {
                LanguageService.SetLanguage(originalLanguage);
                Object.DestroyImmediate(catalog);
            }
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

        private static void AssertValidSliced(Image image, string name)
        {
            Assert.That(image.type, Is.EqualTo(Image.Type.Sliced), $"{name} must use measured 9-slice rendering.");
            Assert.That(image.sprite, Is.Not.Null, $"{name} has no sprite.");
            var border = image.sprite.border;
            Assert.That(border.x + border.z, Is.GreaterThan(0f), $"{name} has no horizontal border.");
            Assert.That(border.y + border.w, Is.GreaterThan(0f), $"{name} has no vertical border.");
            var size = ResolvedRectSize(image.rectTransform);
            Assert.That(size.x, Is.GreaterThanOrEqualTo(border.x + border.z + 16f));
            Assert.That(size.y, Is.GreaterThanOrEqualTo(border.y + border.w + 16f));
        }

        private static Vector2 ResolvedRectSize(RectTransform rectTransform)
        {
            var parent = rectTransform.parent as RectTransform;
            var parentSize = parent != null ? parent.rect.size : Vector2.zero;
            if (parent != null)
            {
                if (parentSize.x <= 0.01f)
                {
                    parentSize.x = Mathf.Abs(parent.sizeDelta.x);
                }

                if (parentSize.y <= 0.01f)
                {
                    parentSize.y = Mathf.Abs(parent.sizeDelta.y);
                }
            }

            var anchorSpan = rectTransform.anchorMax - rectTransform.anchorMin;
            return new Vector2(
                Mathf.Abs(parentSize.x * anchorSpan.x + rectTransform.sizeDelta.x),
                Mathf.Abs(parentSize.y * anchorSpan.y + rectTransform.sizeDelta.y));
        }

        private static Rect LocalRect(RectTransform rect)
        {
            var parent = (RectTransform)rect.parent;
            var parentRect = parent.rect;
            var anchorPoint = parentRect.min + Vector2.Scale(rect.anchorMin, parentRect.size);
            var bottomLeft = anchorPoint + rect.anchoredPosition - Vector2.Scale(rect.pivot, rect.sizeDelta);
            return new Rect(bottomLeft, rect.sizeDelta);
        }

        private static void AssertNoOverlap(RectTransform first, RectTransform second, string message)
        {
            Assert.That(first.parent, Is.EqualTo(second.parent), "Overlap checks require a shared parent.");
            Assert.That(LocalRect(first).Overlaps(LocalRect(second)), Is.False, message);
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        private static void AssertNoWorldOverlap(RectTransform first, RectTransform second, string message)
        {
            Canvas.ForceUpdateCanvases();
            Assert.That(WorldRect(first).Overlaps(WorldRect(second)), Is.False, message);
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
                AssertRect("CreditsCurrencyIcon", new Vector2(-124f, -192f), new Vector2(48f, 48f));
                AssertRect("DepthCurrencyIcon", new Vector2(17.5f, -192f), new Vector2(48f, 48f));
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

            Assert.That(FindRect("PowerValue").GetComponent<Text>().text, Does.Contain("1.9"));
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
                    Assert.That(surface.sizeDelta, Is.EqualTo(new Vector2(246f, 186f)));
                    Assert.That(text.rectTransform.sizeDelta, Is.EqualTo(new Vector2(202f, 50f)));
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
                Assert.That(FindRect("ResearchCard").sizeDelta, Is.EqualTo(new Vector2(920f, 1040f)));
                AssertNoOverlap(
                    FindChildRect(firstRow, "ResearchName"),
                    FindChildRect(firstRow, "ResearchPurchaseButton"),
                    "Research text and purchase action must stay in separate columns.");

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
                Assert.That(card.sizeDelta, Is.EqualTo(new Vector2(920f, 1700f)));
                Assert.That(previous.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)));
                Assert.That(previous.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.5f)));
                Assert.That(firstRow.gameObject.activeSelf, Is.True);
                Assert.That(primary.interactable, Is.True);
                AssertNoOverlap(
                    FindRect("EquipmentTitleSurface"),
                    FindRect("EquipmentCapacitySurface"),
                    "Equipment title and capacity capsule must not collide.");

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
                Assert.That(card.sizeDelta, Is.EqualTo(new Vector2(920f, 1700f)));
                Assert.That(museumRow.gameObject.activeSelf, Is.True);
                Assert.That(museumRow.GetComponent<Image>().type, Is.EqualTo(Image.Type.Sliced));
                var locked = FindChildRect(museumRow, "LockedOverlay");
                AssertNoOverlap(locked, FindChildRect(museumRow, "OreName"), "Museum lock must not cover the name.");
                AssertNoOverlap(locked, FindChildRect(museumRow, "OreDetails"), "Museum lock must not cover details.");

                FindRect("AchievementsTab").GetComponent<Button>().onClick.Invoke();
                var achievementRow = FindRect("AchievementRow1");
                var claim = FindChildRect(achievementRow, "ClaimAchievement").GetComponent<Button>();
                Assert.That(achievementRow.gameObject.activeSelf, Is.True);
                Assert.That(claim.interactable, Is.True);
                AssertNoOverlap(
                    FindChildRect(achievementRow, "IconFrame"),
                    FindChildRect(achievementRow, "AchievementName"),
                    "Achievement icon must not cover the objective title.");
                AssertNoOverlap(
                    FindChildRect(achievementRow, "AchievementReward"),
                    FindChildRect(achievementRow, "ClaimAchievement"),
                    "Achievement reward value must not enter the claim button.");

                claim.onClick.Invoke();

                Assert.That(state.Player.credits, Is.EqualTo(75));
                Assert.That(state.Player.achievementClaims, Has.Length.EqualTo(1));
                Assert.That(state.Player.achievementClaims[0].claimedTiers, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void MissionsNavigation_UsesFourSharedRowsAndFixedActionColumns()
        {
            var now = new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero)
                .ToUnixTimeSeconds();
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog, () => now);
                var state = service.CreateInitialState(new GameSaveData
                {
                    minerLevel = 5,
                    highestRewardedMinerLevel = 5
                }, 1f);
                service.RefreshMissions(state, now);
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();
                view.SetTheme(null, null, null, null, null);

                FindRect("MissionsNavigation").GetComponent<Button>().onClick.Invoke();

                Assert.That(FindRect("MissionsBackdrop").gameObject.activeSelf, Is.True);
                Assert.That(FindRect("MissionsCard").sizeDelta, Is.EqualTo(new Vector2(920f, 1700f)));
                for (var index = 0; index < 4; index++)
                {
                    var row = FindRect($"MissionRow{index}");
                    var icon = FindChildRect(row, "MissionIcon");
                    var name = FindChildRect(row, "MissionName");
                    var reward = FindChildRect(row, "MissionReward");
                    var claim = FindChildRect(row, "MissionClaimButton");
                    Assert.That(row.sizeDelta, Is.EqualTo(new Vector2(824f, 226f)));
                    Assert.That(claim.sizeDelta.y, Is.GreaterThanOrEqualTo(99f));
                    AssertNoOverlap(icon, name, "Mission icon must not cover localized objective text.");
                    AssertNoOverlap(reward, claim, "Mission reward must not enter the fixed action column.");
                }
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void ShopNavigation_UsesSafeColumnsAndSwitchesToWeeklyEvent()
        {
            var now = new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero)
                .ToUnixTimeSeconds();
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            try
            {
                var service = new MiningGameService(catalog, () => now);
                var state = service.CreateInitialState(new GameSaveData
                {
                    minerLevel = 7,
                    highestRewardedMinerLevel = 7,
                    gems = 20,
                    oreCollection = new[]
                    {
                        new OreCollectionData { contentId = "copper", minedCount = 1 }
                    }
                }, 1f);
                service.RefreshCommerce(state, now);
                state.Player.oreCollection[0].minedCount += 50;
                var presenter = new MineHudPresenter(view, service, state);
                presenter.Render();
                view.SetTheme(null, null, null, null, null);

                FindRect("ShopNavigation").GetComponent<Button>().onClick.Invoke();

                Assert.That(FindRect("CommerceBackdrop").gameObject.activeSelf, Is.True);
                Assert.That(FindRect("CommerceCard").sizeDelta, Is.EqualTo(new Vector2(920f, 1700f)));
                for (var index = 0; index < 6; index++)
                {
                    var row = FindRect($"CommerceRow{index}");
                    Assert.That(row.gameObject.activeSelf, Is.True);
                    Assert.That(row.sizeDelta, Is.EqualTo(new Vector2(824f, 160f)));
                    AssertNoOverlap(
                        FindChildRect(row, "CommerceIcon"),
                        FindChildRect(row, "CommerceName"),
                        "Commerce icon must not cover localized product text.");
                    AssertNoOverlap(
                        FindChildRect(row, "CommerceValue"),
                        FindChildRect(row, "CommerceAction"),
                        "Commerce value must not enter the fixed action column.");
                }

                FindChildRect(FindRect("CommerceRow0"), "CommerceAction")
                    .GetComponent<Button>().onClick.Invoke();
                Assert.That(state.Player.credits, Is.EqualTo(125));

                FindRect("EventTab").GetComponent<Button>().onClick.Invoke();
                for (var index = 0; index < 4; index++)
                {
                    Assert.That(FindRect($"CommerceRow{index}").gameObject.activeSelf, Is.True);
                }
                Assert.That(FindRect("CommerceRow4").gameObject.activeSelf, Is.False);
                Assert.That(
                    FindChildRect(FindRect("CommerceRow0"), "CommerceAction")
                        .GetComponent<Button>().interactable,
                    Is.True);
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

        private void AssertMissing(string name)
        {
            Assert.That(
                view.GetComponentsInChildren<Transform>(true).Any(child => child.name == name),
                Is.False,
                $"{name} was intentionally removed from the approved HUD and must not be recreated.");
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
