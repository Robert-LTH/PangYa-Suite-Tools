#nullable disable
using System;
using System.Globalization;
using System.Resources;

namespace PangyaAPI.Utilities.Localization
{
    public static class ResourceAccessor
    {
        public static string GetString(ResourceManager resourceManager, string key)
        {
            if (resourceManager == null)
                throw new ArgumentNullException(nameof(resourceManager));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A resource key is required.", nameof(key));

            return resourceManager.GetString(key, CultureInfo.CurrentUICulture)
                ?? throw new MissingManifestResourceException(
                    $"Resource '{key}' was not found for culture '{CultureInfo.CurrentUICulture.Name}'.");
        }

        public static string Format(ResourceManager resourceManager, string key, params object[] arguments)
        {
            return string.Format(
                CultureInfo.CurrentUICulture,
                GetString(resourceManager, key),
                arguments ?? Array.Empty<object>());
        }
    }
}
