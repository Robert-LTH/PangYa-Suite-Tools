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

    public class offset_index
    {
        public byte[] m_buf;
        public ulong m_index_r;
        public ulong m_index_w;
        public ulong m_size;
        public ulong m_size_alloced;

        public void clear() { if (m_buf != null && m_buf.Length > 0) { Array.Clear(m_buf, 0, m_buf.Length); } }
        public void reset_read() => m_index_r = 0;
        public void reset_write()
        {
            m_index_w = 0;
            m_size = 0;
        }
        public void reset()
        {
            reset_read();
            reset_write();
        }
    }
}
