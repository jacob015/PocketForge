using System;

namespace PocketForge.Iap
{
    public enum IapState
    {
        Initializing,
        Ready,
        Purchasing,
        Restoring,
        Purchased,
        Deferred,
        Cancelled,
        Failed
    }

    public interface IIapService : IDisposable
    {
        IapState State { get; }
        string LocalizedPrice { get; }
        string StarterPackLocalizedPrice { get; }
        bool RemoveAdsAvailable { get; }
        bool StarterPackAvailable { get; }

        event Action<IapState> StateChanged;
        event Action<bool> RemoveAdsEntitlementReceived;
        event Action<bool> StarterPackEntitlementReceived;

        void Initialize();
        void PurchaseRemoveAds();
        void PurchaseStarterPack();
        void RestorePurchases();
        void ConfirmPendingRemoveAds();
        void ConfirmPendingStarterPack();
    }
}
