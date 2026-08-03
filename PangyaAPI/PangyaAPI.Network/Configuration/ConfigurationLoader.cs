using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using PangyaAPI.SQL.EntityFramework;
using PangyaAPI.SQL;
using PangyaAPI.Utilities.Localization;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Network.Resources;

namespace PangyaAPI.Network.Configuration
{
    public static class ConfigurationLoader
    {
        public static ServerConfiguration LoadAndValidate(IConfiguration configuration)
        {
            LocalizationCulture.Configure(configuration?["Localization:Culture"]);
            var result = configuration.Get<ServerConfiguration>() ?? new ServerConfiguration();
            PangyaEncoding.Configure(result.Options.Encoding);
            result.Database.ConnectionString = configuration.GetConnectionString("Pangya") ?? "";
            var server = result.Server;
            var database = result.Database;

            if (string.IsNullOrWhiteSpace(server.Version) || string.IsNullOrWhiteSpace(server.ClientVersion))
                throw new InvalidOperationException(NetworkMessages.Get("ServerVersionsRequired"));
            if (server.Uid <= 0 || server.Port <= 0 || server.Port > 65535 || server.MaxUsers <= 0)
                throw new InvalidOperationException(NetworkMessages.Get("ServerValuesInvalid"));
            DatabaseConnectionStringFactory.Validate(database);
            DatabaseConfiguration.Configure(database);
            if (result.AuthServer.Port <= 0 || result.AuthServer.Port > 65535 || string.IsNullOrWhiteSpace(result.AuthServer.Host))
                throw new InvalidOperationException(NetworkMessages.Get("AuthEndpointRequired"));
            if (result.AntiDdos.LimitConnectionPerIp <= 0 || result.AntiDdos.IntervalMilliseconds <= 0
                || result.AntiDdos.Count <= 0 || result.AntiDdos.AutoResetMilliseconds <= 0)
                throw new InvalidOperationException(NetworkMessages.Get("AntiDdosLimitsInvalid"));
            if (result.AntiDdos.Order != "deny,allow" && result.AntiDdos.Order != "allow,deny"
                && result.AntiDdos.Order != "mutual-failure")
                throw new InvalidOperationException(NetworkMessages.Get("AntiDdosOrderInvalid"));
            if (string.IsNullOrWhiteSpace(result.Iff.Path))
                throw new InvalidOperationException("IFF:Path is required.");
            if (result.Iff.RequiredTables is null || result.Iff.RequiredTables.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("IFF:RequiredTables cannot contain empty table names.");

            return result;
        }
    }
}
