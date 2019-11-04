using System.Buffers;
using System.Runtime.InteropServices;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgDH
    {
        public const int KEY_SIZE = 128;
        public int Length;
        public bool Compressed;
        public PacketType Id;
        public int PayloadLength;
        public fixed byte PublicKey[KEY_SIZE];

        public byte[] GetPayload()
        {
            var buffer = new byte[PayloadLength];
            for (var i = 0; i < PayloadLength; i++)
                buffer[i] = PublicKey[i];
            return buffer;
        }
        public void SetPayload(string chunk)
        {
            for (var i = 0; i < PayloadLength; i++)
                PublicKey[i] = (byte)chunk[i];
        }

        public static MsgDH Create(string diffie)
        {
            var ptr = stackalloc MsgDH[1];

            ptr->Length = sizeof(MsgDH);
            ptr->Compressed = false;
            ptr->Id = PacketType.MsgHandshake;
            ptr->PayloadLength = diffie.Length;
            ptr->SetPayload(diffie);

            return *ptr;
        }
        public static implicit operator byte[](MsgDH msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgDH));
            MemoryMarshal.Write(buffer, ref msg);
            return buffer;
        }
        public static implicit operator MsgDH(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgDH*)p;
        }
    }
}
