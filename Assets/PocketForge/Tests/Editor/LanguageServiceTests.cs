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
    }
}
