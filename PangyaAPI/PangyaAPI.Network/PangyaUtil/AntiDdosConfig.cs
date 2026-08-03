using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PangyaAPI.Network.Configuration;

namespace PangyaAPI.Network.PangyaUtil
{
    public class AntiDdosConfig
    {
        public bool EnableIpRules { get; set; } = true;
        public int LimitConnectionPerIp { get; set; } = 10;
        public string Order { get; set; } = "deny,allow";
        public List<string> Allow { get; set; } = new List<string>();
        public List<string> Deny { get; set; } = new List<string>();
        public int DdosInterval { get; set; } = 3000;
        public int DdosCount { get; set; } = 5;
        public int DdosAutoReset { get; set; } = 3000;
    }

}
