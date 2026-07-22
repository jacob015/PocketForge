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

        private static Font font;

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
    }
}
