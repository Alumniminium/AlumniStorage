using System.Security.Cryptography;

namespace Universal.IO.Sockets.Cryptography
{
    public class Crypto
    {
        public readonly Aes Aes;

        public Crypto(byte[] key)
        {
            Aes= Aes.Create();
            //Aes.Padding = PaddingMode.None;
            //Aes.Mode = CipherMode.CBC;
            //Aes.KeySize = 256;
            //Aes.BlockSize = 128;
            Aes.Key = key;
        }

        public void SetIV(byte[] iv) => Aes.IV = iv;
        public byte[] SetRandomIV() => Aes.IV = CryptoRandom.NextBytes(16);
        public byte[] Encrypt(byte[] plain) => Aes.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);
        public byte[] Decrypt(byte[] encrypted) => Aes.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
    }
}
