using System;
using System.Collections.Generic;
using PocketForge.Economy;
using PocketForge.Localization;
using PocketForge.Presentation;
using PocketForge.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace PocketForge.Mining
{
    public sealed partial class MineHudView
    {
        private Text blueprintCoreText;
        private Text chapterStageText;
        private Text mineNameText;
        private Text powerText;
        private Text powerLabelText;
        private Text recommendedPowerText;
        private Text bossWarningText;
        private Text oreHealthLabel;
        private Text oreHealthValue;
        private Text bossActionText;
        private Text offlineRewardTitle;
        private Text researchShortcutText;
        private Text researchShortcutValue;
        private Image portraitFrame;
        private Image portraitImage;
        private Image minerExperienceIcon;
        private Image blueprintCoreIcon;
        private Image chapterPanel;
        private Image powerPanel;
        private Image bossWarningPanel;
        private Image offlineChestIcon;
        private Image researchShortcutSurface;
        private Image researchShortcutIcon;
        private Image researchCompletionBadge;
        private Image bottomNavigationBar;
        private Image researchNotificationBadge;
        private Image missionNotificationBadge;
        private Button bossChallengeButton;
        private Button researchShortcutButton;
        private readonly List<Image> recommendationBadges = new();
        private readonly List<Text> upgradeEffectTexts = new();
        private readonly Dictionary<ProgressionFeature, Image> navigationLocks = new();
        private readonly Dictionary<ProgressionFeature, Button> navigationButtons = new();
        private readonly List<Image> navigationIcons = new();

        private void BuildV5Hud(Transform hudRoot)
        {
            ConfigureRect(topSurface.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -189f), new Vector2(972f, 162f));
            topSurface.raycastTarget = false;

            portraitFrame = CreateSimpleImage(
                "PortraitFrame",
                hudRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-379.5f, -191.5f),
                new Vector2(144f, 144f),
                Color.white);
            portraitImage = CreateSimpleImage(
                "MinerPortrait",
                portraitFrame.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(108f, 108f),
                Color.white);
            portraitImage.transform.SetAsFirstSibling();

            ConfigureRect(minerRankButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(-231.9f, -185f), new Vector2(184f, 104f));
            minerRankButton.image.color = Color.clear;
            minerRankButton.GetComponent<Outline>().enabled = false;
            minerRankButton.GetComponent<Shadow>().enabled = false;
            minerRankText.rectTransform.anchorMin = minerRankText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            minerRankText.rectTransform.anchoredPosition = new Vector2(-146.3f, -86.7f);
            minerRankText.rectTransform.sizeDelta = new Vector2(142f, 42f);
            minerRankText.fontSize = 24;
            // Without best-fit the longer English label ("MINER Lv. 63") is cut off
            // rather than shrunk.
            minerRankText.resizeTextForBestFit = true;
            minerRankText.resizeTextMaxSize = 24;
            minerRankText.resizeTextMinSize = MinimumReadableFontSize;
            // Icon sits left of the baked pill text zone; text and track share the
            // remaining span so neither is covered by the icon or the portrait.
            minerExperienceText.rectTransform.anchoredPosition = new Vector2(35f, -1f);
            minerExperienceText.rectTransform.sizeDelta = new Vector2(110f, 30f);
            minerExperienceText.fontSize = 18;
            minerExperienceText.resizeTextForBestFit = true;
            // Must not shrink below the shared readability floor; the compact XP string
            // ("440 / 620") fits the 110 wide box at that size.
            minerExperienceText.resizeTextMinSize = MinimumReadableFontSize;
            minerExperienceText.resizeTextMaxSize = 18;
            minerExperienceText.horizontalOverflow = HorizontalWrapMode.Wrap;
            var experienceTrack = minerExperienceProgress.transform.parent.GetComponent<RectTransform>();
            experienceTrack.anchoredPosition = new Vector2(28f, -20f);
            experienceTrack.sizeDelta = new Vector2(96f, 8f);
            minerExperienceProgress.rectTransform.anchoredPosition = new Vector2(4f, 0f);
            minerExperienceProgress.rectTransform.sizeDelta = new Vector2(MinerExperienceProgressWidth, 6f);
            minerExperienceIcon = CreateSimpleImage(
                "MinerExperienceIcon",
                minerRankButton.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-50f, -1f),
                new Vector2(48f, 48f),
                Color.white);

            ConfigureRect(creditsCurrencyIcon.rectTransform, new Vector2(0.5f, 1f), new Vector2(-124f, -192f), new Vector2(48f, 48f));
            ConfigureText(creditsText, new Vector2(0.5f, 1f), new Vector2(-55f, -189f), new Vector2(90f, 60f), 26, TextAnchor.MiddleLeft);
            ConfigureRect(depthCurrencyIcon.rectTransform, new Vector2(0.5f, 1f), new Vector2(17.5f, -192f), new Vector2(48f, 48f));
            ConfigureText(depthText, new Vector2(0.5f, 1f), new Vector2(83f, -189f), new Vector2(76f, 60f), 26, TextAnchor.MiddleLeft);
            blueprintCoreIcon = CreateSimpleImage(
                "BlueprintCoreIcon",
                hudRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(156f, -189f),
                new Vector2(48f, 48f),
                Color.white);
            blueprintCoreText = CreateText(
                "BlueprintCoreValue",
                hudRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(220.9f, -189f),
                new Vector2(76f, 60f),
                26,
                TextAnchor.MiddleLeft);

            ConfigureRect(settingsButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(403f, -189f), new Vector2(84f, 84f));
            ConfigureRect(settingsIcon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(74f, 74f));
            // Centered inside the fourth recessed pill baked into 01_HudPanel.
            ConfigureRect(rewardedAdButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(308f, -190f), new Vector2(100f, 62f));
            rewardedAdButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            rewardedVideoIcon.gameObject.SetActive(false);
            rewardedPlusIcon.rectTransform.anchorMin = rewardedPlusIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rewardedPlusIcon.rectTransform.anchoredPosition = Vector2.zero;
            rewardedPlusIcon.rectTransform.sizeDelta = new Vector2(46f, 46f);
            rewardedPlusIcon.rectTransform.localScale = Vector3.one;

            BuildV5InformationPanels(hudRoot);
            BuildV5ActionArea(hudRoot);
            BuildV5Navigation(hudRoot);
        }

        private void BuildV5InformationPanels(Transform hudRoot)
        {
            chapterPanel = CreatePanel(
                "ChapterInformationPanel",
                hudRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-168f, -394f),
                new Vector2(636f, 188f),
                Color.white);
            chapterPanel.GetComponent<Shadow>().enabled = false;
            chapterStatusButton = chapterPanel.gameObject.AddComponent<Button>();
            chapterStatusButton.targetGraphic = chapterPanel;
            chapterPanel.gameObject.AddComponent<MobileButtonFeedback>();
            chapterStageText = CreateText(
                "ChapterStage",
                chapterPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(70f, 34f),
                new Vector2(430f, 42f),
                25,
                TextAnchor.MiddleLeft);
            chapterStageText.color = new Color(0.5f, 0.92f, 1f);
            mineNameText = CreateText(
                "MineName",
                chapterPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(70f, -22.4f),
                new Vector2(430f, 66f),
                42,
                TextAnchor.MiddleLeft);
            chapterStatusText.gameObject.SetActive(false);

            // 265 wide makes the panel art draw 145 tall, matching the chapter panel's
            // drawn height so both tops and bottoms line up; x keeps the outer margin
            // symmetric with the chapter panel's left margin.
            powerPanel = CreatePanel(
                "PowerComparisonPanel",
                hudRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(353f, -394f),
                new Vector2(265f, 188f),
                Color.white);
            powerPanel.GetComponent<Shadow>().enabled = false;
            powerPanel.raycastTarget = false;
            // Offsets and boxes scale with the panel (265/306) so the text keeps clearing
            // the strength icon baked into the left of the panel art.
            powerText = CreateText(
                "PowerValue",
                powerPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(24f, 7f),
                new Vector2(190f, 62f),
                38,
                TextAnchor.MiddleCenter);
            powerLabelText = CreateText(
                "PowerLabel",
                powerPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(33f, 50f),
                new Vector2(170f, 28f),
                MinimumReadableFontSize,
                TextAnchor.MiddleCenter);
            recommendedPowerText = CreateText(
                "RecommendedPower",
                powerPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(16f, -43f),
                new Vector2(204f, 32f),
                MinimumReadableFontSize,
                TextAnchor.MiddleCenter);
            recommendedPowerText.color = new Color(1f, 0.76f, 0.3f);

            bossWarningPanel = CreatePanel(
                "BossWarningPanel",
                hudRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -560f),
                new Vector2(300f, 96f),
                Color.white);
            bossWarningPanel.GetComponent<Shadow>().enabled = false;
            bossWarningPanel.raycastTarget = false;
            bossWarningText = CreateText(
                "BossWarning",
                bossWarningPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(38f, 0f),
                new Vector2(202f, 72f),
                24,
                TextAnchor.MiddleCenter);
            bossWarningText.color = new Color(1f, 0.68f, 0.58f);

            ConfigureRect(progressBackground.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 1014f), new Vector2(560f, 86f));
            ConfigureRect(progressTrack.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-1f, 0.5f), new Vector2(OreProgressWidth, 54f));
            ConfigureRect(oreProgress.rectTransform, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(OreProgressWidth, 54f));
            oreProgress.rectTransform.pivot = new Vector2(0f, 0.5f);
            oreHealthLabel = CreateText(
                "OreHealthLabel",
                progressBackground.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 64f),
                new Vector2(270f, 38f),
                24,
                TextAnchor.MiddleCenter);
            oreHealthValue = CreateText(
                "OreHealthValue",
                progressBackground.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(430f, 40f),
                24,
                TextAnchor.MiddleCenter);
            oreHealthValue.raycastTarget = false;
        }

        private void BuildV5ActionArea(Transform hudRoot)
        {
            ConfigureRect(mineButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 843f), new Vector2(450f, 186f));
            ConfigureRect(mineIcon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(78.912f, 85.248f));

            ConfigureRect(offlineRewardSurface.rectTransform, new Vector2(0.5f, 0f), new Vector2(-363f, 843f), new Vector2(246f, 186f));
            offlineRewardSurface.GetComponent<Shadow>().enabled = false;
            offlineRewardText.transform.SetParent(offlineRewardSurface.transform, false);
            ConfigureText(offlineRewardText, new Vector2(0.5f, 0.5f), new Vector2(0f, -56f), new Vector2(202f, 50f), 18, TextAnchor.MiddleCenter);
            offlineRewardTitle = CreateText(
                "OfflineRewardTitle",
                offlineRewardSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f),
                new Vector2(202f, 36f),
                18,
                TextAnchor.MiddleCenter);
            offlineChestIcon = CreateSimpleImage(
                "OfflineChestIcon",
                offlineRewardSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 8f),
                new Vector2(112f, 92f),
                Color.white);

            researchShortcutButton = CreateButton(
                "ResearchShortcut",
                hudRoot,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(363f, 843f),
                new Vector2(246f, 186f),
                Color.white);
            researchShortcutSurface = researchShortcutButton.image;
            researchShortcutButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            researchShortcutButton.onClick.AddListener(ShowMinerRankSummary);
            // Mirrors the offline card's title / icon / value stack so the two shortcut
            // cards flanking the mine button read as a matched pair.
            researchShortcutIcon = CreateSimpleImage(
                "ResearchShortcutIcon",
                researchShortcutButton.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 8f),
                new Vector2(112f, 92f),
                Color.white);
            researchShortcutText = CreateText(
                "ResearchShortcutText",
                researchShortcutButton.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f),
                new Vector2(204f, 36f),
                MinimumReadableFontSize,
                TextAnchor.MiddleCenter);
            researchShortcutValue = CreateText(
                "ResearchShortcutValue",
                researchShortcutButton.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -56f),
                new Vector2(202f, 50f),
                MinimumReadableFontSize,
                TextAnchor.MiddleCenter);
            researchShortcutValue.color = new Color(0.55f, 0.92f, 1f);
            // The card art draws ~111 canvas units wide inside the 246-wide rect, so the
            // badge is anchored to the drawn card corner instead of the rect corner.
            researchCompletionBadge = CreateSimpleImage(
                "ResearchCompletionBadge",
                researchShortcutButton.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-76f, -10f),
                new Vector2(48f, 48f),
                Color.white);

            bossChallengeButton = CreateButton(
                "BossChallengeButton",
                hudRoot,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 650f),
                new Vector2(322f, 90f),
                new Color(0.92f, 0.32f, 0.26f));
            bossChallengeButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            bossChallengeButton.onClick.AddListener(TryStartBossChallenge);
            var bossIcon = CreateSimpleImage(
                "BossChallengeIcon",
                bossChallengeButton.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(52f, 0f),
                new Vector2(76f, 76f),
                Color.white);
            bossIcon.name = "BossChallengeIcon";
            bossActionText = CreateText(
                "BossActionText",
                bossChallengeButton.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(43.051f, 0f),
                new Vector2(209.897f, 64f),
                25,
                TextAnchor.MiddleCenter);
            bossActionText.font = UiFontProvider.GetCasual();
            bossActionText.resizeTextForBestFit = true;
            bossActionText.resizeTextMinSize = MinimumReadableFontSize;
            bossActionText.resizeTextMaxSize = 25;

            ConfigureRect(pickaxeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(-327f, 420f), new Vector2(302f, 330f));
            ConfigureRect(drillButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 420f), new Vector2(302f, 330f));
            ConfigureRect(robotButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(327f, 420f), new Vector2(302f, 330f));
            ConfigureV5UpgradeCard(pickaxeButton, pickaxeIcon);
            ConfigureV5UpgradeCard(drillButton, drillIcon);
            ConfigureV5UpgradeCard(robotButton, robotIcon);
        }

        private void BuildV5Navigation(Transform hudRoot)
        {
            bottomNavigationBar = CreateSimpleImage(
                "BottomNavigationBar",
                hudRoot,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 120f),
                new Vector2(948f, 160f),
                Color.white);
            bottomNavigationBar.transform.SetSiblingIndex(1);
            CreateNavigationSlot("EquipmentNavigation", bottomNavigationBar.transform, new Vector2(-296f, -28f), ProgressionFeature.Equipment);
            CreateNavigationSlot("ResearchNavigation", bottomNavigationBar.transform, new Vector2(-162f, -28f), ProgressionFeature.Research);
            CreateNavigationSlot("HomeNavigation", bottomNavigationBar.transform, new Vector2(0f, -13f), null);
            CreateNavigationSlot("MuseumNavigation", bottomNavigationBar.transform, new Vector2(162f, -32f), ProgressionFeature.Museum);
            CreateNavigationSlot("MissionsNavigation", bottomNavigationBar.transform, new Vector2(455f, 1529f), ProgressionFeature.Missions);
            CreateNavigationSlot("ShopNavigation", bottomNavigationBar.transform, new Vector2(291f, -28f), ProgressionFeature.Shop);
            ConfigureRect(
                navigationButtons[ProgressionFeature.Missions].transform.Find("Icon").GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f),
                new Vector2(93.6f, 93.6f));

            // Anchored to the drawn icon corners (icons are smaller than the 144 slot rect).
            researchNotificationBadge = CreateSimpleImage(
                "ResearchNotificationBadge",
                navigationButtons[ProgressionFeature.Research].transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-38f, -20f),
                new Vector2(38f, 38f),
                Color.white);
            missionNotificationBadge = CreateSimpleImage(
                "MissionNotificationBadge",
                navigationButtons[ProgressionFeature.Missions].transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-28f, -12f),
                new Vector2(38f, 38f),
                Color.white);
        }

        private void CreateNavigationSlot(
            string name,
            Transform parent,
            Vector2 position,
            ProgressionFeature? feature)
        {
            var button = CreateButton(
                name,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(144f, 144f),
                Color.clear);
            button.image.color = Color.clear;
            button.GetComponent<Outline>().enabled = false;
            button.GetComponent<Shadow>().enabled = false;
            button.GetComponentInChildren<Text>().gameObject.SetActive(false);

            var icon = CreateSimpleImage(
                "Icon",
                button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f),
                new Vector2(72f, 72f),
                Color.white);
            navigationIcons.Add(icon);
            if (!feature.HasValue)
            {
                button.interactable = false;
                return;
            }

            navigationButtons[feature.Value] = button;
            var captured = feature.Value;
            if (captured == ProgressionFeature.Equipment)
            {
                button.onClick.AddListener(ShowEquipment);
            }
            else if (captured == ProgressionFeature.Research)
            {
                button.onClick.AddListener(ShowMinerRankSummary);
            }
            else if (captured == ProgressionFeature.Museum)
            {
                button.onClick.AddListener(ShowCollection);
            }
            else if (captured == ProgressionFeature.Missions)
            {
                button.onClick.AddListener(ShowMissions);
            }
            else if (captured == ProgressionFeature.Shop)
            {
                button.onClick.AddListener(ShowCommerce);
            }
            else
            {
                button.onClick.AddListener(() => ShowFeaturePlaceholder(captured));
            }

            var lockIcon = CreateSimpleImage(
                "LockedIcon",
                button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(31f, 24f),
                new Vector2(42f, 42f),
                Color.white);
            navigationLocks[captured] = lockIcon;
        }

        private void ConfigureV5UpgradeCard(Button button, RawImage icon)
        {
            ConfigureRect(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 68f), new Vector2(190f, 148f));
            var level = button.transform.Find("Label").GetComponent<Text>();
            level.rectTransform.anchorMin = level.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            level.rectTransform.anchoredPosition = new Vector2(0f, -35f);
            level.rectTransform.sizeDelta = new Vector2(250f, 42f);
            level.fontSize = 27;
            var cost = button.transform.Find("CostText").GetComponent<Text>();
            cost.rectTransform.anchorMin = cost.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            cost.rectTransform.anchoredPosition = new Vector2(11.8f, -120.7f);
            cost.rectTransform.sizeDelta = new Vector2(140f, 42f);
            cost.fontSize = 26;
            var costIcon = button.transform.Find("CostIcon").GetComponent<Image>();
            ConfigureRect(costIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-60.2f, -124.6f), new Vector2(38f, 38f));
            var action = button.transform.Find("UpgradeAction").GetComponent<Image>();
            ConfigureRect(action.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(101f, -128f), new Vector2(48f, 48f));
            action.color = Color.clear;
            var actionIcon = action.transform.Find("UpgradeArrow").GetComponent<Image>();
            ConfigureRect(actionIcon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(46f, 46f));
            for (var index = 1; index <= 3; index++)
            {
                button.transform.Find($"LevelPip{index}").gameObject.SetActive(false);
            }

            // The card art bakes a recessed pill between the level and cost rows; it is
            // filled with this facility's current contribution instead of left empty.
            var effect = CreateText(
                "UpgradeEffect",
                button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -82f),
                new Vector2(152f, 26f),
                MinimumReadableFontSize,
                TextAnchor.MiddleCenter);
            effect.color = new Color(0.55f, 0.92f, 1f);
            effect.raycastTarget = false;
            upgradeEffectTexts.Add(effect);

            var badge = CreateSimpleImage(
                "RecommendationBadge",
                button.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-34f, -34f),
                new Vector2(54f, 54f),
                Color.white);
            recommendationBadges.Add(badge);
        }

        private void ApplyV5HudSkin()
        {
            if (finalSkin == null)
            {
                return;
            }

            ApplyV5(topSurface, "01_HudPanel");
            minerRankButton.image.sprite = null;
            minerRankButton.image.color = Color.clear;
            ApplyV5(portraitFrame, "02_PortraitFrame");
            ApplyV5(portraitImage, "03_MinerPortrait");
            ApplyV5(creditsCurrencyIcon, "04_CreditsIcon");
            ApplyV5(depthCurrencyIcon, "05_GemIcon");
            ApplyV5(blueprintCoreIcon, "06_BlueprintCoreIcon");
            ApplyV5(minerExperienceIcon, "07_MinerExperienceIcon");
            ApplyV5(settingsButton.image, "08_SettingsButton");
            settingsIcon.gameObject.SetActive(false);
            ApplyV5(chapterPanel, "09_ChapterPanel");
            ApplyV5(powerPanel, "10_PowerPanel");
            ApplyV5(bossWarningPanel, "11_BossWarningPanel");
            ApplyV5(progressBackground, "12_OreHealthFrame");
            progressTrack.sprite = null;
            progressTrack.color = Color.clear;
            ApplyV5(oreProgress, "13_OreHealthFill", false);
            oreProgress.type = Image.Type.Filled;
            oreProgress.fillMethod = Image.FillMethod.Horizontal;
            oreProgress.fillOrigin = 0;
            ApplyV5(offlineRewardSurface, "14_OfflineShortcutCard");
            ApplyV5(offlineChestIcon, "15_OfflineChestIcon");
            ApplyV5(researchShortcutSurface, "14_OfflineShortcutCard");
            ApplyV5(researchShortcutIcon, "16_ResearchCompleteIcon");
            ApplyV5(researchCompletionBadge, "17_CompletionBadge");
            ApplyV5(mineButton.image, "18_MineButton");
            ApplyRawTexture(mineIcon, finalSkin.V5Texture("19_MinePickaxeIcon"), new Vector2(148f, 148f));
            ConfigureRect(mineIcon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(78.912f, 85.248f));
            ApplyV5(bossChallengeButton.image, "20_BossButton");
            ApplyV5(bossChallengeButton.transform.Find("BossChallengeIcon").GetComponent<Image>(), "21_BossChallengeIcon");
            ApplyV5(pickaxeButton.image, "22_UpgradeCard");
            ApplyV5(drillButton.image, "22_UpgradeCard");
            ApplyV5(robotButton.image, "22_UpgradeCard");
            foreach (var badge in recommendationBadges)
            {
                ApplyV5(badge, "23_RecommendationBadge");
            }

            foreach (var icon in upgradeActionIcons)
            {
                ApplyV5(icon, "24_UpgradeArrow");
            }

            ApplyRawTexture(pickaxeIcon, finalSkin.V5Texture("25_PickaxeEquipmentIcon"), new Vector2(190f, 156f));
            ApplyRawTexture(drillIcon, finalSkin.V5Texture("26_DrillEquipmentIcon"), new Vector2(190f, 156f));
            ApplyRawTexture(robotIcon, finalSkin.V5Texture("27_RobotEquipmentIcon"), new Vector2(190f, 156f));
            ConfigureRect(pickaxeIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 68f), new Vector2(144f, 156f));
            ConfigureRect(drillIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 68f), new Vector2(164.255f, 140.79f));
            ConfigureRect(robotIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 68f), new Vector2(117f, 156f));
            foreach (var icon in upgradeCostIcons)
            {
                ApplyV5(icon, "04_CreditsIcon");
            }

            ApplyV5(bottomNavigationBar, "28_BottomNavigationBar");
            var navigationAssets = new[]
            {
                "30_EquipmentMenuIcon",
                "31_ResearchMenuIcon",
                "32_HomeMenuIcon",
                "33_MuseumMenuIcon",
                "34_MissionMenuIcon",
                "35_ShopMenuIcon"
            };
            for (var index = 0; index < navigationIcons.Count && index < navigationAssets.Length; index++)
            {
                ApplyV5(navigationIcons[index], navigationAssets[index]);
            }

            ApplyV5(researchNotificationBadge, "36_NotificationBadge");
            ApplyV5(missionNotificationBadge, "36_NotificationBadge");
            foreach (var lockIcon in navigationLocks.Values)
            {
                ApplyV5(lockIcon, "37_LockedIcon");
            }

            ApplyV5(rewardedPlusIcon, "38_AddActionIcon");
            rewardedAdButton.image.sprite = null;
            rewardedAdButton.image.color = Color.clear;
            foreach (var surface in upgradeActionSurfaces)
            {
                surface.sprite = null;
                surface.color = Color.clear;
            }
            DisableOutline(bossChallengeButton);
            DisableOutline(researchShortcutButton);
        }

        private void RenderV5Hud(MiningGameState state, MiningGameService service)
        {
            if (blueprintCoreText == null)
            {
                return;
            }

            var player = state.Player;
            var ore = state.Ore;
            var chapterStage = ore.Chapter.GetStageNumber(player.stage);
            var stagesUntilBoss = Mathf.Max(0, ore.Chapter.StagesPerChapter - chapterStage);
            var power = service.GetMiningPower(state);
            var requiredPower = service.GetBossRecommendedPower(state);
            var bossReady = service.IsBossChallengeReady(state);

            creditsText.text = CompactNumberFormatter.Format(player.credits);
            depthText.text = CompactNumberFormatter.Format(player.gems);
            blueprintCoreText.text = CompactNumberFormatter.Format(player.blueprintCores);
            chapterStageText.text =
                $"{LanguageService.Get("chapter").ToUpper()} {ore.Chapter.ChapterNumber}  ·  {LanguageService.Get("stage").ToUpper()} {chapterStage}/{ore.Chapter.StagesPerChapter}";
            mineNameText.text = LanguageService.Get(GetMineNameKey(ore.Chapter.ContentId)).ToUpper();
            powerText.text = CompactNumberFormatter.Format(power.ActivePowerPerSecond);
            powerLabelText.text = LanguageService.Get("power").ToUpper();
            recommendedPowerText.text = requiredPower > 0f
                ? $"{LanguageService.Get("recommended").ToUpper()} {CompactNumberFormatter.Format(requiredPower)}"
                : LanguageService.Get("power").ToUpper();
            bossWarningText.text = ore.IsBoss
                ? $"{LanguageService.Get("boss").ToUpper()}\n{FormatTime(ore.BossTimeRemaining)}"
                : bossReady
                    ? LanguageService.Get("boss_ready").ToUpper()
                    : string.Format(LanguageService.Get("boss_in_stages"), stagesUntilBoss).ToUpper();
            oreHealthLabel.text = LanguageService.Get("ore_health").ToUpper();
            oreHealthValue.text =
                $"{CompactNumberFormatter.Format((long)Mathf.Ceil(ore.Health))} / {CompactNumberFormatter.Format((long)Mathf.Ceil(ore.Durability))}";
            oreProgress.fillAmount = Mathf.Clamp01(ore.Health / Mathf.Max(1f, ore.Durability));
            bossActionText.text =
                $"{LanguageService.Get("boss").ToUpper()} {LanguageService.Get("challenge").ToUpper()}";
            offlineRewardTitle.text = LanguageService.Get("offline_rewards").ToUpper();
            researchShortcutText.text = LanguageService.Get("feature_research").ToUpper();
            researchShortcutValue.text =
                $"+{(service.GetResearchPowerMultiplier(state) - 1f) * 100f:0.#}%";
            researchCompletionBadge.gameObject.SetActive(
                service.IsFeatureUnlocked(player.minerLevel, ProgressionFeature.Research));
            researchNotificationBadge.gameObject.SetActive(
                service.IsFeatureUnlocked(player.minerLevel, ProgressionFeature.Research) &&
                player.blueprintCores > 0);
            if (service.IsFeatureUnlocked(player.minerLevel, ProgressionFeature.Missions))
            {
                var dailyMissions = service.GetMissionBoard(
                    state,
                    PocketForge.Content.MissionPeriod.Daily);
                var weeklyMissions = service.GetMissionBoard(
                    state,
                    PocketForge.Content.MissionPeriod.Weekly);
                missionNotificationBadge.gameObject.SetActive(
                    dailyMissions.ClaimableCount > 0 ||
                    weeklyMissions.ClaimableCount > 0 ||
                    dailyMissions.CanClaimCompletion ||
                    weeklyMissions.CanClaimCompletion);
            }
            else
            {
                missionNotificationBadge.gameObject.SetActive(false);
            }
            bossChallengeButton.gameObject.SetActive(bossReady || ore.IsBoss);
            bossChallengeButton.interactable = bossReady && !ore.IsBoss;

            var costs = new[]
            {
                service.GetUpgradeCost(UpgradeType.Pickaxe, player.pickaxeLevel),
                service.GetUpgradeCost(UpgradeType.Drill, player.drillLevel),
                service.GetUpgradeCost(UpgradeType.Robot, player.robotLevel)
            };
            var recommendation = -1;
            var lowestAffordableCost = long.MaxValue;
            for (var index = 0; index < costs.Length; index++)
            {
                if (costs[index] <= player.credits && costs[index] < lowestAffordableCost)
                {
                    recommendation = index;
                    lowestAffordableCost = costs[index];
                }
            }

            for (var index = 0; index < recommendationBadges.Count; index++)
            {
                recommendationBadges[index].gameObject.SetActive(index == recommendation);
            }

            if (upgradeEffectTexts.Count >= 3)
            {
                upgradeEffectTexts[0].text =
                    $"{LanguageService.Get("tap")} {CompactNumberFormatter.Format(power.TapDamage)}";
                upgradeEffectTexts[1].text =
                    $"{LanguageService.Get("auto")} {CompactNumberFormatter.Format(power.DrillPower)}";
                upgradeEffectTexts[2].text =
                    $"{LanguageService.Get("reward")} \u00D7{power.RobotMultiplier:0.00}";
            }

            UpdateNavigationState(service, player.minerLevel);
        }

        private void RenderV5OfflineReward(OfflineProgressResult result)
        {
            if (offlineRewardText == null)
            {
                return;
            }

            offlineRewardText.text =
                $"{FormatOfflineDuration(result.RewardedSeconds)}\n+{CompactNumberFormatter.Format(result.RewardCredits)}";
        }

        private void UpdateNavigationState(MiningGameService service, int minerLevel)
        {
            foreach (var pair in navigationLocks)
            {
                pair.Value.gameObject.SetActive(!service.IsFeatureUnlocked(minerLevel, pair.Key));
            }
        }

        private void TryStartBossChallenge()
        {
            if (lastState == null || chapterSelectionAction == null)
            {
                return;
            }

            chapterSelectionAction(lastState.Ore.Chapter.ChapterNumber);
        }

        private void ShowFeaturePlaceholder(ProgressionFeature feature)
        {
            if (lastState == null || lastService == null)
            {
                return;
            }

            var featureName = LanguageService.Get(feature.LocalizationKey()).ToUpper();
            var stateText = lastService.IsFeatureUnlocked(lastState.Player.minerLevel, feature)
                ? LanguageService.Get("coming_soon").ToUpper()
                : LanguageService.Get("locked").ToUpper();
            ShowFeedback($"{featureName} · {stateText}", new Color(0.66f, 0.82f, 1f));
        }

        private static string GetMineNameKey(string contentId)
        {
            return contentId switch
            {
                "magma_depths" => "mine_magma_depths",
                "ancient_city" => "mine_ancient_city",
                _ => "mine_crystal_cavern"
            };
        }

        private void ApplyV5(Image image, string assetName, bool preserveAspect = true)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = finalSkin.V5Simple(assetName);
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.color = Color.white;
        }

        private static void ConfigureRect(
            RectTransform rect,
            Vector2 anchor,
            Vector2 position,
            Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void ConfigureText(
            Text text,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            int fontSize,
            TextAnchor alignment)
        {
            ConfigureRect(text.rectTransform, anchor, position, size);
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.resizeTextForBestFit = true;
            text.resizeTextMaxSize = Mathf.Max(fontSize, MinimumReadableFontSize);
            text.resizeTextMinSize = Mathf.Clamp(
                fontSize - 8,
                MinimumReadableFontSize,
                text.resizeTextMaxSize);
        }
    }
}
