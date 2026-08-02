using System.Collections.Generic;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Configuration
{

    public sealed class EndpointOptions
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; }
    }
}
