using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MsgBench
    {
        public const int MAX_ARRAY_LENGTH = 64;
        public MsgHeader Header;
        public fixed byte Array[MAX_ARRAY_LENGTH];

        public Span<byte> GetArray()
        {
            fixed (byte* p = Array)
                return new Span<byte>(p, MAX_ARRAY_LENGTH);
        }
        public void SetArray(byte[] array)
        {
            fixed (byte* p = Array)
                array.AsSpan().CopyTo(new Span<byte>(p, Math.Min(MAX_ARRAY_LENGTH, array.Length)));
        }

        public static MsgBench Create(byte[] array, bool compression)
        {
            var ptr = stackalloc MsgBench[1];

            ptr->Header = new MsgHeader
            {
                Length = sizeof(MsgBench), 
                Compressed = compression, 
                Id = PacketType.MsgBench
            };

            ptr->SetArray(array);
            return *ptr;
        }
        public static implicit operator byte[](MsgBench msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgBench));
            MemoryMarshal.Write(buffer, ref msg);
            return buffer;
        }
        public static implicit operator MsgBench(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgBench*)p;
        }
    }
}
