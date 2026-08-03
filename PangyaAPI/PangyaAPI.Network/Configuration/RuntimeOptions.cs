using System.Collections.Generic;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Configuration
{

    public sealed class RuntimeOptions
    {
        public string Encoding { get; set; } = "shift_jis";
        public uint SessionTtl { get; set; }
        public int AntiBotTtl { get; set; } = 1000;
        public bool SameIdLogin { get; set; }
        public bool CreateUser { get; set; } = true;
        public bool AccessRestricted { get; set; }
        public bool Maintenance { get; set; }
    }
}
