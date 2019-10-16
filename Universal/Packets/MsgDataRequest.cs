using System;
using System.Runtime.InteropServices;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgDataRequest
    {
        public short Length { get; private set; }
        public PacketType Id { get; private set; }
        public int UserId { get; set; }
        public int TargetId { get; set; }
        public int Param { get; set; }
        public MsgDataRequestType Type { get; set; }

        public static MsgDataRequest Create(int userId, int targetId, int param, MsgDataRequestType type)
        {
            Span<MsgDataRequest> span = stackalloc MsgDataRequest[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Length = (short)sizeof(MsgDataRequest);
            ptr.Id = PacketType.MsgDataRequest;
            ptr.Type = type;
            ptr.UserId = userId;
            ptr.TargetId = targetId;
            ptr.Param = param;
            return ptr;
        }

        public static MsgDataRequest CreateFriendListRequest(int userId) => Create(userId, 0, 0, MsgDataRequestType.Friends);
        public static MsgDataRequest CreateServerListRequest(int userId) => Create(userId, 0, 0, MsgDataRequestType.VServers);
        public static MsgDataRequest CreateServerChannelListRequest(int userId, int serverId) => Create(userId, serverId, 0, MsgDataRequestType.Channels);
        public static MsgDataRequest CreateRequestMissedMessagesPacket(int userId, int serverId, int channelId) => Create(userId, serverId, channelId, MsgDataRequestType.Messages);
        public static MsgDataRequest CreateRequestMissedMessagesPacket(int userId, int friendId) => Create(userId, 0, friendId, MsgDataRequestType.Messages);

        public static implicit operator byte[](MsgDataRequest msg)
        {
            Span<byte> buffer = stackalloc byte[sizeof(MsgDataRequest)];
            fixed (byte* p = buffer)
                *(MsgDataRequest*)p = *&msg;
            return buffer.ToArray();
        }
        public static implicit operator MsgDataRequest(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgDataRequest*)p;
        }
    }
}