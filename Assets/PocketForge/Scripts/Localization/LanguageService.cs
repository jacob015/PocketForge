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
