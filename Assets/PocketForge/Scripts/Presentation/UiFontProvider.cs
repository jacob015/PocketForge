using UnityEngine;

namespace PocketForge.Presentation
{
    public static class UiFontProvider
    {
        private static readonly string[] PreferredFonts =
        {
            "Noto Sans CJK KR",
            "Noto Sans KR",
            "Noto Sans CJK JP",
            "Noto Sans CJK SC",
            "Malgun Gothic",
            "Yu Gothic UI",
            "Microsoft YaHei UI",
            "sans-serif",
            "Arial"
        };

        private static readonly string[] CasualPreferredFonts =
        {
            "sans-serif-black",
            "sans-serif-rounded",
            "Noto Sans CJK KR Black",
            "Noto Sans KR Black",
            "Noto Sans CJK JP Black",
            "Noto Sans CJK SC Black",
            "Malgun Gothic",
            "Yu Gothic UI",
            "Microsoft YaHei UI",
            "sans-serif",
            "Arial"
        };

        private static Font font;
        private static Font casualFont;

        public static Font Get()
        {
            if (font != null)
            {
                return font;
            }

            font = Font.CreateDynamicFontFromOSFont(PreferredFonts, 48);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return font;
        }

        public static Font GetCasual()
        {
            if (casualFont != null)
            {
                return casualFont;
            }

            casualFont = Font.CreateDynamicFontFromOSFont(CasualPreferredFonts, 64);
            if (casualFont == null)
            {
                casualFont = Get();
            }

            return casualFont;
        }
    }
}
