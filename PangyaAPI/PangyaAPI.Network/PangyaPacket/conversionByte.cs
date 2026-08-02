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

    public class conversionByte
    {
        public const byte CB_BASE_256 = 10;
        public const byte CB_BASE_255 = 20;
        public const byte CB_SEQ_NORMAL = 1;
        public const byte CB_SEQ_INVERTIDA = 2;
        public const byte CB_PARAM_DEFAULT = 0;

        public unionConvertidoStruct unionConvertido;
        private byte m_flag;
        private uint ulNumber_temp;

        public conversionByte()
        {
            unionConvertido = new unionConvertidoStruct();
        }

        public conversionByte(uint _dwConvertido, byte _flag = CB_PARAM_DEFAULT)
        {
            unionConvertido = new unionConvertidoStruct { dwConvertido = _dwConvertido };
            m_flag = _flag;
            if (m_flag != CB_PARAM_DEFAULT) invert();
        }

        public conversionByte(byte[] _ucpConvertido, byte _flag = CB_PARAM_DEFAULT)
        {
            unionConvertido = new unionConvertidoStruct();
            m_flag = _flag;

            if (_ucpConvertido != null && _ucpConvertido.Length >= 4)
                unionConvertido.dwConvertido = BitConverter.ToUInt32(_ucpConvertido, 0);

            if (m_flag != CB_PARAM_DEFAULT)
                invert();
        }

        private void invert()
        {
            if ((m_flag & CB_BASE_255) != 0)
            {
                unionConvertido.dwConvertido = getNumberIS();
                unionConvertido.dwConvertido = getNumberBase256();
            }
            else
            {
                unionConvertido.dwConvertido = getNumberBase255();
                unionConvertido.dwConvertido = getNumberIS();
            }
        }

        public uint getNumberNS() => unionConvertido.dwConvertido;

        public uint getNumberIS()
        {
            return (uint)(unionConvertido.a << 24 | unionConvertido.b << 16 | unionConvertido.c << 8 | unionConvertido.d);
        }

        public byte[] getLPUCNS()
        {
            ulNumber_temp = getNumberNS();
            return BitConverter.GetBytes(ulNumber_temp);
        }

        public byte[] getLPUCIS()
        {
            ulNumber_temp = getNumberIS();
            return BitConverter.GetBytes(ulNumber_temp);
        }

        public uint getNumberBase256() => getNumberNS() * 255 / 256 + 1;
        public uint getNumberBase255() => ((unionConvertido.dwConvertido / 255) << 8) | unionConvertido.dwConvertido % 255;

        public uint getISNumberBase256() => getNumberIS() * 255 / 256 + 1;
        public uint getISNumberBase255() => ((getNumberIS() / 255) << 8) | getNumberIS() % 255;

        public int putNumberBuffer(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 4)
                return -1;

            var bytes = BitConverter.GetBytes(unionConvertido.dwConvertido);
            Array.Copy(bytes, 0, buffer, 0, 4);
            return 4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct unionConvertidoStruct
        {
            public uint dwConvertido;
            public byte a => (byte)((dwConvertido >> 24) & 0xFF);
            public byte b => (byte)((dwConvertido >> 16) & 0xFF);
            public byte c => (byte)((dwConvertido >> 8) & 0xFF);
            public byte d => (byte)(dwConvertido & 0xFF);
        }
    }
}
