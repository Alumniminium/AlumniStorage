using System;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgText
    {
        public const int MAX_TEXT_LENGTH = 1024;
        public short Length { get; private set; }
        public PacketType Id { get; private set; }
        public int UniqueId { get; set; }
        public int ServerId { get; set; } // 0 == Direct Message
        public int ChannelId { get; set; }// if ServerId == 0  then  ChannelId = FriendId
        public int FriendId => ChannelId;
        public int AuthorId { get; set; }
        public long SentTime { get; set; }

        public fixed char Text[MAX_TEXT_LENGTH];

        public string GetText()
        {
            fixed (char* p = Text)
                return new string(p);
        }
        public void SetText(string text)
        {
            text = text.ToLength(MAX_TEXT_LENGTH);
            for (var i = 0; i < text.Length; i++)
                Text[i] = text[i];
        }
        public static MsgText Create(int uniqueId, int authorId, string text, int serverId, int channelId, DateTime createdTime)
        {
            Span<MsgText> span = stackalloc MsgText[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Length = (short)sizeof(MsgText);
            ptr.Id = PacketType.MsgText;
            ptr.UniqueId = uniqueId;
            ptr.ServerId = serverId;
            ptr.AuthorId = authorId;
            ptr.ChannelId = channelId;
            ptr.SentTime = createdTime.Ticks;
            ptr.SetText(text);
            return ptr;
        }

        public static implicit operator byte[](MsgText msg)
        {
            Span<byte> buffer = stackalloc byte[sizeof(MsgText)];
            fixed (byte* p = buffer)
                *(MsgText*)p = *&msg;
            return buffer.ToArray();
        }
        public static implicit operator MsgText(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgText*)p;
        }
    }
}