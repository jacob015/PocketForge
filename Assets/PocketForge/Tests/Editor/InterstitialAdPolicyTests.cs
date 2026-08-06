using NUnit.Framework;
using PocketForge.Ads;

namespace PocketForge.Tests.Editor
{
    public sealed class InterstitialAdPolicyTests
    {
        [Test]
        public void Interstitial_RequiresBreakIntervalAndCooldown()
        {
            var policy = new InterstitialAdPolicy(5, 180f);
            policy.Tick(180f);

            for (var index = 0; index < 4; index++)
            {
                Assert.IsFalse(policy.RegisterOreBreak());
            }

            Assert.IsTrue(policy.RegisterOreBreak());
        }

        [Test]
        public void GracePeriod_KeepsTheFirstSessionFreeOfInterstitials()
        {
            var policy = new InterstitialAdPolicy(1, 0f, 600f);

            policy.Tick(599f);
            Assert.IsFalse(policy.RegisterOreBreak(), "No interstitial may fire inside the grace window.");

            policy.Tick(2f);
            Assert.IsTrue(policy.RegisterOreBreak(), "The gate must open once the grace window passes.");
        }

        [Test]
        public void MarkShown_ResetsBothGates()
        {
            var policy = new InterstitialAdPolicy(1, 10f);
            policy.Tick(10f);
            Assert.IsTrue(policy.RegisterOreBreak());

            policy.MarkShown();

            Assert.IsFalse(policy.RegisterOreBreak());
            policy.Tick(10f);
            Assert.IsTrue(policy.RegisterOreBreak());
        }
    }
}
