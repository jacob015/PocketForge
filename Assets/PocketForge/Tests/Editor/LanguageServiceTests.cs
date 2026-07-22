using NUnit.Framework;
using PocketForge.Localization;

namespace PocketForge.Tests.Editor
{
    public sealed class LanguageServiceTests
    {
        [TestCase(SupportedLanguage.Korean, "채굴")]
        [TestCase(SupportedLanguage.English, "Mine")]
        [TestCase(SupportedLanguage.Japanese, "採掘")]
        [TestCase(SupportedLanguage.ChineseSimplified, "采矿")]
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
    }
}
