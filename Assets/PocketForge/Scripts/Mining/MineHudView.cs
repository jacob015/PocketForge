using System;
using System.Collections.Generic;
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
        private RawImage mineIcon;
        private Image headerCoin;
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
        private Image settingsCard;
        private readonly List<Image> languageButtonSurfaces = new();
        private readonly List<Image> upgradeActionSurfaces = new();
        private Image[] pickaxePips;
        private Image[] drillPips;
        private Image[] robotPips;
        private Text feedbackText;
        private MiningGameState lastState;
        private MiningGameService lastService;
        private RectTransform safeAreaRoot;
        private Rect appliedSafeArea = new(-1f, -1f, -1f, -1f);
        private Vector2Int appliedScreenSize = new(-1, -1);

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

        public void SetTheme(Texture upgradeIcons, Texture2D uiKit, Texture2D upgradeButton)
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
        }

        public void Render(MiningGameState state, MiningGameService service)
        {
            lastState = state;
            lastService = service;
            var player = state.Player;
            var ore = state.Ore;
            headerText.text = $"<b><color=#FFD75A>{LanguageService.Get("credits").ToUpper()}  {player.credits:N0}</color>        <color=#9FE8FF>{LanguageService.Get("depth").ToUpper()}  {player.stage}</color></b>";
            oreText.text = $"<b>{(ore.IsRare ? "RARE " : string.Empty)}{LanguageService.Get("ore").ToUpper()}</b>  <color=#D9E4FF>{Mathf.CeilToInt(ore.Health)} / {Mathf.CeilToInt(ore.Durability)}</color>";
            oreProgress.fillAmount = Mathf.Clamp01(ore.Health / ore.Durability);
            oreProgress.color = ore.IsRare ? new Color(0.48f, 0.92f, 1f) : new Color(1f, 0.72f, 0.2f);
            SetUpgradeText(pickaxeButton, LanguageService.Get("pickaxe"), player.pickaxeLevel, $"{LanguageService.Get("tap")} +1", service.GetUpgradeCost(UpgradeType.Pickaxe, player.pickaxeLevel));
            SetUpgradeText(drillButton, LanguageService.Get("drill"), player.drillLevel, $"{LanguageService.Get("auto")} +{service.GetAutoPowerPerSecond(player.drillLevel + 1) - service.GetAutoPowerPerSecond(player.drillLevel):0.0}/s", service.GetUpgradeCost(UpgradeType.Drill, player.drillLevel));
            SetUpgradeText(robotButton, LanguageService.Get("robot"), player.robotLevel, $"{LanguageService.Get("reward")} +10%", service.GetUpgradeCost(UpgradeType.Robot, player.robotLevel));
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
            feedbackText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 1.2f);
        }

        private void HideFeedback() => feedbackText.gameObject.SetActive(false);

        private void LateUpdate() => ApplySafeArea();

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
            safeAreaRoot = CreateSafeAreaRoot(transform);
            ApplySafeArea(true);
            var hudRoot = safeAreaRoot.transform;

            topSurface = CreatePanel("TopSurface", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-62f, -104f), new Vector2(856f, 148f), new Color(0.06f, 0.1f, 0.24f, 0.94f));
            upgradeSurface = CreatePanel("UpgradeSurface", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 228f), new Vector2(970f, 456f), Color.clear);
            actionSurface = CreatePanel("ActionSurface", hudRoot, new Vector2(0.5f, 0.315f), new Vector2(0.5f, 0.315f), Vector2.zero, new Vector2(600f, 250f), Color.clear);
            upgradeSurface.raycastTarget = false;
            actionSurface.raycastTarget = false;

            headerCoin = CreatePanel("HeaderCoin", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-424f, -104f), new Vector2(82f, 82f), Color.white);
            headerCoin.GetComponent<Shadow>().enabled = false;
            headerText = CreateText("Header", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-54f, -104f), new Vector2(650f, 82f), 32, TextAnchor.MiddleCenter);
            settingsButton = CreateButton("SettingsButton", hudRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-62f, -104f), new Vector2(104f, 104f), new Color(0.12f, 0.2f, 0.3f));
            settingsButton.onClick.AddListener(() => settingsPanel.SetActive(true));
            offlineRewardSurface = CreatePanel("OfflineRewardSurface", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -204f), new Vector2(600f, 58f), new Color(0.04f, 0.2f, 0.16f, 0.88f));
            offlineRewardText = CreateText("OfflineReward", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -204f), new Vector2(580f, 48f), 24, TextAnchor.MiddleCenter);
            offlineRewardText.color = new Color(0.45f, 0.95f, 0.7f);
            offlineRewardSurface.gameObject.SetActive(false);
            offlineRewardText.gameObject.SetActive(false);
            oreText = CreateText("OreLabel", hudRoot, new Vector2(0.5f, 0.405f), new Vector2(0.5f, 0.405f), new Vector2(0f, 56f), new Vector2(560f, 50f), 30, TextAnchor.MiddleCenter);
            feedbackText = CreateText("ActionFeedback", hudRoot, new Vector2(0.5f, 0.51f), new Vector2(0.5f, 0.51f), Vector2.zero, new Vector2(600f, 48f), 30, TextAnchor.MiddleCenter);
            feedbackText.gameObject.SetActive(false);

            progressBackground = CreatePanel("ProgressBackground", hudRoot, new Vector2(0.5f, 0.405f), new Vector2(0.5f, 0.405f), Vector2.zero, new Vector2(620f, 54f), new Color(0.05f, 0.1f, 0.24f, 0.96f));
            oreProgress = CreatePanel("ProgressFill", progressBackground.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(582f, 22f), new Color(0.95f, 0.45f, 0.12f));
            oreProgress.GetComponent<Shadow>().enabled = false;
            oreProgress.type = Image.Type.Filled;
            oreProgress.fillMethod = Image.FillMethod.Horizontal;
            oreProgress.fillOrigin = (int)Image.OriginHorizontal.Left;

            mineButton = CreateButton("MineButton", hudRoot, new Vector2(0.5f, 0.315f), new Vector2(0.5f, 0.315f), Vector2.zero, new Vector2(520f, 220f), new Color(1f, 0.48f, 0.12f));
            mineButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            mineIcon = CreateIcon("MineIcon", mineButton.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(168f, 168f));
            pickaxeButton = CreateButton("PickaxeButton", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-324f, 228f), new Vector2(286f, 404f), new Color(0.14f, 0.3f, 0.52f));
            drillButton = CreateButton("DrillButton", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 228f), new Vector2(286f, 404f), new Color(0.14f, 0.3f, 0.52f));
            robotButton = CreateButton("RobotButton", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(324f, 228f), new Vector2(286f, 404f), new Color(0.14f, 0.3f, 0.52f));
            pickaxeIcon = CreateIcon("PickaxeIcon", pickaxeButton.transform);
            drillIcon = CreateIcon("DrillIcon", drillButton.transform);
            robotIcon = CreateIcon("RobotIcon", robotButton.transform);
            ConfigureUpgradeCard(pickaxeButton);
            ConfigureUpgradeCard(drillButton);
            ConfigureUpgradeCard(robotButton);
            pickaxePips = CreateUpgradeDetails(pickaxeButton.transform, new Color(0.28f, 0.72f, 1f));
            drillPips = CreateUpgradeDetails(drillButton.transform, new Color(0.78f, 0.38f, 1f));
            robotPips = CreateUpgradeDetails(robotButton.transform, new Color(1f, 0.7f, 0.16f));
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
            ApplySlicedSprite(settingsCard, cardSprite, Color.white);
            foreach (var surface in languageButtonSurfaces)
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
            settingsCard = CreatePanel("SettingsCard", settingsPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 900f), new Color(0.04f, 0.08f, 0.14f, 0.99f));
            var card = settingsCard;
            settingsTitle = CreateText("SettingsTitle", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(680f, 76f), 48, TextAnchor.MiddleCenter);
            languageLabel = CreateText("LanguageLabel", card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -184f), new Vector2(640f, 54f), 28, TextAnchor.MiddleCenter);

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
            languageButtonSurfaces.Add(button.image);
            button.GetComponentInChildren<Text>().text = label;
            button.onClick.AddListener(() => LanguageService.SetLanguage(language));
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
            rect.anchorMin = new Vector2(0.04f, 0.22f);
            rect.anchorMax = new Vector2(0.96f, 0.50f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            label.fontSize = 22;
            label.alignment = TextAnchor.MiddleCenter;
        }

        private static void SetUpgradeText(Button button, string name, int level, string bonus, int cost)
        {
            button.GetComponentInChildren<Text>().text = $"<b>{name.ToUpper()}</b>  Lv.{level}\n<size=18>{bonus}</size>\n<color=#FFD75A>{cost:N0} C</color>";
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
