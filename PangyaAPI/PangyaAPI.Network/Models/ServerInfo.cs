using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;
namespace PangyaAPI.Network.Models
{



    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 92)]
    public class ServerInfo
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int MaxUsers { get; set; }
        public int ConnectedUsers { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string IpAddress { get; set; } = "";
        public int Port { get; set; }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 4)]
        public PropertyType propriedade { get; set; } = PropertyType.NORMAL;
        public int Angelic_wings_num { get; set; }
        public EventType flagEvent;
        public short EventMap { get; set; }
        public short AppRate { get; set; }
        public short ScratchRate { get; set; } // pode ser scratchy rate ou não
        public short ImageNumber { get; set; }
        public ServerInfo()
        {
            //name_bytes = new byte[40];
        }

        public byte[] ToArray()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteStr(Name, 40);
                p.WriteInt32(Id);
                p.WriteInt32(MaxUsers);
                p.WriteInt32(ConnectedUsers);
                p.WriteStr(IpAddress, 18);
                p.WriteInt32(Port);
                p.WriteUInt32((uint)propriedade);
                p.WriteInt32(Angelic_wings_num);
                p.WriteUInt16((ushort)flagEvent);
                p.WriteInt16(EventMap);
                p.WriteInt16(AppRate);
                p.WriteInt16(ScratchRate); // pode ser scratchy rate ou não
                p.WriteInt16(ImageNumber);
                return p.GetBytes;
            }
        }
    }

}
