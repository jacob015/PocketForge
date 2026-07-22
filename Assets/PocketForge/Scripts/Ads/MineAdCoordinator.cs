using System;
using PocketForge.Localization;
using PocketForge.Mining;
using UnityEngine;

namespace PocketForge.Ads
{
    public sealed class MineAdCoordinator : IDisposable
    {
        private readonly MineHudView view;
        private readonly MiningGameService gameService;
        private readonly MiningGameState gameState;
        private readonly IAdsService adsService;
        private readonly InterstitialAdPolicy interstitialPolicy;

        public MineAdCoordinator(
            MineHudView view,
            MiningGameService gameService,
            MiningGameState gameState,
            IAdsService adsService,
            InterstitialAdPolicy interstitialPolicy)
        {
            this.view = view;
            this.gameService = gameService;
            this.gameState = gameState;
            this.adsService = adsService;
            this.interstitialPolicy = interstitialPolicy;

            view.BindRewardedAd(RequestRewardedAd);
            adsService.RewardedStateChanged += HandleRewardedStateChanged;
        }

        public event Action SaveRequested;

        public void Initialize()
        {
            HandleRewardedStateChanged(adsService.RewardedState);
            adsService.Initialize();
        }

        public void Tick(float unscaledDeltaTime)
        {
            interstitialPolicy.Tick(unscaledDeltaTime);
        }

        public void RecordOreBroken()
        {
            if (interstitialPolicy.RegisterOreBreak() && adsService.ShowInterstitial())
            {
                interstitialPolicy.MarkShown();
            }
        }

        public void Dispose()
        {
            adsService.RewardedStateChanged -= HandleRewardedStateChanged;
            adsService.Dispose();
        }

        private void RequestRewardedAd()
        {
            if (adsService.RewardedState == RewardedAdState.Failed)
            {
                adsService.RetryRewarded();
                view.ShowFeedback(LanguageService.Get("ad_loading").ToUpper(), new Color(0.55f, 0.85f, 1f));
                return;
            }

            if (!adsService.ShowRewarded(GrantReward))
            {
                view.ShowFeedback(LanguageService.Get("ad_unavailable").ToUpper(), new Color(1f, 0.55f, 0.35f));
            }
        }

        private void GrantReward()
        {
            var result = gameService.GrantRewardedAdCredits(gameState);
            view.Render(gameState, gameService);
            view.ShowFeedback(
                $"{LanguageService.Get("ad_rewarded").ToUpper()}  +{result.RewardCredits:N0} C",
                new Color(0.45f, 0.95f, 0.7f));
            SaveRequested?.Invoke();
        }

        private void HandleRewardedStateChanged(RewardedAdState state)
        {
            view.SetRewardedAdState(state, gameService.GetRewardedAdCredits(gameState));
        }
    }
}
