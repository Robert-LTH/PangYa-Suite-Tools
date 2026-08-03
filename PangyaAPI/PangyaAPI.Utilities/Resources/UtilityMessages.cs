#nullable disable
using System.Resources;
using PangyaAPI.Utilities.Localization;

namespace PangyaAPI.Utilities.Resources
{
    public static class UtilityMessages
    {
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("PangyaAPI.Utilities.Resources.UtilityMessages", typeof(UtilityMessages).Assembly);

        public static string Get(string key) => ResourceAccessor.GetString(ResourceManager, key);

        public static string Format(string key, params object[] arguments) =>
            ResourceAccessor.Format(ResourceManager, key, arguments);
    }
}
