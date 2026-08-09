using System;
using System.IO;
using NUnit.Framework;

namespace PocketForge.Tests.Editor
{
    /// <summary>
    /// Google's EU user consent policy requires a certified consent screen before
    /// personalised ads are served in the EEA and the UK. Nothing about a missing
    /// consent screen is visible while testing from Korea — ads simply load — so the
    /// ordering is asserted here rather than left to be noticed after a policy strike.
    ///
    /// The checks read source text because the flow lives inside Google Mobile Ads
    /// callbacks that cannot be driven from an EditMode test.
    /// </summary>
    public sealed class ConsentGateTests
    {
        private const string AdServicePath =
            "Assets/PocketForge/Scripts/Ads/GoogleMobileAdsService.cs";

        private static string Source =>
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), AdServicePath));

        [Test]
        public void ConsentIsRequestedAndTheFormIsShownWhenRequired()
        {
            var source = Source;

            Assert.That(source, Does.Contain("ConsentInformation.Update("),
                "Consent information is never refreshed, so the SDK cannot know whether a form is due.");
            Assert.That(source, Does.Contain("ConsentForm.LoadAndShowConsentFormIfRequired("),
                "The consent form is never presented, so EEA users are never asked.");
        }

        [Test]
        public void AdsAreOnlyInitialisedAfterConsentAllowsIt()
        {
            var source = Source;
            var gate = source.IndexOf("ConsentInformation.CanRequestAds()", StringComparison.Ordinal);
            var initialize = source.IndexOf("MobileAds.Initialize(", StringComparison.Ordinal);

            Assert.That(gate, Is.GreaterThanOrEqualTo(0),
                "CanRequestAds is never consulted, so ads could be requested without consent.");
            Assert.That(initialize, Is.GreaterThanOrEqualTo(0), "The ads SDK is never initialised.");
            Assert.That(gate, Is.LessThan(initialize),
                "MobileAds.Initialize appears before the consent gate; ads would be requested first.");
        }

        [Test]
        public void UsersCanReopenTheConsentScreen()
        {
            var source = Source;

            // Required wherever consent was gathered: the choice has to be changeable.
            Assert.That(source, Does.Contain("ConsentForm.ShowPrivacyOptionsForm("),
                "There is no way to reopen the consent screen after the first answer.");
            Assert.That(source, Does.Contain("PrivacyOptionsRequirementStatus"),
                "Nothing reports whether the privacy entry point is required, so it cannot be shown conditionally.");
        }

        [Test]
        public void DecliningConsentDoesNotLeaveTheRewardButtonInitialising()
        {
            var source = Source;
            var gate = source.IndexOf("!ConsentInformation.CanRequestAds()", StringComparison.Ordinal);

            Assert.That(gate, Is.GreaterThanOrEqualTo(0), "The declined path is not handled.");
            var branch = source.Substring(gate, Math.Min(600, source.Length - gate));
            Assert.That(branch, Does.Contain("RewardedAdState.Failed"),
                "A declined consent leaves the rewarded state on Initializing, so the button spins forever.");
        }
    }
}
