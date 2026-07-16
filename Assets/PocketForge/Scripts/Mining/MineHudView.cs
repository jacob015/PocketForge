using System;
using PocketForge.Economy;
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

        public void Render(MiningGameState state, MiningGameService service)
        {
            var player = state.Player;
            var ore = state.Ore;
            headerText.text = $"POCKET FORGE\n<size=32>Credits  {player.credits:N0}     Depth  {player.stage}</size>";
            oreText.text = $"{(ore.IsRare ? "RARE " : string.Empty)}ORE  {Mathf.CeilToInt(ore.Health)} / {Mathf.CeilToInt(ore.Durability)}";
            oreProgress.fillAmount = Mathf.Clamp01(ore.Health / ore.Durability);
            oreProgress.color = ore.IsRare ? new Color(0.4f, 0.9f, 1f) : new Color(0.95f, 0.45f, 0.12f);
            mineButton.GetComponentInChildren<Text>().text = $"MINE  +{service.GetTapPower(player.pickaxeLevel):0}";
            SetUpgradeText(pickaxeButton, $"PICKAXE  Lv.{player.pickaxeLevel}  Tap +1", service.GetUpgradeCost(UpgradeType.Pickaxe, player.pickaxeLevel));
            SetUpgradeText(drillButton, $"DRILL  Lv.{player.drillLevel}  Auto +{service.GetAutoPowerPerSecond(player.drillLevel + 1) - service.GetAutoPowerPerSecond(player.drillLevel):0.0}/s", service.GetUpgradeCost(UpgradeType.Drill, player.drillLevel));
            SetUpgradeText(robotButton, $"ROBOT  Lv.{player.robotLevel}  Reward +10%", service.GetUpgradeCost(UpgradeType.Robot, player.robotLevel));
        }

        private void Build()
        {
            headerText = CreateText("Header", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(960f, 150f), 46, TextAnchor.MiddleCenter);
            oreText = CreateText("OreLabel", transform, new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.53f), new Vector2(0f, 75f), new Vector2(920f, 56f), 30, TextAnchor.MiddleCenter);

            CreatePanel("ProgressBackground", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(920f, 36f), new Color(0.06f, 0.08f, 0.11f, 0.9f));
            oreProgress = CreatePanel("ProgressFill", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(900f, 22f), new Color(0.95f, 0.45f, 0.12f));
            oreProgress.type = Image.Type.Filled;
            oreProgress.fillMethod = Image.FillMethod.Horizontal;
            oreProgress.fillOrigin = (int)Image.OriginHorizontal.Left;

            mineButton = CreateButton("MineButton", transform, new Vector2(0.5f, 0.41f), new Vector2(0.5f, 0.41f), new Vector2(0f, 0f), new Vector2(920f, 126f), new Color(0.86f, 0.28f, 0.08f));
            pickaxeButton = CreateButton("PickaxeButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 310f), new Vector2(920f, 86f), new Color(0.15f, 0.22f, 0.32f));
            drillButton = CreateButton("DrillButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 210f), new Vector2(920f, 86f), new Color(0.15f, 0.22f, 0.32f));
            robotButton = CreateButton("RobotButton", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(920f, 86f), new Color(0.15f, 0.22f, 0.32f));
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
            return image;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            var image = CreatePanel(name, parent, anchorMin, anchorMax, position, size, color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CreateText("Label", image.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 27, TextAnchor.MiddleCenter);
            return button;
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
