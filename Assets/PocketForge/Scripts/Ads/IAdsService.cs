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

        /// <summary>
        /// True where the user must be able to reopen the ad consent screen. Google's
        /// EU user consent policy requires that entry point wherever consent was
        /// gathered, so settings shows the option only when this is set.
        /// </summary>
        bool IsPrivacyOptionsRequired { get; }

        void Initialize();
        void RetryRewarded();
        bool ShowRewarded(Action rewardGranted);
        bool ShowInterstitial();

        /// <summary>Reopens the consent screen so the user can change their choice.</summary>
        void ShowPrivacyOptions();
    }
}
