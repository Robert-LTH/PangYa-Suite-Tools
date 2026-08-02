using System.Runtime.InteropServices;
namespace PangyaAPI.Network.Models
{

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ServerInfoEx : ServerInfo
    {
        public sbyte tipo { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string version { get; set; } = new string(new char[40]);
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string version_client { get; set; } = new string(new char[40]);
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 2)]
        public uint packet_version { get; set; }
        public RateConfigInfo rate { get; set; } = new RateConfigInfo();
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 4)]
        public uFlag flag { get; set; } = new uFlag();
    }

}
