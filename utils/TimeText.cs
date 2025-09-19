using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Timer.utils
{
    public static class TimeText
    {
        public static string FormatHms(int total)
        {
            if (total < 0) total = 0;
            int h = total / 3600;
            int m = total % 3600 / 60;
            int s = total % 60;
            return $"{h:00}:{m:00}:{s:00}";
        }

        public static bool TryParseFlexible(string input, out int secs)
        {
            secs = 0;
            string s = NormalizeForParse(input);
            if (s.Length == 0) return false;

            if (s.Contains(':'))
            {
                var parts = s.Split(':');
                if (parts.Length is < 2 or > 3) return false;

                int h = 0, m = 0, sec = 0;
                if (parts.Length == 2)
                {
                    if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out m)) return false;
                    if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out sec)) return false;
                }
                else
                {
                    if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out h)) return false;
                    if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out m)) return false;
                    if (!int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out sec)) return false;
                }

                if (m is < 0 or > 59 || sec is < 0 or > 59) return false;

                long total = (long)h * 3600 + (long)m * 60 + sec;
                secs = total > int.MaxValue ? int.MaxValue : (int)total;
                return true;
            }

            var matches = Regex.Matches(s, @"(?ix)\b(?<v>\d+)\s*(?<u>h|m|s|시|분|초)\b");
            if (matches.Count > 0)
            {
                long total = 0;
                foreach (Match x in matches)
                {
                    long v = long.Parse(x.Groups["v"].Value, CultureInfo.InvariantCulture);
                    switch (x.Groups["u"].Value)
                    {
                        case "h":
                        case "시": total += v * 3600; break;
                        case "m":
                        case "분": total += v * 60; break;
                        default: total += v; break;
                    }
                    if (total > int.MaxValue) { secs = int.MaxValue; return true; }
                }
                secs = (int)total;
                return true;
            }

            long plain = 0;
            foreach (char c in s)
            {
                if (!char.IsDigit(c)) return false;
                plain = plain * 10 + (c - '0');
                if (plain > int.MaxValue) { secs = int.MaxValue; return true; }
            }
            secs = (int)plain;
            return true;
        }

        static string NormalizeForParse(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var sb = new StringBuilder(input.Length);

            foreach (var ch in input)
            {
                if (char.IsWhiteSpace(ch)) continue;

                int d = CharUnicodeInfo.GetDecimalDigitValue(ch);
                if (d >= 0) { sb.Append((char)('0' + d)); continue; }

                if (ch == ':' || ch == '\u2236' || ch == '\uFF1A') { sb.Append(':'); continue; }

                char lower = char.ToLowerInvariant(ch);
                if (lower is 'h' or 'm' or 's') { sb.Append(lower); continue; }

                if (ch is '시' or '분' or '초') { sb.Append(ch); continue; }
            }
            return sb.ToString();
        }
    }
}