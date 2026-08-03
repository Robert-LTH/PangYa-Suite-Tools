using PangyaAPI.Utilities.BinaryModels;
namespace PangyaAPI.Network.Models
{

    // Auth Server - Server Send command to Other Server Header
    public class CommandOtherServerHeader
    {
        public uint send_server_uid_or_type { get; set; } // Envia o comando para esse server (UID/TYPE)
        public short command_id { get; set; }         // Comando ID

        public CommandOtherServerHeader(uint ul = 0)
        {
            send_server_uid_or_type = ul;
            command_id = 0;
        }

        public void Clear()
        {
            send_server_uid_or_type = 0;
            command_id = 0;
        }

        public byte[] ToArray()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(send_server_uid_or_type);
                p.Write(command_id);
                return p.GetBytes;
            }
        }
    }

}
