using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace PocketForge.Ads
{
    public sealed class GoogleMobileAdsService : IAdsService
    {
#if UNITY_ANDROID
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        private const string InterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
        private const string InterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        private const string RewardedAdUnitId = "unused";
        private const string InterstitialAdUnitId = "unused";
#endif

        private RewardedAd rewardedAd;
        private InterstitialAd interstitialAd;
        private bool initializationStarted;
        private bool initialized;
        private bool interstitialLoading;
        private bool disposed;

        public RewardedAdState RewardedState { get; private set; } = RewardedAdState.Initializing;
        public event Action<RewardedAdState> RewardedStateChanged;

        public void Initialize()
        {
            if (initializationStarted || disposed)
            {
                return;
            }

            initializationStarted = true;
            SetRewardedState(RewardedAdState.Initializing);
            try
            {
                MobileAds.SetRequestConfiguration(new RequestConfiguration
                {
                    TestDeviceIds = new List<string> { AdRequest.TestDeviceSimulator }
                });

                MobileAds.Initialize(status => RunOnMainThread(() =>
                {
                    if (disposed)
                    {
                        return;
                    }

                    if (status == null)
                    {
                        Debug.LogWarning("Google Mobile Ads initialization failed.");
                        initializationStarted = false;
                        SetRewardedState(RewardedAdState.Failed);
                        return;
                    }

                    initialized = true;
                    LoadRewarded();
                    LoadInterstitial();
                }));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Google Mobile Ads initialization failed: {exception.GetType().Name}");
                initializationStarted = false;
                SetRewardedState(RewardedAdState.Failed);
            }
        }

        public void RetryRewarded()
        {
            if (!disposed && RewardedState == RewardedAdState.Failed)
            {
                if (initialized)
                {
                    LoadRewarded();
                }
                else
                {
                    initializationStarted = false;
                    Initialize();
                }
            }
        }

        public bool ShowRewarded(Action rewardGranted)
        {
            if (disposed || rewardedAd == null || !rewardedAd.CanShowAd())
            {
                return false;
            }

            var rewardDelivered = false;
            SetRewardedState(RewardedAdState.Showing);
            rewardedAd.Show(_ => RunOnMainThread(() =>
            {
                if (disposed || rewardDelivered)
                {
                    return;
                }

                rewardDelivered = true;
                rewardGranted?.Invoke();
            }));
            return true;
        }

        public bool ShowInterstitial()
        {
            if (disposed)
            {
                return false;
            }

            if (interstitialAd == null || !interstitialAd.CanShowAd())
            {
                if (!interstitialLoading)
                {
                    LoadInterstitial();
                }

                return false;
            }

            interstitialAd.Show();
            return true;
        }

        public void Dispose()
        {
            disposed = true;
            DestroyRewarded();
            DestroyInterstitial();
        }

        private void LoadRewarded()
        {
            DestroyRewarded();
            SetRewardedState(RewardedAdState.Loading);
            RewardedAd.Load(RewardedAdUnitId, new AdRequest(), (ad, error) => RunOnMainThread(() =>
            {
                if (disposed)
                {
                    ad?.Destroy();
                    return;
                }

                if (error != null || ad == null)
                {
                    Debug.LogWarning($"Rewarded ad load failed: {error}");
                    SetRewardedState(RewardedAdState.Failed);
                    return;
                }

                rewardedAd = ad;
                RegisterRewardedEvents(ad);
                SetRewardedState(RewardedAdState.Ready);
            }));
        }

        private void LoadInterstitial()
        {
            if (disposed || interstitialLoading)
            {
                return;
            }

            DestroyInterstitial();
            interstitialLoading = true;
            InterstitialAd.Load(InterstitialAdUnitId, new AdRequest(), (ad, error) => RunOnMainThread(() =>
            {
                interstitialLoading = false;
                if (disposed)
                {
                    ad?.Destroy();
                    return;
                }

                if (error != null || ad == null)
                {
                    Debug.LogWarning($"Interstitial ad load failed: {error}");
                    return;
                }

                interstitialAd = ad;
                RegisterInterstitialEvents(ad);
            }));
        }

        private void RegisterRewardedEvents(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () => RunOnMainThread(() => ReloadRewarded(ad));
            ad.OnAdFullScreenContentFailed += error => RunOnMainThread(() =>
            {
                Debug.LogWarning($"Rewarded ad display failed: {error}");
                ReloadRewarded(ad);
            });
        }

        private void RegisterInterstitialEvents(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () => RunOnMainThread(() => ReloadInterstitial(ad));
            ad.OnAdFullScreenContentFailed += error => RunOnMainThread(() =>
            {
                Debug.LogWarning($"Interstitial ad display failed: {error}");
                ReloadInterstitial(ad);
            });
        }

        private void ReloadRewarded(RewardedAd completedAd)
        {
            if (disposed || rewardedAd != completedAd)
            {
                return;
            }

            DestroyRewarded();
            LoadRewarded();
        }

        private void ReloadInterstitial(InterstitialAd completedAd)
        {
            if (disposed || interstitialAd != completedAd)
            {
                return;
            }

            DestroyInterstitial();
            LoadInterstitial();
        }

        private void SetRewardedState(RewardedAdState state)
        {
            RewardedState = state;
            RewardedStateChanged?.Invoke(state);
        }

        private void DestroyRewarded()
        {
            rewardedAd?.Destroy();
            rewardedAd = null;
        }

        private void DestroyInterstitial()
        {
            interstitialAd?.Destroy();
            interstitialAd = null;
        }

        private static void RunOnMainThread(Action action)
        {
            MobileAdsEventExecutor.ExecuteInUpdate(action);
        }
    }
}
