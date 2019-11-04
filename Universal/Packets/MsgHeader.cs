using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct MsgHeader
    {
        public const int LENGTH_OFFSET = 0;
        public const int COMPRESS_OFFSET = 4;
        public const int ID_OFFSET = 5;
        public const int IV_OFFSET = 6;

        [FieldOffset(0)] 
        public int Length;
        [FieldOffset(COMPRESS_OFFSET)]
        public bool Compressed;
        [FieldOffset(ID_OFFSET)]
        public PacketType Id;
        [FieldOffset(IV_OFFSET)]
        public fixed byte IV[16];

        public byte[] GetArray()
        {
            fixed (byte* p = IV)
                return new Span<byte>(p, 16).ToArray();
        }
        public void SetArray(byte[] array)
        {
            fixed (byte* p = IV)
                array.AsSpan().CopyTo(new Span<byte>(p, 16));
        }

        public static implicit operator byte[](MsgHeader msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgHeader));
            MemoryMarshal.Write(buffer, ref msg);
            return buffer;
        }
        public static implicit operator MsgHeader(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgHeader*)p;
        }
    }
}