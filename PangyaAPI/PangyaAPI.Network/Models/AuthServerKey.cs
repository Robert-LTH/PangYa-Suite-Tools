using System.Runtime.InteropServices;
namespace PangyaAPI.Network.Models
{


    // Auth Server m_key Struture
    public class AuthServerKey
    {
        public AuthServerKey()
        {
        }
        public bool isValid()
        {
            return (valid == 1 && !string.IsNullOrEmpty(key));
        }
        public bool checkKey(string _str)
        {
            return (isValid() && string.Compare(_str, key) == 0);
        }
        public int server_uid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string key;               // 16 + null termineted string
        public byte valid = 1;
    }

}
