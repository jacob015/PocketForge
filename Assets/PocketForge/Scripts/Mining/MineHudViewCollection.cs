using System;
using System.Collections.Generic;
using PocketForge.Content;
using PocketForge.Localization;
using PocketForge.Presentation;
using PocketForge.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace PocketForge.Mining
{
    public sealed partial class MineHudView
    {
        private enum CollectionTab
        {
            Museum,
            Achievements
        }

        private sealed class MuseumRowView
        {
            public Image Surface;
            public Image Pedestal;
            public Image Icon;
            public Text Name;
            public Text Details;
            public Text Bonus;
            public Image ProgressTrack;
            public Image ProgressFill;
            public Image LockedOverlay;
        }

        private sealed class AchievementRowView
        {
            public Image Surface;
            public Image IconFrame;
            public Image Icon;
            public Text Name;
            public Text Progress;
            public Image ProgressTrack;
            public Image ProgressFill;
            public Image RewardSlot;
            public Image RewardIcon;
            public Text Reward;
            public Button ClaimButton;
            public Image CompleteBadge;
            public string AchievementId = string.Empty;
        }

        private GameObject collectionPanel;
        private Image collectionCard;
        private Image collectionTitleSurface;
        private Text collectionTitle;
        private Text collectionSummary;
        private Image collectionSummarySurface;
        private Image collectionSummaryIcon;
        private Image museumNextRewardSurface;
        private Image museumNextRewardIcon;
        private Text museumNextRewardText;
        private Button museumTabButton;
        private Button achievementsTabButton;
        private Button closeCollectionButton;
        private Button closeCollectionCornerButton;
        private readonly List<MuseumRowView> museumRows = new();
        private readonly List<AchievementRowView> achievementRows = new();
        private Action<string> achievementClaimAction;
        private CollectionTab collectionTab;

        private void CreateCollectionPanel()
        {
            var backdrop = CreatePanel(
                "CollectionBackdrop",
                transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.005f, 0.012f, 0.04f, 0.86f));
            collectionPanel = backdrop.gameObject;
            collectionCard = CreatePanel(
                "CollectionCard",
                collectionPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 24f),
                new Vector2(920f, 1700f),
                new Color(0.035f, 0.075f, 0.15f, 0.995f));
            collectionTitleSurface = CreatePanel(
                "CollectionTitleSurface",
                collectionCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-30f, -62f),
                new Vector2(610f, 124f),
                new Color(0.12f, 0.17f, 0.42f, 1f));
            collectionTitle = CreateText(
                "CollectionTitle",
                collectionTitleSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(0f, -8f),
                44,
                TextAnchor.MiddleCenter);
            collectionTitle.font = UiFontProvider.GetCasual();

            closeCollectionCornerButton = CreateButton(
                "CloseCollectionCorner",
                collectionCard.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-70f, -70f),
                new Vector2(84f, 84f),
                Color.white);
            closeCollectionCornerButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            var cornerCloseIcon = CreateSimpleImage(
                "Icon",
                closeCollectionCornerButton.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-22f, -22f),
                Color.white);
            cornerCloseIcon.raycastTarget = false;
            closeCollectionCornerButton.onClick.AddListener(() => collectionPanel.SetActive(false));

            museumTabButton = CreateCollectionButton("MuseumTab", -206f, 660f, 400f, false, 92f);
            museumTabButton.onClick.AddListener(() => SelectCollectionTab(CollectionTab.Museum));
            achievementsTabButton = CreateCollectionButton("AchievementsTab", 206f, 660f, 400f, false, 92f);
            achievementsTabButton.onClick.AddListener(() => SelectCollectionTab(CollectionTab.Achievements));
            collectionSummarySurface = CreatePanel(
                "CollectionSummarySurface",
                collectionCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 450f),
                new Vector2(824f, 247f),
                Color.white);
            collectionSummary = CreateText(
                "CollectionSummary",
                collectionSummarySurface.transform,
                Vector2.zero,
                Vector2.one,
                new Vector2(94f, 0f),
                new Vector2(-154f, -30f),
                27,
                TextAnchor.MiddleCenter);
            collectionSummary.font = UiFontProvider.GetCasual();
            collectionSummary.color = new Color(0.5f, 0.92f, 1f);
            collectionSummary.resizeTextForBestFit = true;
            collectionSummary.resizeTextMinSize = 20;
            collectionSummary.resizeTextMaxSize = 27;
            collectionSummaryIcon = CreateSimpleImage(
                "CollectionSummaryIcon",
                collectionSummarySurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-305f, 0f),
                new Vector2(108f, 108f),
                Color.white);
            collectionSummaryIcon.raycastTarget = false;

            museumNextRewardSurface = CreateSimpleImage(
                "MuseumNextRewardSurface",
                collectionCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -1370f),
                new Vector2(824f, 222f),
                Color.white);
            museumNextRewardSurface.raycastTarget = false;
            museumNextRewardIcon = CreateSimpleImage(
                "MuseumNextRewardIcon",
                museumNextRewardSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-245f, 0f),
                new Vector2(96f, 96f),
                Color.white);
            museumNextRewardIcon.raycastTarget = false;
            museumNextRewardText = CreateText(
                "MuseumNextRewardProgress",
                museumNextRewardSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(120f, 0f),
                new Vector2(430f, 72f),
                24,
                TextAnchor.MiddleCenter);
            museumNextRewardText.font = UiFontProvider.GetCasual();

            closeCollectionButton = CreateCollectionButton("CloseCollection", 0f, 70f, 360f, true, 72f);
            closeCollectionButton.onClick.AddListener(() => collectionPanel.SetActive(false));
            collectionPanel.SetActive(false);
        }

        private Button CreateCollectionButton(
            string name,
            float x,
            float y,
            float width,
            bool bottomAnchored = false,
            float height = 82f)
        {
            var anchor = bottomAnchored ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
            var button = CreateButton(
                name,
                collectionCard.transform,
                anchor,
                anchor,
                new Vector2(x, y),
                new Vector2(width, height),
                new Color(0.25f, 0.68f, 0.2f, 1f));
            var label = button.GetComponentInChildren<Text>();
            label.font = UiFontProvider.GetCasual();
            label.fontSize = 23;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 16;
            label.resizeTextMaxSize = 23;
            return button;
        }

        private void EnsureMuseumRows(int count)
        {
            while (museumRows.Count < count)
            {
                var index = museumRows.Count;
                var column = index % 2;
                var line = index / 2;
                var row = new MuseumRowView();
                row.Surface = CreatePanel(
                    $"MuseumRow{index + 1}",
                    collectionCard.transform,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(-206f + column * 412f, -700f - line * 348f),
                    new Vector2(388f, 330f),
                    new Color(0.055f, 0.12f, 0.23f, 0.98f));
                row.Icon = CreateSimpleImage(
                    "OreIcon",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 35f),
                    new Vector2(160f, 126f),
                    Color.white);
                row.Pedestal = CreateSimpleImage(
                    "Pedestal",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 2f),
                    new Vector2(205f, 86f),
                    Color.white);
                row.Pedestal.raycastTarget = false;
                row.Pedestal.transform.SetAsFirstSibling();
                row.Name = CreateText(
                    "OreName",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 124f),
                    new Vector2(310f, 42f),
                    26,
                    TextAnchor.MiddleCenter);
                row.Name.font = UiFontProvider.GetCasual();
                row.Details = CreateText(
                    "OreDetails",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -58f),
                    new Vector2(310f, 42f),
                    20,
                    TextAnchor.MiddleCenter);
                row.Details.color = new Color(0.62f, 0.86f, 1f);
                row.Bonus = CreateText(
                    "OreBonus",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -137f),
                    new Vector2(180f, 40f),
                    22,
                    TextAnchor.MiddleCenter);
                row.Bonus.font = UiFontProvider.GetCasual();
                row.Bonus.color = new Color(0.52f, 1f, 0.58f);
                row.ProgressTrack = CreateSimpleImage(
                    "ProgressTrack",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -98f),
                    new Vector2(276f, 28f),
                    Color.white);
                row.ProgressTrack.raycastTarget = false;
                row.ProgressFill = CreateSimpleImage(
                    "ProgressFill",
                    row.ProgressTrack.transform,
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    new Vector2(-8f, -8f),
                    Color.white);
                row.ProgressFill.type = Image.Type.Filled;
                row.ProgressFill.fillMethod = Image.FillMethod.Horizontal;
                row.ProgressFill.fillOrigin = 0;
                row.ProgressFill.raycastTarget = false;
                row.LockedOverlay = CreateSimpleImage(
                    "LockedOverlay",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 28f),
                    new Vector2(72f, 72f),
                    Color.white);
                row.LockedOverlay.raycastTarget = false;
                museumRows.Add(row);
                ApplyMuseumRowSkin(row);
            }
        }

        private void EnsureAchievementRows(int count)
        {
            while (achievementRows.Count < count)
            {
                var index = achievementRows.Count;
                var row = new AchievementRowView();
                row.Surface = CreatePanel(
                    $"AchievementRow{index + 1}",
                    collectionCard.transform,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -650f - index * 154f),
                    new Vector2(824f, 138f),
                    new Color(0.055f, 0.12f, 0.23f, 0.98f));
                row.IconFrame = CreateSimpleImage(
                    "IconFrame",
                    row.Surface.transform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(68f, 0f),
                    new Vector2(84f, 84f),
                    Color.white);
                row.IconFrame.raycastTarget = false;
                row.Icon = CreateSimpleImage(
                    "Icon",
                    row.IconFrame.transform,
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    new Vector2(-16f, -16f),
                    Color.white);
                row.Icon.raycastTarget = false;
                row.Name = CreateText(
                    "AchievementName",
                    row.Surface.transform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(274f, 28f),
                    new Vector2(320f, 42f),
                    23,
                    TextAnchor.MiddleLeft);
                row.Name.font = UiFontProvider.GetCasual();
                row.Progress = CreateText(
                    "AchievementProgress",
                    row.Surface.transform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(190f, -30f),
                    new Vector2(160f, 34f),
                    18,
                    TextAnchor.MiddleLeft);
                row.Progress.color = new Color(0.62f, 0.86f, 1f);
                row.ProgressTrack = CreateSimpleImage(
                    "ProgressTrack",
                    row.Surface.transform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(350f, -30f),
                    new Vector2(160f, 22f),
                    Color.white);
                row.ProgressTrack.raycastTarget = false;
                row.ProgressFill = CreateSimpleImage(
                    "ProgressFill",
                    row.ProgressTrack.transform,
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    new Vector2(-6f, -6f),
                    Color.white);
                row.ProgressFill.type = Image.Type.Filled;
                row.ProgressFill.fillMethod = Image.FillMethod.Horizontal;
                row.ProgressFill.fillOrigin = 0;
                row.ProgressFill.raycastTarget = false;
                row.RewardSlot = CreateSimpleImage(
                    "RewardSlot",
                    row.Surface.transform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(500f, 0f),
                    new Vector2(64f, 64f),
                    Color.white);
                row.RewardSlot.raycastTarget = false;
                row.RewardIcon = CreateSimpleImage(
                    "RewardIcon",
                    row.RewardSlot.transform,
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    new Vector2(-14f, -14f),
                    Color.white);
                row.RewardIcon.raycastTarget = false;
                row.Reward = CreateText(
                    "AchievementReward",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(161f, 0f),
                    new Vector2(64f, 58f),
                    19,
                    TextAnchor.MiddleCenter);
                row.ClaimButton = CreateButton(
                    "ClaimAchievement",
                    row.Surface.transform,
                    new Vector2(1f, 0.5f),
                    new Vector2(1f, 0.5f),
                    new Vector2(-90f, 0f),
                    new Vector2(156f, 84f),
                    new Color(0.25f, 0.68f, 0.2f, 1f));
                var label = row.ClaimButton.GetComponentInChildren<Text>();
                label.font = UiFontProvider.GetCasual();
                label.fontSize = 21;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 14;
                label.resizeTextMaxSize = 21;
                row.ClaimButton.onClick.AddListener(() =>
                {
                    if (!string.IsNullOrWhiteSpace(row.AchievementId))
                    {
                        achievementClaimAction?.Invoke(row.AchievementId);
                    }
                });
                row.CompleteBadge = CreateSimpleImage(
                    "CompleteBadge",
                    row.Surface.transform,
                    new Vector2(1f, 0.5f),
                    new Vector2(1f, 0.5f),
                    new Vector2(-90f, 0f),
                    new Vector2(74f, 74f),
                    Color.white);
                row.CompleteBadge.raycastTarget = false;
                achievementRows.Add(row);
                ApplyAchievementRowSkin(row);
            }
        }

        private void ShowCollection()
        {
            if (lastState == null || lastService == null)
            {
                return;
            }

            if (!lastService.IsFeatureUnlocked(lastState.Player.minerLevel, ProgressionFeature.Museum))
            {
                ShowFeedback(LanguageService.Get("museum_locked"), new Color(0.66f, 0.82f, 1f));
                return;
            }

            collectionTab = CollectionTab.Museum;
            collectionPanel.SetActive(true);
            collectionPanel.transform.SetAsLastSibling();
            RenderCollection();
        }

        private void SelectCollectionTab(CollectionTab tab)
        {
            collectionTab = tab;
            RenderCollection();
        }

        private void RenderCollection()
        {
            if (collectionPanel == null ||
                !collectionPanel.activeSelf ||
                lastState == null ||
                lastService == null)
            {
                return;
            }

            collectionTitle.text = LanguageService.Get("collection_title").ToUpper();
            museumTabButton.GetComponentInChildren<Text>().text =
                LanguageService.Get("feature_museum").ToUpper();
            achievementsTabButton.GetComponentInChildren<Text>().text =
                LanguageService.Get("achievements").ToUpper();
            closeCollectionButton.GetComponentInChildren<Text>().text =
                LanguageService.Get("close").ToUpper();

            var showMuseum = collectionTab == CollectionTab.Museum;
            museumNextRewardSurface.gameObject.SetActive(showMuseum);
            museumTabButton.interactable = !showMuseum;
            achievementsTabButton.interactable = showMuseum;
            ApplyBorderedSprite(
                museumTabButton.image,
                finalSkin?.Task13Sliced(
                    showMuseum ? "TabCollectionActive" : "TabCollectionInactive",
                    new Vector4(28f, 18f, 28f, 18f)),
                Color.white);
            ApplyBorderedSprite(
                achievementsTabButton.image,
                finalSkin?.Task13Sliced(
                    showMuseum ? "TabCollectionInactive" : "TabCollectionActive",
                    new Vector4(28f, 18f, 28f, 18f)),
                Color.white);
            collectionSummarySurface.rectTransform.sizeDelta = showMuseum
                ? new Vector2(824f, 247f)
                : new Vector2(824f, 255f);
            ApplySimpleSprite(
                collectionSummarySurface,
                finalSkin?.Task13Simple(showMuseum ? "UiMuseumSummaryCard" : "UiAchievementSummaryCard"));
            collectionSummaryIcon.sprite = finalSkin?.Task13Simple(
                showMuseum ? "IconMuseumTab" : "IconAchievementTab");
            collectionSummaryIcon.type = Image.Type.Simple;
            collectionSummaryIcon.preserveAspect = true;
            if (showMuseum)
            {
                RenderMuseum();
            }
            else
            {
                RenderAchievements();
            }
        }

        private void RenderMuseum()
        {
            var states = lastService.GetCollectionStates(lastState);
            EnsureMuseumRows(states.Count);
            var multiplierBonus = (lastService.GetCollectionPowerMultiplier(lastState) - 1f) * 100f;
            var discoveredCount = 0;
            foreach (var state in states)
            {
                if (state.IsDiscovered)
                {
                    discoveredCount++;
                }
            }
            museumNextRewardText.text = $"{discoveredCount} / {states.Count}";
            collectionSummary.text = string.Format(
                LanguageService.Get("museum_summary"),
                multiplierBonus);
            var milestones = lastService.GetCollectionMilestones();
            for (var index = 0; index < museumRows.Count; index++)
            {
                var row = museumRows[index];
                var active = index < states.Count;
                row.Surface.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                var state = states[index];
                row.Name.text = state.IsDiscovered
                    ? LanguageService.Get($"ore_{state.Definition.ContentId}").ToUpper()
                    : LanguageService.Get("undiscovered").ToUpper();
                row.Details.text = string.Format(
                    LanguageService.Get("museum_mined"),
                    CompactNumberFormatter.Format(state.MinedCount));
                row.Bonus.text = state.IsDiscovered
                    ? string.Format(LanguageService.Get("museum_bonus"), state.PowerBonus * 100f)
                    : "--";
                row.Icon.color = state.IsDiscovered
                    ? Color.white
                    : new Color(0.18f, 0.24f, 0.38f, 0.8f);
                row.Icon.sprite = finalSkin?.Task13Simple(GetMuseumOreAsset(state.Definition.ContentId));
                row.Icon.type = Image.Type.Simple;
                row.Icon.preserveAspect = true;
                row.LockedOverlay.gameObject.SetActive(!state.IsDiscovered);
                row.Pedestal.color = state.IsDiscovered
                    ? Color.white
                    : new Color(0.35f, 0.38f, 0.55f, 0.75f);

                var previousTarget = 0L;
                var nextTarget = 0L;
                foreach (var milestone in milestones)
                {
                    if (state.MinedCount >= milestone)
                    {
                        previousTarget = milestone;
                        continue;
                    }

                    nextTarget = milestone;
                    break;
                }

                row.ProgressFill.fillAmount = nextTarget <= 0L
                    ? 1f
                    : Mathf.Clamp01((state.MinedCount - previousTarget) /
                                    (float)Math.Max(1L, nextTarget - previousTarget));
            }

            foreach (var row in achievementRows)
            {
                row.Surface.gameObject.SetActive(false);
            }
        }

        private void RenderAchievements()
        {
            var states = lastService.GetAchievementStates(lastState);
            EnsureAchievementRows(states.Count);
            var claimable = 0;
            foreach (var state in states)
            {
                if (state.CanClaim)
                {
                    claimable++;
                }
            }
            collectionSummary.text = string.Format(
                LanguageService.Get("achievement_summary"),
                claimable);

            for (var index = 0; index < achievementRows.Count; index++)
            {
                var row = achievementRows[index];
                var active = index < states.Count;
                row.Surface.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                var state = states[index];
                row.AchievementId = state.Definition.AchievementId;
                row.Name.text = LanguageService.Get(state.Definition.NameLocalizationKey).ToUpper();
                row.ClaimButton.interactable = state.CanClaim;
                row.Icon.sprite = finalSkin?.Task13Simple(
                    GetAchievementIconAsset(state.Definition.AchievementId));
                row.Icon.type = Image.Type.Simple;
                row.Icon.preserveAspect = true;
                row.CompleteBadge.gameObject.SetActive(state.IsCompleted);
                row.ClaimButton.gameObject.SetActive(!state.IsCompleted);
                row.RewardSlot.gameObject.SetActive(!state.IsCompleted);
                if (state.IsCompleted)
                {
                    row.Progress.text = LanguageService.Get("achievement_all_complete");
                    row.ProgressFill.fillAmount = 1f;
                    row.Reward.text = "";
                    continue;
                }

                var tier = state.NextTier;
                row.Progress.text =
                    $"{CompactNumberFormatter.Format(state.Progress)} / {CompactNumberFormatter.Format(tier.Target)}";
                row.ProgressFill.fillAmount = Mathf.Clamp01(state.Progress / (float)Math.Max(1L, tier.Target));
                row.Reward.text = FormatAchievementReward(tier);
                row.RewardIcon.sprite = GetAchievementRewardSprite(tier.RewardType);
                row.RewardIcon.type = Image.Type.Simple;
                row.RewardIcon.preserveAspect = true;
                ApplyBorderedSprite(
                    row.ClaimButton.image,
                    finalSkin?.Task13Sliced(
                        state.CanClaim ? "ButtonAchievementClaim" : "UiAchievementInProgressState",
                        state.CanClaim
                            ? new Vector4(36f, 28f, 36f, 28f)
                            : new Vector4(24f, 16f, 24f, 16f)),
                    Color.white);
                row.ClaimButton.GetComponentInChildren<Text>().text = LanguageService.Get(
                    state.CanClaim ? "claim" : "in_progress").ToUpper();
            }

            foreach (var row in museumRows)
            {
                row.Surface.gameObject.SetActive(false);
            }
        }

        private static string FormatAchievementReward(AchievementTierDefinition tier)
        {
            return CompactNumberFormatter.Format(tier.RewardAmount);
        }

        private static string GetMuseumOreAsset(string contentId)
        {
            var id = contentId?.ToLowerInvariant() ?? string.Empty;
            if (id.Contains("copper"))
            {
                return "IconMuseumCopperOre";
            }

            if (id.Contains("iron"))
            {
                return "IconMuseumIronOre";
            }

            if (id.Contains("gold"))
            {
                return "IconMuseumGoldOre";
            }

            return "IconMuseumUnknownCrystal";
        }

        private static string GetAchievementIconAsset(string achievementId)
        {
            return achievementId switch
            {
                "mine_ores" => "IconAchievementMining",
                "clear_chapters" => "IconAchievementChapter",
                "upgrade_facilities" => "IconAchievementFacility",
                "raise_miner" => "IconAchievementMiner",
                "complete_research" => "IconAchievementResearch",
                "collect_equipment" => "IconAchievementEquipment",
                _ => "IconAchievementTab"
            };
        }

        private Sprite GetAchievementRewardSprite(AchievementRewardType rewardType)
        {
            return finalSkin?.V5Simple(rewardType switch
            {
                AchievementRewardType.Gems => "05_GemIcon",
                AchievementRewardType.BlueprintCores => "06_BlueprintCoreIcon",
                _ => "04_CreditsIcon"
            });
        }

        private void ApplyCollectionSkin()
        {
            if (finalSkin == null || collectionCard == null)
            {
                return;
            }

            ApplyBorderedSprite(
                collectionCard,
                finalSkin.Task13Sliced("UiCollectionModalBody", new Vector4(30f, 30f, 30f, 30f)),
                Color.white);
            ApplySimpleSprite(collectionTitleSurface, finalSkin.Task13Simple("UiCollectionTitlePlaque"));
            ApplySimpleSprite(collectionSummarySurface, finalSkin.Task13Simple("UiMuseumSummaryCard"));
            ApplySimpleSprite(museumNextRewardSurface, finalSkin.Task13Simple("UiMuseumNextRewardStrip"));
            museumNextRewardIcon.sprite = finalSkin.Task13Simple("IconMuseumMysteryMineral");
            museumNextRewardIcon.type = Image.Type.Simple;
            museumNextRewardIcon.preserveAspect = true;
            ApplyBorderedSprite(
                museumTabButton.image,
                finalSkin.Task13Sliced("TabCollectionActive", new Vector4(28f, 18f, 28f, 18f)),
                Color.white);
            ApplyBorderedSprite(
                achievementsTabButton.image,
                finalSkin.Task13Sliced("TabCollectionInactive", new Vector4(28f, 18f, 28f, 18f)),
                Color.white);
            ApplyBorderedSprite(
                closeCollectionButton.image,
                finalSkin.Task13Sliced("ButtonAchievementClaim", new Vector4(36f, 28f, 36f, 28f)),
                Color.white);
            ApplySimpleSprite(
                closeCollectionCornerButton.image,
                finalSkin.Task13Simple("UiModalCloseButtonSurface"));
            var cornerIcon = closeCollectionCornerButton.transform.Find("Icon")?.GetComponent<Image>();
            if (cornerIcon != null)
            {
                cornerIcon.sprite = finalSkin.Task13Simple("IconCloseX");
                cornerIcon.type = Image.Type.Simple;
                cornerIcon.preserveAspect = true;
            }

            foreach (var row in museumRows)
            {
                ApplyMuseumRowSkin(row);
            }

            foreach (var row in achievementRows)
            {
                ApplyAchievementRowSkin(row);
            }
        }

        private void ApplyMuseumRowSkin(MuseumRowView row)
        {
            if (finalSkin == null || row?.Surface == null)
            {
                return;
            }

            ApplyBorderedSprite(
                row.Surface,
                finalSkin.Task13Sliced("UiMuseumExhibitCardBase", new Vector4(30f, 30f, 30f, 30f)),
                Color.white);
            row.Pedestal.sprite = finalSkin.Task13Simple("UiMuseumPedestal");
            row.Pedestal.type = Image.Type.Simple;
            row.Pedestal.preserveAspect = true;
            row.ProgressTrack.sprite = finalSkin.Task13Simple("UiMuseumProgressTrack");
            row.ProgressTrack.type = Image.Type.Simple;
            row.ProgressFill.sprite = finalSkin.Task13Simple("UiMuseumProgressFill");
            row.ProgressFill.type = Image.Type.Filled;
            row.ProgressFill.fillMethod = Image.FillMethod.Horizontal;
            row.LockedOverlay.sprite = finalSkin.Task13Simple("OverlayMuseumLocked");
            row.LockedOverlay.type = Image.Type.Simple;
            row.LockedOverlay.preserveAspect = true;
        }

        private void ApplyAchievementRowSkin(AchievementRowView row)
        {
            if (finalSkin == null || row?.Surface == null)
            {
                return;
            }

            ApplySimpleSprite(row.Surface, finalSkin.Task13Simple("UiAchievementRowBase"));
            row.IconFrame.sprite = finalSkin.Task13Simple("UiAchievementIconFrame");
            row.IconFrame.type = Image.Type.Simple;
            row.ProgressTrack.sprite = finalSkin.Task13Simple("UiAchievementProgressTrack");
            row.ProgressTrack.type = Image.Type.Simple;
            row.ProgressFill.sprite = finalSkin.Task13Simple("UiAchievementProgressFill");
            row.ProgressFill.type = Image.Type.Filled;
            row.ProgressFill.fillMethod = Image.FillMethod.Horizontal;
            row.RewardSlot.sprite = finalSkin.Task13Simple("UiAchievementRewardSlot");
            row.RewardSlot.type = Image.Type.Simple;
            ApplyBorderedSprite(
                row.ClaimButton.image,
                finalSkin.Task13Sliced("UiAchievementInProgressState", new Vector4(24f, 16f, 24f, 16f)),
                Color.white);
            row.CompleteBadge.sprite = finalSkin.Task13Simple("BadgeAchievementComplete");
            row.CompleteBadge.type = Image.Type.Simple;
            row.CompleteBadge.preserveAspect = true;
        }
    }
}
