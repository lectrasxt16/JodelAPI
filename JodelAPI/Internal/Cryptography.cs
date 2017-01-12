using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace JodelAPI
{
    class SHA256 : System.IDisposable
    {
        public void Dispose()
        {

        }
        public static SHA256 Create()
        {
            return new SHA256();
        }
        public byte[] ComputeHash(byte[] data)
        {
            IBuffer input = data.AsBuffer();

            // hash it...
            var hasher = HashAlgorithmProvider.OpenAlgorithm("SHA256");
            IBuffer hashed = hasher.HashData(input);
            return hashed.ToArray();
        }
    }
    class HMACSHA1 : System.IDisposable
    {
        public byte[] Hash;
        public void Dispose()
        {

        }

        byte[] key;

        public HMACSHA1(byte[] keyb)
        {
            key = keyb;
        }

        public byte[] ComputeHash(byte[] data)
        {
            Hash = HmacSha1Sign(key, data);
            return Hash;
        }
        private byte[] HmacSha1Sign(byte[] keyBytes, byte[] messageBytes)
        {
            MacAlgorithmProvider objMacProv = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            CryptographicKey hmacKey = objMacProv.CreateKey(keyBytes.AsBuffer());
            IBuffer buffHMAC = CryptographicEngine.Sign(hmacKey, messageBytes.AsBuffer());
            return buffHMAC.ToArray();

        }
    }
}
