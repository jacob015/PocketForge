using System;
using System.Globalization;

namespace PocketForge.Presentation
{
    public static class CompactNumberFormatter
    {
        private static readonly string[] Suffixes =
        {
            string.Empty,
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qi"
        };

        public static string Format(long value)
        {
            if (value > -1000L && value < 1000L)
            {
                return value.ToString("N0", CultureInfo.InvariantCulture);
            }

            return FormatScaled(value);
        }

        public static string Format(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "0";
            }

            if (Math.Abs(value) < 1000d)
            {
                return value.ToString("0.##", CultureInfo.InvariantCulture);
            }

            return FormatScaled(value);
        }

        private static string FormatScaled(double value)
        {
            var suffixIndex = 0;
            var scaled = value;
            while (Math.Abs(scaled) >= 1000d && suffixIndex < Suffixes.Length - 1)
            {
                scaled /= 1000d;
                suffixIndex++;
            }

            var decimals = GetDecimalPlaces(Math.Abs(scaled));
            var rounded = Math.Round(scaled, decimals, MidpointRounding.AwayFromZero);
            if (Math.Abs(rounded) >= 1000d && suffixIndex < Suffixes.Length - 1)
            {
                rounded /= 1000d;
                suffixIndex++;
                decimals = GetDecimalPlaces(Math.Abs(rounded));
            }

            var format = decimals switch
            {
                2 => "0.##",
                1 => "0.#",
                _ => "0"
            };
            return rounded.ToString(format, CultureInfo.InvariantCulture) + Suffixes[suffixIndex];
        }

        private static int GetDecimalPlaces(double absoluteValue)
        {
            if (absoluteValue < 10d)
            {
                return 2;
            }

            return absoluteValue < 100d ? 1 : 0;
        }
    }
}
