using System.Linq;
using NUnit.Framework;
using PocketForge.Mining;
using PocketForge.Presentation;
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
            AssertRect("SettingsButton", new Vector2(-92f, -123f), new Vector2(116f, 116f));
            AssertRect("RewardedAdButton", new Vector2(263f, -254f), new Vector2(400f, 88f));
            AssertRect("OreBadge", new Vector2(0f, 64f), new Vector2(322f, 66f));
            AssertRect("ProgressBackground", new Vector2(0f, 22f), new Vector2(638f, 64f));
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
