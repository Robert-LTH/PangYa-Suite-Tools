#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using PangyaAPI.Utilities.Resources;

namespace PangyaAPI.Utilities.Localization
{
    public static class LocalizationCulture
    {
        public const string DefaultCultureName = "en";

        private static readonly HashSet<string> SupportedCultureNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            DefaultCultureName,
            "pt-BR",
            "sv-SE"
        };

        public static IReadOnlyCollection<string> SupportedCultures => SupportedCultureNames;

        public static CultureInfo Current => CultureInfo.CurrentUICulture;

        public static void Configure(string cultureName)
        {
            var requestedCulture = string.IsNullOrWhiteSpace(cultureName)
                ? DefaultCultureName
                : cultureName.Trim();

            if (!SupportedCultureNames.Contains(requestedCulture))
            {
                throw new ArgumentException(
                    UtilityMessages.Format("UnsupportedCulture", requestedCulture, string.Join(", ", SupportedCultureNames)),
                    nameof(cultureName));
            }

            var culture = CultureInfo.GetCultureInfo(requestedCulture);
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}
