#nullable disable
using System;
using System.Text.RegularExpressions;

namespace PangyaAPI.Utilities.Log
{
    public static class LogRedactor
    {
        private const string Redacted = "[REDACTED]";

        private static readonly Regex KeyValueSecret = new Regex(
            @"(?ix)(?<key>password|passwd|pwd|dbpass|auth[_ -]?key|web[_ -]?key|senha|\bpass|\bkey)\s*(?<separator>[:=]\s*|\[\s*(?:value|old|new|key)\s*=\s*)(?<value>[^,;\]\s]+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex ConnectionPassword = new Regex(
            @"(?ix)(?<key>password|pwd)\s*=\s*[^;]+",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Redact(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var redacted = ConnectionPassword.Replace(value, match => match.Groups["key"].Value + "=" + Redacted);
            return KeyValueSecret.Replace(redacted, match =>
                match.Groups["key"].Value + match.Groups["separator"].Value + Redacted);
        }
    }
}
