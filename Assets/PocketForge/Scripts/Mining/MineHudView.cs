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
        private Text offlineRewardText;
        private Image offlineRewardSurface;
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

        public void SetTheme(Texture upgradeIcons)
        {
            if (upgradeIcons == null)
            {
                return;
            }

            SetIcon(pickaxeIcon, upgradeIcons, 0);
            SetIcon(drillIcon, upgradeIcons, 1);
            SetIcon(robotIcon, upgradeIcons, 2);
        }

        public void Render(MiningGameState state, MiningGameService service)
        {
            lastState = state;
            lastService = service;
            var player = state.Player;
            var ore = state.Ore;
            headerText.text = $"POCKET FORGE\n<size=32>{LanguageService.Get("credits")}  {player.credits:N0}     {LanguageService.Get("depth")}  {player.stage}</size>";
            oreText.text = $"{(ore.IsRare ? "RARE " : string.Empty)}{LanguageService.Get("ore").ToUpper()}  {Mathf.CeilToInt(ore.Health)} / {Mathf.CeilToInt(ore.Durability)}";
            oreProgress.fillAmount = Mathf.Clamp01(ore.Health / ore.Durability);
            oreProgress.color = ore.IsRare ? new Color(0.4f, 0.9f, 1f) : new Color(0.95f, 0.45f, 0.12f);
            mineButton.GetComponentInChildren<Text>().text = $"{LanguageService.Get("mine").ToUpper()}  +{service.GetTapPower(player.pickaxeLevel):0}";
            SetUpgradeText(pickaxeButton, $"{LanguageService.Get("pickaxe").ToUpper()}  Lv.{player.pickaxeLevel}  {LanguageService.Get("tap")} +1", service.GetUpgradeCost(UpgradeType.Pickaxe, player.pickaxeLevel));
            SetUpgradeText(drillButton, $"{LanguageService.Get("drill").ToUpper()}  Lv.{player.drillLevel}  {LanguageService.Get("auto")} +{service.GetAutoPowerPerSecond(player.drillLevel + 1) - service.GetAutoPowerPerSecond(player.drillLevel):0.0}/s", service.GetUpgradeCost(UpgradeType.Drill, player.drillLevel));
            SetUpgradeText(robotButton, $"{LanguageService.Get("robot").ToUpper()}  Lv.{player.robotLevel}  {LanguageService.Get("reward")} +10%", service.GetUpgradeCost(UpgradeType.Robot, player.robotLevel));
        }

        public void ShowOfflineReward(int credits)
        {
            offlineRewardSurface.gameObject.SetActive(true);
            offlineRewardText.gameObject.SetActive(true);
            offlineRewardText.text = $"{LanguageService.Get("reward").ToUpper()}  +{credits:N0} C";
        }

        private void RefreshLocalization()
        {
            if (lastState != null && lastService != null)
            {
                Render(lastState, lastService);
            }
        }

        private void Build()
        {
            CreatePanel("TopSurface", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -142f), new Vector2(1000f, 230f), new Color(0.035f, 0.07f, 0.13f, 0.94f));
            CreatePanel("UpgradeSurface", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(1000f, 480f), new Color(0.025f, 0.045f, 0.085f, 0.9f));
            CreatePanel("MineSurface", transform, new Vector2(0.5f, 0.41f), new Vector2(0.5f, 0.41f), new Vector2(0f, 0f), new Vector2(970f, 176f), new Color(0.05f, 0.07f, 0.1f, 0.82f));

            headerText = CreateText("Header", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -126f), new Vector2(920f, 142f), 48, TextAnchor.MiddleCenter);
            offlineRewardSurface = CreatePanel("OfflineRewardSurface", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -258f), new Vector2(790f, 76f), new Color(0.04f, 0.2f, 0.16f, 0.96f));
            offlineRewardText = CreateText("OfflineReward", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -258f), new Vector2(760f, 64f), 28, TextAnchor.MiddleCenter);
            offlineRewardText.color = new Color(0.45f, 0.95f, 0.7f);
            offlineRewardSurface.gameObject.SetActive(false);
            offlineRewardText.gameObject.SetActive(false);
            oreText = CreateText("OreLabel", transform, new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.53f), new Vector2(0f, 88f), new Vector2(920f, 56f), 30, TextAnchor.MiddleCenter);

            CreatePanel("ProgressBackground", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(920f, 36f), new Color(0.06f, 0.08f, 0.11f, 0.9f));
            oreProgress = CreatePanel("ProgressFill", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(900f, 22f), new Color(0.95f, 0.45f, 0.12f));
            oreProgress.type = Image.Type.Filled;
            oreProgress.fillMethod = Image.FillMethod.Horizontal;
            oreProgress.fillOrigin = (int)Image.OriginHorizontal.Left;

            mineButton = CreateButton("MineButton", transform, new Vector2(0.5f, 0.41f), new Vector2(0.5f, 0.41f), new Vector2(0f, 0f), new Vector2(920f, 126f), new Color(0.94f, 0.31f, 0.06f));
            pickaxeButton = CreateButton("PickaxeButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 310f), new Vector2(920f, 86f), new Color(0.15f, 0.22f, 0.32f));
            drillButton = CreateButton("DrillButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 210f), new Vector2(920f, 86f), new Color(0.15f, 0.22f, 0.32f));
            robotButton = CreateButton("RobotButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(920f, 86f), new Color(0.15f, 0.22f, 0.32f));
            pickaxeIcon = CreateIcon("PickaxeIcon", pickaxeButton.transform);
            drillIcon = CreateIcon("DrillIcon", drillButton.transform);
            robotIcon = CreateIcon("RobotIcon", robotButton.transform);
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
            rect.anchoredPosition = new Vector2(62f, 0f);
            rect.sizeDelta = new Vector2(88f, 88f);
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

        private static void SetUpgradeText(Button button, string label, int cost)
        {
            button.GetComponentInChildren<Text>().text = $"{label}    [{cost:N0} C]";
        }
    }
}
