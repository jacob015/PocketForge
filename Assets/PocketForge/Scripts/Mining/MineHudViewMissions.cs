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
        private sealed class MissionRowView
        {
            public Image Surface;
            public Image Icon;
            public Text Name;
            public Text Progress;
            public Image ProgressTrack;
            public Image ProgressFill;
            public Image RewardIcon;
            public Text Reward;
            public Button ClaimButton;
            public string MissionId = string.Empty;
        }

        private GameObject missionsPanel;
        private Image missionsCard;
        private Image missionsTitleSurface;
        private Text missionsTitle;
        private Button closeMissionsCornerButton;
        private Button dailyMissionsTabButton;
        private Button weeklyMissionsTabButton;
        private Image missionsSummarySurface;
        private Text missionsSummary;
        private Text missionsRefreshText;
        private Image missionCompletionSurface;
        private Image missionCompletionIcon;
        private Text missionCompletionText;
        private Button missionCompletionButton;
        private Button closeMissionsButton;
        private readonly List<MissionRowView> missionRows = new();
        private Action<string> missionClaimAction;
        private Action<MissionPeriod> missionCompletionClaimAction;
        private MissionPeriod selectedMissionPeriod = MissionPeriod.Daily;

        private void CreateMissionsPanel()
        {
            var backdrop = CreatePanel(
                "MissionsBackdrop",
                transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.005f, 0.012f, 0.04f, 0.86f));
            missionsPanel = backdrop.gameObject;
            missionsCard = CreatePanel(
                "MissionsCard",
                missionsPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 24f),
                new Vector2(920f, 1700f),
                new Color(0.035f, 0.075f, 0.15f, 0.995f));
            missionsTitleSurface = CreatePanel(
                "MissionsTitleSurface",
                missionsCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-30f, -62f),
                new Vector2(610f, 124f),
                new Color(0.12f, 0.17f, 0.42f, 1f));
            missionsTitle = CreateText(
                "MissionsTitle",
                missionsTitleSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(0f, -8f),
                44,
                TextAnchor.MiddleCenter);
            missionsTitle.font = UiFontProvider.GetCasual();

            closeMissionsCornerButton = CreateButton(
                "CloseMissionsCorner",
                missionsCard.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-70f, -70f),
                new Vector2(84f, 84f),
                Color.white);
            closeMissionsCornerButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            var cornerCloseIcon = CreateSimpleImage(
                "Icon",
                closeMissionsCornerButton.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-22f, -22f),
                Color.white);
            cornerCloseIcon.raycastTarget = false;
            closeMissionsCornerButton.onClick.AddListener(() => missionsPanel.SetActive(false));

            dailyMissionsTabButton = CreateMissionButton(
                "DailyMissionsTab",
                new Vector2(-206f, 650f),
                new Vector2(400f, 92f));
            dailyMissionsTabButton.onClick.AddListener(
                () => SelectMissionPeriod(MissionPeriod.Daily));
            weeklyMissionsTabButton = CreateMissionButton(
                "WeeklyMissionsTab",
                new Vector2(206f, 650f),
                new Vector2(400f, 92f));
            weeklyMissionsTabButton.onClick.AddListener(
                () => SelectMissionPeriod(MissionPeriod.Weekly));

            missionsSummarySurface = CreatePanel(
                "MissionsSummarySurface",
                missionsCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 515f),
                new Vector2(824f, 152f),
                Color.white);
            missionsSummary = CreateText(
                "MissionsSummary",
                missionsSummarySurface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(259f, 20f),
                new Vector2(450f, 56f),
                29,
                TextAnchor.MiddleLeft);
            ConfigureMissionText(missionsSummary, 20, 29);
            missionsRefreshText = CreateText(
                "MissionsRefresh",
                missionsSummarySurface.transform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-170f, -20f),
                new Vector2(300f, 48f),
                22,
                TextAnchor.MiddleRight);
            ConfigureMissionText(missionsRefreshText, 16, 22);

            // 200 pitch keeps ~26 canvas units between the ~174-tall drawn row surfaces
            // instead of spreading four rows across the whole card.
            var rowY = new[] { 330f, 130f, -70f, -270f };
            for (var index = 0; index < rowY.Length; index++)
            {
                missionRows.Add(CreateMissionRow(index, rowY[index]));
            }

            missionCompletionSurface = CreatePanel(
                "MissionCompletionSurface",
                missionsCard.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 387f),
                new Vector2(824f, 156f),
                Color.white);
            missionCompletionIcon = CreateSimpleImage(
                "MissionCompletionIcon",
                missionCompletionSurface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(74f, 0f),
                new Vector2(96f, 96f),
                Color.white);
            missionCompletionIcon.raycastTarget = false;
            missionCompletionText = CreateText(
                "MissionCompletionText",
                missionCompletionSurface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(347f, 0f),
                new Vector2(390f, 92f),
                27,
                TextAnchor.MiddleLeft);
            ConfigureMissionText(missionCompletionText, 18, 27);
            missionCompletionButton = CreateButton(
                "MissionCompletionButton",
                missionCompletionSurface.transform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-110f, 0f),
                new Vector2(190f, 108f),
                Color.white);
            ConfigureMissionButtonLabel(missionCompletionButton);
            missionCompletionButton.onClick.AddListener(
                () => missionCompletionClaimAction?.Invoke(selectedMissionPeriod));

            closeMissionsButton = CreateButton(
                "CloseMissionsButton",
                missionsCard.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 62f),
                new Vector2(360f, 88f),
                Color.white);
            ConfigureMissionButtonLabel(closeMissionsButton);
            closeMissionsButton.onClick.AddListener(() => missionsPanel.SetActive(false));
            missionsPanel.SetActive(false);
        }

        private MissionRowView CreateMissionRow(int index, float y)
        {
            var row = new MissionRowView();
            row.Surface = CreatePanel(
                $"MissionRow{index}",
                missionsCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, y),
                new Vector2(824f, 226f),
                Color.white);
            row.Icon = CreateSimpleImage(
                "MissionIcon",
                row.Surface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(76f, 0f),
                new Vector2(110f, 110f),
                Color.white);
            row.Icon.raycastTarget = false;
            row.Name = CreateText(
                "MissionName",
                row.Surface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(315f, 48f),
                new Vector2(330f, 62f),
                28,
                TextAnchor.MiddleLeft);
            ConfigureMissionText(row.Name, 18, 28);
            row.ProgressTrack = CreateSimpleImage(
                "MissionProgressTrack",
                row.Surface.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(315f, -38f),
                new Vector2(316f, 34f),
                new Color(0.02f, 0.05f, 0.12f, 1f));
            row.ProgressTrack.raycastTarget = false;
            row.ProgressFill = CreatePanel(
                "MissionProgressFill",
                row.ProgressTrack.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(8f, 0f),
                new Vector2(300f, 24f),
                new Color(0.12f, 0.82f, 1f, 1f));
            row.ProgressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            row.ProgressFill.raycastTarget = false;
            row.Progress = CreateText(
                "MissionProgress",
                row.ProgressTrack.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                19,
                TextAnchor.MiddleCenter);
            ConfigureMissionText(row.Progress, 15, 19);
            row.RewardIcon = CreateSimpleImage(
                "MissionRewardIcon",
                row.Surface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(157f, 26f),
                new Vector2(54f, 54f),
                Color.white);
            row.RewardIcon.raycastTarget = false;
            row.Reward = CreateText(
                "MissionReward",
                row.Surface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(160f, -32f),
                new Vector2(150f, 48f),
                23,
                TextAnchor.MiddleCenter);
            ConfigureMissionText(row.Reward, 16, 23);
            row.ClaimButton = CreateButton(
                "MissionClaimButton",
                row.Surface.transform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-92f, 0f),
                new Vector2(154f, 108f),
                Color.white);
            ConfigureMissionButtonLabel(row.ClaimButton);
            row.ClaimButton.onClick.AddListener(() =>
            {
                if (!string.IsNullOrWhiteSpace(row.MissionId))
                {
                    missionClaimAction?.Invoke(row.MissionId);
                }
            });
            return row;
        }

        private Button CreateMissionButton(string name, Vector2 position, Vector2 size)
        {
            var button = CreateButton(
                name,
                missionsCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size,
                Color.white);
            ConfigureMissionButtonLabel(button);
            return button;
        }

        private static void ConfigureMissionText(Text text, int minimumSize, int maximumSize)
        {
            text.font = UiFontProvider.GetCasual();
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minimumSize;
            text.resizeTextMaxSize = maximumSize;
        }

        private static void ConfigureMissionButtonLabel(Button button)
        {
            var label = button.GetComponentInChildren<Text>();
            label.font = UiFontProvider.GetCasual();
            label.fontSize = 24;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 16;
            label.resizeTextMaxSize = 24;
        }

        private void ShowMissions()
        {
            if (lastState == null || lastService == null)
            {
                return;
            }

            if (!lastService.IsFeatureUnlocked(
                    lastState.Player.minerLevel,
                    ProgressionFeature.Missions))
            {
                ShowFeedback(LanguageService.Get("missions_locked"), new Color(0.66f, 0.82f, 1f));
                return;
            }

            selectedMissionPeriod = MissionPeriod.Daily;
            missionsPanel.SetActive(true);
            missionsPanel.transform.SetAsLastSibling();
            RenderMissions();
        }

        private void SelectMissionPeriod(MissionPeriod period)
        {
            selectedMissionPeriod = period;
            RenderMissions();
        }

        private void RenderMissions()
        {
            if (missionsPanel == null ||
                !missionsPanel.activeSelf ||
                lastState == null ||
                lastService == null)
            {
                return;
            }

            var board = lastService.GetMissionBoard(lastState, selectedMissionPeriod);
            missionsTitle.text = LanguageService.Get("feature_missions").ToUpper();
            dailyMissionsTabButton.GetComponentInChildren<Text>().text =
                LanguageService.Get("daily").ToUpper();
            weeklyMissionsTabButton.GetComponentInChildren<Text>().text =
                LanguageService.Get("weekly").ToUpper();
            closeMissionsButton.GetComponentInChildren<Text>().text =
                LanguageService.Get("close").ToUpper();
            dailyMissionsTabButton.interactable = selectedMissionPeriod != MissionPeriod.Daily;
            weeklyMissionsTabButton.interactable = selectedMissionPeriod != MissionPeriod.Weekly;
            ApplyBorderedSprite(
                dailyMissionsTabButton.image,
                finalSkin?.Task13Sliced(
                    selectedMissionPeriod == MissionPeriod.Daily
                        ? "TabCollectionActive"
                        : "TabCollectionInactive"),
                Color.white);
            ApplyBorderedSprite(
                weeklyMissionsTabButton.image,
                finalSkin?.Task13Sliced(
                    selectedMissionPeriod == MissionPeriod.Weekly
                        ? "TabCollectionActive"
                        : "TabCollectionInactive"),
                Color.white);

            missionsSummary.text = string.Format(
                LanguageService.Get("mission_summary"),
                board.ClaimedCount,
                board.Missions.Count);
            missionsRefreshText.text = string.Format(
                LanguageService.Get("mission_refresh_in"),
                FormatMissionRefresh(board.RefreshAtUnixSeconds));

            for (var index = 0; index < missionRows.Count; index++)
            {
                var row = missionRows[index];
                var active = index < board.Missions.Count;
                row.Surface.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                var state = board.Missions[index];
                row.MissionId = state.Definition.MissionId;
                row.Name.text = LanguageService.Get(
                    state.Definition.NameLocalizationKey).ToUpper();
                row.Progress.text =
                    $"{CompactNumberFormatter.Format(Math.Min(state.Progress, state.Definition.Target))} / " +
                    CompactNumberFormatter.Format(state.Definition.Target);
                row.ProgressFill.fillAmount = Mathf.Clamp01(
                    state.Progress / (float)Math.Max(1L, state.Definition.Target));
                row.Icon.sprite = finalSkin?.Task13Simple(
                    GetMissionMetricIcon(state.Definition.Metric));
                row.Icon.type = Image.Type.Simple;
                row.Icon.preserveAspect = true;
                row.RewardIcon.sprite = GetMissionRewardSprite(state.Definition.RewardType);
                row.RewardIcon.type = Image.Type.Simple;
                row.RewardIcon.preserveAspect = true;
                row.Reward.text = state.Definition.RewardType == MissionRewardType.Equipment
                    ? "x1"
                    : CompactNumberFormatter.Format(state.Definition.RewardAmount);
                row.ClaimButton.interactable = state.CanClaim;
                row.ClaimButton.GetComponentInChildren<Text>().text = LanguageService.Get(
                    state.Claimed ? "completed" : state.CanClaim ? "claim" : "in_progress").ToUpper();
                ApplyBorderedSprite(
                    row.ClaimButton.image,
                    finalSkin?.Task13Sliced(
                        state.CanClaim
                            ? "ButtonAchievementClaimRuntime"
                            : "UiAchievementInProgressState"),
                    Color.white);
            }

            missionCompletionIcon.sprite = GetMissionRewardSprite(board.CompletionRewardType);
            missionCompletionIcon.type = Image.Type.Simple;
            missionCompletionIcon.preserveAspect = true;
            missionCompletionText.text = string.Format(
                LanguageService.Get("mission_completion_reward"),
                board.Missions.Count,
                FormatMissionReward(board.CompletionRewardType, board.CompletionRewardAmount));
            missionCompletionButton.interactable = board.CanClaimCompletion;
            missionCompletionButton.GetComponentInChildren<Text>().text = LanguageService.Get(
                board.CompletionRewardClaimed
                    ? "completed"
                    : board.CanClaimCompletion ? "claim" : "in_progress").ToUpper();
            ApplyBorderedSprite(
                missionCompletionButton.image,
                finalSkin?.Task13Sliced(
                    board.CanClaimCompletion
                        ? "ButtonAchievementClaimRuntime"
                        : "UiAchievementInProgressState"),
                Color.white);
        }

        private string FormatMissionReward(MissionRewardType rewardType, long amount)
        {
            return rewardType switch
            {
                MissionRewardType.Equipment => LanguageService.Get("equipment_drop"),
                MissionRewardType.Gems => $"{CompactNumberFormatter.Format(amount)} \u25C6",
                // Non-breaking spaces keep the amount and reward name on one wrapped line.
                MissionRewardType.BlueprintCores =>
                    $"{CompactNumberFormatter.Format(amount)} {LanguageService.Get("blueprint_core").Replace(' ', ' ')}",
                _ => $"{CompactNumberFormatter.Format(amount)} C"
            };
        }

        private static string FormatMissionRefresh(long refreshAtUnixSeconds)
        {
            var remaining = Math.Max(
                0L,
                refreshAtUnixSeconds - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            var hours = remaining / 3600L;
            var minutes = remaining % 3600L / 60L;
            return $"{hours:00}:{minutes:00}";
        }

        private static string GetMissionMetricIcon(MissionMetric metric)
        {
            return metric switch
            {
                MissionMetric.FacilityUpgrades => "IconAchievementFacility",
                MissionMetric.ResearchCompleted => "IconAchievementResearch",
                MissionMetric.BossesDefeated => "IconAchievementChapter",
                MissionMetric.EquipmentAcquired => "IconAchievementEquipment",
                _ => "IconAchievementMining"
            };
        }

        private Sprite GetMissionRewardSprite(MissionRewardType rewardType)
        {
            return rewardType switch
            {
                MissionRewardType.Equipment =>
                    finalSkin?.Task13Simple("IconAchievementEquipment"),
                MissionRewardType.Gems => finalSkin?.V5Simple("05_GemIcon"),
                MissionRewardType.BlueprintCores => finalSkin?.V5Simple("06_BlueprintCoreIcon"),
                _ => finalSkin?.V5Simple("04_CreditsIcon")
            };
        }

        private void ApplyMissionSkin()
        {
            if (finalSkin == null || missionsCard == null)
            {
                return;
            }

            ApplyBorderedSprite(
                missionsCard,
                finalSkin.Task13Sliced("UiCollectionModalBody"),
                Color.white);
            ApplySimpleSprite(
                missionsTitleSurface,
                finalSkin.Task13Simple("UiCollectionTitlePlaque"));
            ApplyBorderedSprite(
                missionsSummarySurface,
                finalSkin.Task13Sliced("UiTask13HorizontalPanelClean"),
                Color.white);
            ApplyBorderedSprite(
                missionCompletionSurface,
                finalSkin.Task13Sliced("UiTask13HorizontalPanelClean"),
                Color.white);
            ApplyBorderedSprite(
                dailyMissionsTabButton.image,
                finalSkin.Task13Sliced("TabCollectionActive"),
                Color.white);
            ApplyBorderedSprite(
                weeklyMissionsTabButton.image,
                finalSkin.Task13Sliced("TabCollectionInactive"),
                Color.white);
            ApplyBorderedSprite(
                closeMissionsButton.image,
                finalSkin.Task13Sliced("ButtonAchievementClaimRuntime"),
                Color.white);
            ApplySimpleSprite(
                closeMissionsCornerButton.image,
                finalSkin.Task13Simple("UiModalCloseButtonSurface"));
            var cornerIcon = closeMissionsCornerButton.transform.Find("Icon")?.GetComponent<Image>();
            if (cornerIcon != null)
            {
                ApplySimpleSprite(cornerIcon, finalSkin.Task13Simple("IconCloseX"));
            }

            foreach (var row in missionRows)
            {
                ApplySimpleSprite(row.Surface, finalSkin.Task13Simple("UiAchievementRowBase"));
                row.ProgressTrack.sprite = finalSkin.Task13Simple("UiAchievementProgressTrack");
                row.ProgressTrack.type = Image.Type.Simple;
                row.ProgressFill.sprite = finalSkin.Task13Simple("UiAchievementProgressFill");
                row.ProgressFill.type = Image.Type.Filled;
                row.ProgressFill.fillMethod = Image.FillMethod.Horizontal;
                ApplyBorderedSprite(
                    row.ClaimButton.image,
                    finalSkin.Task13Sliced("UiAchievementInProgressState"),
                    Color.white);
            }

            ApplyBorderedSprite(
                missionCompletionButton.image,
                finalSkin.Task13Sliced("UiAchievementInProgressState"),
                Color.white);
        }
    }
}
