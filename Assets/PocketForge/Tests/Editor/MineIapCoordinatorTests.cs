using System;
using System.Collections.Generic;
using NUnit.Framework;
using PocketForge.Iap;
using PocketForge.Save;

namespace PocketForge.Tests.Editor
{
    public sealed class MineIapCoordinatorTests
    {
        [Test]
        public void PendingPurchase_SavesEntitlementBeforeConfirmation()
        {
            var service = new FakeIapService();
            var data = new GameSaveData();
            var order = new List<string>();
            using var coordinator = new MineIapCoordinator(service, data, () =>
            {
                order.Add("save");
                return true;
            });
            service.Confirmed = () => order.Add("confirm");

            service.DeliverEntitlement(true);

            Assert.IsTrue(data.adsRemoved);
            CollectionAssert.AreEqual(new[] { "save", "confirm" }, order);
        }

        [Test]
        public void FailedSave_RollsBackEntitlementAndDoesNotConfirm()
        {
            var service = new FakeIapService();
            var data = new GameSaveData();
            using var coordinator = new MineIapCoordinator(service, data, () => false);

            service.DeliverEntitlement(true);

            Assert.IsFalse(data.adsRemoved);
            Assert.AreEqual(0, service.ConfirmCount);
        }

        [Test]
        public void ExistingEntitlement_ConfirmsRedeliveredOrderWithoutSavingAgain()
        {
            var service = new FakeIapService();
            var saveCount = 0;
            using var coordinator = new MineIapCoordinator(
                service,
                new GameSaveData { adsRemoved = true },
                () =>
                {
                    saveCount++;
                    return false;
                });

            service.DeliverEntitlement(true);

            Assert.AreEqual(0, saveCount);
            Assert.AreEqual(1, service.ConfirmCount);
        }

        [Test]
        public void RestoredEntitlement_IsSavedWithoutConfirmingAgain()
        {
            var service = new FakeIapService();
            var data = new GameSaveData();
            var saveCount = 0;
            using var coordinator = new MineIapCoordinator(service, data, () =>
            {
                saveCount++;
                return true;
            });

            service.DeliverEntitlement(false);

            Assert.IsTrue(data.adsRemoved);
            Assert.AreEqual(1, saveCount);
            Assert.AreEqual(0, service.ConfirmCount);
        }

        [Test]
        public void ExistingEntitlement_BlocksDuplicatePurchase()
        {
            var service = new FakeIapService();
            using var coordinator = new MineIapCoordinator(
                service,
                new GameSaveData { adsRemoved = true },
                () => true);

            coordinator.PurchaseRemoveAds();

            Assert.AreEqual(0, service.PurchaseCount);
        }

        [Test]
        public void Dispose_UnsubscribesFromEntitlementEvents()
        {
            var service = new FakeIapService();
            var data = new GameSaveData();
            var saveCount = 0;
            var coordinator = new MineIapCoordinator(service, data, () =>
            {
                saveCount++;
                return true;
            });

            coordinator.Dispose();
            service.DeliverEntitlement(true);

            Assert.IsFalse(data.adsRemoved);
            Assert.AreEqual(0, saveCount);
            Assert.AreEqual(0, service.ConfirmCount);
        }

        [Test]
        public void PurchaseAndRestore_AreForwardedToService()
        {
            var service = new FakeIapService();
            using var coordinator = new MineIapCoordinator(service, new GameSaveData(), () => true);

            coordinator.PurchaseRemoveAds();
            coordinator.RestorePurchases();

            Assert.AreEqual(1, service.PurchaseCount);
            Assert.AreEqual(1, service.RestoreCount);
        }

        private sealed class FakeIapService : IIapService
        {
            public IapState State { get; private set; } = IapState.Ready;
            public string LocalizedPrice => "$1.99";
            public int PurchaseCount { get; private set; }
            public int RestoreCount { get; private set; }
            public int ConfirmCount { get; private set; }
            public Action Confirmed { get; set; }

            public event Action<IapState> StateChanged;
            public event Action<bool> RemoveAdsEntitlementReceived;

            public void Initialize() => StateChanged?.Invoke(State);
            public void PurchaseRemoveAds() => PurchaseCount++;
            public void RestorePurchases() => RestoreCount++;

            public void ConfirmPendingRemoveAds()
            {
                ConfirmCount++;
                Confirmed?.Invoke();
            }

            public void DeliverEntitlement(bool requiresConfirmation)
            {
                RemoveAdsEntitlementReceived?.Invoke(requiresConfirmation);
            }

            public void Dispose()
            {
            }
        }
    }
}
