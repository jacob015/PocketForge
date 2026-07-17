using System;
using PocketForge.Economy;
using PocketForge.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PocketForge.Mining
{
    public sealed class MineHudView : MonoBehaviour
    {
        private Text headerText;
        private Text oreText;
        private Image oreProgress;
        private Button mineButton;
        private Button pickaxeButton;
        private Button drillButton;
        private Button robotButton;
        private RawImage pickaxeIcon;
        private RawImage drillIcon;
        private RawImage robotIcon;
        private RawImage backdropImage;
        private Image topSurface;
        private Image upgradeSurface;
        private Image actionSurface;
        private Image progressBackground;
        private Text offlineRewardText;
        private Image offlineRewardSurface;
        private Button settingsButton;
        private GameObject settingsPanel;
        private Text settingsTitle;
        private Text languageLabel;
        private Button closeSettingsButton;
        private Text feedbackText;
        private MiningGameState lastState;
        private MiningGameService lastService;

        private void OnEnable() => LanguageService.Changed += RefreshLocalization;

        private void OnDisable() => LanguageService.Changed -= RefreshLocalization;

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

        public void SetTheme(Texture upgradeIcons, Texture backdrop, Texture2D uiKit)
        {
            if (backdrop != null)
            {
                backdropImage.texture = backdrop;
                backdropImage.color = new Color(1f, 1f, 1f, 0.34f);
            }

            if (upgradeIcons != null)
            {
                SetIcon(pickaxeIcon, upgradeIcons, 0);
                SetIcon(drillIcon, upgradeIcons, 1);
                SetIcon(robotIcon, upgradeIcons, 2);
            }

            if (uiKit != null)
            {
                ApplyUiKit(uiKit);
            }
        }

        public void Render(MiningGameState state, MiningGameService service)
        {
            lastState = state;
            lastService = service;
            var player = state.Player;
            var ore = state.Ore;
            headerText.text = $"<b>POCKET FORGE</b>\n<size=30><color=#FFD75A>{LanguageService.Get("credits").ToUpper()}  {player.credits:N0}</color>     <color=#7BE8FF>{LanguageService.Get("depth").ToUpper()}  {player.stage}</color></size>";
            oreText.text = $"<b>{(ore.IsRare ? "RARE " : string.Empty)}{LanguageService.Get("ore").ToUpper()}</b>  <color=#D9E4FF>{Mathf.CeilToInt(ore.Health)} / {Mathf.CeilToInt(ore.Durability)}</color>";
            oreProgress.fillAmount = Mathf.Clamp01(ore.Health / ore.Durability);
            oreProgress.color = ore.IsRare ? new Color(0.48f, 0.92f, 1f) : new Color(1f, 0.72f, 0.2f);
            mineButton.GetComponentInChildren<Text>().text = $"<b>{LanguageService.Get("mine").ToUpper()}</b>  +{service.GetTapPower(player.pickaxeLevel):0}";
            SetUpgradeText(pickaxeButton, LanguageService.Get("pickaxe"), player.pickaxeLevel, $"{LanguageService.Get("tap")} +1", service.GetUpgradeCost(UpgradeType.Pickaxe, player.pickaxeLevel));
            SetUpgradeText(drillButton, LanguageService.Get("drill"), player.drillLevel, $"{LanguageService.Get("auto")} +{service.GetAutoPowerPerSecond(player.drillLevel + 1) - service.GetAutoPowerPerSecond(player.drillLevel):0.0}/s", service.GetUpgradeCost(UpgradeType.Drill, player.drillLevel));
            SetUpgradeText(robotButton, LanguageService.Get("robot"), player.robotLevel, $"{LanguageService.Get("reward")} +10%", service.GetUpgradeCost(UpgradeType.Robot, player.robotLevel));
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
            feedbackText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 1.2f);
        }

        private void HideFeedback() => feedbackText.gameObject.SetActive(false);

        private void RefreshLocalization()
        {
            if (lastState != null && lastService != null)
            {
                Render(lastState, lastService);
            }

            RenderSettings();
        }

        private void Build()
        {
            backdropImage = CreateBackdrop(transform);
            CreatePanel("SkyGradient", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1080f, 1920f), new Color(0.055f, 0.09f, 0.23f, 0.05f));
            topSurface = CreatePanel("TopSurface", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(1000f, 150f), new Color(0.06f, 0.1f, 0.24f, 0.78f));
            upgradeSurface = CreatePanel("UpgradeSurface", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 160f), new Vector2(1020f, 300f), new Color(0.04f, 0.07f, 0.17f, 0.72f));
            actionSurface = CreatePanel("ActionSurface", transform, new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), Vector2.zero, new Vector2(940f, 142f), new Color(0.04f, 0.07f, 0.15f, 0.42f));

            headerText = CreateText("Header", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-74f, -84f), new Vector2(760f, 104f), 42, TextAnchor.MiddleCenter);
            settingsButton = CreateButton("SettingsButton", transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-134f, -46f), new Vector2(176f, 52f), new Color(0.12f, 0.2f, 0.3f));
            settingsButton.onClick.AddListener(() => settingsPanel.SetActive(true));
            offlineRewardSurface = CreatePanel("OfflineRewardSurface", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -184f), new Vector2(620f, 64f), new Color(0.04f, 0.2f, 0.16f, 0.88f));
            offlineRewardText = CreateText("OfflineReward", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -184f), new Vector2(600f, 52f), 25, TextAnchor.MiddleCenter);
            offlineRewardText.color = new Color(0.45f, 0.95f, 0.7f);
            offlineRewardSurface.gameObject.SetActive(false);
            offlineRewardText.gameObject.SetActive(false);
            oreText = CreateText("OreLabel", transform, new Vector2(0.5f, 0.61f), new Vector2(0.5f, 0.61f), Vector2.zero, new Vector2(560f, 50f), 28, TextAnchor.MiddleCenter);
            feedbackText = CreateText("ActionFeedback", transform, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(600f, 48f), 30, TextAnchor.MiddleCenter);
            feedbackText.gameObject.SetActive(false);

            progressBackground = CreatePanel("ProgressBackground", transform, new Vector2(0.5f, 0.575f), new Vector2(0.5f, 0.575f), Vector2.zero, new Vector2(620f, 28f), new Color(0.03f, 0.05f, 0.1f, 0.7f));
            oreProgress = CreatePanel("ProgressFill", transform, new Vector2(0.5f, 0.575f), new Vector2(0.5f, 0.575f), Vector2.zero, new Vector2(606f, 16f), new Color(0.95f, 0.45f, 0.12f));
            oreProgress.type = Image.Type.Filled;
            oreProgress.fillMethod = Image.FillMethod.Horizontal;
            oreProgress.fillOrigin = (int)Image.OriginHorizontal.Left;

            mineButton = CreateButton("MineButton", transform, new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), Vector2.zero, new Vector2(860f, 104f), new Color(1f, 0.48f, 0.12f));
            pickaxeButton = CreateButton("PickaxeButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-330f, 160f), new Vector2(292f, 188f), new Color(0.14f, 0.3f, 0.52f));
            drillButton = CreateButton("DrillButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 160f), new Vector2(292f, 188f), new Color(0.14f, 0.3f, 0.52f));
            robotButton = CreateButton("RobotButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(330f, 160f), new Vector2(292f, 188f), new Color(0.14f, 0.3f, 0.52f));
            pickaxeIcon = CreateIcon("PickaxeIcon", pickaxeButton.transform);
            drillIcon = CreateIcon("DrillIcon", drillButton.transform);
            robotIcon = CreateIcon("RobotIcon", robotButton.transform);
            ConfigureUpgradeCard(pickaxeButton);
            ConfigureUpgradeCard(drillButton);
            ConfigureUpgradeCard(robotButton);
            CreateSettingsPanel();
            RenderSettings();
        }

        private void ApplyUiKit(Texture2D atlas)
        {
            var panelSprite = CreateAtlasSprite(atlas, new Rect(0.035f, 0.748f, 0.448f, 0.158f), new Vector4(0.08f, 0.24f, 0.08f, 0.24f));
            var buttonSprite = CreateAtlasSprite(atlas, new Rect(0.518f, 0.735f, 0.447f, 0.177f), new Vector4(0.08f, 0.25f, 0.08f, 0.25f));
            var cardSprite = CreateAtlasSprite(atlas, new Rect(0.124f, 0.312f, 0.264f, 0.359f), new Vector4(0.16f, 0.10f, 0.16f, 0.10f));

            ApplySlicedSprite(topSurface, panelSprite, Color.white);
            ApplySlicedSprite(upgradeSurface, panelSprite, new Color(0.72f, 0.82f, 1f, 0.92f));
            ApplySlicedSprite(actionSurface, panelSprite, new Color(0.72f, 0.82f, 1f, 0.58f));
            ApplySlicedSprite(progressBackground, panelSprite, new Color(0.58f, 0.7f, 0.92f, 0.96f));
            ApplySlicedSprite(mineButton.image, buttonSprite, Color.white);
            ApplySlicedSprite(pickaxeButton.image, cardSprite, Color.white);
            ApplySlicedSprite(drillButton.image, cardSprite, Color.white);
            ApplySlicedSprite(robotButton.image, cardSprite, Color.white);

            mineButton.GetComponent<Outline>().enabled = false;
            pickaxeButton.GetComponent<Outline>().enabled = false;
            drillButton.GetComponent<Outline>().enabled = false;
            robotButton.GetComponent<Outline>().enabled = false;
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
            var backdrop = CreatePanel("SettingsBackdrop", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1080f, 1920f), new Color(0f, 0f, 0f, 0.72f));
            settingsPanel = backdrop.gameObject;
            var card = CreatePanel("SettingsCard", settingsPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 780f), new Color(0.04f, 0.08f, 0.14f, 0.99f));
            settingsTitle = CreateText("SettingsTitle", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(680f, 76f), 48, TextAnchor.MiddleCenter);
            languageLabel = CreateText("LanguageLabel", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -172f), new Vector2(640f, 54f), 28, TextAnchor.MiddleCenter);

            CreateLanguageButton(card.transform, "KoreanLanguageButton", "\uD55C\uAD6D\uC5B4", SupportedLanguage.Korean, 82f);
            CreateLanguageButton(card.transform, "EnglishLanguageButton", "English", SupportedLanguage.English, -28f);
            CreateLanguageButton(card.transform, "JapaneseLanguageButton", "\u65E5\u672C\u8A9E", SupportedLanguage.Japanese, -138f);
            CreateLanguageButton(card.transform, "ChineseLanguageButton", "\u7B80\u4F53\u4E2D\u6587", SupportedLanguage.ChineseSimplified, -248f);
            closeSettingsButton = CreateButton("CloseSettingsButton", card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 74f), new Vector2(360f, 74f), new Color(0.2f, 0.28f, 0.38f));
            closeSettingsButton.onClick.AddListener(() => settingsPanel.SetActive(false));
            settingsPanel.SetActive(false);
        }

        private void CreateLanguageButton(Transform parent, string name, string label, SupportedLanguage language, float positionY)
        {
            var button = CreateButton(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, positionY), new Vector2(620f, 86f), new Color(0.12f, 0.2f, 0.32f));
            button.GetComponentInChildren<Text>().text = label;
            button.onClick.AddListener(() => LanguageService.SetLanguage(language));
        }

        private void RenderSettings()
        {
            if (settingsButton == null)
            {
                return;
            }

            settingsButton.GetComponentInChildren<Text>().text = LanguageService.Get("settings").ToUpper();
            settingsTitle.text = LanguageService.Get("settings").ToUpper();
            languageLabel.text = LanguageService.Get("language").ToUpper();
            closeSettingsButton.GetComponentInChildren<Text>().text = LanguageService.Get("close").ToUpper();
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
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

        private static Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            var image = CreatePanel(name, parent, anchorMin, anchorMax, position, size, color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
            outline.effectDistance = new Vector2(2f, -2f);
            CreateText("Label", image.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 27, TextAnchor.MiddleCenter);
            return button;
        }

        private static RawImage CreateIcon(string name, Transform parent)
        {
            var iconObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            iconObject.transform.SetParent(parent, false);
            var rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(44f, 0f);
            rect.sizeDelta = new Vector2(64f, 64f);
            var image = iconObject.GetComponent<RawImage>();
            image.raycastTarget = false;
            return image;
        }

        private static RawImage CreateBackdrop(Transform parent)
        {
            var imageObject = new GameObject("CrystalQuarryBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            imageObject.transform.SetParent(parent, false);
            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = imageObject.GetComponent<RawImage>();
            image.color = Color.clear;
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
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.supportRichText = true;
            return text;
        }

        private static void ConfigureUpgradeCard(Button button)
        {
            var label = button.GetComponentInChildren<Text>();
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0.2f, 0f);
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(-12f, 0f);
            label.fontSize = 20;
            label.alignment = TextAnchor.MiddleCenter;
        }

        private static void SetUpgradeText(Button button, string name, int level, string bonus, int cost)
        {
            button.GetComponentInChildren<Text>().text = $"<b>{name.ToUpper()}</b>  Lv.{level}\n<size=18>{bonus}</size>\n<color=#FFD75A>{cost:N0} C</color>";
        }
    }
}
