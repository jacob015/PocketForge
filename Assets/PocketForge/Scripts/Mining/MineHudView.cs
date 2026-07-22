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
        private Image topSurface;
        private Image upgradeSurface;
        private Image actionSurface;
        private Image progressBackground;
        private Text offlineRewardText;
        private Image offlineRewardSurface;
        private Button settingsButton;
        private Button rewardedAdButton;
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
        private readonly List<Image> languageButtonSurfaces = new();
        private readonly List<Image> settingsControlSurfaces = new();
        private readonly List<Image> upgradeActionSurfaces = new();
        private Image[] pickaxePips;
        private Image[] drillPips;
        private Image[] robotPips;
        private Text feedbackText;
        private Image feedbackSurface;
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
            scaler.matchWidthOrHeight = 0.5f;

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
        }

        public void BindIap(Action purchaseRemoveAdsAction, Action restorePurchasesAction)
        {
            removeAdsButton.onClick.AddListener(() => purchaseRemoveAdsAction());
            restorePurchasesButton.onClick.AddListener(() => restorePurchasesAction());
        }

        public void SetTheme(Texture upgradeIcons, Texture2D uiKit, Texture2D upgradeButton, Texture2D feedbackPanel)
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

            RenderSettings();
        }

        public void Render(MiningGameState state, MiningGameService service)
        {
            lastState = state;
            lastService = service;
            var player = state.Player;
            var ore = state.Ore;
            creditsText.text = $"<color=#FFD75A>{LanguageService.Get("credits").ToUpper()}</color>  <color=#FFFFFF>{player.credits:N0}</color>";
            depthText.text = $"<color=#9FE8FF>{LanguageService.Get("depth").ToUpper()}</color>  <color=#FFFFFF>{player.stage}</color>";
            oreText.text = $"{(ore.IsRare ? $"<color=#8FEAFF>{LanguageService.Get("rare").ToUpper()}</color>  " : string.Empty)}<color=#FFFFFF>{LanguageService.Get("ore").ToUpper()}</color>  <color=#D9E4FF>{Mathf.CeilToInt(ore.Health)} / {Mathf.CeilToInt(ore.Durability)}</color>";
            oreProgress.fillAmount = Mathf.Clamp01(ore.Health / ore.Durability);
            oreProgress.color = ore.IsRare ? new Color(0.48f, 0.92f, 1f) : new Color(1f, 0.72f, 0.2f);
            SetUpgradeText(pickaxeButton, LanguageService.Get("pickaxe"), player.pickaxeLevel, service.GetUpgradeCost(UpgradeType.Pickaxe, player.pickaxeLevel));
            SetUpgradeText(drillButton, LanguageService.Get("drill"), player.drillLevel, service.GetUpgradeCost(UpgradeType.Drill, player.drillLevel));
            SetUpgradeText(robotButton, LanguageService.Get("robot"), player.robotLevel, service.GetUpgradeCost(UpgradeType.Robot, player.robotLevel));
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
            feedbackSurface.gameObject.SetActive(true);
            feedbackText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 1.2f);
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

        private void HideFeedback()
        {
            feedbackSurface.gameObject.SetActive(false);
            feedbackText.gameObject.SetActive(false);
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

            topSurface = CreatePanel("TopSurface", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-70f, -92f), new Vector2(824f, 132f), new Color(0.06f, 0.1f, 0.24f, 0.96f));
            upgradeSurface = CreatePanel("UpgradeSurface", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 226f), new Vector2(956f, 458f), Color.clear);
            actionSurface = CreatePanel("ActionSurface", hudRoot, new Vector2(0.5f, 0.315f), new Vector2(0.5f, 0.315f), Vector2.zero, new Vector2(600f, 250f), Color.clear);
            upgradeSurface.raycastTarget = false;
            actionSurface.raycastTarget = false;

            headerCoin = CreatePanel("HeaderCoin", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-414f, -92f), new Vector2(74f, 74f), Color.white);
            headerCoin.GetComponent<Shadow>().enabled = false;
            creditsText = CreateText("Credits", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-235f, -92f), new Vector2(260f, 72f), 29, TextAnchor.MiddleLeft);
            depthText = CreateText("Depth", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(184f, -92f), new Vector2(280f, 72f), 29, TextAnchor.MiddleCenter);
            settingsButton = CreateButton("SettingsButton", hudRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-58f, -92f), new Vector2(96f, 96f), new Color(0.12f, 0.2f, 0.3f));
            settingsButton.onClick.AddListener(OpenSettings);
            offlineRewardSurface = CreatePanel("OfflineRewardSurface", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-236f, -174f), new Vector2(382f, 58f), new Color(0.04f, 0.2f, 0.16f, 0.92f));
            offlineRewardText = CreateText("OfflineReward", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-236f, -174f), new Vector2(360f, 46f), 21, TextAnchor.MiddleCenter);
            offlineRewardText.color = new Color(0.45f, 0.95f, 0.7f);
            offlineRewardSurface.gameObject.SetActive(false);
            offlineRewardText.gameObject.SetActive(false);
            rewardedAdButton = CreateButton("RewardedAdButton", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(278f, -174f), new Vector2(374f, 62f), new Color(0.18f, 0.58f, 0.3f));
            rewardedAdButton.GetComponentInChildren<Text>().fontSize = 20;
            RenderRewardedAdState();
            oreText = CreateText("OreLabel", hudRoot, new Vector2(0.5f, 0.405f), new Vector2(0.5f, 0.405f), new Vector2(0f, 56f), new Vector2(560f, 50f), 30, TextAnchor.MiddleCenter);
            feedbackSurface = CreatePanel("ActionFeedbackSurface", hudRoot, new Vector2(0.5f, 0.51f), new Vector2(0.5f, 0.51f), Vector2.zero, new Vector2(520f, 82f), new Color(0.035f, 0.08f, 0.18f, 0.96f));
            feedbackText = CreateText("ActionFeedback", hudRoot, new Vector2(0.5f, 0.51f), new Vector2(0.5f, 0.51f), Vector2.zero, new Vector2(480f, 54f), 28, TextAnchor.MiddleCenter);
            feedbackSurface.gameObject.SetActive(false);
            feedbackText.gameObject.SetActive(false);

            progressBackground = CreatePanel("ProgressBackground", hudRoot, new Vector2(0.5f, 0.405f), new Vector2(0.5f, 0.405f), Vector2.zero, new Vector2(610f, 58f), new Color(0.05f, 0.1f, 0.24f, 0.96f));
            oreProgress = CreatePanel("ProgressFill", progressBackground.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(568f, 24f), new Color(0.95f, 0.45f, 0.12f));
            oreProgress.GetComponent<Shadow>().enabled = false;
            oreProgress.type = Image.Type.Filled;
            oreProgress.fillMethod = Image.FillMethod.Horizontal;
            oreProgress.fillOrigin = (int)Image.OriginHorizontal.Left;

            mineButton = CreateButton("MineButton", hudRoot, new Vector2(0.5f, 0.315f), new Vector2(0.5f, 0.315f), Vector2.zero, new Vector2(560f, 232f), new Color(1f, 0.48f, 0.12f));
            mineButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            mineIcon = CreateIcon("MineIcon", mineButton.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(168f, 168f));
            pickaxeButton = CreateButton("PickaxeButton", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-320f, 226f), new Vector2(292f, 410f), new Color(0.14f, 0.3f, 0.52f));
            drillButton = CreateButton("DrillButton", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 226f), new Vector2(292f, 410f), new Color(0.14f, 0.3f, 0.52f));
            robotButton = CreateButton("RobotButton", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(320f, 226f), new Vector2(292f, 410f), new Color(0.14f, 0.3f, 0.52f));
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
            ApplySlicedSprite(feedbackSurface, panelSprite, new Color(0.7f, 0.82f, 1f, 0.98f));
            ApplySlicedSprite(offlineRewardSurface, panelSprite, new Color(0.55f, 1f, 0.72f, 0.98f));
            headerCoin.sprite = coinSprite;
            headerCoin.type = Image.Type.Simple;
            ApplySlicedSprite(mineButton.image, buttonSprite, Color.white);
            ApplySlicedSprite(pickaxeButton.image, cardSprite, Color.white);
            ApplySlicedSprite(drillButton.image, cardSprite, Color.white);
            ApplySlicedSprite(robotButton.image, cardSprite, Color.white);

            mineButton.GetComponent<Outline>().enabled = false;
            pickaxeButton.GetComponent<Outline>().enabled = false;
            drillButton.GetComponent<Outline>().enabled = false;
            robotButton.GetComponent<Outline>().enabled = false;
            ApplySlicedSprite(settingsButton.image, cardSprite, new Color(0.74f, 0.84f, 1f));
            ApplySlicedSprite(rewardedAdButton.image, buttonSprite, new Color(0.72f, 1f, 0.7f));
            rewardedAdButton.GetComponent<Outline>().enabled = false;
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
            // The generated source keeps generous transparent margins. Cropping here preserves
            // the authored bevel while 9-slicing lets the notification scale across aspect ratios.
            var paddingX = Mathf.RoundToInt(texture.width * 0.068f);
            var paddingY = Mathf.RoundToInt(texture.height * 0.20f);
            var rect = new Rect(
                paddingX,
                paddingY,
                texture.width - paddingX * 2f,
                texture.height - paddingY * 2f);
            var border = new Vector4(rect.height * 0.22f, rect.height * 0.25f, rect.height * 0.22f, rect.height * 0.25f);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            ApplySlicedSprite(feedbackSurface, sprite, Color.white);
            feedbackSurface.GetComponent<Shadow>().enabled = false;
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
            settingsCard = CreatePanel("SettingsCard", settingsPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840f, 1420f), new Color(0.035f, 0.075f, 0.15f, 0.99f));
            var card = settingsCard;
            settingsTitle = CreateText("SettingsTitle", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(680f, 76f), 46, TextAnchor.MiddleCenter);
            audioLabel = CreateText("AudioLabel", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -144f), new Vector2(650f, 42f), 24, TextAnchor.MiddleLeft);

            CreateSettingSliderRow(card.transform, "MusicRow", -236f, out musicLabel, out musicSlider, out musicMuteButton);
            CreateSettingSliderRow(card.transform, "SoundRow", -354f, out soundLabel, out soundSlider, out soundMuteButton);
            CreateSettingToggleRow(card.transform, "HapticsRow", -482f, out hapticsLabel, out hapticsButton);
            CreateSettingToggleRow(card.transform, "ReduceMotionRow", -594f, out reduceMotionLabel, out reduceMotionButton);

            musicSlider.onValueChanged.AddListener(GameSettingsService.SetMusicVolume);
            soundSlider.onValueChanged.AddListener(GameSettingsService.SetSoundVolume);
            musicMuteButton.onClick.AddListener(() => GameSettingsService.SetMusicMuted(!GameSettingsService.MusicMuted));
            soundMuteButton.onClick.AddListener(() => GameSettingsService.SetSoundMuted(!GameSettingsService.SoundMuted));
            hapticsButton.onClick.AddListener(() => GameSettingsService.SetHapticsEnabled(!GameSettingsService.HapticsEnabled));
            reduceMotionButton.onClick.AddListener(() => GameSettingsService.SetReduceMotion(!GameSettingsService.ReduceMotion));

            languageLabel = CreateText("LanguageLabel", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -686f), new Vector2(650f, 42f), 24, TextAnchor.MiddleLeft);
            CreateLanguageButton(card.transform, "KoreanLanguageButton", "\uD55C\uAD6D\uC5B4", SupportedLanguage.Korean, -184f, -768f);
            CreateLanguageButton(card.transform, "EnglishLanguageButton", "English", SupportedLanguage.English, 184f, -768f);
            CreateLanguageButton(card.transform, "JapaneseLanguageButton", "\u65E5\u672C\u8A9E", SupportedLanguage.Japanese, -184f, -860f);
            CreateLanguageButton(card.transform, "ChineseLanguageButton", "\u7B80\u4F53\u4E2D\u6587", SupportedLanguage.ChineseSimplified, 184f, -860f);

            iapStatusText = CreateText("IapStatus", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -950f), new Vector2(680f, 52f), 21, TextAnchor.MiddleCenter);
            removeAdsButton = CreateButton("RemoveAdsButton", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -1032f), new Vector2(700f, 76f), new Color(0.22f, 0.58f, 0.32f));
            restorePurchasesButton = CreateButton("RestorePurchasesButton", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -1122f), new Vector2(700f, 72f), new Color(0.12f, 0.2f, 0.32f));
            languageButtonSurfaces.Add(removeAdsButton.image);
            languageButtonSurfaces.Add(restorePurchasesButton.image);
            closeSettingsButton = CreateButton("CloseSettingsButton", card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 66f), new Vector2(360f, 76f), new Color(0.95f, 0.47f, 0.08f));
            closeSettingsButton.onClick.AddListener(CloseSettings);
            settingsPanel.SetActive(false);
        }

        private void CreateSettingSliderRow(Transform parent, string name, float positionY, out Text label, out Slider slider, out Button muteButton)
        {
            var row = CreatePanel(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, positionY), new Vector2(720f, 102f), new Color(0.055f, 0.12f, 0.23f, 0.98f));
            settingsControlSurfaces.Add(row);
            label = CreateText("Label", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-258f, 0f), new Vector2(170f, 56f), 23, TextAnchor.MiddleLeft);
            slider = CreateSlider("Slider", row.transform, new Vector2(58f, 0f), new Vector2(330f, 54f));
            muteButton = CreateButton("MuteButton", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(292f, 0f), new Vector2(112f, 60f), new Color(0.2f, 0.62f, 0.28f));
            settingsControlSurfaces.Add(muteButton.image);
            muteButton.GetComponentInChildren<Text>().fontSize = 18;
        }

        private void CreateSettingToggleRow(Transform parent, string name, float positionY, out Text label, out Button toggleButton)
        {
            var row = CreatePanel(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, positionY), new Vector2(720f, 94f), new Color(0.055f, 0.12f, 0.23f, 0.98f));
            settingsControlSurfaces.Add(row);
            label = CreateText("Label", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-190f, 0f), new Vector2(340f, 56f), 23, TextAnchor.MiddleLeft);
            toggleButton = CreateButton("Toggle", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(242f, 0f), new Vector2(190f, 60f), new Color(0.2f, 0.62f, 0.28f));
            settingsControlSurfaces.Add(toggleButton.image);
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
            var button = CreateButton(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(positionX, positionY), new Vector2(332f, 76f), new Color(0.12f, 0.2f, 0.32f));
            languageButtonSurfaces.Add(button.image);
            button.GetComponentInChildren<Text>().text = label;
            button.GetComponentInChildren<Text>().fontSize = 22;
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

            settingsButton.GetComponentInChildren<Text>().text = "⚙";
            settingsButton.GetComponentInChildren<Text>().fontSize = 52;
            settingsTitle.text = LanguageService.Get("settings").ToUpper();
            audioLabel.text = LanguageService.Get("audio").ToUpper();
            musicLabel.text = LanguageService.Get("music").ToUpper();
            soundLabel.text = LanguageService.Get("sound").ToUpper();
            hapticsLabel.text = LanguageService.Get("haptics").ToUpper();
            reduceMotionLabel.text = LanguageService.Get("reduce_motion").ToUpper();
            languageLabel.text = LanguageService.Get("language").ToUpper();
            musicSlider.SetValueWithoutNotify(GameSettingsService.MusicVolume);
            soundSlider.SetValueWithoutNotify(GameSettingsService.SoundVolume);
            SetToggleState(musicMuteButton, !GameSettingsService.MusicMuted);
            SetToggleState(soundMuteButton, !GameSettingsService.SoundMuted);
            SetToggleState(hapticsButton, GameSettingsService.HapticsEnabled);
            SetToggleState(reduceMotionButton, GameSettingsService.ReduceMotion);
            closeSettingsButton.GetComponentInChildren<Text>().text = LanguageService.Get("close").ToUpper();
            RenderIapState();
        }

        private static void SetToggleState(Button button, bool enabled)
        {
            button.image.color = enabled ? new Color(0.52f, 0.9f, 0.18f) : new Color(0.28f, 0.34f, 0.43f);
            button.GetComponentInChildren<Text>().text = LanguageService.Get(enabled ? "on" : "off").ToUpper();
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
            rewardedAdButton.GetComponentInChildren<Text>().text = rewardedAdState switch
            {
                RewardedAdState.Ready => $"{LanguageService.Get("free_reward").ToUpper()}  +{rewardedAdCredits:N0} C",
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
            return CreateIcon(name, parent, new Vector2(0.5f, 0.69f), Vector2.zero, new Vector2(214f, 190f));
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
            rect.anchorMin = new Vector2(0.05f, 0.22f);
            rect.anchorMax = new Vector2(0.95f, 0.47f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            label.fontSize = 22;
            label.alignment = TextAnchor.MiddleCenter;
        }

        private static void SetUpgradeText(Button button, string name, int level, int cost)
        {
            button.GetComponentInChildren<Text>().text = $"<color=#FFFFFF>{name.ToUpper()}</color>  <color=#BBD8FF>Lv.{level}</color>\n<color=#FFD75A>{cost:N0} C</color>";
        }

        private Image[] CreateUpgradeDetails(Transform parent, Color activeColor)
        {
            var pips = new Image[5];
            for (var index = 0; index < pips.Length; index++)
            {
                pips[index] = CreatePanel($"LevelPip{index + 1}", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-84f + index * 42f, 82f), new Vector2(34f, 18f), new Color(0.04f, 0.09f, 0.2f, 0.95f));
                pips[index].GetComponent<Shadow>().enabled = false;
            }

            var action = CreatePanel("UpgradeAction", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(232f, 74f), new Color(0.38f, 0.76f, 0.16f));
            upgradeActionSurfaces.Add(action);
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
