using System.Collections.Generic;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Configuration
{

    public sealed class LoggingOptions
    {
        public string Directory { get; set; } = "Log";
        public bool ActiveRoomLog { get; set; }
    }
}
