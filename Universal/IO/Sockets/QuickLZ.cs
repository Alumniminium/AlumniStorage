using System;
using System.Buffers;
using Universal.Extensions;

namespace Universal.IO.Sockets
{
    public static class QuickLz
    {
        private const int HASH_VALUES = 4096;
        private const int MINOFFSET = 2;
        private const int UNCONDITIONAL_MATCHLEN = 6;
        private const int UNCOMPRESSED_END = 4;
        private const int CWORD_LEN = 4;
        private const int DEFAULT_HEADERLEN = 9;
        private const int QLZ_POINTERS_1 = 1;
        private const int QLZ_POINTERS_3 = 16;

        private static int HeaderLen(Span<byte> source)
        {
            return ((source[0] & 2) == 2) ? 9 : 3;
        }

        public static int SizeDecompressed(Span<byte> source)
        {
            if (HeaderLen(source) == 9)
                return source[5] | (source[6] << 8) | (source[7] << 16) | (source[8] << 24);
            else
                return source[2];
        }

        public static int SizeCompressed(Span<byte> source)
        {
            if (HeaderLen(source) == 9)
                return source[1] | (source[2] << 8) | (source[3] << 16) | (source[4] << 24);
            else
                return source[1];
        }

        private static void Write_header(byte[] dst, int level, bool compressible, int sizeCompressed, int sizeDecompressed)
        {
            dst[0] = (byte)(2 | (compressible ? 1 : 0));
            dst[0] |= (byte)(level << 2);
            dst[0] |= (1 << 6);
            dst[0] |= (0 << 4);
            fast_write(dst, 1, sizeDecompressed, 4);
            fast_write(dst, 5, sizeCompressed, 4);
        }

