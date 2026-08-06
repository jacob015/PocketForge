using System;
using System.Collections.Generic;
using System.Linq;
using PocketForge.Ads;
using PocketForge.Content;
using PocketForge.Economy;
using PocketForge.Iap;
using PocketForge.Localization;
using PocketForge.Presentation;
using PocketForge.Progression;
using PocketForge.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PocketForge.Mining
{
    public sealed partial class MineHudView : MonoBehaviour
    {
        // Matches the recessed interior of 12_OreHealthFrame (908x110 art px drawn at
        // 86/174 scale inside the 560x86 rect) so the fill never paints over the rim.
        private const float OreProgressWidth = 449f;
        private const float MinerExperienceProgressWidth = 88f;

        private sealed class ChapterRowView
        {
            public int ChapterNumber;
            public Image Surface;
            public Text Title;
            public Text StageRange;
            public Text Status;
            public Button ActionButton;
        }

        private sealed class ResearchRowView
        {
            public string NodeId;
            public Image Surface;
            public Text Name;
            public Text Level;
            public Text Bonus;
            public Button PurchaseButton;
        }

        private Text creditsText;
        private Text depthText;
        private Text minerRankText;
        private Text minerExperienceText;
        private Image minerExperienceProgress;
        private Button minerRankButton;
        private Image oreProgress;
        private Button mineButton;
        private Button pickaxeButton;
        private Button drillButton;
        private Button robotButton;
        private RawImage pickaxeIcon;
        private RawImage drillIcon;
        private RawImage robotIcon;
        private RawImage mineIcon;
        private Image topSurface;
        private Image upgradeSurface;
        private Image actionSurface;
        private Image progressBackground;
        private Image progressTrack;
        private Text chapterStatusText;
        private Button chapterStatusButton;
        private Text offlineRewardText;
        private Image offlineRewardSurface;
        private Button settingsButton;
        private Button rewardedAdButton;
        private Image creditsCurrencyIcon;
        private Image depthCurrencyIcon;
        private Image settingsIcon;
        private Image rewardedVideoIcon;
        private Image rewardedPlusIcon;
        private GameObject settingsPanel;
        private Text settingsTitle;
        private Text audioLabel;
        private Text musicLabel;
        private Text soundLabel;
        private Text hapticsLabel;
        private Text reduceMotionLabel;
        private Text languageLabel;
        private Text iapStatusText;
        private Slider musicSlider;
        private Slider soundSlider;
        private Button musicMuteButton;
        private Button soundMuteButton;
        private Button hapticsButton;
        private Button reduceMotionButton;
        private Button removeAdsButton;
        private Button restorePurchasesButton;
        private Button closeSettingsButton;
        private Image settingsCard;
        private Image settingsTitleSurface;
        private Image musicSettingIcon;
        private Image soundSettingIcon;
        private Image musicMuteIcon;
        private Image soundMuteIcon;
        private Image hapticsSettingIcon;
        private Image reduceMotionSettingIcon;
        private Image removeAdsIcon;
        private Image restorePurchasesIcon;
        private Image closeSettingsIcon;
        private GameObject chapterCompletePanel;
        private Image chapterCompleteCard;
        private Image chapterCompleteTitleSurface;
        private Image chapterCompleteCreditsRow;
        private Image chapterCompleteGemsRow;
        private Image chapterCompleteCreditsIcon;
        private Image chapterCompleteGemsIcon;
        private Text chapterCompleteTitle;
        private Text chapterCompleteRewardLabel;
        private Text chapterCompleteCreditsValue;
        private Text chapterCompleteGemsValue;
        private Button chapterCompleteContinueButton;
        private GameObject chapterSelectionPanel;
        private Image chapterSelectionCard;
        private Image chapterSelectionTitleSurface;
        private Text chapterSelectionTitle;
        private RectTransform chapterSelectionRowsRoot;
        private Button closeChapterSelectionButton;
        private readonly List<ChapterRowView> chapterRows = new();
        private IReadOnlyList<ChapterSelectionOption> chapterSelectionOptions = Array.Empty<ChapterSelectionOption>();
        private Action<int> chapterSelectionAction;
        private GameObject researchPanel;
        private Image researchCard;
        private Image researchTitleSurface;
        private Text researchTitle;
        private Text researchSummary;
        private Text researchCores;
        private RectTransform researchRowsRoot;
        private Button closeResearchButton;
        private Button closeResearchCornerButton;
        private readonly List<ResearchRowView> researchRows = new();
        private Action<string> researchPurchaseAction;
        private int completedChapterNumber;
        private long completedChapterCredits;
        private long completedChapterGems;
        private long completedChapterBlueprintCores;
        private readonly Dictionary<SupportedLanguage, Button> languageButtons = new();
        private readonly Dictionary<SupportedLanguage, Image> languageIcons = new();
        private readonly List<Image> languageButtonSurfaces = new();
        private readonly List<Image> settingsControlSurfaces = new();
        private readonly List<Image> settingsIconWells = new();
        private readonly List<Image> upgradeActionSurfaces = new();
        private readonly List<Image> upgradeActionIcons = new();
        private readonly List<Image> upgradeCostIcons = new();
        private Image[] pickaxePips;
        private Image[] drillPips;
        private Image[] robotPips;
        private Text feedbackText;
        private Image feedbackSurface;
        private CasualFeedbackText feedbackPopup;
        private PositiveFeedbackBurst positiveFeedback;
        private MiningGameState lastState;
        private MiningGameService lastService;
        private RectTransform safeAreaRoot;
        private Rect appliedSafeArea = new(-1f, -1f, -1f, -1f);
        private Vector2Int appliedScreenSize = new(-1, -1);
        private RewardedAdState rewardedAdState = RewardedAdState.Initializing;
        private long rewardedAdCredits;
        private IapState iapState = IapState.Initializing;
        private string removeAdsPrice = string.Empty;
        private bool adsRemoved;
        private MineUiSkin finalSkin;

        private void OnEnable()
        {
            LanguageService.Changed += RefreshLocalization;
            GameSettingsService.Changed += RenderSettings;
        }

        private void OnDisable()
        {
            LanguageService.Changed -= RefreshLocalization;
            GameSettingsService.Changed -= RenderSettings;
        }

        public static MineHudView Create()
        {
            EnsureEventSystem();
            var canvasObject = new GameObject("MineHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            // Portrait HUD geometry is authored against a stable 1080-wide canvas. Matching
            // width keeps controls equally legible on 16:9, 19.5:9 and 20:9 devices while the
            // additional height becomes breathing room around the world-space ore.
            scaler.matchWidthOrHeight = 0f;

            var view = canvasObject.AddComponent<MineHudView>();
            view.Build();
            return view;
        }

        public void Bind(
            Action mineAction,
            Action<UpgradeType> upgradeAction,
            Action openChapterSelectionAction,
            Action<int> selectChapterAction)
        {
            mineButton.onClick.AddListener(() => mineAction());
            pickaxeButton.onClick.AddListener(() => upgradeAction(UpgradeType.Pickaxe));
            drillButton.onClick.AddListener(() => upgradeAction(UpgradeType.Drill));
            robotButton.onClick.AddListener(() => upgradeAction(UpgradeType.Robot));
            chapterStatusButton.onClick.AddListener(() => openChapterSelectionAction());
            chapterSelectionAction = selectChapterAction;
        }

        public void BindRewardedAd(Action rewardedAdAction)
        {
            rewardedAdButton.onClick.AddListener(() => rewardedAdAction());
        }

        public void BindResearch(Action<string> purchaseAction)
        {
            researchPurchaseAction = purchaseAction;
        }

        public void BindEquipment(
            Action<string> equipAction,
            Action<EquipmentSlot> unequipAction,
            Action<string, EquipmentRarity> fuseAction,
            Action autoEquipAction)
        {
            equipmentEquipAction = equipAction;
            equipmentUnequipAction = unequipAction;
            equipmentFuseAction = fuseAction;
            equipmentAutoEquipAction = autoEquipAction;
        }

        public void BindCollection(Action<string> claimAchievementAction)
        {
            achievementClaimAction = claimAchievementAction;
        }

        public void BindMissions(
            Action<string> claimMissionAction,
            Action<MissionPeriod> claimMissionCompletionAction)
        {
            missionClaimAction = claimMissionAction;
            missionCompletionClaimAction = claimMissionCompletionAction;
        }

        public void BindIap(Action purchaseRemoveAdsAction, Action restorePurchasesAction)
        {
            removeAdsButton.onClick.AddListener(() => purchaseRemoveAdsAction());
            restorePurchasesButton.onClick.AddListener(() => restorePurchasesAction());
        }

        public void SetTheme(Texture upgradeIcons, Texture2D uiKit, Texture2D upgradeButton, Texture2D feedbackPanel, Texture2D hudIcons)
        {
            if (upgradeIcons != null)
            {
                SetIcon(pickaxeIcon, upgradeIcons, 0);
                SetIcon(drillIcon, upgradeIcons, 1);
                SetIcon(robotIcon, upgradeIcons, 2);
                SetIcon(mineIcon, upgradeIcons, 0);
            }

            if (uiKit != null)
            {
                ApplyUiKit(uiKit);
            }

            if (upgradeButton != null)
            {
                ApplyUpgradeButton(upgradeButton);
            }

            if (feedbackPanel != null)
            {
                ApplyFeedbackPanel(feedbackPanel);
            }

            if (hudIcons != null)
            {
                ApplyHudIcons(hudIcons);
            }

            ApplyFinalSkin();
            RenderSettings();
        }

        public void Render(MiningGameState state, MiningGameService service)
        {
            lastState = state;
            lastService = service;
            var player = state.Player;
            var ore = state.Ore;
            creditsText.text = $"<color=#FFFFFF>{CompactNumberFormatter.Format(player.credits)}</color>";
            depthText.text = $"<color=#FFFFFF>{player.stage}</color>";
            var requiredMinerExperience = service.GetRequiredMinerExperience(player.minerLevel);
            var minerExperienceRatio = Mathf.Clamp01(
                player.minerExperience / (float)Mathf.Max(1, requiredMinerExperience));
            minerRankText.text = string.Format(
                LanguageService.Get("miner_rank_short"),
                player.minerLevel);
            minerExperienceText.text =
                $"{CompactNumberFormatter.Format(player.minerExperience)} / {CompactNumberFormatter.Format(requiredMinerExperience)}";
            minerExperienceProgress.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                MinerExperienceProgressWidth * minerExperienceRatio);
            var oreProgressRatio = Mathf.Clamp01(ore.Health / ore.Durability);
            oreProgress.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                oreProgress.type == Image.Type.Filled
                    ? OreProgressWidth
                    : OreProgressWidth * oreProgressRatio);
            if (oreProgress.type == Image.Type.Filled)
            {
                oreProgress.fillAmount = oreProgressRatio;
            }
            oreProgress.color = oreProgress.sprite != null
                ? Color.white
                : ore.IsRare ? new Color(0.52f, 0.83f, 1f) : new Color(0.16f, 0.84f, 1f);
            var chapterStage = ore.Chapter.GetStageNumber(player.stage);
            var chapterPrefix = $"{LanguageService.Get("chapter").ToUpper()} {ore.Chapter.ChapterNumber}";
            var power = service.GetMiningPower(state);
            var requiredPower = service.GetBossRecommendedPower(state);
            var isBossChallengeReady = service.IsBossChallengeReady(state);
            var powerStatus = requiredPower > 0f
                ? $"{CompactNumberFormatter.Format(power.AutoPowerPerSecond)}/{CompactNumberFormatter.Format(requiredPower)}"
                : $"{CompactNumberFormatter.Format(power.AutoPowerPerSecond)}/s";
            chapterStatusText.text = (ore.IsBoss
                ? $"{chapterPrefix}  \u2022  {LanguageService.Get("boss").ToUpper()} {FormatTime(ore.BossTimeRemaining)}  \u2022  {powerStatus}"
                : isBossChallengeReady
                    ? $"{chapterPrefix}  \u2022  {LanguageService.Get("boss_ready").ToUpper()}  \u2022  {powerStatus}"
                    : $"{chapterPrefix}  \u2022  {chapterStage:00}/{ore.Chapter.StagesPerChapter:00}  \u2022  {powerStatus}") + "  \u25BE";
            chapterStatusText.color = ore.IsBoss
                ? new Color(1f, 0.55f, 0.24f)
                : Color.white;
            SetUpgradeText(pickaxeButton, player.pickaxeLevel, service.GetUpgradeCost(UpgradeType.Pickaxe, player.pickaxeLevel));
            SetUpgradeText(drillButton, player.drillLevel, service.GetUpgradeCost(UpgradeType.Drill, player.drillLevel));
            SetUpgradeText(robotButton, player.robotLevel, service.GetUpgradeCost(UpgradeType.Robot, player.robotLevel));
            UpdatePips(pickaxePips, player.pickaxeLevel, new Color(0.28f, 0.72f, 1f));
            UpdatePips(drillPips, player.drillLevel, new Color(0.78f, 0.38f, 1f));
            UpdatePips(robotPips, player.robotLevel, new Color(1f, 0.7f, 0.16f));
            RenderV5Hud(state, service);
            RenderResearch();
            RenderEquipment();
            RenderCollection();
            RenderMissions();
            RenderCommerce();
        }

        public void ShowOfflineReward(OfflineProgressResult result)
        {
            offlineRewardSurface.gameObject.SetActive(true);
            offlineRewardText.gameObject.SetActive(true);
            var duration = FormatOfflineDuration(result.RewardedSeconds);
            offlineRewardText.text = string.Format(
                LanguageService.Get("offline_summary"),
                duration,
                CompactNumberFormatter.Format(result.ProcessedOres),
                CompactNumberFormatter.Format(result.RewardCredits),
                CompactNumberFormatter.Format(result.Progression.ExperienceGained));
            RenderV5OfflineReward(result);
        }

        private static string FormatOfflineDuration(long totalSeconds)
        {
            var safeSeconds = Math.Max(0L, totalSeconds);
            var hours = safeSeconds / 3600L;
            var minutes = safeSeconds % 3600L / 60L;
            if (hours > 0)
            {
                return string.Format(LanguageService.Get("offline_duration_hm"), hours, minutes);
            }

            if (minutes > 0)
            {
                return string.Format(LanguageService.Get("offline_duration_m"), minutes);
            }

            return string.Format(LanguageService.Get("offline_duration_s"), safeSeconds);
        }

        public void ShowChapterComplete(
            int chapterNumber,
            long credits,
            long gems,
            long blueprintCores = 0)
        {
            completedChapterNumber = Mathf.Max(1, chapterNumber);
            completedChapterCredits = Math.Max(0L, credits);
            completedChapterGems = Math.Max(0L, gems);
            completedChapterBlueprintCores = Math.Max(0L, blueprintCores);
            RenderChapterComplete();
            chapterCompletePanel.SetActive(true);
            chapterCompletePanel.transform.SetAsLastSibling();
        }

        public void ShowChapterSelection(IReadOnlyList<ChapterSelectionOption> options)
        {
            chapterSelectionOptions = options ?? Array.Empty<ChapterSelectionOption>();
            EnsureChapterRows(chapterSelectionOptions.Count);
            RenderChapterSelection();
            chapterSelectionPanel.SetActive(true);
            chapterSelectionPanel.transform.SetAsLastSibling();
        }

        public void ShowFeedback(string message, Color color)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            feedbackSurface.gameObject.SetActive(false);
            feedbackPopup.Show();
        }

        private void ShowMinerRankSummary()
        {
            if (lastState == null || lastService == null)
            {
                return;
            }

            var player = lastState.Player;
            if (lastService.IsFeatureUnlocked(player.minerLevel, ProgressionFeature.Research))
            {
                RenderResearch();
                researchPanel.SetActive(true);
                researchPanel.transform.SetAsLastSibling();
                return;
            }

            var powerBonus = (lastService.GetMinerRankPowerMultiplier(player.minerLevel) - 1f) * 100f;
            var message = string.Format(
                LanguageService.Get("miner_rank_summary"),
                player.minerLevel,
                powerBonus);
            if (lastService.TryGetNextFeatureUnlock(player.minerLevel, out var nextUnlock))
            {
                message += "\n" + string.Format(
                    LanguageService.Get("next_unlock"),
                    LanguageService.Get(nextUnlock.Feature.LocalizationKey()),
                    nextUnlock.RequiredLevel);
            }
            else
            {
                message += $"\n{LanguageService.Get("all_features_unlocked")}";
            }

            ShowFeedback(message, new Color(0.45f, 0.9f, 1f));
        }

        public void PlayUpgradeSuccess(UpgradeType type)
        {
            var button = type switch
            {
                UpgradeType.Pickaxe => pickaxeButton,
                UpgradeType.Drill => drillButton,
                _ => robotButton
            };
            var accent = type switch
            {
                UpgradeType.Pickaxe => new Color(0.28f, 0.72f, 1f),
                UpgradeType.Drill => new Color(0.78f, 0.38f, 1f),
                _ => new Color(1f, 0.7f, 0.16f)
            };

            button.GetComponent<MobileButtonFeedback>()?.Celebrate(accent);
            positiveFeedback?.Play(button.GetComponent<RectTransform>(), accent);
        }

        public void SetRewardedAdState(RewardedAdState state, long rewardCredits)
        {
            rewardedAdState = state;
            rewardedAdCredits = rewardCredits;
            RenderRewardedAdState();
            RenderCommerce();
        }

        public void SetIapState(IapState state, string localizedPrice, bool ownsRemoveAds)
        {
            iapState = state;
            removeAdsPrice = localizedPrice ?? string.Empty;
            adsRemoved = ownsRemoveAds;
            RenderIapState();
            RenderCommerce();
        }

        private void LateUpdate() => ApplySafeArea();

        private void RefreshLocalization()
        {
            if (lastState != null && lastService != null)
            {
                Render(lastState, lastService);
            }

            RenderSettings();
            RenderRewardedAdState();
            RenderIapState();
            RenderChapterComplete();
            RenderResearch();
            RenderEquipment();
            RenderCollection();
            if (chapterSelectionPanel != null && chapterSelectionPanel.activeSelf && lastState != null && lastService != null)
            {
                chapterSelectionOptions = lastService.GetChapterSelectionOptions(lastState);
            }
            RenderChapterSelection();
        }

        private void Build()
        {
            safeAreaRoot = CreateSafeAreaRoot(transform);
            ApplySafeArea(true);
            var hudRoot = safeAreaRoot.transform;

            // Final HUD geometry is measured from the approved 853x1844 reference and
            // authored on the stable 1080-wide canvas (scale 1080 / 853).
            topSurface = CreatePanel("TopSurface", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-61f, -123f), new Vector2(826f, 140f), new Color(0.06f, 0.1f, 0.24f, 0.98f));
            upgradeSurface = CreatePanel("UpgradeSurface", hudRoot, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(968f, 470f), Color.clear);
            actionSurface = CreatePanel("ActionSurface", hudRoot, new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.33f), Vector2.zero, new Vector2(560f, 220f), Color.clear);
            upgradeSurface.raycastTarget = false;
            actionSurface.raycastTarget = false;

            minerRankButton = CreateButton("MinerRankButton", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-321f, -123f), new Vector2(260f, 94f), new Color(0.06f, 0.16f, 0.3f, 0.98f));
            minerRankButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            minerRankButton.onClick.AddListener(ShowMinerRankSummary);
            minerRankText = CreateText("MinerRank", minerRankButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 21f), new Vector2(226f, 38f), 26, TextAnchor.MiddleCenter);
            minerRankText.font = UiFontProvider.GetCasual();
            minerExperienceText = CreateText("MinerExperience", minerRankButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -9f), new Vector2(226f, 26f), 17, TextAnchor.MiddleCenter);
            var minerExperienceTrack = CreateSimpleImage("MinerExperienceTrack", minerRankButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -34f), new Vector2(220f, 14f), new Color(0.01f, 0.04f, 0.1f, 0.95f));
            minerExperienceProgress = CreateSimpleImage("MinerExperienceProgress", minerExperienceTrack.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(5f, 0f), new Vector2(MinerExperienceProgressWidth, 8f), new Color(0.12f, 0.84f, 1f));
            minerExperienceProgress.rectTransform.pivot = new Vector2(0f, 0.5f);
            minerExperienceProgress.raycastTarget = false;
            creditsCurrencyIcon = CreateSimpleImage("CreditsCurrencyIcon", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-52f, -123f), new Vector2(52f, 52f), Color.white);
            creditsText = CreateText("Credits", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(31f, -117f), new Vector2(104f, 78f), 38, TextAnchor.MiddleLeft);
            depthCurrencyIcon = CreateSimpleImage("DepthCurrencyIcon", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(151f, -123f), new Vector2(54f, 54f), Color.white);
            depthText = CreateText("Depth", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(238f, -117f), new Vector2(108f, 78f), 38, TextAnchor.MiddleLeft);
            settingsButton = CreateButton("SettingsButton", hudRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92f, -123f), new Vector2(116f, 116f), new Color(0.12f, 0.2f, 0.3f));
            settingsButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            settingsIcon = CreateSimpleImage("SettingsIcon", settingsButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f), Color.white);
            settingsButton.onClick.AddListener(OpenSettings);
            offlineRewardSurface = CreatePanel("OfflineRewardSurface", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-248f, -228f), new Vector2(410f, 82f), new Color(0.04f, 0.2f, 0.16f, 0.96f));
            offlineRewardText = CreateText("OfflineReward", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-248f, -228f), new Vector2(380f, 58f), 24, TextAnchor.MiddleCenter);
            offlineRewardText.color = new Color(0.45f, 0.95f, 0.7f);
            offlineRewardSurface.gameObject.SetActive(false);
            offlineRewardText.gameObject.SetActive(false);
            rewardedAdButton = CreateButton("RewardedAdButton", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(263f, -254f), new Vector2(400f, 88f), new Color(0.08f, 0.16f, 0.34f));
            var rewardedLabel = rewardedAdButton.GetComponentInChildren<Text>();
            rewardedLabel.fontSize = 30;
            rewardedLabel.rectTransform.offsetMin = new Vector2(76f, 0f);
            rewardedLabel.rectTransform.offsetMax = new Vector2(-66f, 0f);
            rewardedVideoIcon = CreateSimpleImage("VideoIcon", rewardedAdButton.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(66f, 0f), new Vector2(84f, -4f), Color.white);
            rewardedPlusIcon = CreateSimpleImage("PlusIcon", rewardedAdButton.transform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-65f, 0f), new Vector2(72f, -8f), Color.white);
            rewardedPlusIcon.rectTransform.localScale = new Vector3(0.8f, 0.8f, 1f);
            RenderRewardedAdState();
            feedbackSurface = CreatePanel("ActionFeedbackSurface", hudRoot, new Vector2(0.5f, 0.51f), new Vector2(0.5f, 0.51f), Vector2.zero, new Vector2(540f, 88f), Color.clear);
            feedbackSurface.raycastTarget = false;
            feedbackSurface.GetComponent<Shadow>().enabled = false;
            feedbackText = CreateText("ActionFeedback", hudRoot, new Vector2(0.5f, 0.51f), new Vector2(0.5f, 0.51f), Vector2.zero, new Vector2(680f, 106f), 46, TextAnchor.MiddleCenter);
            feedbackText.font = UiFontProvider.GetCasual();
            feedbackText.resizeTextForBestFit = true;
            feedbackText.resizeTextMinSize = 25;
            feedbackText.resizeTextMaxSize = 46;
            feedbackText.horizontalOverflow = HorizontalWrapMode.Overflow;
            feedbackText.verticalOverflow = VerticalWrapMode.Overflow;
            feedbackText.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -1.5f);
            var feedbackShadow = feedbackText.GetComponent<Shadow>();
            feedbackShadow.effectColor = new Color(0.08f, 0.06f, 0.05f, 0.9f);
            feedbackShadow.effectDistance = new Vector2(2f, -7f);
            var feedbackOutline = feedbackText.gameObject.AddComponent<Outline>();
            feedbackOutline.effectColor = new Color(0.12f, 0.08f, 0.05f, 0.96f);
            feedbackOutline.effectDistance = new Vector2(3.5f, -3.5f);
            feedbackOutline.useGraphicAlpha = true;
            feedbackPopup = feedbackText.gameObject.AddComponent<CasualFeedbackText>();
            feedbackSurface.gameObject.SetActive(false);
            feedbackText.gameObject.SetActive(false);

            progressBackground = CreatePanel("ProgressBackground", hudRoot, new Vector2(0.5f, 0.405f), new Vector2(0.5f, 0.405f), new Vector2(0f, 29f), new Vector2(638f, 78f), new Color(0.05f, 0.1f, 0.24f, 0.98f));
            chapterStatusText = CreateText("ChapterStatus", progressBackground.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 66f), new Vector2(600f, 52f), 30, TextAnchor.MiddleCenter);
            chapterStatusText.font = UiFontProvider.GetCasual();
            chapterStatusText.resizeTextForBestFit = true;
            chapterStatusText.resizeTextMinSize = 21;
            chapterStatusText.resizeTextMaxSize = 30;
            chapterStatusButton = chapterStatusText.gameObject.AddComponent<Button>();
            chapterStatusButton.targetGraphic = chapterStatusText;
            chapterStatusButton.transition = Selectable.Transition.None;
            progressTrack = CreateSimpleImage("ProgressTrack", progressBackground.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(604f, 50f), new Color(0.015f, 0.04f, 0.11f, 1f));
            oreProgress = CreatePanel("ProgressFill", progressTrack.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(OreProgressWidth, 36f), Color.white);
            oreProgress.GetComponent<Shadow>().enabled = false;
            oreProgress.rectTransform.pivot = new Vector2(0f, 0.5f);
            oreProgress.type = Image.Type.Simple;
            oreProgress.raycastTarget = false;

            mineButton = CreateButton("MineButton", hudRoot, new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.33f), new Vector2(0f, 14f), new Vector2(504f, 232f), new Color(1f, 0.48f, 0.12f));
            mineButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            mineIcon = CreateIcon("MineIcon", mineButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 13f), new Vector2(158f, 158f));
            pickaxeButton = CreateButton("PickaxeButton", hudRoot, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(-320f, -6f), new Vector2(300f, 460f), new Color(0.14f, 0.3f, 0.52f));
            drillButton = CreateButton("DrillButton", hudRoot, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(0f, -6f), new Vector2(300f, 460f), new Color(0.14f, 0.3f, 0.52f));
            robotButton = CreateButton("RobotButton", hudRoot, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(320f, -6f), new Vector2(300f, 460f), new Color(0.14f, 0.3f, 0.52f));
            pickaxeIcon = CreateIcon("PickaxeIcon", pickaxeButton.transform);
            drillIcon = CreateIcon("DrillIcon", drillButton.transform);
            robotIcon = CreateIcon("RobotIcon", robotButton.transform);
            ConfigureUpgradeCard(pickaxeButton);
            ConfigureUpgradeCard(drillButton);
            ConfigureUpgradeCard(robotButton);
            pickaxePips = CreateUpgradeDetails(pickaxeButton.transform, new Color(0.28f, 0.72f, 1f));
            drillPips = CreateUpgradeDetails(drillButton.transform, new Color(0.78f, 0.38f, 1f));
            robotPips = CreateUpgradeDetails(robotButton.transform, new Color(1f, 0.7f, 0.16f));
            BuildV5Hud(hudRoot);
            positiveFeedback = safeAreaRoot.gameObject.AddComponent<PositiveFeedbackBurst>();
            positiveFeedback.Initialize(safeAreaRoot);
            CreateSettingsPanel();
            CreateChapterCompletePanel();
            CreateChapterSelectionPanel();
            CreateResearchPanel();
            CreateEquipmentPanel();
            CreateCollectionPanel();
            CreateMissionsPanel();
            CreateCommercePanel();
            RenderSettings();
        }

        private void ApplyUiKit(Texture2D atlas)
        {
            var panelSprite = CreateAtlasSprite(atlas, new Rect(0.035f, 0.748f, 0.448f, 0.158f), new Vector4(0.08f, 0.24f, 0.08f, 0.24f));
            var buttonSprite = CreateAtlasSprite(atlas, new Rect(0.518f, 0.735f, 0.447f, 0.177f), new Vector4(0.08f, 0.25f, 0.08f, 0.25f));
            var cardSprite = CreateAtlasSprite(atlas, new Rect(0.124f, 0.312f, 0.264f, 0.359f), new Vector4(0.16f, 0.10f, 0.16f, 0.10f));
            var coinSprite = CreateAtlasSprite(atlas, new Rect(0.159f, 0.061f, 0.185f, 0.205f), Vector4.zero);

            ApplySlicedSprite(topSurface, panelSprite, Color.white);
            upgradeSurface.sprite = null;
            upgradeSurface.color = Color.clear;
            actionSurface.color = Color.clear;
            ApplySlicedSprite(progressBackground, panelSprite, new Color(0.72f, 0.82f, 1f, 0.96f));
            ApplySlicedSprite(offlineRewardSurface, panelSprite, new Color(0.55f, 1f, 0.72f, 0.98f));
            creditsCurrencyIcon.sprite = coinSprite;
            creditsCurrencyIcon.type = Image.Type.Simple;
            ApplySlicedSprite(mineButton.image, buttonSprite, Color.white);
            ApplySlicedSprite(pickaxeButton.image, cardSprite, Color.white);
            ApplySlicedSprite(drillButton.image, cardSprite, Color.white);
            ApplySlicedSprite(robotButton.image, cardSprite, Color.white);

            mineButton.GetComponent<Outline>().enabled = false;
            pickaxeButton.GetComponent<Outline>().enabled = false;
            drillButton.GetComponent<Outline>().enabled = false;
            robotButton.GetComponent<Outline>().enabled = false;
            ApplySlicedSprite(settingsButton.image, cardSprite, new Color(0.74f, 0.84f, 1f));
            ApplySlicedSprite(rewardedAdButton.image, panelSprite, Color.white);
            rewardedAdButton.GetComponent<Outline>().enabled = false;
            foreach (var costIcon in upgradeCostIcons)
            {
                costIcon.sprite = coinSprite;
                costIcon.type = Image.Type.Simple;
            }
            ApplyStretchedSimpleSprite(settingsCard, cardSprite, Color.white);
            foreach (var surface in languageButtonSurfaces)
            {
                ApplyStretchedSimpleSprite(surface, panelSprite, Color.white);
            }

            foreach (var surface in settingsControlSurfaces)
            {
                ApplyStretchedSimpleSprite(surface, panelSprite, Color.white);
            }

            ApplyStretchedSimpleSprite(closeSettingsButton.image, buttonSprite, Color.white);
        }

        private void ApplyHudIcons(Texture2D atlas)
        {
            var gear = CreateAtlasSprite(atlas, new Rect(0f, 0.5f, 0.5f, 0.5f), Vector4.zero);
            var plus = CreateAtlasSprite(atlas, new Rect(0.5f, 0.5f, 0.5f, 0.5f), Vector4.zero);
            var video = CreateAtlasSprite(atlas, new Rect(0f, 0f, 0.5f, 0.5f), Vector4.zero);
            var crystal = CreateAtlasSprite(atlas, new Rect(0.5f, 0f, 0.5f, 0.5f), Vector4.zero);

            ApplySimpleSprite(settingsIcon, gear);
            ApplySimpleSprite(rewardedPlusIcon, plus);
            ApplySimpleSprite(rewardedVideoIcon, video);
            ApplySimpleSprite(depthCurrencyIcon, crystal);
        }

        private void ApplyFinalSkin()
        {
            finalSkin = MineUiSkin.Load();
            if (finalSkin == null)
            {
                Debug.LogWarning("Pocket Forge final UI skin could not be loaded. Keeping the fallback HUD theme.");
                return;
            }

            ApplyStretchedSimpleSprite(topSurface, finalSkin.Simple("HudHeader"), Color.white);
            ApplyStretchedSimpleSprite(minerRankButton.image, finalSkin.Simple("HudCounterSlot"), Color.white);
            ApplyStretchedSimpleSprite(settingsButton.image, finalSkin.Simple("HudSettingsButton"), Color.white);
            ApplyStretchedSimpleSprite(rewardedAdButton.image, finalSkin.Simple("HudRewardPill"), Color.white);
            ApplyStretchedSimpleSprite(progressBackground, finalSkin.Simple("HudOreHealthFrame"), Color.white);
            ApplyStretchedSimpleSprite(progressTrack, finalSkin.Simple("HudOreHealthTrack"), Color.white);
            ApplyStretchedSimpleSprite(oreProgress, finalSkin.Simple("HudOreHealthFill"), Color.white);
            ApplyStretchedSimpleSprite(mineButton.image, finalSkin.Simple("HudMineButton"), Color.white);

            ApplyStretchedSimpleSprite(pickaxeButton.image, finalSkin.Simple("HudUpgradeCard"), Color.white);
            ApplyStretchedSimpleSprite(drillButton.image, finalSkin.Simple("HudUpgradeCard"), Color.white);
            ApplyStretchedSimpleSprite(robotButton.image, finalSkin.Simple("HudUpgradeCard"), Color.white);
            foreach (var surface in upgradeActionSurfaces)
            {
                ApplyStretchedSimpleSprite(surface, finalSkin.Simple("HudUpgradeButton"), Color.white);
                surface.GetComponent<Shadow>().enabled = false;
            }

            foreach (var pip in pickaxePips.Concat(drillPips).Concat(robotPips))
            {
                ApplyStretchedSimpleSprite(pip, finalSkin.Simple("HudLevelPip"), pip.color);
            }

            foreach (var icon in upgradeActionIcons)
            {
                ApplySimpleSprite(icon, finalSkin.Simple("IconUpgradeArrow"));
            }

            ApplySimpleSprite(creditsCurrencyIcon, finalSkin.Simple("IconGoldCoin"));
            ApplySimpleSprite(depthCurrencyIcon, finalSkin.Simple("IconPurpleGem"));
            ApplySimpleSprite(settingsIcon, finalSkin.Simple("IconGear"));
            ApplySimpleSprite(rewardedVideoIcon, finalSkin.Simple("IconVideo"));
            ApplySimpleSprite(rewardedPlusIcon, finalSkin.Simple("IconPlus"));
            ApplyRawTexture(mineIcon, finalSkin.Texture("IconPickaxe"), new Vector2(178f, 158f), 8f);
            ApplyRawTexture(pickaxeIcon, finalSkin.Texture("IconPickaxe"), new Vector2(230f, 204f), 8f);
            ApplyRawTexture(drillIcon, finalSkin.Texture("IconDrill"), new Vector2(230f, 204f));
            ApplyRawTexture(robotIcon, finalSkin.Texture("IconRobot"), new Vector2(230f, 204f));

            var costAssets = new[] { "IconCyanCrystal", "IconPurpleGem", "IconGoldBadge" };
            for (var index = 0; index < upgradeCostIcons.Count && index < costAssets.Length; index++)
            {
                ApplySimpleSprite(upgradeCostIcons[index], finalSkin.Simple(costAssets[index]));
            }

            ApplyStretchedSimpleSprite(settingsCard, finalSkin.Simple("SettingsModal"), Color.white);
            ApplyStretchedSimpleSprite(settingsTitleSurface, finalSkin.Simple("SettingsTitlePlaque"), Color.white);
            foreach (var surface in settingsControlSurfaces)
            {
                ApplyStretchedSimpleSprite(surface, finalSkin.Simple("SettingsRow"), Color.white);
            }

            foreach (var well in settingsIconWells)
            {
                ApplyStretchedSimpleSprite(well, finalSkin.Simple("SettingsIconWell"), Color.white);
            }

            ApplySettingSliderSkin(musicSlider);
            ApplySettingSliderSkin(soundSlider);
            ApplyStretchedSimpleSprite(musicMuteButton.image, finalSkin.Simple("SettingsIconButton"), Color.white);
            ApplyStretchedSimpleSprite(soundMuteButton.image, finalSkin.Simple("SettingsIconButton"), Color.white);

            ApplySimpleSprite(musicSettingIcon, finalSkin.Simple("IconMusic"));
            ApplySimpleSprite(soundSettingIcon, finalSkin.Simple("IconSound"));
            ApplySimpleSprite(musicMuteIcon, finalSkin.Simple("IconMusic"));
            ApplySimpleSprite(soundMuteIcon, finalSkin.Simple("IconSound"));
            ApplySimpleSprite(hapticsSettingIcon, finalSkin.Simple("IconHaptics"));
            ApplySimpleSprite(reduceMotionSettingIcon, finalSkin.Simple("IconReduceMotion"));

            ApplyStretchedSimpleSprite(removeAdsButton.image, finalSkin.Simple("SettingsActionButton"), Color.white);
            ApplyStretchedSimpleSprite(restorePurchasesButton.image, finalSkin.Simple("SettingsActionButton"), Color.white);
            ApplyStretchedSimpleSprite(closeSettingsButton.image, finalSkin.Simple("SettingsCloseButton"), Color.white);
            ApplySimpleSprite(removeAdsIcon, finalSkin.Simple("IconAdsOff"));
            ApplySimpleSprite(restorePurchasesIcon, finalSkin.Simple("IconRestore"));
            ApplySimpleSprite(closeSettingsIcon, finalSkin.Simple("IconClose"));

            ApplyStretchedSimpleSprite(chapterCompleteCard, finalSkin.Simple("SettingsModal"), Color.white);
            ApplyStretchedSimpleSprite(chapterCompleteTitleSurface, finalSkin.Simple("SettingsTitlePlaque"), Color.white);
            ApplyStretchedSimpleSprite(chapterCompleteCreditsRow, finalSkin.Simple("SettingsRow"), Color.white);
            ApplyStretchedSimpleSprite(chapterCompleteGemsRow, finalSkin.Simple("SettingsRow"), Color.white);
            ApplyStretchedSimpleSprite(chapterCompleteContinueButton.image, finalSkin.Simple("SettingsActionButton"), Color.white);
            ApplySimpleSprite(chapterCompleteCreditsIcon, finalSkin.Simple("IconGoldCoin"));
            ApplySimpleSprite(chapterCompleteGemsIcon, finalSkin.Simple("IconPurpleGem"));
            ApplyStretchedSimpleSprite(chapterSelectionCard, finalSkin.Simple("SettingsModal"), Color.white);
            ApplyStretchedSimpleSprite(chapterSelectionTitleSurface, finalSkin.Simple("SettingsTitlePlaque"), Color.white);
            ApplyStretchedSimpleSprite(closeChapterSelectionButton.image, finalSkin.Simple("SettingsCloseButton"), Color.white);
            foreach (var row in chapterRows)
            {
                ApplyChapterRowSkin(row);
            }

            ApplyBorderedSprite(
                researchCard,
                finalSkin.Task13Sliced("UiCollectionModalBody"),
                Color.white);
            ApplySimpleSprite(researchTitleSurface, finalSkin.Task13Simple("UiCollectionTitlePlaque"));
            ApplyBorderedSprite(
                closeResearchButton.image,
                finalSkin.Task13Sliced("ButtonAchievementClaimRuntime"),
                Color.white);
            ApplySimpleSprite(
                closeResearchCornerButton.image,
                finalSkin.Task13Simple("UiModalCloseButtonSurface"));
            var researchCloseIcon = closeResearchCornerButton.transform.Find("Icon")?.GetComponent<Image>();
            if (researchCloseIcon != null)
            {
                ApplySimpleSprite(researchCloseIcon, finalSkin.Task13Simple("IconCloseX"));
            }
            foreach (var row in researchRows)
            {
                ApplyResearchRowSkin(row);
            }

            ApplyLanguageIcon(SupportedLanguage.Korean, "IconFlagKorean");
            ApplyLanguageIcon(SupportedLanguage.English, "IconFlagEnglish");
            ApplyLanguageIcon(SupportedLanguage.Japanese, "IconFlagJapanese");
            ApplyLanguageIcon(SupportedLanguage.ChineseSimplified, "IconFlagChinese");
            ApplyV5HudSkin();
            ApplyEquipmentSkin();
            ApplyCollectionSkin();
            ApplyMissionSkin();
            ApplyCommerceSkin();

            DisableOutline(mineButton);
            DisableOutline(pickaxeButton);
            DisableOutline(drillButton);
            DisableOutline(robotButton);
            DisableOutline(settingsButton);
            DisableOutline(rewardedAdButton);
            DisableOutline(removeAdsButton);
            DisableOutline(restorePurchasesButton);
            DisableOutline(closeSettingsButton);
            DisableOutline(chapterCompleteContinueButton);
            DisableOutline(closeChapterSelectionButton);
            DisableOutline(closeResearchButton);
            DisableOutline(closeResearchCornerButton);
        }

        private void ApplyChapterRowSkin(ChapterRowView row)
        {
            if (finalSkin == null || row == null)
            {
                return;
            }

            ApplyStretchedSimpleSprite(row.Surface, finalSkin.Simple("SettingsRow"), Color.white);
            ApplyStretchedSimpleSprite(row.ActionButton.image, finalSkin.Simple("SettingsActionButton"), Color.white);
            DisableOutline(row.ActionButton);
        }

        private void ApplyResearchRowSkin(ResearchRowView row)
        {
            if (finalSkin == null || row == null)
            {
                return;
            }

            ApplyBorderedSprite(
                row.Surface,
                finalSkin.Task13Sliced("UiTask13HorizontalPanelClean"),
                Color.white);
            // Gold pill is the shared primary-action surface across every modal.
            ApplyBorderedSprite(
                row.PurchaseButton.image,
                finalSkin.Task13Sliced("ButtonAchievementClaimRuntime"),
                Color.white);
            DisableOutline(row.PurchaseButton);
        }

        private void ApplySettingSliderSkin(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            var background = slider.transform.Find("Background")?.GetComponent<Image>();
            var handle = slider.handleRect != null ? slider.handleRect.GetComponent<Image>() : null;
            if (background != null)
            {
                ApplyStretchedSimpleSprite(background, finalSkin.Simple("SettingsSliderTrack"), Color.white);
            }

            if (handle != null)
            {
                ApplySimpleSprite(handle, finalSkin.Simple("SettingsSliderKnob"));
            }
        }

        private void ApplyLanguageIcon(SupportedLanguage language, string assetName)
        {
            if (languageIcons.TryGetValue(language, out var icon))
            {
                ApplySimpleSprite(icon, finalSkin.Simple(assetName));
            }
        }

        private static void ApplyRawTexture(RawImage image, Texture texture, Vector2 fitBounds, float topCropPixels = 0f)
        {
            if (image == null || texture == null)
            {
                return;
            }

            var crop = Mathf.Clamp(topCropPixels / texture.height, 0f, 0.25f);
            image.texture = texture;
            image.uvRect = new Rect(0f, 0f, 1f, 1f - crop);
            image.color = Color.white;

            var sourceAspect = texture.width / (texture.height * (1f - crop));
            var width = fitBounds.x;
            var height = width / sourceAspect;
            if (height > fitBounds.y)
            {
                height = fitBounds.y;
                width = height * sourceAspect;
            }

            image.rectTransform.sizeDelta = new Vector2(Mathf.Round(width), Mathf.Round(height));
        }

        private static void DisableOutline(Button button)
        {
            var outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
        }

        private static void ApplySimpleSprite(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
        }

        private static void ApplyStretchedSimpleSprite(Image image, Sprite sprite, Color color)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.color = color;
        }

        private static void ApplyBorderedSprite(
            Image image,
            Sprite sprite,
            Color color,
            float minimumCenterWidth = 16f,
            float minimumCenterHeight = 16f)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            var border = sprite.border;
            var hasHorizontalBorder = border.x > 0f || border.z > 0f;
            var hasVerticalBorder = border.y > 0f || border.w > 0f;
            Debug.Assert(
                hasHorizontalBorder && hasVerticalBorder,
                $"{image.name} cannot use Image.Type.Sliced because {sprite.name} has a zero border.");

            var size = ResolveRectSize(image.rectTransform);
            var width = size.x;
            var height = size.y;
            Debug.Assert(
                width + 0.01f >= border.x + border.z + minimumCenterWidth,
                $"{image.name} width {width:0.#} is smaller than the sliced minimum " +
                $"{border.x + border.z + minimumCenterWidth:0.#}.");
            Debug.Assert(
                height + 0.01f >= border.y + border.w + minimumCenterHeight,
                $"{image.name} height {height:0.#} is smaller than the sliced minimum " +
                $"{border.y + border.w + minimumCenterHeight:0.#}.");

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.fillCenter = true;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = color;
        }

        private static Vector2 ResolveRectSize(RectTransform rectTransform)
        {
            var rectSize = rectTransform.rect.size;
            if (rectSize.x > 0.01f && rectSize.y > 0.01f)
            {
                return new Vector2(Mathf.Abs(rectSize.x), Mathf.Abs(rectSize.y));
            }

            var parentRect = rectTransform.parent as RectTransform;
            var parentSize = parentRect != null ? parentRect.rect.size : Vector2.zero;
            if (parentRect != null)
            {
                if (parentSize.x <= 0.01f)
                {
                    parentSize.x = Mathf.Abs(parentRect.sizeDelta.x);
                }

                if (parentSize.y <= 0.01f)
                {
                    parentSize.y = Mathf.Abs(parentRect.sizeDelta.y);
                }
            }

            var anchorSpan = rectTransform.anchorMax - rectTransform.anchorMin;
            return new Vector2(
                Mathf.Abs(parentSize.x * anchorSpan.x + rectTransform.sizeDelta.x),
                Mathf.Abs(parentSize.y * anchorSpan.y + rectTransform.sizeDelta.y));
        }

        private void ApplyUpgradeButton(Texture2D texture)
        {
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            foreach (var surface in upgradeActionSurfaces)
            {
                surface.sprite = sprite;
                surface.type = Image.Type.Simple;
                surface.color = Color.white;
                surface.GetComponent<Shadow>().enabled = false;
            }
        }

        private void ApplyFeedbackPanel(Texture2D texture)
        {
            // Feedback is intentionally text-only. Keep this compatibility hook so old theme
            // callers do not need to change while preventing the legacy panel from reappearing.
            feedbackSurface.sprite = null;
            feedbackSurface.color = Color.clear;
            feedbackSurface.gameObject.SetActive(false);
        }

        private void CreateChapterCompletePanel()
        {
            var backdrop = CreatePanel(
                "ChapterCompleteBackdrop",
                transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.005f, 0.012f, 0.04f, 0.82f));
            chapterCompletePanel = backdrop.gameObject;
            chapterCompleteCard = CreatePanel(
                "ChapterCompleteCard",
                chapterCompletePanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 24f),
                new Vector2(780f, 720f),
                new Color(0.035f, 0.075f, 0.15f, 0.99f));
            chapterCompleteTitleSurface = CreatePanel(
                "ChapterCompleteTitleSurface",
                chapterCompleteCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -42f),
                new Vector2(620f, 132f),
                Color.white);
            chapterCompleteTitle = CreateText(
                "ChapterCompleteTitle",
                chapterCompleteTitleSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-44f, -18f),
                44,
                TextAnchor.MiddleCenter);
            chapterCompleteTitle.font = UiFontProvider.GetCasual();
            chapterCompleteTitle.resizeTextForBestFit = true;
            chapterCompleteTitle.resizeTextMinSize = 24;
            chapterCompleteTitle.resizeTextMaxSize = 44;

            chapterCompleteRewardLabel = CreateText(
                "ChapterCompleteRewardLabel",
                chapterCompleteCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -176f),
                new Vector2(620f, 54f),
                30,
                TextAnchor.MiddleCenter);
            chapterCompleteRewardLabel.font = UiFontProvider.GetCasual();
            chapterCompleteRewardLabel.color = new Color(1f, 0.82f, 0.3f);

            chapterCompleteCreditsRow = CreatePanel(
                "ChapterCompleteCreditsRow",
                chapterCompleteCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 76f),
                new Vector2(580f, 112f),
                new Color(0.055f, 0.12f, 0.23f, 0.98f));
            chapterCompleteCreditsIcon = CreateSimpleImage(
                "ChapterCompleteCreditsIcon",
                chapterCompleteCreditsRow.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-176f, 0f),
                new Vector2(72f, 72f),
                Color.white);
            chapterCompleteCreditsValue = CreateText(
                "ChapterCompleteCreditsValue",
                chapterCompleteCreditsRow.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(48f, 0f),
                new Vector2(300f, 74f),
                38,
                TextAnchor.MiddleCenter);

            chapterCompleteGemsRow = CreatePanel(
                "ChapterCompleteGemsRow",
                chapterCompleteCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -62f),
                new Vector2(580f, 112f),
                new Color(0.055f, 0.12f, 0.23f, 0.98f));
            chapterCompleteGemsIcon = CreateSimpleImage(
                "ChapterCompleteGemsIcon",
                chapterCompleteGemsRow.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-176f, 0f),
                new Vector2(72f, 72f),
                Color.white);
            chapterCompleteGemsValue = CreateText(
                "ChapterCompleteGemsValue",
                chapterCompleteGemsRow.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(48f, 0f),
                new Vector2(300f, 74f),
                38,
                TextAnchor.MiddleCenter);

            chapterCompleteContinueButton = CreateButton(
                "ChapterCompleteContinueButton",
                chapterCompleteCard.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 88f),
                new Vector2(420f, 104f),
                new Color(1f, 0.48f, 0.12f));
            var continueLabel = chapterCompleteContinueButton.GetComponentInChildren<Text>();
            continueLabel.font = UiFontProvider.GetCasual();
            continueLabel.fontSize = 34;
            chapterCompleteContinueButton.onClick.AddListener(CloseChapterComplete);
            chapterCompletePanel.SetActive(false);
        }

        private void RenderChapterComplete()
        {
            if (chapterCompletePanel == null || completedChapterNumber <= 0)
            {
                return;
            }

            chapterCompleteTitle.text = string.Format(
                LanguageService.Get("chapter_clear"),
                completedChapterNumber).ToUpper();
            chapterCompleteRewardLabel.text = LanguageService.Get("first_clear_reward").ToUpper();
            if (completedChapterBlueprintCores > 0)
            {
                chapterCompleteRewardLabel.text +=
                    $"  \u2022  +{CompactNumberFormatter.Format(completedChapterBlueprintCores)} {LanguageService.Get("blueprint_core").ToUpper()}";
            }

            chapterCompleteCreditsValue.text =
                $"+{CompactNumberFormatter.Format(completedChapterCredits)}";
            chapterCompleteGemsValue.text =
                $"+{CompactNumberFormatter.Format(completedChapterGems)}";
            chapterCompleteContinueButton.GetComponentInChildren<Text>().text = LanguageService.Get("continue").ToUpper();
        }

        private void CloseChapterComplete()
        {
            chapterCompletePanel.SetActive(false);
        }

        private void CreateChapterSelectionPanel()
        {
            var backdrop = CreatePanel(
                "ChapterSelectionBackdrop",
                transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.005f, 0.012f, 0.04f, 0.82f));
            chapterSelectionPanel = backdrop.gameObject;
            chapterSelectionCard = CreatePanel(
                "ChapterSelectionCard",
                chapterSelectionPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 24f),
                new Vector2(820f, 900f),
                new Color(0.035f, 0.075f, 0.15f, 0.99f));
            chapterSelectionTitleSurface = CreatePanel(
                "ChapterSelectionTitleSurface",
                chapterSelectionCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -42f),
                new Vector2(650f, 132f),
                Color.white);
            chapterSelectionTitle = CreateText(
                "ChapterSelectionTitle",
                chapterSelectionTitleSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-44f, -18f),
                44,
                TextAnchor.MiddleCenter);
            chapterSelectionTitle.font = UiFontProvider.GetCasual();
            chapterSelectionTitle.resizeTextForBestFit = true;
            chapterSelectionTitle.resizeTextMinSize = 24;
            chapterSelectionTitle.resizeTextMaxSize = 44;

            var rowsRoot = CreatePanel(
                "ChapterSelectionRows",
                chapterSelectionCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -12f),
                new Vector2(700f, 620f),
                Color.clear);
            rowsRoot.raycastTarget = false;
            rowsRoot.GetComponent<Shadow>().enabled = false;
            chapterSelectionRowsRoot = rowsRoot.rectTransform;

            closeChapterSelectionButton = CreateButton(
                "CloseChapterSelectionButton",
                chapterSelectionCard.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 68f),
                new Vector2(360f, 92f),
                new Color(0.95f, 0.47f, 0.08f));
            var closeLabel = closeChapterSelectionButton.GetComponentInChildren<Text>();
            closeLabel.font = UiFontProvider.GetCasual();
            closeLabel.fontSize = 30;
            closeChapterSelectionButton.onClick.AddListener(CloseChapterSelection);
            chapterSelectionPanel.SetActive(false);
        }

        private void EnsureChapterRows(int count)
        {
            while (chapterRows.Count < count)
            {
                chapterRows.Add(CreateChapterRow(chapterRows.Count));
            }

            for (var index = 0; index < chapterRows.Count; index++)
            {
                chapterRows[index].Surface.gameObject.SetActive(index < count);
            }
        }

        private ChapterRowView CreateChapterRow(int index)
        {
            var row = new ChapterRowView();
            row.Surface = CreatePanel(
                $"ChapterRow{index + 1}",
                chapterSelectionRowsRoot,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 196f - index * 190f),
                new Vector2(680f, 160f),
                new Color(0.055f, 0.12f, 0.23f, 0.98f));
            row.Title = CreateText(
                "ChapterTitle",
                row.Surface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-175f, 28f),
                new Vector2(280f, 54f),
                32,
                TextAnchor.MiddleLeft);
            row.Title.font = UiFontProvider.GetCasual();
            row.StageRange = CreateText(
                "StageRange",
                row.Surface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-175f, -29f),
                new Vector2(300f, 42f),
                21,
                TextAnchor.MiddleLeft);
            row.StageRange.color = new Color(0.68f, 0.78f, 0.92f);
            row.Status = CreateText(
                "ChapterState",
                row.Surface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(42f, 0f),
                new Vector2(150f, 52f),
                22,
                TextAnchor.MiddleCenter);
            row.Status.font = UiFontProvider.GetCasual();
            row.Status.resizeTextForBestFit = true;
            row.Status.resizeTextMinSize = 15;
            row.Status.resizeTextMaxSize = 22;
            row.ActionButton = CreateButton(
                "ChapterActionButton",
                row.Surface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(236f, 0f),
                new Vector2(190f, 86f),
                new Color(0.2f, 0.62f, 0.28f));
            var actionLabel = row.ActionButton.GetComponentInChildren<Text>();
            actionLabel.font = UiFontProvider.GetCasual();
            actionLabel.fontSize = 24;
            ApplyChapterRowSkin(row);
            return row;
        }

        private void RenderChapterSelection()
        {
            if (chapterSelectionPanel == null)
            {
                return;
            }

            chapterSelectionTitle.text = LanguageService.Get("chapter_select").ToUpper();
            closeChapterSelectionButton.GetComponentInChildren<Text>().text = LanguageService.Get("close").ToUpper();
            EnsureChapterRows(chapterSelectionOptions.Count);
            for (var index = 0; index < chapterSelectionOptions.Count; index++)
            {
                var option = chapterSelectionOptions[index];
                var row = chapterRows[index];
                row.ChapterNumber = option.ChapterNumber;
                row.Title.text = $"{LanguageService.Get("chapter").ToUpper()} {option.ChapterNumber}";
                row.StageRange.text = string.Format(
                    LanguageService.Get("stage_range"),
                    option.StartStage,
                    option.EndStage).ToUpper();

                string statusKey;
                string actionKey;
                if (option.IsLocked)
                {
                    statusKey = "locked";
                    actionKey = "locked";
                    row.Status.color = new Color(0.55f, 0.6f, 0.68f);
                }
                else if (option.IsBossChallenge)
                {
                    statusKey = "boss_ready";
                    actionKey = "challenge";
                    row.Status.color = new Color(1f, 0.62f, 0.22f);
                }
                else if (option.IsCurrent)
                {
                    statusKey = "current";
                    actionKey = "current";
                    row.Status.color = new Color(0.3f, 0.86f, 1f);
                }
                else if (option.IsCleared)
                {
                    statusKey = "cleared";
                    actionKey = "retry";
                    row.Status.color = new Color(1f, 0.78f, 0.22f);
                }
                else
                {
                    statusKey = option.TargetStage > option.StartStage ? "resume" : "enter";
                    actionKey = statusKey;
                    row.Status.color = new Color(0.4f, 0.95f, 0.62f);
                }

                row.Status.text = LanguageService.Get(statusKey).ToUpper();
                row.ActionButton.GetComponentInChildren<Text>().text = LanguageService.Get(actionKey).ToUpper();
                row.ActionButton.interactable =
                    !option.IsLocked &&
                    (!option.IsCurrent || option.IsBossChallenge);
                row.ActionButton.onClick.RemoveAllListeners();
                if (row.ActionButton.interactable)
                {
                    var chapterNumber = option.ChapterNumber;
                    row.ActionButton.onClick.AddListener(() =>
                    {
                        chapterSelectionAction?.Invoke(chapterNumber);
                        CloseChapterSelection();
                    });
                }
            }
        }

        private void CloseChapterSelection()
        {
            chapterSelectionPanel.SetActive(false);
        }

        private void CreateResearchPanel()
        {
            var backdrop = CreatePanel(
                "ResearchBackdrop",
                transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.005f, 0.012f, 0.04f, 0.82f));
            researchPanel = backdrop.gameObject;
            researchCard = CreatePanel(
                "ResearchCard",
                researchPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f),
                new Vector2(920f, 1040f),
                new Color(0.035f, 0.075f, 0.15f, 0.99f));
            researchTitleSurface = CreatePanel(
                "ResearchTitleSurface",
                researchCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-30f, -62f),
                new Vector2(610f, 124f),
                Color.white);
            researchTitle = CreateText(
                "ResearchTitle",
                researchTitleSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(0f, -8f),
                43,
                TextAnchor.MiddleCenter);
            researchTitle.font = UiFontProvider.GetCasual();
            researchSummary = CreateText(
                "ResearchSummary",
                researchCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-110f, -180f),
                new Vector2(570f, 56f),
                27,
                TextAnchor.MiddleCenter);
            researchSummary.font = UiFontProvider.GetCasual();
            researchSummary.color = new Color(0.55f, 0.92f, 1f);
            researchCores = CreateText(
                "ResearchCores",
                researchCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(280f, -180f),
                new Vector2(190f, 56f),
                29,
                TextAnchor.MiddleCenter);
            researchCores.color = new Color(1f, 0.78f, 0.28f);

            researchRowsRoot = CreateRect(
                "ResearchRows",
                researchCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -600f),
                new Vector2(824f, 620f));
            EnsureResearchRows(3);

            closeResearchCornerButton = CreateButton(
                "CloseResearchCorner",
                researchCard.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-70f, -70f),
                new Vector2(84f, 84f),
                Color.white);
            closeResearchCornerButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            var closeResearchIcon = CreateSimpleImage(
                "Icon",
                closeResearchCornerButton.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-22f, -22f),
                Color.white);
            closeResearchIcon.raycastTarget = false;
            closeResearchCornerButton.onClick.AddListener(CloseResearch);

            closeResearchButton = CreateButton(
                "CloseResearchButton",
                researchCard.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 68f),
                new Vector2(360f, 82f),
                new Color(1f, 0.48f, 0.12f));
            closeResearchButton.GetComponentInChildren<Text>().font = UiFontProvider.GetCasual();
            closeResearchButton.onClick.AddListener(CloseResearch);
            researchPanel.SetActive(false);
        }

        private void EnsureResearchRows(int count)
        {
            while (researchRows.Count < count)
            {
                researchRows.Add(CreateResearchRow(researchRows.Count));
            }

            for (var index = 0; index < researchRows.Count; index++)
            {
                researchRows[index].Surface.gameObject.SetActive(index < count);
            }
        }

        private ResearchRowView CreateResearchRow(int index)
        {
            var row = new ResearchRowView();
            row.Surface = CreatePanel(
                $"ResearchRow{index + 1}",
                researchRowsRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -95f - index * 190f),
                new Vector2(824f, 170f),
                new Color(0.055f, 0.12f, 0.23f, 0.98f));
            row.Name = CreateText(
                "ResearchName",
                row.Surface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(230f, 44f),
                new Vector2(400f, 50f),
                28,
                TextAnchor.MiddleLeft);
            row.Name.font = UiFontProvider.GetCasual();
            row.Level = CreateText(
                "ResearchLevel",
                row.Surface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(230f, -8f),
                new Vector2(400f, 42f),
                24,
                TextAnchor.MiddleLeft);
            row.Bonus = CreateText(
                "ResearchBonus",
                row.Surface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(230f, -50f),
                new Vector2(400f, 38f),
                21,
                TextAnchor.MiddleLeft);
            row.Bonus.color = new Color(0.45f, 0.92f, 1f);
            row.PurchaseButton = CreateButton(
                "ResearchPurchaseButton",
                row.Surface.transform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-148f, 0f),
                new Vector2(250f, 100f),
                new Color(0.35f, 0.75f, 0.16f));
            row.PurchaseButton.GetComponentInChildren<Text>().font = UiFontProvider.GetCasual();
            row.PurchaseButton.GetComponentInChildren<Text>().fontSize = 23;
            ApplyResearchRowSkin(row);
            return row;
        }

        private void RenderResearch()
        {
            if (researchPanel == null || lastState == null || lastService == null)
            {
                return;
            }

            var states = lastService.GetResearchNodeStates(lastState);
            EnsureResearchRows(states.Count);
            researchTitle.text = LanguageService.Get("feature_research").ToUpper();
            var researchPowerBonus =
                (lastService.GetResearchPowerMultiplier(lastState) - 1f) * 100f;
            researchSummary.text = string.Format(
                LanguageService.Get("research_summary"),
                lastState.Player.minerLevel,
                researchPowerBonus);
            researchCores.text = string.Format(
                LanguageService.Get("blueprint_cores"),
                CompactNumberFormatter.Format(lastState.Player.blueprintCores));
            closeResearchButton.GetComponentInChildren<Text>().text =
                LanguageService.Get("close").ToUpper();

            for (var index = 0; index < states.Count; index++)
            {
                var state = states[index];
                var row = researchRows[index];
                row.NodeId = state.Definition.NodeId;
                row.Name.text =
                    LanguageService.Get(state.Definition.NameLocalizationKey).ToUpper();
                row.Level.text = string.Format(
                    LanguageService.Get("research_level"),
                    state.CurrentLevel,
                    state.Definition.MaxLevel);
                row.Bonus.text = string.Format(
                    LanguageService.Get("research_power_bonus"),
                    state.Definition.PowerBonusPerLevel * 100f);
                row.PurchaseButton.GetComponentInChildren<Text>().text =
                    state.PurchaseStatus switch
                    {
                        ResearchPurchaseStatus.FeatureLocked =>
                            LanguageService.Get("locked").ToUpper(),
                        ResearchPurchaseStatus.PrerequisiteMissing =>
                            LanguageService.Get("research_prerequisite_short").ToUpper(),
                        ResearchPurchaseStatus.MaxLevel =>
                            LanguageService.Get("research_max_level").ToUpper(),
                        _ => string.Format(
                            LanguageService.Get("research_cost"),
                            CompactNumberFormatter.Format(state.Cost)).ToUpper()
                    };
                row.PurchaseButton.interactable = state.CanPurchase;
                row.PurchaseButton.onClick.RemoveAllListeners();
                if (state.CanPurchase)
                {
                    var nodeId = state.Definition.NodeId;
                    row.PurchaseButton.onClick.AddListener(
                        () => researchPurchaseAction?.Invoke(nodeId));
                }
            }
        }

        private void CloseResearch()
        {
            researchPanel.SetActive(false);
        }

        private static Sprite CreateAtlasSprite(Texture2D atlas, Rect normalizedRect, Vector4 normalizedBorder)
        {
            var rect = new Rect(
                normalizedRect.x * atlas.width,
                normalizedRect.y * atlas.height,
                normalizedRect.width * atlas.width,
                normalizedRect.height * atlas.height);
            var border = new Vector4(
                normalizedBorder.x * rect.width,
                normalizedBorder.y * rect.height,
                normalizedBorder.z * rect.width,
                normalizedBorder.w * rect.height);
            return Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static void ApplySlicedSprite(Image image, Sprite sprite, Color color)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
        }

        private void CreateSettingsPanel()
        {
            var backdrop = CreatePanel("SettingsBackdrop", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.005f, 0.012f, 0.04f, 0.78f));
            settingsPanel = backdrop.gameObject;
            settingsCard = CreatePanel("SettingsCard", settingsPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 53f), new Vector2(900f, 1344f), new Color(0.035f, 0.075f, 0.15f, 0.99f));
            var card = settingsCard;
            settingsTitleSurface = CreatePanel("SettingsTitleSurface", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(560f, 130f), Color.white);
            settingsTitle = CreateText("SettingsTitle", settingsTitleSurface.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-38f, -14f), 46, TextAnchor.MiddleCenter);
            audioLabel = CreateText("AudioLabel", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -136f), new Vector2(650f, 36f), 22, TextAnchor.MiddleLeft);
            audioLabel.gameObject.SetActive(false);

            CreateSettingSliderRow(card.transform, "MusicRow", -220f, out musicLabel, out musicSlider, out musicMuteButton, out musicSettingIcon, out musicMuteIcon);
            CreateSettingSliderRow(card.transform, "SoundRow", -340f, out soundLabel, out soundSlider, out soundMuteButton, out soundSettingIcon, out soundMuteIcon);
            CreateSettingToggleRow(card.transform, "HapticsRow", -460f, out hapticsLabel, out hapticsButton, out hapticsSettingIcon);
            CreateSettingToggleRow(card.transform, "ReduceMotionRow", -580f, out reduceMotionLabel, out reduceMotionButton, out reduceMotionSettingIcon);

            musicSlider.onValueChanged.AddListener(GameSettingsService.SetMusicVolume);
            soundSlider.onValueChanged.AddListener(GameSettingsService.SetSoundVolume);
            musicMuteButton.onClick.AddListener(() => GameSettingsService.SetMusicMuted(!GameSettingsService.MusicMuted));
            soundMuteButton.onClick.AddListener(() => GameSettingsService.SetSoundMuted(!GameSettingsService.SoundMuted));
            hapticsButton.onClick.AddListener(() => GameSettingsService.SetHapticsEnabled(!GameSettingsService.HapticsEnabled));
            reduceMotionButton.onClick.AddListener(() => GameSettingsService.SetReduceMotion(!GameSettingsService.ReduceMotion));

            languageLabel = CreateText("LanguageLabel", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-286f, -692f), new Vector2(100f, 42f), 20, TextAnchor.MiddleLeft);
            CreateLanguageButton(card.transform, "KoreanLanguageButton", "\uD55C\uAD6D\uC5B4", SupportedLanguage.Korean, -270f, -775f);
            CreateLanguageButton(card.transform, "EnglishLanguageButton", "English", SupportedLanguage.English, -90f, -775f);
            CreateLanguageButton(card.transform, "JapaneseLanguageButton", "\u65E5\u672C\u8A9E", SupportedLanguage.Japanese, 90f, -775f);
            CreateLanguageButton(card.transform, "ChineseLanguageButton", "\u7B80\u4F53\u4E2D\u6587", SupportedLanguage.ChineseSimplified, 270f, -775f);

            iapStatusText = CreateText("IapStatus", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -894f), new Vector2(680f, 38f), 18, TextAnchor.MiddleCenter);

            var removeAdsRow = CreatePanel("RemoveAdsRow", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -982f), new Vector2(720f, 104f), new Color(0.055f, 0.12f, 0.23f, 0.98f));
            settingsControlSurfaces.Add(removeAdsRow);
            var removeAdsWell = CreatePanel("IconWell", removeAdsRow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-308f, 0f), new Vector2(82f, 82f), Color.white);
            settingsIconWells.Add(removeAdsWell);
            removeAdsIcon = CreateSimpleImage("RemoveAdsIcon", removeAdsWell.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f), Color.white);
            removeAdsButton = CreateButton("RemoveAdsButton", removeAdsRow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(66f, 0f), new Vector2(560f, 78f), new Color(0.22f, 0.58f, 0.32f));
            removeAdsButton.GetComponentInChildren<Text>().fontSize = 21;

            var restoreRow = CreatePanel("RestorePurchasesRow", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -1102f), new Vector2(720f, 104f), new Color(0.055f, 0.12f, 0.23f, 0.98f));
            settingsControlSurfaces.Add(restoreRow);
            var restoreWell = CreatePanel("IconWell", restoreRow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-308f, 0f), new Vector2(82f, 82f), Color.white);
            settingsIconWells.Add(restoreWell);
            restorePurchasesIcon = CreateSimpleImage("RestorePurchasesIcon", restoreWell.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f), Color.white);
            restorePurchasesButton = CreateButton("RestorePurchasesButton", restoreRow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(66f, 0f), new Vector2(560f, 78f), new Color(0.12f, 0.2f, 0.32f));
            restorePurchasesButton.GetComponentInChildren<Text>().fontSize = 21;
            closeSettingsButton = CreateButton("CloseSettingsButton", card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 66f), new Vector2(326f, 88f), new Color(0.95f, 0.47f, 0.08f));
            closeSettingsButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            closeSettingsIcon = CreateSimpleImage("CloseIcon", closeSettingsButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f), Color.white);
            closeSettingsButton.onClick.AddListener(CloseSettings);
            settingsPanel.SetActive(false);
        }

        private void CreateSettingSliderRow(
            Transform parent,
            string name,
            float positionY,
            out Text label,
            out Slider slider,
            out Button muteButton,
            out Image settingIcon,
            out Image muteIcon)
        {
            var row = CreatePanel(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, positionY), new Vector2(720f, 104f), new Color(0.055f, 0.12f, 0.23f, 0.98f));
            settingsControlSurfaces.Add(row);
            var iconWell = CreatePanel("IconWell", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-308f, 0f), new Vector2(82f, 82f), Color.white);
            settingsIconWells.Add(iconWell);
            settingIcon = CreateSimpleImage("SettingIcon", iconWell.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f), Color.white);
            label = CreateText("Label", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-210f, 0f), new Vector2(104f, 56f), 18, TextAnchor.MiddleLeft);
            label.gameObject.SetActive(false);
            slider = CreateSlider("Slider", row.transform, Vector2.zero, new Vector2(500f, 54f));
            muteButton = CreateButton("MuteButton", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(304f, 0f), new Vector2(78f, 72f), new Color(0.2f, 0.62f, 0.28f));
            muteButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            muteIcon = CreateSimpleImage("MuteIcon", muteButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52f, 52f), Color.white);
        }

        private void CreateSettingToggleRow(Transform parent, string name, float positionY, out Text label, out Button toggleButton, out Image settingIcon)
        {
            var row = CreatePanel(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, positionY), new Vector2(720f, 104f), new Color(0.055f, 0.12f, 0.23f, 0.98f));
            settingsControlSurfaces.Add(row);
            var iconWell = CreatePanel("IconWell", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-308f, 0f), new Vector2(82f, 82f), Color.white);
            settingsIconWells.Add(iconWell);
            settingIcon = CreateSimpleImage("SettingIcon", iconWell.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 60f), Color.white);
            label = CreateText("Label", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-164f, 0f), new Vector2(220f, 56f), 21, TextAnchor.MiddleLeft);
            label.gameObject.SetActive(false);
            toggleButton = CreateButton("Toggle", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(242f, 0f), new Vector2(190f, 60f), new Color(0.2f, 0.62f, 0.28f));
            toggleButton.GetComponentInChildren<Text>().fontSize = 20;
        }

        private static Slider CreateSlider(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = position;
            rootRect.sizeDelta = size;

            var background = CreateSimpleImage("Background", root.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -28f), new Color(0.015f, 0.035f, 0.09f, 1f));
            var fillArea = CreateRect("FillArea", root.transform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(-34f, 24f));
            var fill = CreateSimpleImage("Fill", fillArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 0.58f, 0.08f, 1f));
            var handleArea = CreateRect("HandleArea", root.transform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(-34f, 44f));
            var handle = CreateSimpleImage("Handle", handleArea, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(44f, 44f), new Color(0.9f, 0.95f, 1f, 1f));

            var slider = root.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            return slider;
        }

        private void CreateLanguageButton(Transform parent, string name, string label, SupportedLanguage language, float positionX, float positionY)
        {
            var button = CreateButton(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(positionX, positionY), new Vector2(150f, 96f), new Color(0.12f, 0.2f, 0.32f));
            languageButtonSurfaces.Add(button.image);
            languageButtons[language] = button;
            var buttonLabel = button.GetComponentInChildren<Text>();
            buttonLabel.text = label;
            buttonLabel.gameObject.SetActive(false);
            languageIcons[language] = CreateSimpleImage("FlagIcon", button.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(114f, 68f), Color.white);
            button.onClick.AddListener(() => LanguageService.SetLanguage(language));
        }

        private void OpenSettings()
        {
            RenderSettings();
            settingsPanel.SetActive(true);
        }

        private void CloseSettings()
        {
            GameSettingsService.Flush();
            settingsPanel.SetActive(false);
        }

        private void RenderSettings()
        {
            if (settingsButton == null)
            {
                return;
            }

            settingsTitle.text = LanguageService.Get("settings").ToUpper();
            audioLabel.text = LanguageService.Get("audio").ToUpper();
            musicLabel.text = LanguageService.Get("music").ToUpper();
            soundLabel.text = LanguageService.Get("sound").ToUpper();
            hapticsLabel.text = LanguageService.Get("haptics").ToUpper();
            reduceMotionLabel.text = LanguageService.Get("reduce_motion").ToUpper();
            languageLabel.text = LanguageService.Get("language").ToUpper();
            musicSlider.SetValueWithoutNotify(GameSettingsService.MusicVolume);
            soundSlider.SetValueWithoutNotify(GameSettingsService.SoundVolume);
            SetIconButtonState(musicMuteButton, !GameSettingsService.MusicMuted);
            SetIconButtonState(soundMuteButton, !GameSettingsService.SoundMuted);
            SetToggleState(hapticsButton, GameSettingsService.HapticsEnabled);
            SetToggleState(reduceMotionButton, GameSettingsService.ReduceMotion);
            var closeLabel = closeSettingsButton.GetComponentInChildren<Text>(true);
            if (closeLabel != null)
            {
                closeLabel.text = LanguageService.Get("close").ToUpper();
            }

            foreach (var entry in languageButtons)
            {
                var selected = entry.Key == LanguageService.Current;
                entry.Value.image.color = Color.white;
                if (finalSkin != null)
                {
                    ApplyStretchedSimpleSprite(
                        entry.Value.image,
                        finalSkin.Simple(selected ? "SettingsLanguageSelected" : "SettingsLanguageButton"),
                        Color.white);
                }
            }

            RenderIapState();
        }

        private void SetIconButtonState(Button button, bool enabled)
        {
            if (finalSkin != null)
            {
                ApplyStretchedSimpleSprite(
                    button.image,
                    finalSkin.Simple("SettingsIconButton"),
                    enabled ? Color.white : new Color(0.48f, 0.52f, 0.6f, 1f));
            }
            else
            {
                button.image.color = enabled ? new Color(0.52f, 0.9f, 0.18f) : new Color(0.28f, 0.34f, 0.43f);
            }
        }

        private void SetToggleState(Button button, bool enabled)
        {
            if (finalSkin != null)
            {
                ApplyStretchedSimpleSprite(
                    button.image,
                    finalSkin.Simple(enabled ? "SettingsToggleOn" : "SettingsToggleOff"),
                    Color.white);
            }
            else
            {
                button.image.color = enabled ? new Color(0.52f, 0.9f, 0.18f) : new Color(0.28f, 0.34f, 0.43f);
            }

            var label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = LanguageService.Get(enabled ? "on" : "off").ToUpper();
            }
        }

        private void RenderIapState()
        {
            if (removeAdsButton == null)
            {
                return;
            }

            var busy = iapState is IapState.Initializing or IapState.Purchasing or IapState.Restoring;
            removeAdsButton.interactable = !adsRemoved && iapState is IapState.Ready or IapState.Cancelled;
            restorePurchasesButton.interactable = !busy;
            iapStatusText.gameObject.SetActive(
                adsRemoved || iapState is IapState.Initializing or IapState.Purchasing or IapState.Restoring or IapState.Deferred or IapState.Failed);
            iapStatusText.text = adsRemoved
                ? LanguageService.Get("iap_purchased").ToUpper()
                : LanguageService.Get(iapState switch
                {
                    IapState.Purchasing => "iap_purchasing",
                    IapState.Restoring => "iap_restoring",
                    IapState.Deferred => "iap_deferred",
                    IapState.Cancelled => "iap_cancelled",
                    IapState.Failed => "iap_unavailable",
                    IapState.Ready => "remove_ads",
                    _ => "iap_loading"
                }).ToUpper();

            removeAdsButton.GetComponentInChildren<Text>().text = adsRemoved
                ? LanguageService.Get("iap_purchased").ToUpper()
                : $"{LanguageService.Get("remove_ads").ToUpper()}{(string.IsNullOrEmpty(removeAdsPrice) ? string.Empty : $"  {removeAdsPrice}")}";
            restorePurchasesButton.GetComponentInChildren<Text>().text = LanguageService.Get("restore_purchases").ToUpper();
        }

        private void RenderRewardedAdState()
        {
            if (rewardedAdButton == null)
            {
                return;
            }

            rewardedAdButton.interactable = rewardedAdState is RewardedAdState.Ready or RewardedAdState.Failed;
            var rewardedAdLabel = rewardedAdButton.GetComponentInChildren<Text>(true);
            if (rewardedAdLabel == null)
            {
                return;
            }

            rewardedAdLabel.text = rewardedAdState switch
            {
                RewardedAdState.Ready => $"+{CompactNumberFormatter.Format(rewardedAdCredits)} C",
                RewardedAdState.Failed => LanguageService.Get("ad_retry").ToUpper(),
                RewardedAdState.Showing => LanguageService.Get("ad_showing").ToUpper(),
                _ => LanguageService.Get("ad_loading").ToUpper()
            };
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static RectTransform CreateSafeAreaRoot(Transform parent)
        {
            var root = new GameObject("SafeArea", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private void ApplySafeArea(bool force = false)
        {
            if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            var safeArea = Screen.safeArea;
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (!force && safeArea == appliedSafeArea && screenSize == appliedScreenSize)
            {
                return;
            }

            // Device Simulator and Game View can report the previous frame's safe area
            // immediately after a resolution change. Clamp it to the current screen so
            // transient stale values can never create anchors outside the 0..1 range.
            var xMin = Mathf.Clamp(safeArea.xMin, 0f, Screen.width);
            var yMin = Mathf.Clamp(safeArea.yMin, 0f, Screen.height);
            var xMax = Mathf.Clamp(safeArea.xMax, xMin, Screen.width);
            var yMax = Mathf.Clamp(safeArea.yMax, yMin, Screen.height);
            if (xMax <= xMin || yMax <= yMin)
            {
                xMin = 0f;
                yMin = 0f;
                xMax = Screen.width;
                yMax = Screen.height;
            }

            safeAreaRoot.anchorMin = new Vector2(xMin / Screen.width, yMin / Screen.height);
            safeAreaRoot.anchorMax = new Vector2(xMax / Screen.width, yMax / Screen.height);
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
            appliedSafeArea = safeArea;
            appliedScreenSize = screenSize;
        }

        private static Image CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = panel.GetComponent<Image>();
            image.color = color;
            var shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -6f);
            return image;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            var rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            var rect = rectObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Image CreateSimpleImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            var image = CreatePanel(name, parent, anchorMin, anchorMax, position, size, color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
            outline.effectDistance = new Vector2(2f, -2f);
            CreateText("Label", image.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 27, TextAnchor.MiddleCenter);
            image.gameObject.AddComponent<MobileButtonFeedback>();
            return button;
        }

        private static RawImage CreateIcon(string name, Transform parent)
        {
            return CreateIcon(name, parent, new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(230f, 204f));
        }

        private static RawImage CreateIcon(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var iconObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            iconObject.transform.SetParent(parent, false);
            var rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = iconObject.GetComponent<RawImage>();
            image.raycastTarget = false;
            return image;
        }

        private static void SetIcon(RawImage icon, Texture texture, int index)
        {
            icon.texture = texture;
            icon.uvRect = new Rect(index / 3f, 0f, 1f / 3f, 1f);
            var bounds = icon.rectTransform.sizeDelta;
            var sourceAspect = texture.width / 3f / texture.height;
            var width = bounds.x;
            var height = width / sourceAspect;
            if (height > bounds.y)
            {
                height = bounds.y;
                width = height * sourceAspect;
            }

            icon.rectTransform.sizeDelta = new Vector2(Mathf.Round(width), Mathf.Round(height));
        }

        private static Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = textObject.GetComponent<Text>();
            text.font = UiFontProvider.Get();
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.supportRichText = true;
            var shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.01f, 0.02f, 0.08f, 0.82f);
            shadow.effectDistance = new Vector2(1.5f, -2f);
            return text;
        }

        private static void ConfigureUpgradeCard(Button button)
        {
            var label = button.GetComponentInChildren<Text>();
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0.05f, 0f);
            rect.anchorMax = new Vector2(0.95f, 0f);
            rect.anchoredPosition = new Vector2(0f, 148f);
            rect.sizeDelta = new Vector2(0f, 46f);
            label.fontSize = 31;
            label.alignment = TextAnchor.MiddleCenter;
        }

        private static void SetUpgradeText(Button button, int level, long cost)
        {
            button.transform.Find("Label").GetComponent<Text>().text = $"<color=#FFFFFF>Lv. {level}</color>";
            button.transform.Find("CostText").GetComponent<Text>().text =
                $"<color=#FFD75A>{CompactNumberFormatter.Format(cost)}</color>";
        }

        private static string FormatTime(float seconds)
        {
            var totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }

        private Image[] CreateUpgradeDetails(Transform parent, Color activeColor)
        {
            var pips = new Image[3];
            for (var index = 0; index < pips.Length; index++)
            {
                pips[index] = CreatePanel($"LevelPip{index + 1}", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-52f + index * 52f, 196f), new Vector2(44f, 20f), new Color(0.04f, 0.09f, 0.2f, 0.95f));
                pips[index].GetComponent<Shadow>().enabled = false;
            }

            var costIcon = CreateSimpleImage("CostIcon", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-52f, 103f), new Vector2(34f, 34f), Color.white);
            upgradeCostIcons.Add(costIcon);
            var costText = CreateText("CostText", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(26f, 103f), new Vector2(130f, 38f), 29, TextAnchor.MiddleCenter);
            costText.color = new Color(1f, 0.84f, 0.35f);
            var action = CreatePanel("UpgradeAction", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 49f), new Vector2(198.4f, 60.8f), new Color(0.38f, 0.76f, 0.16f));
            upgradeActionSurfaces.Add(action);
            var actionIcon = CreateSimpleImage("UpgradeArrow", action.transform, Vector2.zero, Vector2.one, new Vector2(0f, 7f), new Vector2(-22f, -14f), Color.white);
            upgradeActionIcons.Add(actionIcon);
            UpdatePips(pips, 0, activeColor);
            return pips;
        }

        private static void UpdatePips(Image[] pips, int level, Color activeColor)
        {
            var activeCount = Mathf.Clamp(level % 6, 0, pips.Length);
            for (var index = 0; index < pips.Length; index++)
            {
                pips[index].color = index < activeCount ? activeColor : new Color(0.035f, 0.08f, 0.18f, 0.95f);
            }
        }
    }
}
