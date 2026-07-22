using System;

namespace PocketForge.Ads
{
    public enum RewardedAdState
    {
        Initializing,
        Loading,
        Ready,
        Showing,
        Failed
    }

    public interface IAdsService : IDisposable
    {
        RewardedAdState RewardedState { get; }
        event Action<RewardedAdState> RewardedStateChanged;
        void Initialize();
        void RetryRewarded();
        bool ShowRewarded(Action rewardGranted);
        bool ShowInterstitial();
    }
}
