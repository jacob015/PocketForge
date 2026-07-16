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

        public void Render() => view.Render(state, gameService);

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
            if (!result.StateChanged)
            {
                return;
            }

            Render();
            StateChanged?.Invoke();
            if (result.OreBroken || result.PurchaseSucceeded)
            {
                SaveRequested?.Invoke();
            }
        }
    }
}
