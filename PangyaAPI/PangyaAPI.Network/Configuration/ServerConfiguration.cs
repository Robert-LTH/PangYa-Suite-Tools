using System.Collections.Generic;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Configuration
{
    public sealed class ServerConfiguration
    {
        public ServerOptions Server { get; set; } = new ServerOptions();
        public RuntimeOptions Options { get; set; } = new RuntimeOptions();
        public DatabaseOptions Database { get; set; } = new DatabaseOptions();
        public EndpointOptions AuthServer { get; set; } = new EndpointOptions();
        public EndpointOptions GameGuardAuthServer { get; set; } = new EndpointOptions();
        public LoggingOptions Logging { get; set; } = new LoggingOptions();
        public AntiDdosOptions AntiDdos { get; set; } = new AntiDdosOptions();
        public LocalizationOptions Localization { get; set; } = new LocalizationOptions();
        public IffOptions Iff { get; set; } = new IffOptions();
    }
}
