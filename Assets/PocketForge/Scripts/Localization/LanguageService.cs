using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketForge.Localization
{
    public enum SupportedLanguage
    {
        Korean,
        English,
        Japanese,
        ChineseSimplified
    }

    public static class LanguageService
    {
        private const string LanguageKey = "PocketForge.Language";

        private static readonly Dictionary<SupportedLanguage, Dictionary<string, string>> Tables = new()
        {
            [SupportedLanguage.Korean] = new()
            {
                ["credits"] = "\uD06C\uB808\uB527", ["depth"] = "\uC2EC\uB3C4", ["ore"] = "\uAD11\uC11D", ["mine"] = "\uCC44\uAD74",
                ["pickaxe"] = "\uACE1\uAD2D\uC774", ["drill"] = "\uB4DC\uB9B4", ["robot"] = "\uB85C\uBD07", ["tap"] = "\uD0ED",
                ["auto"] = "\uC790\uB3D9", ["reward"] = "\uBCF4\uC0C1", ["rare"] = "\uD76C\uADC0", ["free_reward"] = "\uBB34\uB8CC \uBCF4\uC0C1",
                ["ad_loading"] = "\uAD11\uACE0 \uBD88\uB7EC\uC624\uB294 \uC911", ["ad_showing"] = "\uAD11\uACE0 \uC7AC\uC0DD \uC911", ["ad_retry"] = "\uAD11\uACE0 \uB2E4\uC2DC \uBC1B\uAE30",
                ["ad_unavailable"] = "\uAD11\uACE0\uB97C \uC900\uBE44\uD560 \uC218 \uC5C6\uC2B5\uB2C8\uB2E4", ["ad_rewarded"] = "\uAD11\uACE0 \uBCF4\uC0C1",
                ["settings"] = "\uC124\uC815", ["language"] = "\uC5B8\uC5B4", ["close"] = "\uB2EB\uAE30", ["audio"] = "\uC624\uB514\uC624",
                ["music"] = "\uBC30\uACBD\uC74C", ["sound"] = "\uD6A8\uACFC\uC74C", ["mute"] = "\uC74C\uC18C\uAC70", ["haptics"] = "\uC9C4\uB3D9",
                ["reduce_motion"] = "\uBAA8\uC158 \uC904\uC774\uAE30", ["accessibility"] = "\uC811\uADFC\uC131", ["on"] = "\uCF1C\uC9D0", ["off"] = "\uAEBC\uC9D0",
                ["not_enough_credits"] = "\uD06C\uB808\uB527 \uBD80\uC871", ["chapter"] = "\uCC55\uD130", ["boss"] = "\uBCF4\uC2A4",
                ["boss_time_up"] = "\uC2DC\uAC04 \uCD08\uACFC! \uC774\uC804 \uC2A4\uD14C\uC774\uC9C0\uB97C \uC790\uB3D9 \uCC44\uAD74\uD569\uB2C8\uB2E4",
                ["boss_ready"] = "\uBCF4\uC2A4 \uC900\uBE44", ["challenge"] = "\uB3C4\uC804",
                ["offline_summary"] = "{0} \u00B7 \uAD11\uC11D {1}\n+{2} C \u00B7 +{3} XP",
                ["offline_duration_hm"] = "{0}\uC2DC\uAC04 {1}\uBD84", ["offline_duration_m"] = "{0}\uBD84", ["offline_duration_s"] = "{0}\uCD08",
                ["chapter_clear"] = "\uCC55\uD130 {0} \uD074\uB9AC\uC5B4", ["first_clear_reward"] = "\uCD5C\uCD08 \uD074\uB9AC\uC5B4 \uBCF4\uC0C1", ["continue"] = "\uACC4\uC18D",
                ["chapter_select"] = "\uCC55\uD130 \uC120\uD0DD", ["current"] = "\uD604\uC7AC", ["cleared"] = "\uD074\uB9AC\uC5B4", ["locked"] = "\uC7A0\uAE40",
                ["retry"] = "\uC7AC\uB3C4\uC804", ["enter"] = "\uC785\uC7A5", ["resume"] = "\uC774\uC5B4\uD558\uAE30", ["stage_range"] = "\uC2A4\uD14C\uC774\uC9C0 {0}-{1}",
                ["miner_rank_short"] = "\uAD11\uBD80 Lv. {0}", ["miner_level_up"] = "\uAD11\uBD80 Lv. {0}!", ["miner_rank_summary"] = "\uAD11\uBD80 Lv. {0} \u00B7 \uCD1D \uCC44\uAD74\uB825 +{1:0.#}%",
                ["next_unlock"] = "\uB2E4\uC74C \uD574\uAE08: {0} (Lv. {1})", ["all_features_unlocked"] = "\uBAA8\uB4E0 \uAE30\uB2A5 \uD574\uAE08", ["unlocked"] = "\uD574\uAE08!",
                ["feature_equipment"] = "\uC7A5\uBE44", ["feature_museum"] = "\uBC15\uBB3C\uAD00", ["feature_research"] = "\uC5F0\uAD6C",
                ["feature_missions"] = "\uBBF8\uC158", ["feature_shop"] = "\uC0C1\uC810", ["feature_events"] = "\uC774\uBCA4\uD2B8",
                ["blueprint_core"] = "\uC124\uACC4\uB3C4 \uCF54\uC5B4", ["blueprint_cores"] = "\uC124\uACC4\uB3C4 \uCF54\uC5B4 {0}",
                ["research_summary"] = "\uAD11\uBD80 Lv. {0} \u00B7 \uC5F0\uAD6C \uCC44\uAD74\uB825 +{1:0.#}%",
                ["research_level"] = "Lv. {0}/{1}", ["research_power_bonus"] = "\uB808\uBCA8\uB2F9 \uCC44\uAD74\uB825 +{0:0.#}%",
                ["research_cost"] = "{0} \uCF54\uC5B4", ["research_complete"] = "\uC5F0\uAD6C \uC644\uB8CC!",
                ["research_locked"] = "\uC5F0\uAD6C\uB294 \uAD11\uBD80 Lv. 4\uC5D0 \uD574\uAE08\uB429\uB2C8\uB2E4",
                ["research_prerequisite"] = "\uC120\uD589 \uC5F0\uAD6C \uB808\uBCA8\uC774 \uBD80\uC871\uD569\uB2C8\uB2E4",
                ["research_prerequisite_short"] = "\uC120\uD589 \uD544\uC694", ["research_max_level"] = "\uCD5C\uB300",
                ["not_enough_cores"] = "\uC124\uACC4\uB3C4 \uCF54\uC5B4 \uBD80\uC871", ["research_unavailable"] = "\uC5F0\uAD6C\uB97C \uC774\uC6A9\uD560 \uC218 \uC5C6\uC2B5\uB2C8\uB2E4",
                ["research_core_output"] = "\uCF54\uC5B4 \uCD9C\uB825", ["research_precision_tools"] = "\uC815\uBC00 \uACF5\uAD6C",
                ["research_deep_automation"] = "\uC2EC\uBD80 \uC790\uB3D9\uD654",
                ["stage"] = "\uC2A4\uD14C\uC774\uC9C0", ["power"] = "\uC804\uD22C\uB825", ["recommended"] = "\uAD8C\uC7A5",
                ["ore_health"] = "\uAD11\uC11D \uB0B4\uAD6C\uB3C4", ["boss_in_stages"] = "\uBCF4\uC2A4\uAE4C\uC9C0 {0} \uC2A4\uD14C\uC774\uC9C0",
                ["offline_rewards"] = "\uBC29\uCE58 \uBCF4\uC0C1", ["home"] = "\uD648", ["coming_soon"] = "\uC900\uBE44 \uC911",
                ["mine_crystal_cavern"] = "\uC218\uC815 \uB3D9\uAD74", ["mine_magma_depths"] = "\uB9C8\uADF8\uB9C8 \uC2EC\uCE35",
                ["mine_ancient_city"] = "\uACE0\uB300 \uB3C4\uC2DC"
            },
            [SupportedLanguage.English] = new()
            {
                ["credits"] = "Credits", ["depth"] = "Depth", ["ore"] = "Ore", ["mine"] = "Mine", ["pickaxe"] = "Pickaxe",
                ["drill"] = "Drill", ["robot"] = "Robot", ["tap"] = "Tap", ["auto"] = "Auto", ["reward"] = "Reward", ["rare"] = "Rare",
                ["free_reward"] = "Free reward", ["ad_loading"] = "Loading ad", ["ad_showing"] = "Playing ad", ["ad_retry"] = "Retry ad",
                ["ad_unavailable"] = "Ad unavailable", ["ad_rewarded"] = "Ad reward", ["settings"] = "Settings", ["language"] = "Language",
                ["close"] = "Close", ["audio"] = "Audio", ["music"] = "Music", ["sound"] = "Sound", ["mute"] = "Mute",
                ["haptics"] = "Vibration", ["reduce_motion"] = "Reduce motion", ["accessibility"] = "Accessibility", ["on"] = "On", ["off"] = "Off",
                ["not_enough_credits"] = "Not enough credits", ["chapter"] = "Chapter", ["boss"] = "Boss",
                ["boss_time_up"] = "Time up! Auto-mining the previous stage", ["boss_ready"] = "Boss Ready", ["challenge"] = "Challenge",
                ["offline_summary"] = "{0} \u00B7 {1} ore\n+{2} C \u00B7 +{3} XP",
                ["offline_duration_hm"] = "{0}h {1}m", ["offline_duration_m"] = "{0}m", ["offline_duration_s"] = "{0}s",
                ["chapter_clear"] = "Chapter {0} Clear",
                ["first_clear_reward"] = "First clear reward", ["continue"] = "Continue",
                ["chapter_select"] = "Select Chapter", ["current"] = "Current", ["cleared"] = "Cleared", ["locked"] = "Locked",
                ["retry"] = "Retry", ["enter"] = "Enter", ["resume"] = "Resume", ["stage_range"] = "Stage {0}-{1}",
                ["miner_rank_short"] = "MINER Lv. {0}", ["miner_level_up"] = "MINER Lv. {0}!", ["miner_rank_summary"] = "Miner Lv. {0} \u00B7 Total power +{1:0.#}%",
                ["next_unlock"] = "Next: {0} (Lv. {1})", ["all_features_unlocked"] = "All features unlocked", ["unlocked"] = "Unlocked!",
                ["feature_equipment"] = "Equipment", ["feature_museum"] = "Museum", ["feature_research"] = "Research",
                ["feature_missions"] = "Missions", ["feature_shop"] = "Shop", ["feature_events"] = "Events",
                ["blueprint_core"] = "Blueprint Core", ["blueprint_cores"] = "Blueprint Cores {0}",
                ["research_summary"] = "Miner Lv. {0} \u00B7 Research power +{1:0.#}%",
                ["research_level"] = "Lv. {0}/{1}", ["research_power_bonus"] = "Power +{0:0.#}% per level",
                ["research_cost"] = "{0} Cores", ["research_complete"] = "Research complete!",
                ["research_locked"] = "Research unlocks at Miner Lv. 4",
                ["research_prerequisite"] = "Prerequisite research level required",
                ["research_prerequisite_short"] = "Requires prior", ["research_max_level"] = "Max",
                ["not_enough_cores"] = "Not enough Blueprint Cores", ["research_unavailable"] = "Research unavailable",
                ["research_core_output"] = "Core Output", ["research_precision_tools"] = "Precision Tools",
                ["research_deep_automation"] = "Deep Automation",
                ["stage"] = "Stage", ["power"] = "Power", ["recommended"] = "Recommended",
                ["ore_health"] = "Ore Health", ["boss_in_stages"] = "Boss in {0} stages",
                ["offline_rewards"] = "Offline Rewards", ["home"] = "Home", ["coming_soon"] = "Coming Soon",
                ["mine_crystal_cavern"] = "Crystal Cavern", ["mine_magma_depths"] = "Magma Depths",
                ["mine_ancient_city"] = "Ancient City"
            },
            [SupportedLanguage.Japanese] = new()
            {
                ["credits"] = "\u30AF\u30EC\u30B8\u30C3\u30C8", ["depth"] = "\u6DF1\u5EA6", ["ore"] = "\u9271\u77F3", ["mine"] = "\u63A1\u6398",
                ["pickaxe"] = "\u3064\u308B\u306F\u3057", ["drill"] = "\u30C9\u30EA\u30EB", ["robot"] = "\u30ED\u30DC\u30C3\u30C8", ["tap"] = "\u30BF\u30C3\u30D7",
                ["auto"] = "\u81EA\u52D5", ["reward"] = "\u5831\u916C", ["rare"] = "\u30EC\u30A2", ["free_reward"] = "\u7121\u6599\u5831\u916C",
                ["ad_loading"] = "\u5E83\u544A\u3092\u8AAD\u307F\u8FBC\u307F\u4E2D", ["ad_showing"] = "\u5E83\u544A\u3092\u518D\u751F\u4E2D", ["ad_retry"] = "\u5E83\u544A\u3092\u518D\u8A66\u884C",
                ["ad_unavailable"] = "\u5E83\u544A\u3092\u5229\u7528\u3067\u304D\u307E\u305B\u3093", ["ad_rewarded"] = "\u5E83\u544A\u5831\u916C",
                ["settings"] = "\u8A2D\u5B9A", ["language"] = "\u8A00\u8A9E", ["close"] = "\u9589\u3058\u308B", ["audio"] = "\u30AA\u30FC\u30C7\u30A3\u30AA",
                ["music"] = "BGM", ["sound"] = "\u52B9\u679C\u97F3", ["mute"] = "\u30DF\u30E5\u30FC\u30C8", ["haptics"] = "\u632F\u52D5",
                ["reduce_motion"] = "\u30E2\u30FC\u30B7\u30E7\u30F3\u8EFD\u6E1B", ["accessibility"] = "\u30A2\u30AF\u30BB\u30B7\u30D3\u30EA\u30C6\u30A3",
                ["on"] = "\u30AA\u30F3", ["off"] = "\u30AA\u30D5", ["not_enough_credits"] = "\u30AF\u30EC\u30B8\u30C3\u30C8\u4E0D\u8DB3",
                ["chapter"] = "\u30C1\u30E3\u30D7\u30BF\u30FC", ["boss"] = "\u30DC\u30B9",
                ["boss_time_up"] = "\u30BF\u30A4\u30E0\u30A2\u30C3\u30D7! \u524D\u306E\u30B9\u30C6\u30FC\u30B8\u3092\u81EA\u52D5\u63A1\u6398\u4E2D",
                ["boss_ready"] = "\u30DC\u30B9\u6E96\u5099", ["challenge"] = "\u6311\u6226",
                ["offline_summary"] = "{0}\u30FB\u9271\u77F3 {1}\n+{2} C\u30FB+{3} XP",
                ["offline_duration_hm"] = "{0}\u6642\u9593 {1}\u5206", ["offline_duration_m"] = "{0}\u5206", ["offline_duration_s"] = "{0}\u79D2",
                ["chapter_clear"] = "\u30C1\u30E3\u30D7\u30BF\u30FC {0} \u30AF\u30EA\u30A2", ["first_clear_reward"] = "\u521D\u56DE\u30AF\u30EA\u30A2\u5831\u916C", ["continue"] = "\u7D9A\u3051\u308B",
                ["chapter_select"] = "\u30C1\u30E3\u30D7\u30BF\u30FC\u9078\u629E", ["current"] = "\u73FE\u5728", ["cleared"] = "\u30AF\u30EA\u30A2", ["locked"] = "\u30ED\u30C3\u30AF",
                ["retry"] = "\u518D\u6311\u6226", ["enter"] = "\u5165\u5834", ["resume"] = "\u7D9A\u304D\u304B\u3089", ["stage_range"] = "\u30B9\u30C6\u30FC\u30B8 {0}-{1}",
                ["miner_rank_short"] = "\u9271\u592B Lv. {0}", ["miner_level_up"] = "\u9271\u592B Lv. {0}!", ["miner_rank_summary"] = "\u9271\u592B Lv. {0}\u30FB\u7DCF\u63A1\u6398\u529B +{1:0.#}%",
                ["next_unlock"] = "\u6B21\u306E\u89E3\u653E: {0} (Lv. {1})", ["all_features_unlocked"] = "\u3059\u3079\u3066\u306E\u6A5F\u80FD\u3092\u89E3\u653E\u6E08\u307F", ["unlocked"] = "\u89E3\u653E!",
                ["feature_equipment"] = "\u88C5\u5099", ["feature_museum"] = "\u535A\u7269\u9928", ["feature_research"] = "\u7814\u7A76",
                ["feature_missions"] = "\u30DF\u30C3\u30B7\u30E7\u30F3", ["feature_shop"] = "\u30B7\u30E7\u30C3\u30D7", ["feature_events"] = "\u30A4\u30D9\u30F3\u30C8",
                ["blueprint_core"] = "\u8A2D\u8A08\u30B3\u30A2", ["blueprint_cores"] = "\u8A2D\u8A08\u30B3\u30A2 {0}",
                ["research_summary"] = "\u9271\u592B Lv. {0}\u30FB\u7814\u7A76\u63A1\u6398\u529B +{1:0.#}%",
                ["research_level"] = "Lv. {0}/{1}", ["research_power_bonus"] = "Lv.\u3054\u3068\u306B\u63A1\u6398\u529B +{0:0.#}%",
                ["research_cost"] = "{0} \u30B3\u30A2", ["research_complete"] = "\u7814\u7A76\u5B8C\u4E86!",
                ["research_locked"] = "\u7814\u7A76\u306F\u9271\u592B Lv. 4 \u3067\u89E3\u653E",
                ["research_prerequisite"] = "\u5148\u884C\u7814\u7A76\u30EC\u30D9\u30EB\u304C\u5FC5\u8981\u3067\u3059",
                ["research_prerequisite_short"] = "\u5148\u884C\u5FC5\u8981", ["research_max_level"] = "\u6700\u5927",
                ["not_enough_cores"] = "\u8A2D\u8A08\u30B3\u30A2\u304C\u8DB3\u308A\u307E\u305B\u3093", ["research_unavailable"] = "\u7814\u7A76\u3092\u5229\u7528\u3067\u304D\u307E\u305B\u3093",
                ["research_core_output"] = "\u30B3\u30A2\u51FA\u529B", ["research_precision_tools"] = "\u7CBE\u5BC6\u5DE5\u5177",
                ["research_deep_automation"] = "\u6DF1\u5C64\u81EA\u52D5\u5316",
                ["stage"] = "\u30B9\u30C6\u30FC\u30B8", ["power"] = "\u6226\u529B", ["recommended"] = "\u63A8\u5968",
                ["ore_health"] = "\u9271\u77F3\u8010\u4E45\u5EA6", ["boss_in_stages"] = "\u30DC\u30B9\u307E\u3067 {0} \u30B9\u30C6\u30FC\u30B8",
                ["offline_rewards"] = "\u653E\u7F6E\u5831\u916C", ["home"] = "\u30DB\u30FC\u30E0", ["coming_soon"] = "\u6E96\u5099\u4E2D",
                ["mine_crystal_cavern"] = "\u6C34\u6676\u306E\u6D1E\u7A9F", ["mine_magma_depths"] = "\u30DE\u30B0\u30DE\u6DF1\u5C64",
                ["mine_ancient_city"] = "\u53E4\u4EE3\u90FD\u5E02"
            },
            [SupportedLanguage.ChineseSimplified] = new()
            {
                ["credits"] = "\u91D1\u5E01", ["depth"] = "\u6DF1\u5EA6", ["ore"] = "\u77FF\u77F3", ["mine"] = "\u91C7\u77FF", ["pickaxe"] = "\u9550",
                ["drill"] = "\u94BB\u673A", ["robot"] = "\u673A\u5668\u4EBA", ["tap"] = "\u70B9\u51FB", ["auto"] = "\u81EA\u52A8", ["reward"] = "\u5956\u52B1",
                ["rare"] = "\u7A00\u6709", ["free_reward"] = "\u514D\u8D39\u5956\u52B1", ["ad_loading"] = "\u6B63\u5728\u52A0\u8F7D\u5E7F\u544A",
                ["ad_showing"] = "\u6B63\u5728\u64AD\u653E\u5E7F\u544A", ["ad_retry"] = "\u91CD\u8BD5\u5E7F\u544A", ["ad_unavailable"] = "\u5E7F\u544A\u6682\u4E0D\u53EF\u7528",
                ["ad_rewarded"] = "\u5E7F\u544A\u5956\u52B1", ["settings"] = "\u8BBE\u7F6E", ["language"] = "\u8BED\u8A00", ["close"] = "\u5173\u95ED",
                ["audio"] = "\u97F3\u9891", ["music"] = "\u97F3\u4E50", ["sound"] = "\u97F3\u6548", ["mute"] = "\u9759\u97F3", ["haptics"] = "\u632F\u52A8",
                ["reduce_motion"] = "\u51CF\u5C11\u52A8\u6001\u6548\u679C", ["accessibility"] = "\u8F85\u52A9\u529F\u80FD", ["on"] = "\u5F00", ["off"] = "\u5173",
                ["not_enough_credits"] = "\u91D1\u5E01\u4E0D\u8DB3", ["chapter"] = "\u7AE0\u8282", ["boss"] = "\u9996\u9886",
                ["boss_time_up"] = "\u65F6\u95F4\u5230! \u6B63\u5728\u81EA\u52A8\u5F00\u91C7\u4E0A\u4E00\u5173",
                ["boss_ready"] = "\u9996\u9886\u5DF2\u5C31\u7EEA", ["challenge"] = "\u6311\u6218",
                ["offline_summary"] = "{0} \u00B7 \u77FF\u77F3 {1}\n+{2} C \u00B7 +{3} XP",
                ["offline_duration_hm"] = "{0}\u5C0F\u65F6 {1}\u5206", ["offline_duration_m"] = "{0}\u5206\u949F", ["offline_duration_s"] = "{0}\u79D2",
                ["chapter_clear"] = "\u7B2C {0} \u7AE0\u5B8C\u6210", ["first_clear_reward"] = "\u9996\u6B21\u901A\u5173\u5956\u52B1", ["continue"] = "\u7EE7\u7EED",
                ["chapter_select"] = "\u9009\u62E9\u7AE0\u8282", ["current"] = "\u5F53\u524D", ["cleared"] = "\u5DF2\u901A\u5173", ["locked"] = "\u672A\u89E3\u9501",
                ["retry"] = "\u91CD\u65B0\u6311\u6218", ["enter"] = "\u8FDB\u5165", ["resume"] = "\u7EE7\u7EED", ["stage_range"] = "\u5173\u5361 {0}-{1}",
                ["miner_rank_short"] = "\u77FF\u5DE5 Lv. {0}", ["miner_level_up"] = "\u77FF\u5DE5 Lv. {0}!", ["miner_rank_summary"] = "\u77FF\u5DE5 Lv. {0} \u00B7 \u603B\u91C7\u77FF\u529B +{1:0.#}%",
                ["next_unlock"] = "\u4E0B\u4E00\u4E2A\u89E3\u9501: {0} (Lv. {1})", ["all_features_unlocked"] = "\u6240\u6709\u529F\u80FD\u5DF2\u89E3\u9501", ["unlocked"] = "\u5DF2\u89E3\u9501!",
                ["feature_equipment"] = "\u88C5\u5907", ["feature_museum"] = "\u535A\u7269\u9986", ["feature_research"] = "\u7814\u7A76",
                ["feature_missions"] = "\u4EFB\u52A1", ["feature_shop"] = "\u5546\u5E97", ["feature_events"] = "\u6D3B\u52A8",
                ["blueprint_core"] = "\u84DD\u56FE\u6838\u5FC3", ["blueprint_cores"] = "\u84DD\u56FE\u6838\u5FC3 {0}",
                ["research_summary"] = "\u77FF\u5DE5 Lv. {0} \u00B7 \u7814\u7A76\u91C7\u77FF\u529B +{1:0.#}%",
                ["research_level"] = "Lv. {0}/{1}", ["research_power_bonus"] = "\u6BCF\u7EA7\u91C7\u77FF\u529B +{0:0.#}%",
                ["research_cost"] = "{0} \u6838\u5FC3", ["research_complete"] = "\u7814\u7A76\u5B8C\u6210!",
                ["research_locked"] = "\u7814\u7A76\u5728\u77FF\u5DE5 Lv. 4 \u89E3\u9501",
                ["research_prerequisite"] = "\u9700\u8981\u5148\u884C\u7814\u7A76\u7B49\u7EA7",
                ["research_prerequisite_short"] = "\u9700\u5148\u884C", ["research_max_level"] = "\u5DF2\u6EE1\u7EA7",
                ["not_enough_cores"] = "\u84DD\u56FE\u6838\u5FC3\u4E0D\u8DB3", ["research_unavailable"] = "\u7814\u7A76\u4E0D\u53EF\u7528",
                ["research_core_output"] = "\u6838\u5FC3\u8F93\u51FA", ["research_precision_tools"] = "\u7CBE\u5BC6\u5DE5\u5177",
                ["research_deep_automation"] = "\u6DF1\u5C42\u81EA\u52A8\u5316",
                ["stage"] = "\u5173\u5361", ["power"] = "\u6218\u529B", ["recommended"] = "\u63A8\u8350",
                ["ore_health"] = "\u77FF\u77F3\u8010\u4E45\u5EA6", ["boss_in_stages"] = "\u8DDD\u9996\u9886\u8FD8\u6709 {0} \u5173",
                ["offline_rewards"] = "\u79BB\u7EBF\u5956\u52B1", ["home"] = "\u4E3B\u9875", ["coming_soon"] = "\u656C\u8BF7\u671F\u5F85",
                ["mine_crystal_cavern"] = "\u6C34\u6676\u6D1E\u7A9F", ["mine_magma_depths"] = "\u7194\u5CA9\u6DF1\u5904",
                ["mine_ancient_city"] = "\u53E4\u4EE3\u57CE\u5E02"
            }
        };

        public static event Action Changed;
        public static SupportedLanguage Current { get; private set; }

        public static void Initialize()
        {
            Current = PlayerPrefs.HasKey(LanguageKey)
                ? (SupportedLanguage)PlayerPrefs.GetInt(LanguageKey)
                : FromSystemLanguage(Application.systemLanguage);
        }

        public static void SetLanguage(SupportedLanguage language)
        {
            Current = language;
            PlayerPrefs.SetInt(LanguageKey, (int)language);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static string Get(string key)
        {
            if (Tables[Current].TryGetValue(key, out var value))
            {
                return value;
            }

            return GetCommerceText(key) ?? GetEquipmentText(key) ?? key;
        }

        private static string GetEquipmentText(string key)
        {
            return (Current, key) switch
            {
                (SupportedLanguage.Korean, "equipment_locked") => "장비는 광부 Lv. 2에 해금됩니다",
                (SupportedLanguage.Korean, "equipment_power_summary") => "장비 채굴력 +{0:0.#}%",
                (SupportedLanguage.Korean, "equipment_empty") => "비어 있음",
                (SupportedLanguage.Korean, "equipment_equipped") => "장착 중",
                (SupportedLanguage.Korean, "equipment_equip") => "장착",
                (SupportedLanguage.Korean, "equipment_unequip") => "해제",
                (SupportedLanguage.Korean, "equipment_fuse") => "3개 합성",
                (SupportedLanguage.Korean, "equipment_auto_equip") => "자동 장착",
                (SupportedLanguage.Korean, "equipment_no_items") => "보스를 처치해 장비를 획득하세요",
                (SupportedLanguage.Korean, "equipment_compare") => "{0} · 현재 대비 {1:+0.#;-0.#;0}%",
                (SupportedLanguage.Korean, "equipment_slot_pickaxe") => "곡괭이",
                (SupportedLanguage.Korean, "equipment_slot_drill") => "드릴",
                (SupportedLanguage.Korean, "equipment_slot_robot") => "로봇",
                (SupportedLanguage.Korean, "equipment_slot_charm") => "부적",
                (SupportedLanguage.Korean, "equipment_rarity_common") => "일반",
                (SupportedLanguage.Korean, "equipment_rarity_rare") => "희귀",
                (SupportedLanguage.Korean, "equipment_rarity_epic") => "영웅",
                (SupportedLanguage.Korean, "equipment_rarity_legendary") => "전설",
                (SupportedLanguage.Korean, "equipment_rugged_pickaxe") => "튼튼한 곡괭이",
                (SupportedLanguage.Korean, "equipment_core_drill") => "코어 드릴",
                (SupportedLanguage.Korean, "equipment_forge_bot") => "포지 봇",
                (SupportedLanguage.Korean, "equipment_lucky_crystal") => "행운 수정",
                (SupportedLanguage.Korean, "equipment_equipped_feedback") => "장비 장착!",
                (SupportedLanguage.Korean, "equipment_unequipped_feedback") => "장비 해제",
                (SupportedLanguage.Korean, "equipment_fused_feedback") => "장비 합성 완료!",
                (SupportedLanguage.Korean, "equipment_auto_equipped_feedback") => "최고 장비 자동 장착!",
                (SupportedLanguage.Korean, "equipment_need_three") => "장착하지 않은 같은 장비 3개가 필요합니다",
                (SupportedLanguage.Korean, "equipment_max_rarity") => "이미 최고 등급입니다",
                (SupportedLanguage.Korean, "equipment_already_equipped") => "이미 장착 중입니다",
                (SupportedLanguage.Korean, "equipment_not_equipped") => "해제할 장비가 없습니다",
                (SupportedLanguage.Korean, "equipment_unavailable") => "장비를 사용할 수 없습니다",
                (SupportedLanguage.Korean, "equipment_drop") => "보스 장비",

                (SupportedLanguage.Japanese, "equipment_locked") => "装備は鉱夫 Lv. 2 で解放されます",
                (SupportedLanguage.Japanese, "equipment_power_summary") => "装備採掘力 +{0:0.#}%",
                (SupportedLanguage.Japanese, "equipment_empty") => "空き",
                (SupportedLanguage.Japanese, "equipment_equipped") => "装備中",
                (SupportedLanguage.Japanese, "equipment_equip") => "装備",
                (SupportedLanguage.Japanese, "equipment_unequip") => "外す",
                (SupportedLanguage.Japanese, "equipment_fuse") => "3個合成",
                (SupportedLanguage.Japanese, "equipment_auto_equip") => "自動装備",
                (SupportedLanguage.Japanese, "equipment_no_items") => "ボスを倒して装備を獲得しよう",
                (SupportedLanguage.Japanese, "equipment_compare") => "{0}・現在比 {1:+0.#;-0.#;0}%",
                (SupportedLanguage.Japanese, "equipment_slot_pickaxe") => "つるはし",
                (SupportedLanguage.Japanese, "equipment_slot_drill") => "ドリル",
                (SupportedLanguage.Japanese, "equipment_slot_robot") => "ロボット",
                (SupportedLanguage.Japanese, "equipment_slot_charm") => "チャーム",
                (SupportedLanguage.Japanese, "equipment_rarity_common") => "コモン",
                (SupportedLanguage.Japanese, "equipment_rarity_rare") => "レア",
                (SupportedLanguage.Japanese, "equipment_rarity_epic") => "エピック",
                (SupportedLanguage.Japanese, "equipment_rarity_legendary") => "伝説",
                (SupportedLanguage.Japanese, "equipment_rugged_pickaxe") => "頑丈なつるはし",
                (SupportedLanguage.Japanese, "equipment_core_drill") => "コアドリル",
                (SupportedLanguage.Japanese, "equipment_forge_bot") => "フォージボット",
                (SupportedLanguage.Japanese, "equipment_lucky_crystal") => "幸運の結晶",
                (SupportedLanguage.Japanese, "equipment_equipped_feedback") => "装備しました!",
                (SupportedLanguage.Japanese, "equipment_unequipped_feedback") => "装備を外しました",
                (SupportedLanguage.Japanese, "equipment_fused_feedback") => "装備合成完了!",
                (SupportedLanguage.Japanese, "equipment_auto_equipped_feedback") => "最強装備を自動装備!",
                (SupportedLanguage.Japanese, "equipment_need_three") => "未装備の同じ装備が3個必要です",
                (SupportedLanguage.Japanese, "equipment_max_rarity") => "すでに最高レア度です",
                (SupportedLanguage.Japanese, "equipment_already_equipped") => "すでに装備中です",
                (SupportedLanguage.Japanese, "equipment_not_equipped") => "外す装備がありません",
                (SupportedLanguage.Japanese, "equipment_unavailable") => "装備を利用できません",
                (SupportedLanguage.Japanese, "equipment_drop") => "ボス装備",

                (SupportedLanguage.ChineseSimplified, "equipment_locked") => "装备将在矿工 Lv. 2 解锁",
                (SupportedLanguage.ChineseSimplified, "equipment_power_summary") => "装备采矿力 +{0:0.#}%",
                (SupportedLanguage.ChineseSimplified, "equipment_empty") => "空",
                (SupportedLanguage.ChineseSimplified, "equipment_equipped") => "已装备",
                (SupportedLanguage.ChineseSimplified, "equipment_equip") => "装备",
                (SupportedLanguage.ChineseSimplified, "equipment_unequip") => "卸下",
                (SupportedLanguage.ChineseSimplified, "equipment_fuse") => "3件合成",
                (SupportedLanguage.ChineseSimplified, "equipment_auto_equip") => "自动装备",
                (SupportedLanguage.ChineseSimplified, "equipment_no_items") => "击败首领获得装备",
                (SupportedLanguage.ChineseSimplified, "equipment_compare") => "{0} · 对比当前 {1:+0.#;-0.#;0}%",
                (SupportedLanguage.ChineseSimplified, "equipment_slot_pickaxe") => "镐",
                (SupportedLanguage.ChineseSimplified, "equipment_slot_drill") => "钻机",
                (SupportedLanguage.ChineseSimplified, "equipment_slot_robot") => "机器人",
                (SupportedLanguage.ChineseSimplified, "equipment_slot_charm") => "护符",
                (SupportedLanguage.ChineseSimplified, "equipment_rarity_common") => "普通",
                (SupportedLanguage.ChineseSimplified, "equipment_rarity_rare") => "稀有",
                (SupportedLanguage.ChineseSimplified, "equipment_rarity_epic") => "史诗",
                (SupportedLanguage.ChineseSimplified, "equipment_rarity_legendary") => "传说",
                (SupportedLanguage.ChineseSimplified, "equipment_rugged_pickaxe") => "坚固镐",
                (SupportedLanguage.ChineseSimplified, "equipment_core_drill") => "核心钻机",
                (SupportedLanguage.ChineseSimplified, "equipment_forge_bot") => "锻造机器人",
                (SupportedLanguage.ChineseSimplified, "equipment_lucky_crystal") => "幸运水晶",
                (SupportedLanguage.ChineseSimplified, "equipment_equipped_feedback") => "装备成功!",
                (SupportedLanguage.ChineseSimplified, "equipment_unequipped_feedback") => "已卸下装备",
                (SupportedLanguage.ChineseSimplified, "equipment_fused_feedback") => "装备合成完成!",
                (SupportedLanguage.ChineseSimplified, "equipment_auto_equipped_feedback") => "已自动装备最佳物品!",
                (SupportedLanguage.ChineseSimplified, "equipment_need_three") => "需要3件未装备的同款装备",
                (SupportedLanguage.ChineseSimplified, "equipment_max_rarity") => "已达到最高品质",
                (SupportedLanguage.ChineseSimplified, "equipment_already_equipped") => "已经装备",
                (SupportedLanguage.ChineseSimplified, "equipment_not_equipped") => "没有可卸下的装备",
                (SupportedLanguage.ChineseSimplified, "equipment_unavailable") => "装备不可用",
                (SupportedLanguage.ChineseSimplified, "equipment_drop") => "首领装备",

                (_, "equipment_locked") => "Equipment unlocks at Miner Lv. 2",
                (_, "equipment_power_summary") => "Equipment power +{0:0.#}%",
                (_, "equipment_empty") => "Empty",
                (_, "equipment_equipped") => "Equipped",
                (_, "equipment_equip") => "Equip",
                (_, "equipment_unequip") => "Unequip",
                (_, "equipment_fuse") => "Fuse 3",
                (_, "equipment_auto_equip") => "Auto Equip",
                (_, "equipment_no_items") => "Defeat a boss to earn equipment",
                (_, "equipment_compare") => "{0} · vs current {1:+0.#;-0.#;0}%",
                (_, "equipment_slot_pickaxe") => "Pickaxe",
                (_, "equipment_slot_drill") => "Drill",
                (_, "equipment_slot_robot") => "Robot",
                (_, "equipment_slot_charm") => "Charm",
                (_, "equipment_rarity_common") => "Common",
                (_, "equipment_rarity_rare") => "Rare",
                (_, "equipment_rarity_epic") => "Epic",
                (_, "equipment_rarity_legendary") => "Legendary",
                (_, "equipment_rugged_pickaxe") => "Rugged Pickaxe",
                (_, "equipment_core_drill") => "Core Drill",
                (_, "equipment_forge_bot") => "Forge Bot",
                (_, "equipment_lucky_crystal") => "Lucky Crystal",
                (_, "equipment_equipped_feedback") => "Equipment equipped!",
                (_, "equipment_unequipped_feedback") => "Equipment unequipped",
                (_, "equipment_fused_feedback") => "Equipment fused!",
                (_, "equipment_auto_equipped_feedback") => "Best equipment auto-equipped!",
                (_, "equipment_need_three") => "Three unequipped matching items are required",
                (_, "equipment_max_rarity") => "Already at maximum rarity",
                (_, "equipment_already_equipped") => "Already equipped",
                (_, "equipment_not_equipped") => "No equipment to unequip",
                (_, "equipment_unavailable") => "Equipment unavailable",
                (_, "equipment_drop") => "Boss equipment",
                _ => null
            };
        }

        private static string GetCommerceText(string key)
        {
            return (Current, key) switch
            {
                (SupportedLanguage.Korean, "remove_ads") => "\uAD11\uACE0 \uC81C\uAC70",
                (SupportedLanguage.Korean, "iap_purchased") => "\uAD6C\uB9E4 \uC644\uB8CC",
                (SupportedLanguage.Korean, "restore_purchases") => "\uAD6C\uB9E4 \uBCF5\uC6D0",
                (SupportedLanguage.Korean, "iap_loading") => "\uC0C1\uD488 \uBD88\uB7EC\uC624\uB294 \uC911",
                (SupportedLanguage.Korean, "iap_purchasing") => "\uAD6C\uB9E4 \uC9C4\uD589 \uC911",
                (SupportedLanguage.Korean, "iap_restoring") => "\uAD6C\uB9E4 \uBCF5\uC6D0 \uC911",
                (SupportedLanguage.Korean, "iap_deferred") => "\uAD6C\uB9E4 \uC2B9\uC778 \uB300\uAE30 \uC911",
                (SupportedLanguage.Korean, "iap_cancelled") => "\uAD6C\uB9E4\uAC00 \uCDE8\uC18C\uB428",
                (SupportedLanguage.Korean, "iap_unavailable") => "\uC0C1\uD488\uC744 \uC774\uC6A9\uD560 \uC218 \uC5C6\uC74C",

                (SupportedLanguage.Japanese, "remove_ads") => "\u5E83\u544A\u3092\u524A\u9664",
                (SupportedLanguage.Japanese, "iap_purchased") => "\u8CFC\u5165\u6E08\u307F",
                (SupportedLanguage.Japanese, "restore_purchases") => "\u8CFC\u5165\u3092\u5FA9\u5143",
                (SupportedLanguage.Japanese, "iap_loading") => "\u5546\u54C1\u3092\u8AAD\u307F\u8FBC\u307F\u4E2D",
                (SupportedLanguage.Japanese, "iap_purchasing") => "\u8CFC\u5165\u51E6\u7406\u4E2D",
                (SupportedLanguage.Japanese, "iap_restoring") => "\u5FA9\u5143\u4E2D",
                (SupportedLanguage.Japanese, "iap_deferred") => "\u8CFC\u5165\u627F\u8A8D\u5F85\u3061",
                (SupportedLanguage.Japanese, "iap_cancelled") => "\u8CFC\u5165\u304C\u30AD\u30E3\u30F3\u30BB\u30EB\u3055\u308C\u307E\u3057\u305F",
                (SupportedLanguage.Japanese, "iap_unavailable") => "\u5546\u54C1\u3092\u5229\u7528\u3067\u304D\u307E\u305B\u3093",

                (SupportedLanguage.ChineseSimplified, "remove_ads") => "\u79FB\u9664\u5E7F\u544A",
                (SupportedLanguage.ChineseSimplified, "iap_purchased") => "\u5DF2\u8D2D\u4E70",
                (SupportedLanguage.ChineseSimplified, "restore_purchases") => "\u6062\u590D\u8D2D\u4E70",
                (SupportedLanguage.ChineseSimplified, "iap_loading") => "\u6B63\u5728\u52A0\u8F7D\u5546\u54C1",
                (SupportedLanguage.ChineseSimplified, "iap_purchasing") => "\u6B63\u5728\u8D2D\u4E70",
                (SupportedLanguage.ChineseSimplified, "iap_restoring") => "\u6B63\u5728\u6062\u590D",
                (SupportedLanguage.ChineseSimplified, "iap_deferred") => "\u7B49\u5F85\u8D2D\u4E70\u6279\u51C6",
                (SupportedLanguage.ChineseSimplified, "iap_cancelled") => "\u8D2D\u4E70\u5DF2\u53D6\u6D88",
                (SupportedLanguage.ChineseSimplified, "iap_unavailable") => "\u5546\u54C1\u6682\u4E0D\u53EF\u7528",

                (_, "remove_ads") => "Remove ads",
                (_, "iap_purchased") => "Purchased",
                (_, "restore_purchases") => "Restore purchases",
                (_, "iap_loading") => "Loading product",
                (_, "iap_purchasing") => "Purchase in progress",
                (_, "iap_restoring") => "Restoring purchases",
                (_, "iap_deferred") => "Awaiting purchase approval",
                (_, "iap_cancelled") => "Purchase cancelled",
                (_, "iap_unavailable") => "Product unavailable",
                _ => null
            };
        }

        private static SupportedLanguage FromSystemLanguage(SystemLanguage language)
        {
            return language switch
            {
                SystemLanguage.Japanese => SupportedLanguage.Japanese,
                SystemLanguage.ChineseSimplified => SupportedLanguage.ChineseSimplified,
                SystemLanguage.English => SupportedLanguage.English,
                _ => SupportedLanguage.Korean
            };
        }
    }
}
