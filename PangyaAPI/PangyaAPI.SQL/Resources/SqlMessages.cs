using System.Resources;
using PangyaAPI.Utilities.Localization;

namespace PangyaAPI.SQL.Resources
{
    public static class SqlMessages
    {
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("PangyaAPI.SQL.Resources.SqlMessages", typeof(SqlMessages).Assembly);

        public static string Get(string key) => ResourceAccessor.GetString(ResourceManager, key);

        public static string Format(string key, params object[] arguments) =>
            ResourceAccessor.Format(ResourceManager, key, arguments);
    }
}
