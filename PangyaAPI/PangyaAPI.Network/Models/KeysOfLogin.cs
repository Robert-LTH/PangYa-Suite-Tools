using System.Runtime.InteropServices;
namespace PangyaAPI.Network.Models
{


    // Keys Of Login
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class KeysOfLogin
    {
        public KeysOfLogin()
        {
            keys = new string[2];
        }
        public byte valid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string[] keys { get; set; } = new string[2];
    }

}
