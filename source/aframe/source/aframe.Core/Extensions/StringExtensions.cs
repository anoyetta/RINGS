using System;
using Microsoft.VisualBasic;

namespace aframe
{
    public static class StringExtensions
    {
        private const int JPLocaleID = 0x411;

        public static string ToWide(
            this string s)
            => Strings.StrConv(s, VbStrConv.Wide, JPLocaleID);

        public static string ToNarrow(
            this string s)
            => Strings.StrConv(s, VbStrConv.Narrow, JPLocaleID);

        public static string ToHiragana(
            this string s)
            => Strings.StrConv(s, VbStrConv.Hiragana, JPLocaleID);

        public static string ToKatakana(
            this string s)
            => Strings.StrConv(s, VbStrConv.Katakana, JPLocaleID);

        public static bool ContainsIgnoreCase(
            this string source,
            string value)
            => Contains(source, value, StringComparison.InvariantCultureIgnoreCase);

        public static bool Contains(
            this string source,
            string value,
            StringComparison comprarison)
        {
            return source.IndexOf(value, comprarison) >= 0;
        }
    }
}
