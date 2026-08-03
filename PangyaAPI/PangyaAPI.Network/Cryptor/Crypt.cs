using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Network.Cryptor
{

    public class Crypt
    {
        public uint PrivateKey { get; private set; }
        public uint PublicKey { get; private set; }

        public Crypt(int parseKey, int lowKey)
        {
            int index = (parseKey << 8) | lowKey;
            PrivateKey = CryptoOracle.PRIVATE_KEY_TABLE[index];
            PublicKey = CryptoOracle.PUBLIC_KEY_TABLE[index];
        }

        public uint Encrypt(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Buffer inválido para criptografia.");

            buffer[0] = (byte)(PrivateKey & 0xFF);

            CryptUtils.SimpleStreamEncrypt(buffer, PublicKey);

            return PublicKey;
        }

        public uint Decrypt(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Buffer inválido para descriptografia.");

            CryptUtils.SimpleStreamDecrypt(buffer, PublicKey);

            return PrivateKey;
        }
    }
}
