using System.Resources;
using PangyaAPI.Utilities.Localization;

namespace PangyaAPI.Network.Resources
{
    public static class NetworkMessages
    {
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("PangyaAPI.Network.Resources.NetworkMessages", typeof(NetworkMessages).Assembly);

        public static string Get(string key) => ResourceAccessor.GetString(ResourceManager, key);

        public static string Format(string key, params object[] arguments) =>
            ResourceAccessor.Format(ResourceManager, key, arguments);
    }
}
