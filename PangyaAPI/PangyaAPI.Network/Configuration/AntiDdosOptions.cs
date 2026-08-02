using System.Collections.Generic;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Configuration
{

    public sealed class AntiDdosOptions
    {
        public bool EnableIpRules { get; set; } = true;
        public int LimitConnectionPerIp { get; set; } = 10;
        public string Order { get; set; } = "deny,allow";
        public List<string> Allow { get; set; } = new List<string>();
        public List<string> Deny { get; set; } = new List<string>();
        public int IntervalMilliseconds { get; set; } = 3000;
        public int Count { get; set; } = 5;
        public int AutoResetMilliseconds { get; set; } = 600000;
    }
}
