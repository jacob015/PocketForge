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
    }
}
