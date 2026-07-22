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
            [SupportedLanguage.Korean] = new() { ["credits"] = "크레딧", ["depth"] = "심도", ["ore"] = "광석", ["mine"] = "채굴", ["pickaxe"] = "곡괭이", ["drill"] = "드릴", ["robot"] = "로봇", ["tap"] = "탭", ["auto"] = "자동", ["reward"] = "보상", ["free_reward"] = "무료 보상", ["ad_loading"] = "광고 로딩 중", ["ad_showing"] = "광고 재생 중", ["ad_retry"] = "광고 다시 받기", ["ad_unavailable"] = "광고를 준비할 수 없습니다", ["ad_rewarded"] = "광고 보상" },
            [SupportedLanguage.English] = new() { ["credits"] = "Credits", ["depth"] = "Depth", ["ore"] = "Ore", ["mine"] = "Mine", ["pickaxe"] = "Pickaxe", ["drill"] = "Drill", ["robot"] = "Robot", ["tap"] = "Tap", ["auto"] = "Auto", ["reward"] = "Reward", ["free_reward"] = "Free reward", ["ad_loading"] = "Loading ad", ["ad_showing"] = "Playing ad", ["ad_retry"] = "Retry ad", ["ad_unavailable"] = "Ad unavailable", ["ad_rewarded"] = "Ad reward" },
            [SupportedLanguage.Japanese] = new() { ["credits"] = "クレジット", ["depth"] = "深度", ["ore"] = "鉱石", ["mine"] = "採掘", ["pickaxe"] = "つるはし", ["drill"] = "ドリル", ["robot"] = "ロボット", ["tap"] = "タップ", ["auto"] = "自動", ["reward"] = "報酬", ["free_reward"] = "無料報酬", ["ad_loading"] = "広告を読み込み中", ["ad_showing"] = "広告を再生中", ["ad_retry"] = "広告を再試行", ["ad_unavailable"] = "広告を準備できません", ["ad_rewarded"] = "広告報酬" },
            [SupportedLanguage.ChineseSimplified] = new() { ["credits"] = "积分", ["depth"] = "深度", ["ore"] = "矿石", ["mine"] = "采矿", ["pickaxe"] = "镐", ["drill"] = "钻机", ["robot"] = "机器人", ["tap"] = "点击", ["auto"] = "自动", ["reward"] = "奖励", ["free_reward"] = "免费奖励", ["ad_loading"] = "广告加载中", ["ad_showing"] = "广告播放中", ["ad_retry"] = "重试广告", ["ad_unavailable"] = "广告暂不可用", ["ad_rewarded"] = "广告奖励" }
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
            var commerceText = GetCommerceText(key);
            if (commerceText != null)
            {
                return commerceText;
            }

            if (key == "settings")
            {
                return Current switch
                {
                    SupportedLanguage.Korean => "\uC124\uC815",
                    SupportedLanguage.Japanese => "\u8A2D\u5B9A",
                    SupportedLanguage.ChineseSimplified => "\u8BBE\u7F6E",
                    _ => "Settings"
                };
            }

            if (key == "language")
            {
                return Current switch
                {
                    SupportedLanguage.Korean => "\uC5B8\uC5B4",
                    SupportedLanguage.Japanese => "\u8A00\u8A9E",
                    SupportedLanguage.ChineseSimplified => "\u8BED\u8A00",
                    _ => "Language"
                };
            }

            if (key == "close")
            {
                return Current switch
                {
                    SupportedLanguage.Korean => "\uB2EB\uAE30",
                    SupportedLanguage.Japanese => "\u9589\u3058\u308B",
                    SupportedLanguage.ChineseSimplified => "\u5173\u95ED",
                    _ => "Close"
                };
            }

            return Tables[Current].TryGetValue(key, out var value) ? value : key;
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
