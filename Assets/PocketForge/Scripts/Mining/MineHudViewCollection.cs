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
            public Image Icon;
            public Text Name;
            public Text Details;
            public Text Bonus;
        }

        private sealed class AchievementRowView
        {
            public Image Surface;
            public Text Name;
            public Text Progress;
            public Text Reward;
            public Button ClaimButton;
            public string AchievementId = string.Empty;
        }

        private GameObject collectionPanel;
        private Image collectionCard;
        private Image collectionTitleSurface;
        private Text collectionTitle;
        private Text collectionSummary;
        private Button museumTabButton;
        private Button achievementsTabButton;
        private Button closeCollectionButton;
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
                new Vector2(920f, 1380f),
                new Color(0.035f, 0.075f, 0.15f, 0.995f));
            collectionTitleSurface = CreatePanel(
                "CollectionTitleSurface",
                collectionCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -44f),
                new Vector2(650f, 132f),
                new Color(0.12f, 0.17f, 0.42f, 1f));
            collectionTitle = CreateText(
                "CollectionTitle",
                collectionTitleSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-42f, -14f),
                44,
                TextAnchor.MiddleCenter);
            collectionTitle.font = UiFontProvider.GetCasual();

            museumTabButton = CreateCollectionButton("MuseumTab", -210f, 482f, 360f);
            museumTabButton.onClick.AddListener(() => SelectCollectionTab(CollectionTab.Museum));
            achievementsTabButton = CreateCollectionButton("AchievementsTab", 210f, 482f, 360f);
            achievementsTabButton.onClick.AddListener(() => SelectCollectionTab(CollectionTab.Achievements));
            collectionSummary = CreateText(
                "CollectionSummary",
                collectionCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 388f),
                new Vector2(780f, 64f),
                27,
                TextAnchor.MiddleCenter);
            collectionSummary.font = UiFontProvider.GetCasual();
            collectionSummary.color = new Color(0.5f, 0.92f, 1f);
            collectionSummary.resizeTextForBestFit = true;
            collectionSummary.resizeTextMinSize = 20;
            collectionSummary.resizeTextMaxSize = 27;

            closeCollectionButton = CreateCollectionButton("CloseCollection", 0f, -610f, 360f);
            closeCollectionButton.onClick.AddListener(() => collectionPanel.SetActive(false));
            collectionPanel.SetActive(false);
        }

        private Button CreateCollectionButton(string name, float x, float y, float width)
        {
            var button = CreateButton(
                name,
                collectionCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(x, y),
                new Vector2(width, 82f),
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
                var row = new MuseumRowView();
                row.Surface = CreatePanel(
                    $"MuseumRow{index + 1}",
                    collectionCard.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 270f - index * 190f),
                    new Vector2(780f, 166f),
                    new Color(0.055f, 0.12f, 0.23f, 0.98f));
                row.Icon = CreateSimpleImage(
                    "OreIcon",
                    row.Surface.transform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(86f, 0f),
                    new Vector2(116f, 116f),
                    Color.white);
                row.Name = CreateText(
                    "OreName",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(-80f, 35f),
                    new Vector2(420f, 50f),
                    30,
                    TextAnchor.MiddleLeft);
                row.Name.font = UiFontProvider.GetCasual();
                row.Details = CreateText(
                    "OreDetails",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(-80f, -27f),
                    new Vector2(420f, 58f),
                    23,
                    TextAnchor.MiddleLeft);
                row.Details.color = new Color(0.62f, 0.86f, 1f);
                row.Bonus = CreateText(
                    "OreBonus",
                    row.Surface.transform,
                    new Vector2(1f, 0.5f),
                    new Vector2(1f, 0.5f),
                    new Vector2(-108f, 0f),
                    new Vector2(180f, 74f),
                    26,
                    TextAnchor.MiddleCenter);
                row.Bonus.font = UiFontProvider.GetCasual();
                row.Bonus.color = new Color(0.52f, 1f, 0.58f);
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
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 292f - index * 142f),
                    new Vector2(780f, 124f),
                    new Color(0.055f, 0.12f, 0.23f, 0.98f));
                row.Name = CreateText(
                    "AchievementName",
                    row.Surface.transform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(192f, 26f),
                    new Vector2(330f, 42f),
                    25,
                    TextAnchor.MiddleLeft);
                row.Name.font = UiFontProvider.GetCasual();
                row.Progress = CreateText(
                    "AchievementProgress",
                    row.Surface.transform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(192f, -27f),
                    new Vector2(330f, 38f),
                    20,
                    TextAnchor.MiddleLeft);
                row.Progress.color = new Color(0.62f, 0.86f, 1f);
                row.Reward = CreateText(
                    "AchievementReward",
                    row.Surface.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(120f, 0f),
                    new Vector2(180f, 64f),
                    22,
                    TextAnchor.MiddleCenter);
                row.ClaimButton = CreateButton(
                    "ClaimAchievement",
                    row.Surface.transform,
                    new Vector2(1f, 0.5f),
                    new Vector2(1f, 0.5f),
                    new Vector2(-112f, 0f),
                    new Vector2(196f, 78f),
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
            museumTabButton.interactable = !showMuseum;
            achievementsTabButton.interactable = showMuseum;
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
            collectionSummary.text = string.Format(
                LanguageService.Get("museum_summary"),
                multiplierBonus);
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
                if (state.IsCompleted)
                {
                    row.Progress.text = LanguageService.Get("achievement_all_complete");
                    row.Reward.text = "";
                    row.ClaimButton.GetComponentInChildren<Text>().text =
                        LanguageService.Get("completed").ToUpper();
                    continue;
                }

                var tier = state.NextTier;
                row.Progress.text =
                    $"{CompactNumberFormatter.Format(state.Progress)} / {CompactNumberFormatter.Format(tier.Target)}";
                row.Reward.text = FormatAchievementReward(tier);
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
            var suffix = tier.RewardType switch
            {
                AchievementRewardType.Gems => "\u25C6",
                AchievementRewardType.BlueprintCores => "CORE",
                _ => "C"
            };
            return $"+{CompactNumberFormatter.Format(tier.RewardAmount)} {suffix}";
        }

        private void ApplyCollectionSkin()
        {
            if (finalSkin == null || collectionCard == null)
            {
                return;
            }

            ApplyStretchedSimpleSprite(collectionCard, finalSkin.Simple("SettingsModal"), Color.white);
            ApplyStretchedSimpleSprite(
                collectionTitleSurface,
                finalSkin.Simple("SettingsTitlePlaque"),
                Color.white);
            foreach (var button in new[] { museumTabButton, achievementsTabButton, closeCollectionButton })
            {
                ApplyStretchedSimpleSprite(
                    button.image,
                    finalSkin.Simple(button == closeCollectionButton
                        ? "SettingsCloseButton"
                        : "SettingsActionButton"),
                    Color.white);
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

            ApplyStretchedSimpleSprite(row.Surface, finalSkin.Simple("SettingsRow"), Color.white);
            row.Icon.sprite = finalSkin.V5Simple("21_BossChallengeIcon");
            row.Icon.type = Image.Type.Simple;
            row.Icon.preserveAspect = true;
        }

        private void ApplyAchievementRowSkin(AchievementRowView row)
        {
            if (finalSkin == null || row?.Surface == null)
            {
                return;
            }

            ApplyStretchedSimpleSprite(row.Surface, finalSkin.Simple("SettingsRow"), Color.white);
            ApplyStretchedSimpleSprite(
                row.ClaimButton.image,
                finalSkin.Simple("SettingsActionButton"),
                Color.white);
        }
    }
}
