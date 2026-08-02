using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace PocketForge.Iap
{
    public sealed class UnityIapService : IIapService
    {
        public const string RemoveAdsProductId = "remove_ads";
        public const string StarterPackProductId = "starter_pack";

        private StoreController storeController;
        private Product removeAdsProduct;
        private Product starterPackProduct;
        private PendingOrder pendingRemoveAdsOrder;
        private PendingOrder pendingStarterPackOrder;
        private bool disposed;

        public IapState State { get; private set; } = IapState.Initializing;
        public string LocalizedPrice { get; private set; } = string.Empty;
        public string StarterPackLocalizedPrice { get; private set; } = string.Empty;
        public bool RemoveAdsAvailable => removeAdsProduct?.availableToPurchase == true;
        public bool StarterPackAvailable => starterPackProduct?.availableToPurchase == true;

        public event Action<IapState> StateChanged;
        public event Action<bool> RemoveAdsEntitlementReceived;
        public event Action<bool> StarterPackEntitlementReceived;

        public async void Initialize()
        {
            if (storeController != null || disposed)
            {
                return;
            }

            SetState(IapState.Initializing);
            storeController = UnityIAPServices.StoreController();
            Subscribe();

            try
            {
                await storeController.Connect();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unity IAP connection failed: {exception.Message}");
                SetState(IapState.Failed);
            }
        }

        public void PurchaseRemoveAds()
        {
            if (State is not (IapState.Ready or IapState.Cancelled) || removeAdsProduct == null || !removeAdsProduct.availableToPurchase)
            {
                SetState(IapState.Failed);
                return;
            }

            SetState(IapState.Purchasing);
            storeController.PurchaseProduct(removeAdsProduct);
        }

        public void PurchaseStarterPack()
        {
            if (State is not (IapState.Ready or IapState.Cancelled or IapState.Purchased) ||
                starterPackProduct == null ||
                !starterPackProduct.availableToPurchase)
            {
                SetState(IapState.Failed);
                return;
            }

            SetState(IapState.Purchasing);
            storeController.PurchaseProduct(starterPackProduct);
        }

        public void RestorePurchases()
        {
            if (storeController == null)
            {
                SetState(IapState.Failed);
                return;
            }

            SetState(IapState.Restoring);
            storeController.RestoreTransactions((success, error) =>
            {
                if (!success)
                {
                    Debug.LogWarning($"Unity IAP restore failed: {error}");
                    SetState(IapState.Failed);
                }
            });
        }

        public void ConfirmPendingRemoveAds()
        {
            if (pendingRemoveAdsOrder == null)
            {
                return;
            }

            var order = pendingRemoveAdsOrder;
            pendingRemoveAdsOrder = null;
            storeController.ConfirmPurchase(order);
        }

        public void ConfirmPendingStarterPack()
        {
            if (pendingStarterPackOrder == null)
            {
                return;
            }

            var order = pendingStarterPackOrder;
            pendingStarterPackOrder = null;
            storeController.ConfirmPurchase(order);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            Unsubscribe();
        }

        private void Subscribe()
        {
            storeController.OnStoreConnected += HandleStoreConnected;
            storeController.OnStoreDisconnected += HandleStoreDisconnected;
            storeController.OnProductsFetched += HandleProductsFetched;
            storeController.OnProductsFetchFailed += HandleProductsFetchFailed;
            storeController.OnPurchasesFetched += HandlePurchasesFetched;
            storeController.OnPurchasesFetchFailed += HandlePurchasesFetchFailed;
            storeController.OnPurchasePending += HandlePurchasePending;
            storeController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
            storeController.OnPurchaseFailed += HandlePurchaseFailed;
            storeController.OnPurchaseDeferred += HandlePurchaseDeferred;
        }

        private void Unsubscribe()
        {
            if (storeController == null)
            {
                return;
            }

            storeController.OnStoreConnected -= HandleStoreConnected;
            storeController.OnStoreDisconnected -= HandleStoreDisconnected;
            storeController.OnProductsFetched -= HandleProductsFetched;
            storeController.OnProductsFetchFailed -= HandleProductsFetchFailed;
            storeController.OnPurchasesFetched -= HandlePurchasesFetched;
            storeController.OnPurchasesFetchFailed -= HandlePurchasesFetchFailed;
            storeController.OnPurchasePending -= HandlePurchasePending;
            storeController.OnPurchaseConfirmed -= HandlePurchaseConfirmed;
            storeController.OnPurchaseFailed -= HandlePurchaseFailed;
            storeController.OnPurchaseDeferred -= HandlePurchaseDeferred;
        }

        private void HandleStoreConnected()
        {
            storeController.FetchProducts(new List<ProductDefinition>
            {
                new(RemoveAdsProductId, ProductType.NonConsumable),
                new(StarterPackProductId, ProductType.NonConsumable)
            });
        }

        private void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            Debug.LogWarning($"Unity IAP disconnected: {failure.Message}");
            SetState(IapState.Failed);
        }

        private void HandleProductsFetched(List<Product> products)
        {
            removeAdsProduct = products.FirstOrDefault(product => product.definition.id == RemoveAdsProductId);
            starterPackProduct = products.FirstOrDefault(product => product.definition.id == StarterPackProductId);
            LocalizedPrice = removeAdsProduct?.metadata?.localizedPriceString ?? string.Empty;
            StarterPackLocalizedPrice = starterPackProduct?.metadata?.localizedPriceString ?? string.Empty;
            if (!RemoveAdsAvailable && !StarterPackAvailable)
            {
                SetState(IapState.Failed);
                return;
            }

            storeController.FetchPurchases();
        }

        private void HandleProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogWarning($"Unity IAP product fetch failed: {failure}");
            SetState(IapState.Failed);
        }

        private void HandlePurchasesFetched(Orders orders)
        {
            var ownsRemoveAds = orders.ConfirmedOrders.Any(order => ContainsProduct(order, RemoveAdsProductId));
            var ownsStarterPack = orders.ConfirmedOrders.Any(order => ContainsProduct(order, StarterPackProductId));
            if (ownsRemoveAds)
            {
                RemoveAdsEntitlementReceived?.Invoke(false);
            }

            if (ownsStarterPack)
            {
                StarterPackEntitlementReceived?.Invoke(false);
            }

            SetState(ownsRemoveAds && ownsStarterPack ? IapState.Purchased : IapState.Ready);
        }

        private void HandlePurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning($"Unity IAP purchase fetch failed: {failure.Message}");
            SetState(IapState.Failed);
        }

        private void HandlePurchasePending(PendingOrder order)
        {
            if (ContainsProduct(order, RemoveAdsProductId))
            {
                pendingRemoveAdsOrder = order;
                RemoveAdsEntitlementReceived?.Invoke(true);
                return;
            }

            if (ContainsProduct(order, StarterPackProductId))
            {
                pendingStarterPackOrder = order;
                StarterPackEntitlementReceived?.Invoke(true);
            }
        }

        private void HandlePurchaseConfirmed(Order order)
        {
            SetState(order is FailedOrder ? IapState.Failed : IapState.Purchased);
        }

        private void HandlePurchaseFailed(FailedOrder order)
        {
            Debug.LogWarning($"Unity IAP purchase failed: {order.FailureReason} - {order.Details}");
            SetState(order.FailureReason == PurchaseFailureReason.UserCancelled ? IapState.Cancelled : IapState.Failed);
        }

        private void HandlePurchaseDeferred(DeferredOrder _)
        {
            SetState(IapState.Deferred);
        }

        private static bool ContainsProduct(Order order, string productId)
        {
            return order?.CartOrdered?.Items().Any(item =>
                item.Product?.definition?.id == productId) == true;
        }

        private void SetState(IapState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
