using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PangyaAPI.Network.Cryptor;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using uint8_t = System.Byte;

namespace PangyaAPI.Network.PangyaPacket
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class packet_head_client : packet_head
    {
        public byte seq;
    }
}
