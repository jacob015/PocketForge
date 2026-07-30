using System;
using PocketForge.Audio;
using PocketForge.Economy;
using PocketForge.Localization;
using PocketForge.Presentation;
using PocketForge.Progression;
using UnityEngine;

namespace PocketForge.Mining
{
    public sealed class MineHudPresenter
    {
        private readonly MineHudView view;
        private readonly MiningGameService gameService;
        private readonly MiningGameState state;

        public MineHudPresenter(MineHudView view, MiningGameService gameService, MiningGameState state)
        {
            this.view = view;
            this.gameService = gameService;
            this.state = state;
            view.Bind(Mine, Upgrade, OpenChapterSelection, SelectChapter);
            view.BindResearch(Research);
        }

        public event Action StateChanged;
        public event Action SaveRequested;
        public event Action OreBroken;

        public void Render() => view.Render(state, gameService);

        public void ShowOfflineReward(OfflineProgressResult result)
        {
            if (result.HasReward)
            {
                view.ShowOfflineReward(result);
            }
        }

        public void Tick(float deltaTime)
        {
            Apply(gameService.Tick(state, deltaTime, UnityEngine.Random.value));
        }

        private void Mine()
        {
            Apply(gameService.Mine(state, UnityEngine.Random.value));
        }

        private void Upgrade(UpgradeType type)
        {
            Apply(gameService.TryUpgrade(state, type), type);
        }

        private void OpenChapterSelection()
        {
            view.ShowChapterSelection(gameService.GetChapterSelectionOptions(state));
        }

        private void SelectChapter(int chapterNumber)
        {
            var result = gameService.SelectChapter(state, chapterNumber, UnityEngine.Random.value);
            if (!result.StateChanged)
            {
                return;
            }

            Render();
            StateChanged?.Invoke();
            SaveRequested?.Invoke();
        }

        private void Research(string nodeId)
        {
            var status = gameService.TryPurchaseResearch(state, nodeId);
            if (status != ResearchPurchaseStatus.Success)
            {
                var key = status switch
                {
                    ResearchPurchaseStatus.FeatureLocked => "research_locked",
                    ResearchPurchaseStatus.PrerequisiteMissing => "research_prerequisite",
                    ResearchPurchaseStatus.MaxLevel => "research_max_level",
                    ResearchPurchaseStatus.InsufficientCores => "not_enough_cores",
                    _ => "research_unavailable"
                };
                view.ShowFeedback(LanguageService.Get(key), new Color(1f, 0.55f, 0.3f));
                return;
            }

            Render();
            StateChanged?.Invoke();
            SaveRequested?.Invoke();
            view.ShowFeedback(LanguageService.Get("research_complete"), new Color(0.45f, 0.95f, 1f));
            GameAudioController.Instance?.PlayUpgradeSuccess();
        }

        private void Apply(MiningGameResult result, UpgradeType? upgradedType = null)
        {
            if (result.PurchaseFailed)
            {
                view.ShowFeedback(LanguageService.Get("not_enough_credits"), new Color(1f, 0.45f, 0.35f));
                return;
            }

            if (!result.StateChanged)
            {
                return;
            }

            Render();
            StateChanged?.Invoke();
            if (result.OreBroken)
            {
                OreBroken?.Invoke();
            }

            if (result.FirstChapterClear)
            {
                view.ShowChapterComplete(
                    result.CompletedChapterNumber,
                    result.RewardCredits,
                    result.RewardGems,
                    result.RewardBlueprintCores);
                GameAudioController.Instance?.PlayReward();
            }
            else if (result.Progression.DidLevelUp)
            {
                var message = string.Format(
                    LanguageService.Get("miner_level_up"),
                    result.Progression.CurrentLevel);
                if (result.Progression.RewardCredits > 0)
                {
                    message += $"  +{CompactNumberFormatter.Format(result.Progression.RewardCredits)} C";
                }

                if (result.Progression.RewardGems > 0)
                {
                    message += $"  +{CompactNumberFormatter.Format(result.Progression.RewardGems)} \u25C6";
                }

                if (result.Progression.UnlockedFeatures.Count > 0)
                {
                    var unlocked = LanguageService.Get(
                        result.Progression.UnlockedFeatures[0].LocalizationKey());
                    message += $"\n{unlocked} {LanguageService.Get("unlocked")}";
                }

                view.ShowFeedback(message, new Color(0.45f, 0.95f, 1f));
                GameAudioController.Instance?.PlayReward();
            }
            else if (result.RewardCredits > 0 ||
                     result.RewardGems > 0 ||
                     result.RewardBlueprintCores > 0)
            {
                var message = result.RewardCredits > 0
                    ? $"+{CompactNumberFormatter.Format(result.RewardCredits)} C"
                    : string.Empty;
                if (result.RewardGems > 0)
                {
                    message += $"  +{CompactNumberFormatter.Format(result.RewardGems)} \u25C6";
                }

                if (result.RewardBlueprintCores > 0)
                {
                    message += $"  +{CompactNumberFormatter.Format(result.RewardBlueprintCores)} CORE";
                }

                view.ShowFeedback(message.Trim(), new Color(1f, 0.82f, 0.3f));
                GameAudioController.Instance?.PlayReward();
            }
            else if (result.BossFailed)
            {
                view.ShowFeedback(LanguageService.Get("boss_time_up"), new Color(1f, 0.42f, 0.28f));
            }
            else if (result.PurchaseSucceeded && upgradedType.HasValue)
            {
                view.PlayUpgradeSuccess(upgradedType.Value);
                GameAudioController.Instance?.PlayUpgradeSuccess();
            }

            if (result.OreBroken || result.PurchaseSucceeded || result.BossFailed)
            {
                SaveRequested?.Invoke();
            }
        }
    }
}
