using System;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgUser
    {
        public const int MAX_NICKNAME_LENGTH = 32;
        public const int MAX_AVATAR_URL_LENGTH = 128;
        public short Length { get; private set; }
        public PacketType Id { get; private set; }
        public int UniqueId { get; set; }
        public int ServerId { get; set; }
        public bool Online { get; set; }

        public fixed char Nickname[MAX_NICKNAME_LENGTH];
        public fixed char AvatarUrl[MAX_AVATAR_URL_LENGTH];

        public string GetNickname()
        {
            fixed (char* p = Nickname)
                return new string(p);
        }
        public string GetAvatarUrl()
        {
            fixed (char* p = AvatarUrl)
                return new string(p);
        }

        public void SetNickname(string nickname)
        {
            nickname = nickname.ToLength(MAX_NICKNAME_LENGTH);
            for (var i = 0; i < nickname.Length; i++)
                Nickname[i] = nickname[i];
        }
        public void SetAvatarUrl(string url)
        {
            url = url.ToLength(MAX_AVATAR_URL_LENGTH);
            for (var i = 0; i < url.Length; i++)
                AvatarUrl[i] = url[i];
        }
        public static MsgUser Create(int uniqueId, int serverId, string nickname, string avatarUrl, string email, bool online)
        {
            Span<MsgUser> span = stackalloc MsgUser[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Length = (short)sizeof(MsgUser);
            ptr.Id = PacketType.MsgUser;
            ptr.UniqueId = uniqueId;
            ptr.ServerId = serverId;
            ptr.Online = online;
            ptr.SetNickname(nickname);
            ptr.SetAvatarUrl(avatarUrl);

            return ptr;
        }

        public static MsgUser Create(int uniqueId, string nickname, string avatarUrl, string email, bool online) => Create(uniqueId, 0, nickname, avatarUrl, email, online);

        public static implicit operator byte[](MsgUser msg)
        {
            Span<byte> buffer = stackalloc byte[sizeof(MsgUser)];
            fixed (byte* p = buffer)
                *(MsgUser*)p = *&msg;
            return buffer.ToArray();
        }
        public static implicit operator MsgUser(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgUser*)p;
        }
    }
}