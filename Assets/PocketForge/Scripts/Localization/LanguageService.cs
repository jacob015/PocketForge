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
            [SupportedLanguage.Korean] = new() { ["credits"] = "크레딧", ["depth"] = "심도", ["ore"] = "광석", ["mine"] = "채굴", ["pickaxe"] = "곡괭이", ["drill"] = "드릴", ["robot"] = "로봇", ["tap"] = "탭", ["auto"] = "자동", ["reward"] = "보상" },
            [SupportedLanguage.English] = new() { ["credits"] = "Credits", ["depth"] = "Depth", ["ore"] = "Ore", ["mine"] = "Mine", ["pickaxe"] = "Pickaxe", ["drill"] = "Drill", ["robot"] = "Robot", ["tap"] = "Tap", ["auto"] = "Auto", ["reward"] = "Reward" },
            [SupportedLanguage.Japanese] = new() { ["credits"] = "クレジット", ["depth"] = "深度", ["ore"] = "鉱石", ["mine"] = "採掘", ["pickaxe"] = "つるはし", ["drill"] = "ドリル", ["robot"] = "ロボット", ["tap"] = "タップ", ["auto"] = "自動", ["reward"] = "報酬" },
            [SupportedLanguage.ChineseSimplified] = new() { ["credits"] = "积分", ["depth"] = "深度", ["ore"] = "矿石", ["mine"] = "采矿", ["pickaxe"] = "镐", ["drill"] = "钻机", ["robot"] = "机器人", ["tap"] = "点击", ["auto"] = "自动", ["reward"] = "奖励" }
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

        public static string Get(string key) => Tables[Current].TryGetValue(key, out var value) ? value : key;

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