        public static byte[] Compress(byte[] source, int size, int level, byte[] destination)
        {
            var src = 0;
            var dst = DEFAULT_HEADERLEN + CWORD_LEN;
            var cwordVal = 0x80000000;
            var cwordPtr = DEFAULT_HEADERLEN;
            var cachetable = new int[HASH_VALUES];
            var hashCounter = new byte[HASH_VALUES];
            byte[] d2;
            var fetch = 0;
            var lastMatchstart = (size - UNCONDITIONAL_MATCHLEN - UNCOMPRESSED_END - 1);
            var lits = 0;

            var hashtable = level == 1 ? new int[HASH_VALUES, QLZ_POINTERS_1] : new int[HASH_VALUES, QLZ_POINTERS_3];

            if (size == 0)
                return new byte[0];

            if (src <= lastMatchstart)
                fetch = source[src] | (source[src + 1] << 8) | (source[src + 2] << 16);

            while (src <= lastMatchstart)
            {
                if ((cwordVal & 1) == 1)
                {
                    if (src > size >> 1 && dst > src - (src >> 5))
                    {
                        d2 = new byte[size + DEFAULT_HEADERLEN];
                        Write_header(d2, level, false, size, size + DEFAULT_HEADERLEN);
                        Buffer.BlockCopy(source, 0, d2, DEFAULT_HEADERLEN, size);
                        return d2;
                    }

                    fast_write(destination, cwordPtr, (int)((cwordVal >> 1) | 0x80000000), 4);
                    cwordPtr = dst;
                    dst += CWORD_LEN;
                    cwordVal = 0x80000000;
                }

                if (level == 1)
                {
                    var hash = ((fetch >> 12) ^ fetch) & (HASH_VALUES - 1);
                    var o = hashtable[hash, 0];
                    var cache = cachetable[hash] ^ fetch;
                    cachetable[hash] = fetch;
                    hashtable[hash, 0] = src;

                    if (cache == 0 && hashCounter[hash] != 0 && (src - o > MINOFFSET || (src == o + 1 && lits >= 3 && src > 3 && source[src] == source[src - 3] && source[src] == source[src - 2] && source[src] == source[src - 1] && source[src] == source[src + 1] && source[src] == source[src + 2])))
                    {
                        cwordVal = ((cwordVal >> 1) | 0x80000000);
                        if (source[o + 3] != source[src + 3])
                        {
                            var f = 3 - 2 | (hash << 4);
                            destination[dst + 0] = (byte)(f >> 0 * 8);
                            destination[dst + 1] = (byte)(f >> 1 * 8);
                            src += 3;
                            dst += 2;
                        }
                        else
                        {
                            var oldSrc = src;
                            var remaining = ((size - UNCOMPRESSED_END - src + 1 - 1) > 255 ? 255 : (size - UNCOMPRESSED_END - src + 1 - 1));

                            src += 4;
                            if (source[o + src - oldSrc] == source[src])
                            {
                                src++;
                                if (source[o + src - oldSrc] == source[src])
                                {
                                    src++;
                                    while (source[o + (src - oldSrc)] == source[src] && (src - oldSrc) < remaining)
                                        src++;
                                }
                            }

                            var matchlen = src - oldSrc;

                            hash <<= 4;
                            if (matchlen < 18)
                            {
                                var f = (hash | (matchlen - 2));
                                destination[dst + 0] = (byte)(f >> 0 * 8);
                                destination[dst + 1] = (byte)(f >> 1 * 8);
                                dst += 2;
                            }
                            else
                            {
                                fast_write(destination, dst, hash | (matchlen << 16), 3);
                                dst += 3;
                            }
                        }
                        fetch = source[src] | (source[src + 1] << 8) | (source[src + 2] << 16);
                        lits = 0;
                    }
                    else
                    {
                        lits++;
                        hashCounter[hash] = 1;
                        destination[dst] = source[src];
                        cwordVal = (cwordVal >> 1);
                        src++;
                        dst++;
                        fetch = ((fetch >> 8) & 0xffff) | (source[src + 2] << 16);
                    }

                }
                else
                {
                    fetch = source[src] | (source[src + 1] << 8) | (source[src + 2] << 16);

                    int o;
                    int k;
                    var remaining = ((size - UNCOMPRESSED_END - src + 1 - 1) > 255 ? 255 : (size - UNCOMPRESSED_END - src + 1 - 1));
                    var hash = ((fetch >> 12) ^ fetch) & (HASH_VALUES - 1);

                    var c = hashCounter[hash];
                    var matchlen = 0;
                    var offset2 = 0;
                    for (k = 0; k < QLZ_POINTERS_3 && c > k; k++)
                    {
                        o = hashtable[hash, k];
                        if ((byte) fetch != source[o] || (byte) (fetch >> 8) != source[o + 1] || (byte) (fetch >> 16) != source[o + 2] || o >= src - MINOFFSET)
                            continue;
                        var m = 3;
                        while (source[o + m] == source[src + m] && m < remaining)
                            m++;
                        if ((m > matchlen) || (m == matchlen && o > offset2))
                        {
                            offset2 = o;
                            matchlen = m;
                        }
                    }
                    o = offset2;
                    hashtable[hash, c & (QLZ_POINTERS_3 - 1)] = src;
                    c++;
                    hashCounter[hash] = c;

                    if (matchlen >= 3 && src - o < 131071)
                    {
                        var offset = src - o;

                        for (var u = 1; u < matchlen; u++)
                        {
                            fetch = source[src + u] | (source[src + u + 1] << 8) | (source[src + u + 2] << 16);
                            hash = ((fetch >> 12) ^ fetch) & (HASH_VALUES - 1);
                            c = hashCounter[hash]++;
                            hashtable[hash, c & (QLZ_POINTERS_3 - 1)] = src + u;
                        }

                        src += matchlen;
                        cwordVal = ((cwordVal >> 1) | 0x80000000);

                        if (matchlen == 3 && offset <= 63)
                        {
                            fast_write(destination, dst, offset << 2, 1);
                            dst++;
                        }
                        else if (matchlen == 3 && offset <= 16383)
                        {
                            fast_write(destination, dst, (offset << 2) | 1, 2);
                            dst += 2;
                        }
                        else if (matchlen <= 18 && offset <= 1023)
                        {
                            fast_write(destination, dst, ((matchlen - 3) << 2) | (offset << 6) | 2, 2);
                            dst += 2;
                        }
                        else if (matchlen <= 33)
                        {
                            fast_write(destination, dst, ((matchlen - 2) << 2) | (offset << 7) | 3, 3);
                            dst += 3;
                        }
                        else
                        {
                            fast_write(destination, dst, ((matchlen - 3) << 7) | (offset << 15) | 3, 4);
                            dst += 4;
                        }
                        lits = 0;
                    }
                    else
                    {
                        destination[dst] = source[src];
                        cwordVal = (cwordVal >> 1);
                        src++;
                        dst++;
                    }
                }
            }
            while (src <= size - 1)
            {
                if ((cwordVal & 1) == 1)
                {
                    fast_write(destination, cwordPtr, (int)((cwordVal >> 1) | 0x80000000), 4);
                    cwordPtr = dst;
                    dst += CWORD_LEN;
                    cwordVal = 0x80000000;
                }

                destination[dst] = source[src];
                src++;
                dst++;
                cwordVal = (cwordVal >> 1);
            }
            while ((cwordVal & 1) != 1)
            {
                cwordVal = (cwordVal >> 1);
            }
            fast_write(destination, cwordPtr, (int)((cwordVal >> 1) | 0x80000000), CWORD_LEN);
            Write_header(destination, level, true, size, dst);
            d2 = ArrayPool<byte>.Shared.Rent(dst).SelfSetToDefaults();
            Buffer.BlockCopy(destination, 0, d2, 0, dst);
            ArrayPool<byte>.Shared.Return(destination);
            return d2;
        }


        private static void fast_write(byte[] a, int i, int value, int numbytes)
        {
            for (var j = 0; j < numbytes; j++)
                a[i + j] = (byte)(value >> (j * 8));
        }

