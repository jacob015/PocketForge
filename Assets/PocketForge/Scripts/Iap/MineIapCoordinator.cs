using System;
using PocketForge.Mining;
using PocketForge.Progression;
using PocketForge.Save;

namespace PocketForge.Iap
{
    public sealed class MineIapCoordinator : IDisposable
    {
        private readonly IIapService iapService;
        private readonly GameSaveData saveData;
        private readonly Func<bool> saveEntitlement;
        private readonly MiningGameService gameService;
        private readonly MiningGameState gameState;

        public MineIapCoordinator(
            IIapService iapService,
            GameSaveData saveData,
            Func<bool> saveEntitlement,
            MiningGameService gameService = null,
            MiningGameState gameState = null)
        {
            this.iapService = iapService;
            this.saveData = saveData;
            this.saveEntitlement = saveEntitlement;
            this.gameService = gameService;
            this.gameState = gameState;
            iapService.StateChanged += HandleStateChanged;
            iapService.RemoveAdsEntitlementReceived += HandleEntitlementReceived;
            iapService.StarterPackEntitlementReceived += HandleStarterPackEntitlementReceived;
        }

        public event Action<IapState, string, bool> DisplayChanged;
        public event Action<IapState, string, bool, bool> StarterPackDisplayChanged;

        public void Initialize()
        {
            Publish();
            iapService.Initialize();
        }

        public void PurchaseRemoveAds()
        {
            if (!saveData.adsRemoved)
            {
                iapService.PurchaseRemoveAds();
            }
        }

        public void RestorePurchases() => iapService.RestorePurchases();

        public void PurchaseStarterPack()
        {
            if (!saveData.starterPackPurchased)
            {
                iapService.PurchaseStarterPack();
            }
        }

        public void Dispose()
        {
            iapService.StateChanged -= HandleStateChanged;
            iapService.RemoveAdsEntitlementReceived -= HandleEntitlementReceived;
            iapService.StarterPackEntitlementReceived -= HandleStarterPackEntitlementReceived;
            iapService.Dispose();
        }

        private void HandleEntitlementReceived(bool requiresConfirmation)
        {
            if (!saveData.adsRemoved)
            {
                saveData.adsRemoved = true;
                if (!saveEntitlement())
                {
                    saveData.adsRemoved = false;
                    DisplayChanged?.Invoke(IapState.Failed, iapService.LocalizedPrice, false);
                    return;
                }
            }

            if (requiresConfirmation)
            {
                iapService.ConfirmPendingRemoveAds();
            }

            DisplayChanged?.Invoke(IapState.Purchased, iapService.LocalizedPrice, true);
        }

        private void HandleStateChanged(IapState _) => Publish();

        private void HandleStarterPackEntitlementReceived(bool requiresConfirmation)
        {
            if (!saveData.starterPackPurchased)
            {
                if (gameService == null || gameState == null)
                {
                    StarterPackDisplayChanged?.Invoke(
                        IapState.Failed,
                        iapService.StarterPackLocalizedPrice,
                        false,
                        iapService.StarterPackAvailable);
                    return;
                }

                var credits = saveData.credits;
                var gems = saveData.gems;
                var cores = saveData.blueprintCores;
                var result = gameService.GrantStarterPack(gameState);
                if (result.Status != ShopActionStatus.Success || !saveEntitlement())
                {
                    saveData.credits = credits;
                    saveData.gems = gems;
                    saveData.blueprintCores = cores;
                    saveData.starterPackPurchased = false;
                    StarterPackDisplayChanged?.Invoke(
                        IapState.Failed,
                        iapService.StarterPackLocalizedPrice,
                        false,
                        iapService.StarterPackAvailable);
                    return;
                }
            }

            if (requiresConfirmation)
            {
                iapService.ConfirmPendingStarterPack();
            }

            Publish();
        }

        private void Publish()
        {
            DisplayChanged?.Invoke(
                saveData.adsRemoved ? IapState.Purchased : iapService.State,
                iapService.LocalizedPrice,
                saveData.adsRemoved);
            StarterPackDisplayChanged?.Invoke(
                saveData.starterPackPurchased ? IapState.Purchased : iapService.State,
                iapService.StarterPackLocalizedPrice,
                saveData.starterPackPurchased,
                iapService.StarterPackAvailable);
        }
    }
}
