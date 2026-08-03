using System.Collections.Generic;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Configuration
{

    public sealed class ServerOptions
    {
        public string Version { get; set; } = "";
        public string ClientVersion { get; set; } = "";
        public uint PacketVersion { get; set; }
        public string Name { get; set; } = "Pangya Server Csharp";
        public int Uid { get; set; }
        public string Address { get; set; } = "127.0.0.1";
        public int Port { get; set; }
        public int MaxUsers { get; set; }
        public uint Property { get; set; }
        public ulong Flag { get; set; }
        public short Icon { get; set; }
    }
}
