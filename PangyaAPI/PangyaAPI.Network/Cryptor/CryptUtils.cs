using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Network.Cryptor
{
    public static class CryptUtils
    {
        private static int _8bitShift(uint bits, int shift)
        {
            shift *= 8;
            return (int)((bits >> shift) & 0xFF);
        }

        public static void SimpleStreamEncrypt(byte[] buffer, uint publicKey)
        {
            if (buffer == null || buffer.Length == 0) return;

            var plain = new byte[buffer.Length];
            Array.Copy(buffer, plain, buffer.Length);

            int limit = buffer.Length >= 4 ? 4 : buffer.Length;

            for (int i = 0; i < limit; i++)
                buffer[i] = (byte)((plain[i] ^ _8bitShift(publicKey, i)) & 0xFF);

            for (int i = 4; i < buffer.Length; i++)
                buffer[i] = (byte)((buffer[i] ^ plain[i - 4]) & 0xFF);
        }

        public static void SimpleStreamDecrypt(byte[] buffer, uint publicKey)
        {
            if (buffer == null || buffer.Length == 0) return;

            int limit = buffer.Length >= 4 ? 4 : buffer.Length;

            for (int i = 0; i < limit; i++)
                buffer[i] = (byte)((buffer[i] ^ _8bitShift(publicKey, i)) & 0xFF);

            for (int i = 4; i < buffer.Length; i++)
                buffer[i] = (byte)((buffer[i] ^ buffer[i - 4]) & 0xFF);
        }
    }
}
