using System;
using System.Numerics;

namespace Universal.Extensions
{
    public static class ByteCopyVectorized
    {
        // Will be Jit'd to consts https://github.com/dotnet/coreclr/issues/1079
        private static readonly int VectorSpan = Vector<byte>.Count;
        private static readonly int VectorSpan2 = Vector<byte>.Count + Vector<byte>.Count;
        private static readonly int VectorSpan3 = Vector<byte>.Count + Vector<byte>.Count + Vector<byte>.Count;

        private static readonly int VectorSpan4 = Vector<byte>.Count + Vector<byte>.Count + Vector<byte>.Count + Vector<byte>.Count;

        private const int LONG_SPAN = sizeof(long);
        private const int LONG_SPAN2 = sizeof(long) + sizeof(long);
        private const int LONG_SPAN3 = sizeof(long) + sizeof(long) + sizeof(long);
        private const int INT_SPAN = sizeof(int);

        /// <remarks>
        /// Code must be optimized, in release mode and Vector.IsHardwareAccelerated must be true for the performance benefits.
        /// </remarks>
        public static unsafe void VectorizedCopy(this byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            if (Vector.IsHardwareAccelerated)
            {
                /*if (count > 512 + 64)
                {
                    // In-built copy faster for large arrays (vs repeated bounds checks on Vector.ctor?)
                    Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
                    return;
                }*/

                while (count >= VectorSpan4)
                {
                    new Vector<byte>(src, srcOffset).CopyTo(dst, dstOffset);
                    new Vector<byte>(src, srcOffset + VectorSpan).CopyTo(dst, dstOffset + VectorSpan);
                    new Vector<byte>(src, srcOffset + VectorSpan2).CopyTo(dst, dstOffset + VectorSpan2);
                    new Vector<byte>(src, srcOffset + VectorSpan3).CopyTo(dst, dstOffset + VectorSpan3);
                    if (count == VectorSpan4) return;
                    count -= VectorSpan4;
                    srcOffset += VectorSpan4;
                    dstOffset += VectorSpan4;
                }

                if (count >= VectorSpan2)
                {
                    new Vector<byte>(src, srcOffset).CopyTo(dst, dstOffset);
                    new Vector<byte>(src, srcOffset + VectorSpan).CopyTo(dst, dstOffset + VectorSpan);
                    if (count == VectorSpan2) return;
                    count -= VectorSpan2;
                    srcOffset += VectorSpan2;
                    dstOffset += VectorSpan2;
                }

                if (count >= VectorSpan)
                {
                    new Vector<byte>(src, srcOffset).CopyTo(dst, dstOffset);
                    if (count == VectorSpan) return;
                    count -= VectorSpan;
                    srcOffset += VectorSpan;
                    dstOffset += VectorSpan;
                }

                if (count > 0)
                {
                    fixed (byte* srcOrigin = src)
                    fixed (byte* dstOrigin = dst)
                    {
                        var pSrc = srcOrigin + srcOffset;
                        var dSrc = dstOrigin + dstOffset;

                        if (count >= LONG_SPAN)
                        {
                            var lpSrc = (long*)pSrc;
                            var ldSrc = (long*)dSrc;

                            if (count < LONG_SPAN2)
                            {
                                count -= LONG_SPAN;
                                pSrc += LONG_SPAN;
                                dSrc += LONG_SPAN;
                                *ldSrc = *lpSrc;
                            }
                            else if (count < LONG_SPAN3)
                            {
                                count -= LONG_SPAN2;
                                pSrc += LONG_SPAN2;
                                dSrc += LONG_SPAN2;
                                *ldSrc = *lpSrc;
                                *(ldSrc + 1) = *(lpSrc + 1);
                            }
                            else
                            {
                                count -= LONG_SPAN3;
                                pSrc += LONG_SPAN3;
                                dSrc += LONG_SPAN3;
                                *ldSrc = *lpSrc;
                                *(ldSrc + 1) = *(lpSrc + 1);
                                *(ldSrc + 2) = *(lpSrc + 2);
                            }
                        }

                        if (count >= INT_SPAN)
                        {
                            var ipSrc = (int*)pSrc;
                            var idSrc = (int*)dSrc;
                            count -= INT_SPAN;
                            pSrc += INT_SPAN;
                            dSrc += INT_SPAN;
                            *idSrc = *ipSrc;
                        }

                        while (count > 0)
                        {
                            count--;
                            *dSrc = *pSrc;
                            dSrc += 1;
                            pSrc += 1;
                        }
                    }
                }
            }
            else
            {
                Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
            }
        }
    }
}