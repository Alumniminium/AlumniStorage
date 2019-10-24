using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgBench
    {
        public const int MAX_ARRAY_LENGTH = 5;
        public MsgHeader Header { get; set; }
        public fixed byte Array[MAX_ARRAY_LENGTH];

        public byte[] GetArray()
        {
            var buffer = new byte[MAX_ARRAY_LENGTH];
            for (int i = 0; i < MAX_ARRAY_LENGTH; i++)
                buffer[i] = Array[i];
            return buffer;
        }

        public void SetToken(byte[] array)
        {
            fixed (byte* b = Array)
                Unsafe.Copy(b, ref array);
        }

        public static MsgBench Create(byte[] array, bool compression)
        {
            Span<MsgBench> span = stackalloc MsgBench[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);

            ptr.Header = new MsgHeader
            {
                Length = sizeof(MsgBench),
                Compressed = compression,
                Id = PacketType.MsgBench,
            };

            ptr.SetToken(array);
            return ptr;
        }
        public static implicit operator byte[](MsgBench msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(msg.Header.Length).SelfSetToDefaults();
            fixed (byte* p = buffer)
                *(MsgBench*)p = *&msg;
            return buffer;
        }
        public static implicit operator MsgBench(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgBench*)p;
        }
    }
}
