using System;
using PocketForge.Economy;
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
            Apply(gameService.TryUpgrade(state, type));
        }

        private void Apply(MiningGameResult result)
        {
            if (result.PurchaseFailed)
            {
                view.ShowFeedback("NOT ENOUGH CREDITS", new Color(1f, 0.45f, 0.35f));
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
            }
            else if (result.PurchaseSucceeded)
            {
                view.ShowFeedback("UPGRADE COMPLETE", new Color(0.45f, 0.95f, 0.7f));
            }
            if (result.OreBroken || result.PurchaseSucceeded)
            {
                SaveRequested?.Invoke();
            }
        }
    }
}
