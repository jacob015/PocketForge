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

        [TestCase(SupportedLanguage.Korean, "\uC2DC\uAC04 \uCD08\uACFC! \uC774\uC804 \uC2A4\uD14C\uC774\uC9C0\uB97C \uC790\uB3D9 \uCC44\uAD74\uD569\uB2C8\uB2E4")]
        [TestCase(SupportedLanguage.English, "Time up! Auto-mining the previous stage")]
        [TestCase(SupportedLanguage.Japanese, "\u30BF\u30A4\u30E0\u30A2\u30C3\u30D7! \u524D\u306E\u30B9\u30C6\u30FC\u30B8\u3092\u81EA\u52D5\u63A1\u6398\u4E2D")]
        [TestCase(SupportedLanguage.ChineseSimplified, "\u65F6\u95F4\u5230! \u6B63\u5728\u81EA\u52A8\u5F00\u91C7\u4E0A\u4E00\u5173")]
        public void SelectedLanguage_ResolvesBossFarmFeedback(SupportedLanguage language, string expected)
        {
            LanguageService.SetLanguage(language);
            Assert.AreEqual(expected, LanguageService.Get("boss_time_up"));
        }

        [TestCase(SupportedLanguage.Korean, "\uB3C4\uC804")]
        [TestCase(SupportedLanguage.English, "Challenge")]
        [TestCase(SupportedLanguage.Japanese, "\u6311\u6226")]
        [TestCase(SupportedLanguage.ChineseSimplified, "\u6311\u6218")]
        public void SelectedLanguage_ResolvesBossChallengeAction(SupportedLanguage language, string expected)
        {
            LanguageService.SetLanguage(language);
            Assert.AreEqual(expected, LanguageService.Get("challenge"));
        }

        [TestCase(SupportedLanguage.Korean, "\uAD11\uC11D")]
        [TestCase(SupportedLanguage.English, "ore")]
        [TestCase(SupportedLanguage.Japanese, "\u9271\u77F3")]
        [TestCase(SupportedLanguage.ChineseSimplified, "\u77FF\u77F3")]
        public void SelectedLanguage_ResolvesOfflineProgressSummary(
            SupportedLanguage language,
            string expectedOreLabel)
        {
            LanguageService.SetLanguage(language);
            var summary = string.Format(
                LanguageService.Get("offline_summary"),
                string.Format(LanguageService.Get("offline_duration_hm"), 1, 2),
                3,
                4,
                5);

            Assert.That(summary, Does.Contain(expectedOreLabel));
            Assert.That(summary, Does.Contain("3"));
            Assert.That(summary, Does.Contain("+4 C"));
            Assert.That(summary, Does.Contain("+5 XP"));
            Assert.That(summary, Does.Contain("\n"));
        }

        [TestCase(SupportedLanguage.Korean)]
        [TestCase(SupportedLanguage.English)]
        [TestCase(SupportedLanguage.Japanese)]
        [TestCase(SupportedLanguage.ChineseSimplified)]
        public void SelectedLanguage_ResolvesMinerProgressionLabels(SupportedLanguage language)
        {
            LanguageService.SetLanguage(language);

            Assert.That(LanguageService.Get("miner_rank_short"), Does.Not.Contain("miner_rank_short"));
            Assert.That(LanguageService.Get("miner_level_up"), Does.Not.Contain("miner_level_up"));
            Assert.That(LanguageService.Get("feature_research"), Does.Not.Contain("feature_research"));
            Assert.That(LanguageService.Get("next_unlock"), Does.Not.Contain("next_unlock"));
            Assert.That(LanguageService.Get("blueprint_cores"), Does.Not.Contain("blueprint_cores"));
            Assert.That(LanguageService.Get("research_core_output"), Does.Not.Contain("research_core_output"));
            Assert.That(LanguageService.Get("research_complete"), Does.Not.Contain("research_complete"));
            Assert.That(LanguageService.Get("stage"), Does.Not.Contain("stage"));
            Assert.That(LanguageService.Get("power"), Does.Not.Contain("power"));
            Assert.That(LanguageService.Get("recommended"), Does.Not.Contain("recommended"));
            Assert.That(LanguageService.Get("ore_health"), Does.Not.Contain("ore_health"));
            Assert.That(LanguageService.Get("boss_in_stages"), Does.Not.Contain("boss_in_stages"));
            Assert.That(LanguageService.Get("offline_rewards"), Does.Not.Contain("offline_rewards"));
            Assert.That(LanguageService.Get("home"), Does.Not.Contain("home"));
            Assert.That(LanguageService.Get("coming_soon"), Does.Not.Contain("coming_soon"));
            Assert.That(LanguageService.Get("mine_crystal_cavern"), Does.Not.Contain("mine_crystal_cavern"));
            Assert.That(LanguageService.Get("mine_magma_depths"), Does.Not.Contain("mine_magma_depths"));
            Assert.That(LanguageService.Get("mine_ancient_city"), Does.Not.Contain("mine_ancient_city"));
        }

        [TestCase(SupportedLanguage.Korean)]
        [TestCase(SupportedLanguage.English)]
        [TestCase(SupportedLanguage.Japanese)]
        [TestCase(SupportedLanguage.ChineseSimplified)]
        public void SelectedLanguage_ResolvesEquipmentLabels(SupportedLanguage language)
        {
            LanguageService.SetLanguage(language);

            Assert.That(LanguageService.Get("equipment_power_summary"), Does.Not.Contain("equipment_power_summary"));
            Assert.That(LanguageService.Get("equipment_slot_charm"), Does.Not.Contain("equipment_slot_charm"));
            Assert.That(LanguageService.Get("equipment_rarity_legendary"), Does.Not.Contain("equipment_rarity_legendary"));
            Assert.That(LanguageService.Get("equipment_rugged_pickaxe"), Does.Not.Contain("equipment_rugged_pickaxe"));
            Assert.That(LanguageService.Get("equipment_auto_equip"), Does.Not.Contain("equipment_auto_equip"));
            Assert.That(LanguageService.Get("equipment_need_three"), Does.Not.Contain("equipment_need_three"));
        }

        [TestCase(SupportedLanguage.Korean)]
        [TestCase(SupportedLanguage.English)]
        [TestCase(SupportedLanguage.Japanese)]
        [TestCase(SupportedLanguage.ChineseSimplified)]
        public void SelectedLanguage_ResolvesCollectionAndAchievementLabels(SupportedLanguage language)
        {
            LanguageService.SetLanguage(language);

            Assert.That(LanguageService.Get("collection_title"), Does.Not.Contain("collection_title"));
            Assert.That(LanguageService.Get("museum_summary"), Does.Not.Contain("museum_summary"));
            Assert.That(LanguageService.Get("achievement_summary"), Does.Not.Contain("achievement_summary"));
            Assert.That(LanguageService.Get("achievement_mine_ores"), Does.Not.Contain("achievement_mine_ores"));
            Assert.That(LanguageService.Get("achievement_collect_equipment"), Does.Not.Contain("achievement_collect_equipment"));
            Assert.That(LanguageService.Get("ore_copper"), Does.Not.Contain("ore_copper"));
            Assert.That(LanguageService.Get("claim"), Does.Not.Contain("claim"));
        }
    }
}
