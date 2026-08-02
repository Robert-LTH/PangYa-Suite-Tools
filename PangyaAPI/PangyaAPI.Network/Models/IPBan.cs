namespace PangyaAPI.Network.Models
{
    public class IPBan
    {
        public enum _TYPE : byte
        {
            IP_BLOCK_NORMAL,
            IP_BLOCK_RANGE
        }
        public _TYPE type;
        public uint ip;
        public uint mask;
    }

}
