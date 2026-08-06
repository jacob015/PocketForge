using UnityEngine;

namespace PocketForge.Ads
{
    public sealed class InterstitialAdPolicy
    {
        private readonly int oreBreakInterval;
        private readonly float cooldownSeconds;
        private readonly float graceSeconds;
        private int oreBreaksSinceLastAd;
        private float elapsedSinceLastAd;
        private float elapsedSinceStart;

        /// <param name="graceSeconds">
        /// Play time before the first interstitial may appear at all, so a new player
        /// reaches the first boss without being interrupted.
        /// </param>
        public InterstitialAdPolicy(int oreBreakInterval, float cooldownSeconds, float graceSeconds = 0f)
        {
            this.oreBreakInterval = Mathf.Max(1, oreBreakInterval);
            this.cooldownSeconds = Mathf.Max(0f, cooldownSeconds);
            this.graceSeconds = Mathf.Max(0f, graceSeconds);
        }

        public void Tick(float unscaledDeltaTime)
        {
            var delta = Mathf.Max(0f, unscaledDeltaTime);
            elapsedSinceLastAd += delta;
            elapsedSinceStart += delta;
        }

        public bool RegisterOreBreak()
        {
            oreBreaksSinceLastAd++;
            return elapsedSinceStart >= graceSeconds &&
                   oreBreaksSinceLastAd >= oreBreakInterval &&
                   elapsedSinceLastAd >= cooldownSeconds;
        }

        public void MarkShown()
        {
            oreBreaksSinceLastAd = 0;
            elapsedSinceLastAd = 0f;
        }
    }
}
