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

        event Action<IapState> StateChanged;
        event Action<bool> RemoveAdsEntitlementReceived;

        void Initialize();
        void PurchaseRemoveAds();
        void RestorePurchases();
        void ConfirmPendingRemoveAds();
    }
}
