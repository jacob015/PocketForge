using System;
using PocketForge.Save;

namespace PocketForge.Iap
{
    public sealed class MineIapCoordinator : IDisposable
    {
        private readonly IIapService iapService;
        private readonly GameSaveData saveData;
        private readonly Func<bool> saveEntitlement;

        public MineIapCoordinator(IIapService iapService, GameSaveData saveData, Func<bool> saveEntitlement)
        {
            this.iapService = iapService;
            this.saveData = saveData;
            this.saveEntitlement = saveEntitlement;
            iapService.StateChanged += HandleStateChanged;
            iapService.RemoveAdsEntitlementReceived += HandleEntitlementReceived;
        }

        public event Action<IapState, string, bool> DisplayChanged;

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

        public void Dispose()
        {
            iapService.StateChanged -= HandleStateChanged;
            iapService.RemoveAdsEntitlementReceived -= HandleEntitlementReceived;
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

        private void Publish()
        {
            DisplayChanged?.Invoke(
                saveData.adsRemoved ? IapState.Purchased : iapService.State,
                iapService.LocalizedPrice,
                saveData.adsRemoved);
        }
    }
}