        public static byte[] Decompress(Span<byte> source)
        {
            var size = SizeDecompressed(source);
            var src = HeaderLen(source);
            var dst = 0;
            uint cwordVal = 1;
            var destination = new byte[size];
            var hashtable = new int[4096];
            var hashCounter = new byte[4096];
            var lastMatchstart = size - UNCONDITIONAL_MATCHLEN - UNCOMPRESSED_END - 1;
            var lastHashed = -1;
            uint fetch = 0;

            var level = (source[0] >> 2) & 0x3;

            if ((source[0] & 1) != 1)
            {
                var hl = HeaderLen(source);
                var d2 = new byte[size];
                for (var i = 0; i < size; i++)
                    d2[i] = source[hl + i];
                return d2;
            }

            for (; ; )
            {
                if (cwordVal == 1)
                {
                    cwordVal = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                    src += 4;
                    if (dst <= lastMatchstart)
                    {
                        if (level == 1)
                            fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16));
                        else
                            fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                    }
                }

                int hash;
                if ((cwordVal & 1) == 1)
                {
                    uint matchlen;
                    uint offset2;

                    cwordVal = cwordVal >> 1;

                    if (level == 1)
                    {
                        hash = ((int)fetch >> 4) & 0xfff;
                        offset2 = (uint)hashtable[hash];

                        if ((fetch & 0xf) != 0)
                        {
                            matchlen = (fetch & 0xf) + 2;
                            src += 2;
                        }
                        else
                        {
                            matchlen = source[src + 2];
                            src += 3;
                        }
                    }
                    else
                    {
                        uint offset;
                        if ((fetch & 3) == 0)
                        {
                            offset = (fetch & 0xff) >> 2;
                            matchlen = 3;
                            src++;
                        }
                        else if ((fetch & 2) == 0)
                        {
                            offset = (fetch & 0xffff) >> 2;
                            matchlen = 3;
                            src += 2;
                        }
                        else if ((fetch & 1) == 0)
                        {
                            offset = (fetch & 0xffff) >> 6;
                            matchlen = ((fetch >> 2) & 15) + 3;
                            src += 2;
                        }
                        else if ((fetch & 127) != 3)
                        {
                            offset = (fetch >> 7) & 0x1ffff;
                            matchlen = ((fetch >> 2) & 0x1f) + 2;
                            src += 3;
                        }
                        else
                        {
                            offset = (fetch >> 15);
                            matchlen = ((fetch >> 7) & 255) + 3;
                            src += 4;
                        }
                        offset2 = (uint)(dst - offset);
                    }

                    destination[dst + 0] = destination[offset2 + 0];
                    destination[dst + 1] = destination[offset2 + 1];
                    destination[dst + 2] = destination[offset2 + 2];

                    for (var i = 3; i < matchlen; i += 1)
                    {
                        destination[dst + i] = destination[offset2 + i];
                    }

                    dst += (int)matchlen;

                    if (level == 1)
                    {
                        fetch = (uint)(destination[lastHashed + 1] | (destination[lastHashed + 2] << 8) | (destination[lastHashed + 3] << 16));
                        while (lastHashed < dst - matchlen)
                        {
                            lastHashed++;
                            hash = (int)(((fetch >> 12) ^ fetch) & (HASH_VALUES - 1));
                            hashtable[hash] = lastHashed;
                            hashCounter[hash] = 1;
                            fetch = (uint)(fetch >> 8 & 0xffff | destination[lastHashed + 3] << 16);
                        }
                        fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16));
                    }
                    else
                    {
                        fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                    }
                    lastHashed = dst - 1;
                }
                else
                {
                    if (dst <= lastMatchstart)
                    {
                        destination[dst] = source[src];
                        dst += 1;
                        src += 1;
                        cwordVal = cwordVal >> 1;

                        if (level == 1)
                        {
                            while (lastHashed < dst - 3)
                            {
                                lastHashed++;
                                var fetch2 = destination[lastHashed] | (destination[lastHashed + 1] << 8) | (destination[lastHashed + 2] << 16);
                                hash = ((fetch2 >> 12) ^ fetch2) & (HASH_VALUES - 1);
                                hashtable[hash] = lastHashed;
                                hashCounter[hash] = 1;
                            }
                            fetch = (uint)(fetch >> 8 & 0xffff | source[src + 2] << 16);
                        }
                        else
                        {
                            fetch = (uint)(fetch >> 8 & 0xffff | source[src + 2] << 16 | source[src + 3] << 24);
                        }
                    }
                    else
                    {
                        while (dst <= size - 1)
                        {
                            if (cwordVal == 1)
                            {
                                src += CWORD_LEN;
                                cwordVal = 0x80000000;
                            }

                            destination[dst] = source[src];
                            dst++;
                            src++;
                            cwordVal >>= 1;
                        }
                        return destination;
                    }
                }
            }
        }
    }
}