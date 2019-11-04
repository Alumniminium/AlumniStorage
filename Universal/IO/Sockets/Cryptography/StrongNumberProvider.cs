using System;
using System.Security.Cryptography;

namespace Universal.IO.Sockets.Cryptography
{
    public static class CryptoRandom
    {
        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        private const float DENOMINATOR = uint.MaxValue;
        public static uint NextUInt32()
        {
            var res = new byte[4];
            Random.GetBytes(res);
            return BitConverter.ToUInt32(res, 0);
        }
        public static byte[] NextBytes(int count)
        {
            var res = new byte[count];
            Random.GetBytes(res);
            return res;
        }

        public static int NextInt()
        {
            var res = new byte[4];
            Random.GetBytes(res);
            return BitConverter.ToInt32(res, 0);
        }

        public static float NextSingle()
        {
            float numerator = NextUInt32();
            return numerator / DENOMINATOR;
        }
    }
}