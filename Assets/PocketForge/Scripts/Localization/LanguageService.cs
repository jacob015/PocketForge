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
                ["not_enough_credits"] = "\uD06C\uB808\uB527 \uBD80\uC871"
            },
            [SupportedLanguage.English] = new()
            {
                ["credits"] = "Credits", ["depth"] = "Depth", ["ore"] = "Ore", ["mine"] = "Mine", ["pickaxe"] = "Pickaxe",
                ["drill"] = "Drill", ["robot"] = "Robot", ["tap"] = "Tap", ["auto"] = "Auto", ["reward"] = "Reward", ["rare"] = "Rare",
                ["free_reward"] = "Free reward", ["ad_loading"] = "Loading ad", ["ad_showing"] = "Playing ad", ["ad_retry"] = "Retry ad",
                ["ad_unavailable"] = "Ad unavailable", ["ad_rewarded"] = "Ad reward", ["settings"] = "Settings", ["language"] = "Language",
                ["close"] = "Close", ["audio"] = "Audio", ["music"] = "Music", ["sound"] = "Sound", ["mute"] = "Mute",
                ["haptics"] = "Vibration", ["reduce_motion"] = "Reduce motion", ["accessibility"] = "Accessibility", ["on"] = "On", ["off"] = "Off",
                ["not_enough_credits"] = "Not enough credits"
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
                ["on"] = "\u30AA\u30F3", ["off"] = "\u30AA\u30D5", ["not_enough_credits"] = "\u30AF\u30EC\u30B8\u30C3\u30C8\u4E0D\u8DB3"
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
                ["not_enough_credits"] = "\u91D1\u5E01\u4E0D\u8DB3"
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

            return GetCommerceText(key) ?? key;
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
