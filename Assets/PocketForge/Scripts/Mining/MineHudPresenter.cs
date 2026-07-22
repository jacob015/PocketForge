using System;
using PocketForge.Audio;
using PocketForge.Economy;
using PocketForge.Localization;
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
            view.Bind(Mine, Upgrade);
        }

        public event Action StateChanged;
        public event Action SaveRequested;
        public event Action OreBroken;

        public void Render() => view.Render(state, gameService);

        public void ShowOfflineReward(int credits)
        {
            if (credits > 0)
            {
                view.ShowOfflineReward(credits);
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

            if (result.RewardCredits > 0)
            {
                view.ShowFeedback($"+{result.RewardCredits:N0} C", new Color(1f, 0.82f, 0.3f));
                GameAudioController.Instance?.PlayReward();
            }
            else if (result.PurchaseSucceeded && upgradedType.HasValue)
            {
                view.PlayUpgradeSuccess(upgradedType.Value);
                GameAudioController.Instance?.PlayUpgradeSuccess();
            }
            if (result.OreBroken || result.PurchaseSucceeded)
            {
                SaveRequested?.Invoke();
            }
        }
    }
}
