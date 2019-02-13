using System.ComponentModel.DataAnnotations;

namespace RINGS.Common
{
    public enum PCNameStyles
    {
        [Display(Name = "Naoki Yoshida")]
        FullName = 0,

        [Display(Name = "Naoki Y.")]
        FullInitial,

        [Display(Name = "N. Yoshida")]
        InitialFull,

        [Display(Name = "N. Y.")]
        Initial
    }

    public static class PCNameStylesExtensions
    {
        public static string FormatName(
            this PCNameStyles style,
            string name)
        {
            var result = name;

            if (string.IsNullOrEmpty(result))
            {
                return result;
            }

            var server = string.Empty;
            var parts = result.Split('@');
            if (parts.Length >= 2)
            {
                result = parts[0];
                server = parts[1];
            }

            parts = result.Split(' ');
            if (parts.Length < 2)
            {
                return result;
            }

            var part1 = parts[0];
            var part2 = parts[1];

            if (string.IsNullOrEmpty(part1) ||
                string.IsNullOrEmpty(part2))
            {
                return result;
            }

            switch (style)
            {
                case PCNameStyles.FullName:
                    break;

                case PCNameStyles.FullInitial:
                    if (!result.Contains("."))
                    {
                        result = $"{part1} {part2.Substring(0, 1)}.";
                    }
                    break;

                case PCNameStyles.InitialFull:
                    if (!result.Contains("."))
                    {
                        result = $"{part1.Substring(0, 1)}. {part2}.";
                    }
                    break;

                case PCNameStyles.Initial:
                    result = $"{part1.Substring(0, 1)}. {part2.Substring(0, 1)}.";
                    break;
            }

            if (!string.IsNullOrEmpty(server))
            {
                result = $"{result}@{server}";
            }

            return result;
        }
    }
}
