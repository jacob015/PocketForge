using System;
using System.Linq;
using PocketForge.Localization;
using PocketForge.Mining;
using PocketForge.Presentation;
using PocketForge.Progression;
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
            if (gameState.Player.adsRemoved)
            {
                return;
            }

            if (interstitialPolicy.RegisterOreBreak() && adsService.ShowInterstitial())
            {
                interstitialPolicy.MarkShown();
            }
        }

        public void RequestShopReward(string productId)
        {
            var state = gameService.GetShopBoard(gameState).Products
                .FirstOrDefault(candidate => candidate.Definition.ProductId == productId);
            if (state.Definition == null || state.Remaining <= 0)
            {
                view.ShowFeedback(
                    LanguageService.Get("shop_daily_limit"),
                    new Color(1f, 0.55f, 0.35f));
                return;
            }

            if (adsService.RewardedState == RewardedAdState.Failed)
            {
                adsService.RetryRewarded();
                view.ShowFeedback(LanguageService.Get("ad_loading").ToUpper(), new Color(0.55f, 0.85f, 1f));
                return;
            }

            if (!adsService.ShowRewarded(() => GrantShopReward(productId)))
            {
                view.ShowFeedback(LanguageService.Get("ad_unavailable").ToUpper(), new Color(1f, 0.55f, 0.35f));
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
                $"{LanguageService.Get("ad_rewarded").ToUpper()}  +{CompactNumberFormatter.Format(result.RewardCredits)} C",
                new Color(0.45f, 0.95f, 0.7f));
            SaveRequested?.Invoke();
        }

        private void GrantShopReward(string productId)
        {
            var result = gameService.GrantRewardedShopProduct(gameState, productId);
            if (result.Status != ShopActionStatus.Success)
            {
                view.ShowFeedback(LanguageService.Get("shop_unavailable"), new Color(1f, 0.55f, 0.35f));
                return;
            }

            view.Render(gameState, gameService);
            view.ShowFeedback(
                $"{LanguageService.Get("ad_rewarded").ToUpper()}  +{CompactNumberFormatter.Format(result.RewardCredits)} C",
                new Color(0.45f, 0.95f, 0.7f));
            SaveRequested?.Invoke();
        }

        private void HandleRewardedStateChanged(RewardedAdState state)
        {
            view.SetRewardedAdState(state, gameService.GetRewardedAdCredits(gameState));
        }
    }
}
