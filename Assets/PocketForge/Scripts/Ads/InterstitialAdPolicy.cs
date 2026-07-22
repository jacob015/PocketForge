using UnityEngine;

namespace PocketForge.Ads
{
    public sealed class InterstitialAdPolicy
    {
        private readonly int oreBreakInterval;
        private readonly float cooldownSeconds;
        private int oreBreaksSinceLastAd;
        private float elapsedSinceLastAd;

        public InterstitialAdPolicy(int oreBreakInterval, float cooldownSeconds)
        {
            this.oreBreakInterval = Mathf.Max(1, oreBreakInterval);
            this.cooldownSeconds = Mathf.Max(0f, cooldownSeconds);
        }

        public void Tick(float unscaledDeltaTime)
        {
            elapsedSinceLastAd += Mathf.Max(0f, unscaledDeltaTime);
        }

        public bool RegisterOreBreak()
        {
            oreBreaksSinceLastAd++;
            return oreBreaksSinceLastAd >= oreBreakInterval && elapsedSinceLastAd >= cooldownSeconds;
        }

        public void MarkShown()
        {
            oreBreaksSinceLastAd = 0;
            elapsedSinceLastAd = 0f;
        }
    }
}
