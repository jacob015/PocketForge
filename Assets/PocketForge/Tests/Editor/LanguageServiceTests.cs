using NUnit.Framework;
using PocketForge.Localization;

namespace PocketForge.Tests.Editor
{
    public sealed class LanguageServiceTests
    {
        private SupportedLanguage originalLanguage;

        [SetUp]
        public void SetUp()
        {
            LanguageService.Initialize();
            originalLanguage = LanguageService.Current;
        }

        [TearDown]
        public void TearDown()
        {
            LanguageService.SetLanguage(originalLanguage);
        }

        [TestCase(SupportedLanguage.Korean, "\uCC44\uAD74")]
        [TestCase(SupportedLanguage.English, "Mine")]
        [TestCase(SupportedLanguage.Japanese, "\u63A1\u6398")]
        [TestCase(SupportedLanguage.ChineseSimplified, "\u91C7\u77FF")]
        public void SelectedLanguage_ResolvesMineLabel(SupportedLanguage language, string expected)
        {
            LanguageService.SetLanguage(language);
            Assert.AreEqual(expected, LanguageService.Get("mine"));
        }

        [TestCase(SupportedLanguage.Korean, "\uC124\uC815")]
        [TestCase(SupportedLanguage.English, "Settings")]
        [TestCase(SupportedLanguage.Japanese, "\u8A2D\u5B9A")]
        [TestCase(SupportedLanguage.ChineseSimplified, "\u8BBE\u7F6E")]
        public void SelectedLanguage_ResolvesSettingsLabel(SupportedLanguage language, string expected)
        {
            LanguageService.SetLanguage(language);
            Assert.AreEqual(expected, LanguageService.Get("settings"));
        }

        [TestCase(SupportedLanguage.Korean, "\uAD11\uACE0 \uC81C\uAC70")]
        [TestCase(SupportedLanguage.English, "Remove ads")]
        [TestCase(SupportedLanguage.Japanese, "\u5E83\u544A\u3092\u524A\u9664")]
        [TestCase(SupportedLanguage.ChineseSimplified, "\u79FB\u9664\u5E7F\u544A")]
        public void SelectedLanguage_ResolvesRemoveAdsLabel(SupportedLanguage language, string expected)
        {
            LanguageService.SetLanguage(language);
            Assert.AreEqual(expected, LanguageService.Get("remove_ads"));
        }

        [TestCase(SupportedLanguage.Korean, "\uBC30\uACBD\uC74C")]
        [TestCase(SupportedLanguage.English, "Music")]
        [TestCase(SupportedLanguage.Japanese, "BGM")]
        [TestCase(SupportedLanguage.ChineseSimplified, "\u97F3\u4E50")]
        public void SelectedLanguage_ResolvesMusicSetting(SupportedLanguage language, string expected)
        {
            LanguageService.SetLanguage(language);
            Assert.AreEqual(expected, LanguageService.Get("music"));
        }
    }
}
