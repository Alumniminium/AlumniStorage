using System;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgChannel
    {
        public const int MAX_NAME_ENGTH = 48;
        public short Length { get; private set; }
        public PacketType Id { get; private set; }
        public int UniqueId { get; set; }
        public int ServerId { get; set; }

        public fixed char Name[MAX_NAME_ENGTH];

        public string GetName()
        {
            fixed (char* p = Name)
                return new string(p);
        }
        public void SetName(string username)
        {
            username = username.ToLength(MAX_NAME_ENGTH);
            for (var i = 0; i < username.Length; i++)
                Name[i] = username[i];
        }

        public static MsgChannel Create(int uniqueId, int serverId, string name)
        {
            Span<MsgChannel> span = stackalloc MsgChannel[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Length = (short)sizeof(MsgChannel);
            ptr.Id = PacketType.MsgChannel;
            ptr.UniqueId = uniqueId;
            ptr.ServerId = serverId;
            ptr.SetName(name);
            return ptr;
        }

        public static implicit operator byte[](MsgChannel msg)
        {
            Span<byte> buffer = stackalloc byte[sizeof(MsgChannel)];
            fixed (byte* p = buffer)
                *(MsgChannel*)p = *&msg;
            return buffer.ToArray();
        }
        public static implicit operator MsgChannel(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgChannel*)p;
        }
    }
}