
using System;
using System.Security.Cryptography;

namespace Universal.IO.Sockets.Crypto
{
    public static class CryptoRandom
    {
        private static RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();

        public static uint NextUInt32()
        {
            byte[] res = new byte[4];
            csp.GetBytes(res);
            return BitConverter.ToUInt32(res, 0);
        }
        public static byte[] NextBytes(int count)
        {
            byte[] res = new byte[count];
            csp.GetBytes(res);
            return res;
        }

        public static int NextInt()
        {
            byte[] res = new byte[4];
            csp.GetBytes(res);
            return BitConverter.ToInt32(res, 0);
        }

        public static float NextSingle()
        {
            float numerator = NextUInt32();
            float denominator = uint.MaxValue;
            return numerator / denominator;
        }
    }
}