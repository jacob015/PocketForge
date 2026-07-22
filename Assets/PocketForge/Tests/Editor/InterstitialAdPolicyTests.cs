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
