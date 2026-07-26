using System;
using System.Collections.Generic;
using PocketForge.Ads;
using PocketForge.Economy;
using PocketForge.Iap;
using PocketForge.Localization;
using PocketForge.Presentation;
using PocketForge.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PocketForge.Mining
{
    public sealed class MineHudView : MonoBehaviour
    {
        private Text creditsText;
        private Text depthText;
        private Text oreText;
        private Image oreProgress;
        private Button mineButton;
        private Button pickaxeButton;
        private Button drillButton;
        private Button robotButton;
        private RawImage pickaxeIcon;
        private RawImage drillIcon;
        private RawImage robotIcon;
        private RawImage mineIcon;
        private Image headerCoin;
        private Image headerCounterSlot;
        private Image topSurface;
        private Image upgradeSurface;
        private Image actionSurface;
        private Image progressBackground;
        private Text offlineRewardText;
        private Image offlineRewardSurface;
        private Button settingsButton;
        private Button creditsRewardButton;
        private Button rewardedAdButton;
        private Image creditsCurrencyIcon;
        private Image depthCurrencyIcon;
        private Image settingsIcon;
        private Image creditsPlusIcon;
        private Image rewardedVideoIcon;
        private Image rewardedPlusIcon;
        private Image oreBadgeSurface;
        private Image progressShine;
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
        private int rewardedAdCredits;
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

        public void Bind(Action mineAction, Action<UpgradeType> upgradeAction)
        {
            mineButton.onClick.AddListener(() => mineAction());
            pickaxeButton.onClick.AddListener(() => upgradeAction(UpgradeType.Pickaxe));
            drillButton.onClick.AddListener(() => upgradeAction(UpgradeType.Drill));
            robotButton.onClick.AddListener(() => upgradeAction(UpgradeType.Robot));
        }

        public void BindRewardedAd(Action rewardedAdAction)
        {
            rewardedAdButton.onClick.AddListener(() => rewardedAdAction());
            creditsRewardButton.onClick.AddListener(() => rewardedAdAction());
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
            creditsText.text = $"<color=#FFFFFF>{player.credits:N0}</color>";
            depthText.text = $"<color=#FFFFFF>{player.stage:N0}</color>";
            var oreKind = ore.IsRare
                ? $"<color=#8FEAFF>{LanguageService.Get("rare").ToUpper()}</color>  {LanguageService.Get("ore").ToUpper()}"
                : LanguageService.Get("ore").ToUpper();
            oreText.text = $"{oreKind}  <color=#D9E4FF>{Mathf.CeilToInt(ore.Health):00} / {Mathf.CeilToInt(ore.Durability):00}</color>";
            oreProgress.fillAmount = Mathf.Clamp01(ore.Health / ore.Durability);
            oreProgress.color = ore.IsRare ? new Color(0.52f, 0.83f, 1f) : new Color(0.16f, 0.84f, 1f);
            SetUpgradeText(pickaxeButton, player.pickaxeLevel, service.GetUpgradeCost(UpgradeType.Pickaxe, player.pickaxeLevel));
            SetUpgradeText(drillButton, player.drillLevel, service.GetUpgradeCost(UpgradeType.Drill, player.drillLevel));
            SetUpgradeText(robotButton, player.robotLevel, service.GetUpgradeCost(UpgradeType.Robot, player.robotLevel));
            UpdatePips(pickaxePips, player.pickaxeLevel, new Color(0.28f, 0.72f, 1f));
            UpdatePips(drillPips, player.drillLevel, new Color(0.78f, 0.38f, 1f));
            UpdatePips(robotPips, player.robotLevel, new Color(1f, 0.7f, 0.16f));
        }

        public void ShowOfflineReward(int credits)
        {
            offlineRewardSurface.gameObject.SetActive(true);
            offlineRewardText.gameObject.SetActive(true);
            offlineRewardText.text = $"{LanguageService.Get("reward").ToUpper()}  +{credits:N0} C";
        }

        public void ShowFeedback(string message, Color color)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            feedbackSurface.gameObject.SetActive(false);
            feedbackPopup.Show();
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

        public void SetRewardedAdState(RewardedAdState state, int rewardCredits)
        {
            rewardedAdState = state;
            rewardedAdCredits = rewardCredits;
            RenderRewardedAdState();
        }

        public void SetIapState(IapState state, string localizedPrice, bool ownsRemoveAds)
        {
            iapState = state;
            removeAdsPrice = localizedPrice ?? string.Empty;
            adsRemoved = ownsRemoveAds;
            RenderIapState();
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

            headerCoin = CreatePanel("HeaderCoin", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-405f, -123f), new Vector2(80f, 88f), Color.white);
            headerCoin.GetComponent<Shadow>().enabled = false;
            headerCounterSlot = CreatePanel("HeaderCounterSlot", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-286f, -123f), new Vector2(132f, 62f), new Color(0.025f, 0.07f, 0.16f, 0.96f));
            creditsRewardButton = CreateButton("CreditsRewardButton", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-181f, -123f), new Vector2(66f, 66f), new Color(0.42f, 0.84f, 0.14f));
            creditsRewardButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            creditsPlusIcon = CreateSimpleImage("PlusIcon", creditsRewardButton.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-4f, -4f), Color.white);
            creditsCurrencyIcon = CreateSimpleImage("CreditsCurrencyIcon", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-52f, -123f), new Vector2(52f, 52f), Color.white);
            creditsText = CreateText("Credits", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(69f, -123f), new Vector2(104f, 78f), 38, TextAnchor.MiddleLeft);
            depthCurrencyIcon = CreateSimpleImage("DepthCurrencyIcon", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(151f, -123f), new Vector2(54f, 54f), Color.white);
            depthText = CreateText("Depth", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(265f, -123f), new Vector2(108f, 78f), 38, TextAnchor.MiddleLeft);
            settingsButton = CreateButton("SettingsButton", hudRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92f, -123f), new Vector2(116f, 116f), new Color(0.12f, 0.2f, 0.3f));
            settingsButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            settingsIcon = CreateSimpleImage("SettingsIcon", settingsButton.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-4f, -4f), Color.white);
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
            rewardedPlusIcon = CreateSimpleImage("PlusIcon", rewardedAdButton.transform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-48f, 0f), new Vector2(72f, -8f), Color.white);
            RenderRewardedAdState();
            oreBadgeSurface = CreatePanel("OreBadge", hudRoot, new Vector2(0.5f, 0.405f), new Vector2(0.5f, 0.405f), new Vector2(0f, 64f), new Vector2(322f, 66f), new Color(0.05f, 0.1f, 0.24f, 0.98f));
            oreText = CreateText("OreLabel", oreBadgeSurface.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-24f, -10f), 29, TextAnchor.MiddleCenter);
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

            progressBackground = CreatePanel("ProgressBackground", hudRoot, new Vector2(0.5f, 0.405f), new Vector2(0.5f, 0.405f), new Vector2(0f, 22f), new Vector2(638f, 64f), new Color(0.05f, 0.1f, 0.24f, 0.98f));
            var progressTrack = CreateSimpleImage("ProgressTrack", progressBackground.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(608f, 32f), new Color(0.015f, 0.04f, 0.11f, 1f));
            oreProgress = CreatePanel("ProgressFill", progressTrack.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(304f, 0f), new Vector2(600f, 24f), new Color(0.16f, 0.84f, 1f));
            oreProgress.GetComponent<Shadow>().enabled = false;
            oreProgress.type = Image.Type.Filled;
            oreProgress.fillMethod = Image.FillMethod.Horizontal;
            oreProgress.fillOrigin = (int)Image.OriginHorizontal.Left;
            progressShine = CreateSimpleImage("ProgressShine", oreProgress.transform, new Vector2(0f, 0.55f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.22f));
            oreBadgeSurface.transform.SetSiblingIndex(progressBackground.transform.GetSiblingIndex() + 1);

            mineButton = CreateButton("MineButton", hudRoot, new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.33f), new Vector2(0f, 14f), new Vector2(504f, 232f), new Color(1f, 0.48f, 0.12f));
            mineButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            mineIcon = CreateIcon("MineIcon", mineButton.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(158f, 158f));
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
            positiveFeedback = safeAreaRoot.gameObject.AddComponent<PositiveFeedbackBurst>();
            positiveFeedback.Initialize(safeAreaRoot);
            CreateSettingsPanel();
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
            ApplySlicedSprite(oreBadgeSurface, panelSprite, new Color(0.86f, 0.92f, 1f, 1f));
            ApplySlicedSprite(offlineRewardSurface, panelSprite, new Color(0.55f, 1f, 0.72f, 0.98f));
            headerCoin.sprite = coinSprite;
            headerCoin.type = Image.Type.Simple;
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
            ApplySlicedSprite(creditsRewardButton.image, buttonSprite, new Color(0.72f, 1f, 0.7f));
            rewardedAdButton.GetComponent<Outline>().enabled = false;
            creditsRewardButton.GetComponent<Outline>().enabled = false;
            foreach (var costIcon in upgradeCostIcons)
            {
                costIcon.sprite = coinSprite;
                costIcon.type = Image.Type.Simple;
            }
            ApplySlicedSprite(settingsCard, cardSprite, Color.white);
            foreach (var surface in languageButtonSurfaces)
            {
                ApplySlicedSprite(surface, panelSprite, Color.white);
            }

            foreach (var surface in settingsControlSurfaces)
            {
                ApplySlicedSprite(surface, panelSprite, Color.white);
            }

            ApplySlicedSprite(closeSettingsButton.image, buttonSprite, Color.white);
        }

        private void ApplyHudIcons(Texture2D atlas)
        {
            var gear = CreateAtlasSprite(atlas, new Rect(0f, 0.5f, 0.5f, 0.5f), Vector4.zero);
            var plus = CreateAtlasSprite(atlas, new Rect(0.5f, 0.5f, 0.5f, 0.5f), Vector4.zero);
            var video = CreateAtlasSprite(atlas, new Rect(0f, 0f, 0.5f, 0.5f), Vector4.zero);
            var crystal = CreateAtlasSprite(atlas, new Rect(0.5f, 0f, 0.5f, 0.5f), Vector4.zero);

            ApplySimpleSprite(settingsIcon, gear);
            ApplySimpleSprite(creditsPlusIcon, plus);
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

            ApplySlicedSprite(topSurface, finalSkin.Sliced("HudHeader", new Vector4(0.08f, 0.24f, 0.08f, 0.24f)), Color.white);
            ApplySlicedSprite(headerCounterSlot, finalSkin.Sliced("HudCounterSlot", new Vector4(0.16f, 0.25f, 0.16f, 0.25f)), Color.white);
            ApplySlicedSprite(settingsButton.image, finalSkin.Sliced("HudSettingsButton", new Vector4(0.16f, 0.16f, 0.16f, 0.16f)), Color.white);
            ApplySlicedSprite(rewardedAdButton.image, finalSkin.Sliced("HudRewardPill", new Vector4(0.12f, 0.24f, 0.12f, 0.24f)), Color.white);
            ApplySlicedSprite(oreBadgeSurface, finalSkin.Sliced("HudOreBadge", new Vector4(0.14f, 0.24f, 0.14f, 0.24f)), Color.white);
            ApplySlicedSprite(progressBackground, finalSkin.Sliced("HudProgressFrame", new Vector4(0.08f, 0.28f, 0.08f, 0.28f)), Color.white);
            ApplySlicedSprite(mineButton.image, finalSkin.Sliced("HudMineButton", new Vector4(0.1f, 0.18f, 0.1f, 0.18f)), Color.white);
            ApplySlicedSprite(creditsRewardButton.image, finalSkin.Sliced("HudPlusButton", new Vector4(0.18f, 0.18f, 0.18f, 0.18f)), Color.white);

            ApplySlicedSprite(pickaxeButton.image, finalSkin.Sliced("HudUpgradeCard", new Vector4(0.1f, 0.08f, 0.1f, 0.08f)), Color.white);
            ApplySlicedSprite(drillButton.image, finalSkin.Sliced("HudUpgradeCard", new Vector4(0.1f, 0.08f, 0.1f, 0.08f)), Color.white);
            ApplySlicedSprite(robotButton.image, finalSkin.Sliced("HudUpgradeCard", new Vector4(0.1f, 0.08f, 0.1f, 0.08f)), Color.white);
            foreach (var surface in upgradeActionSurfaces)
            {
                ApplySlicedSprite(surface, finalSkin.Sliced("HudUpgradeButton", new Vector4(0.12f, 0.18f, 0.12f, 0.18f)), Color.white);
                surface.GetComponent<Shadow>().enabled = false;
            }

            foreach (var icon in upgradeActionIcons)
            {
                ApplySimpleSprite(icon, finalSkin.Simple("IconUpgradeArrow"));
            }

            ApplySimpleSprite(headerCoin, finalSkin.Simple("IconGoldBadge"));
            ApplySimpleSprite(creditsCurrencyIcon, finalSkin.Simple("IconGoldCoin"));
            ApplySimpleSprite(depthCurrencyIcon, finalSkin.Simple("IconPurpleGem"));
            ApplySimpleSprite(settingsIcon, finalSkin.Simple("IconGear"));
            ApplySimpleSprite(creditsPlusIcon, finalSkin.Simple("IconPlus"));
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

            ApplySlicedSprite(settingsCard, finalSkin.Sliced("SettingsModal", new Vector4(0.08f, 0.05f, 0.08f, 0.15f)), Color.white);
            ApplySlicedSprite(settingsTitleSurface, finalSkin.Sliced("SettingsTitlePlaque", new Vector4(0.1f, 0.18f, 0.1f, 0.18f)), Color.white);
            foreach (var surface in settingsControlSurfaces)
            {
                ApplySlicedSprite(surface, finalSkin.Sliced("SettingsRow", new Vector4(0.08f, 0.2f, 0.08f, 0.2f)), Color.white);
            }

            foreach (var well in settingsIconWells)
            {
                ApplySlicedSprite(well, finalSkin.Sliced("SettingsIconWell", new Vector4(0.14f, 0.14f, 0.14f, 0.14f)), Color.white);
            }

            ApplySettingSliderSkin(musicSlider);
            ApplySettingSliderSkin(soundSlider);
            ApplySlicedSprite(musicMuteButton.image, finalSkin.Sliced("SettingsIconButton", new Vector4(0.16f, 0.16f, 0.16f, 0.16f)), Color.white);
            ApplySlicedSprite(soundMuteButton.image, finalSkin.Sliced("SettingsIconButton", new Vector4(0.16f, 0.16f, 0.16f, 0.16f)), Color.white);

            ApplySimpleSprite(musicSettingIcon, finalSkin.Simple("IconMusic"));
            ApplySimpleSprite(soundSettingIcon, finalSkin.Simple("IconSound"));
            ApplySimpleSprite(musicMuteIcon, finalSkin.Simple("IconMusic"));
            ApplySimpleSprite(soundMuteIcon, finalSkin.Simple("IconSound"));
            ApplySimpleSprite(hapticsSettingIcon, finalSkin.Simple("IconHaptics"));
            ApplySimpleSprite(reduceMotionSettingIcon, finalSkin.Simple("IconReduceMotion"));

            ApplySlicedSprite(removeAdsButton.image, finalSkin.Sliced("SettingsActionButton", new Vector4(0.1f, 0.2f, 0.1f, 0.2f)), Color.white);
            ApplySlicedSprite(restorePurchasesButton.image, finalSkin.Sliced("SettingsActionButton", new Vector4(0.1f, 0.2f, 0.1f, 0.2f)), Color.white);
            ApplySlicedSprite(closeSettingsButton.image, finalSkin.Sliced("SettingsCloseButton", new Vector4(0.1f, 0.2f, 0.1f, 0.2f)), Color.white);
            ApplySimpleSprite(removeAdsIcon, finalSkin.Simple("IconAdsOff"));
            ApplySimpleSprite(restorePurchasesIcon, finalSkin.Simple("IconRestore"));
            ApplySimpleSprite(closeSettingsIcon, finalSkin.Simple("IconClose"));

            ApplyLanguageIcon(SupportedLanguage.Korean, "IconFlagKorean");
            ApplyLanguageIcon(SupportedLanguage.English, "IconFlagEnglish");
            ApplyLanguageIcon(SupportedLanguage.Japanese, "IconFlagJapanese");
            ApplyLanguageIcon(SupportedLanguage.ChineseSimplified, "IconFlagChinese");

            DisableOutline(mineButton);
            DisableOutline(pickaxeButton);
            DisableOutline(drillButton);
            DisableOutline(robotButton);
            DisableOutline(settingsButton);
            DisableOutline(rewardedAdButton);
            DisableOutline(creditsRewardButton);
            DisableOutline(removeAdsButton);
            DisableOutline(restorePurchasesButton);
            DisableOutline(closeSettingsButton);
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
                ApplySlicedSprite(background, finalSkin.Sliced("SettingsSliderTrack", new Vector4(0.08f, 0.25f, 0.08f, 0.25f)), Color.white);
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
            removeAdsIcon = CreateSimpleImage("RemoveAdsIcon", removeAdsWell.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-14f, -14f), Color.white);
            removeAdsButton = CreateButton("RemoveAdsButton", removeAdsRow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(66f, 0f), new Vector2(560f, 78f), new Color(0.22f, 0.58f, 0.32f));
            removeAdsButton.GetComponentInChildren<Text>().fontSize = 21;

            var restoreRow = CreatePanel("RestorePurchasesRow", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -1102f), new Vector2(720f, 104f), new Color(0.055f, 0.12f, 0.23f, 0.98f));
            settingsControlSurfaces.Add(restoreRow);
            var restoreWell = CreatePanel("IconWell", restoreRow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-308f, 0f), new Vector2(82f, 82f), Color.white);
            settingsIconWells.Add(restoreWell);
            restorePurchasesIcon = CreateSimpleImage("RestorePurchasesIcon", restoreWell.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-14f, -14f), Color.white);
            restorePurchasesButton = CreateButton("RestorePurchasesButton", restoreRow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(66f, 0f), new Vector2(560f, 78f), new Color(0.12f, 0.2f, 0.32f));
            restorePurchasesButton.GetComponentInChildren<Text>().fontSize = 21;
            closeSettingsButton = CreateButton("CloseSettingsButton", card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 66f), new Vector2(326f, 88f), new Color(0.95f, 0.47f, 0.08f));
            closeSettingsButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            closeSettingsIcon = CreateSimpleImage("CloseIcon", closeSettingsButton.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-16f, -16f), Color.white);
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
            settingIcon = CreateSimpleImage("SettingIcon", iconWell.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-14f, -14f), Color.white);
            label = CreateText("Label", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-210f, 0f), new Vector2(104f, 56f), 18, TextAnchor.MiddleLeft);
            label.gameObject.SetActive(false);
            slider = CreateSlider("Slider", row.transform, Vector2.zero, new Vector2(500f, 54f));
            muteButton = CreateButton("MuteButton", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(304f, 0f), new Vector2(78f, 72f), new Color(0.2f, 0.62f, 0.28f));
            muteButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            muteIcon = CreateSimpleImage("MuteIcon", muteButton.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-14f, -14f), Color.white);
        }

        private void CreateSettingToggleRow(Transform parent, string name, float positionY, out Text label, out Button toggleButton, out Image settingIcon)
        {
            var row = CreatePanel(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, positionY), new Vector2(720f, 104f), new Color(0.055f, 0.12f, 0.23f, 0.98f));
            settingsControlSurfaces.Add(row);
            var iconWell = CreatePanel("IconWell", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-308f, 0f), new Vector2(82f, 82f), Color.white);
            settingsIconWells.Add(iconWell);
            settingIcon = CreateSimpleImage("SettingIcon", iconWell.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-14f, -14f), Color.white);
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
            languageIcons[language] = CreateSimpleImage("FlagIcon", button.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-24f, -20f), Color.white);
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
                    ApplySlicedSprite(
                        entry.Value.image,
                        finalSkin.Sliced(selected ? "SettingsLanguageSelected" : "SettingsLanguageButton", new Vector4(0.14f, 0.18f, 0.14f, 0.18f)),
                        Color.white);
                }
            }

            RenderIapState();
        }

        private void SetIconButtonState(Button button, bool enabled)
        {
            if (finalSkin != null)
            {
                ApplySlicedSprite(
                    button.image,
                    finalSkin.Sliced("SettingsIconButton", new Vector4(0.16f, 0.16f, 0.16f, 0.16f)),
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
                ApplySlicedSprite(
                    button.image,
                    finalSkin.Sliced(enabled ? "SettingsToggleOn" : "SettingsToggleOff", new Vector4(0.18f, 0.2f, 0.18f, 0.2f)),
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
            creditsRewardButton.interactable = rewardedAdButton.interactable;
            rewardedAdButton.GetComponentInChildren<Text>().text = rewardedAdState switch
            {
                RewardedAdState.Ready => $"+{rewardedAdCredits:N0} C",
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

            safeAreaRoot.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
            safeAreaRoot.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
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

        private static void SetUpgradeText(Button button, int level, int cost)
        {
            button.transform.Find("Label").GetComponent<Text>().text = $"<color=#FFFFFF>Lv. {level}</color>";
            button.transform.Find("CostText").GetComponent<Text>().text = $"<color=#FFD75A>{cost:N0}</color>";
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
            var action = CreatePanel("UpgradeAction", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 44f), new Vector2(248f, 76f), new Color(0.38f, 0.76f, 0.16f));
            upgradeActionSurfaces.Add(action);
            var actionIcon = CreateSimpleImage("UpgradeArrow", action.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-22f, -14f), Color.white);
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
