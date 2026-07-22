using System.Linq;
using NUnit.Framework;
using PocketForge.Mining;
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
    }
}
