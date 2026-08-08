using System.IO;
using NUnit.Framework;

namespace PocketForge.Tests.Editor
{
    /// <summary>
    /// Shipping Google's public test identifiers earns no revenue and the mistake is
    /// invisible at runtime — test ads render exactly like live ones. This was a listed
    /// release blocker for weeks, so the check is automated rather than remembered.
    ///
    /// The identifiers are asserted through the source text because the ad unit
    /// constants sit behind UNITY_ANDROID, which is not defined while tests run unless
    /// the active build target happens to be Android.
    /// </summary>
    public sealed class ReleaseAdIdentifierTests
    {
        private const string TestPublisher = "ca-app-pub-3940256099942544";
        private const string AdServicePath =
            "Assets/PocketForge/Scripts/Ads/GoogleMobileAdsService.cs";
        private const string AdSettingsPath =
            "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

        [Test]
        public void AndroidAppId_IsNotGooglesTestPublisher()
        {
            var settings = ReadProjectFile(AdSettingsPath);
            var line = FindLine(settings, "adMobAndroidAppId:");

            Assert.That(
                line,
                Does.Not.Contain(TestPublisher),
                "The Android AdMob app ID is still Google's test publisher.");
            Assert.That(line, Does.Contain("ca-app-pub-"), $"Unexpected app ID line: {line}");
            Assert.That(line, Does.Contain("~"), $"An app ID separates its publisher with '~': {line}");
        }

        [Test]
        public void AndroidAdUnitIds_AreNotGooglesTestUnits()
        {
            var android = ExtractAndroidBranch(ReadProjectFile(AdServicePath));

            Assert.That(
                android,
                Does.Not.Contain(TestPublisher),
                "The Android ad unit IDs are still Google's test units.");
            foreach (var name in new[] { "RewardedAdUnitId", "InterstitialAdUnitId" })
            {
                var line = FindLine(android, name);
                Assert.That(line, Does.Contain("ca-app-pub-"), $"Unexpected {name} line: {line}");
                Assert.That(line, Does.Contain("/"), $"An ad unit separates its publisher with '/': {line}");
            }
        }

        [Test]
        public void AndroidRewardedAndInterstitialUnits_AreDistinct()
        {
            var android = ExtractAndroidBranch(ReadProjectFile(AdServicePath));

            // Pointing both at one unit is the failure mode that survives a smoke test:
            // an ad still plays, just the wrong format for the placement.
            Assert.That(
                Quoted(FindLine(android, "RewardedAdUnitId")),
                Is.Not.EqualTo(Quoted(FindLine(android, "InterstitialAdUnitId"))),
                "The rewarded and interstitial placements share one ad unit.");
        }

        private static string ExtractAndroidBranch(string source)
        {
            var start = source.IndexOf("#if UNITY_ANDROID", System.StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "No UNITY_ANDROID branch to inspect.");
            var end = source.IndexOf("#elif", start, System.StringComparison.Ordinal);
            Assert.That(end, Is.GreaterThan(start), "The UNITY_ANDROID branch is not delimited.");
            return source.Substring(start, end - start);
        }

        private static string Quoted(string line)
        {
            var open = line.IndexOf('"');
            var close = line.LastIndexOf('"');
            Assert.That(close, Is.GreaterThan(open), $"No quoted value on: {line}");
            return line.Substring(open + 1, close - open - 1);
        }

        private static string FindLine(string text, string needle)
        {
            foreach (var line in text.Split('\n'))
            {
                if (line.Contains(needle))
                {
                    return line.Trim();
                }
            }

            Assert.Fail($"Expected to find a line containing \"{needle}\".");
            return null;
        }

        private static string ReadProjectFile(string relativePath)
        {
            var full = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            Assert.That(File.Exists(full), Is.True, $"Missing project file: {relativePath}");
            return File.ReadAllText(full);
        }
    }
}
