using System;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgVServer
    {
        public const int MAX_SERVER_NAME_LENGTH = 32;
        public const int MAX_SERVER_ICON_URL_LENGTH = 128;
        public short Length { get; private set; }
        public PacketType Id { get; private set; }
        public int UniqueId { get; set; }
        public long Created { get; set; }
        public long LastActivity { get; set; }

        public fixed char ServerName[MAX_SERVER_NAME_LENGTH];
        public fixed char ServerIconUrl[MAX_SERVER_ICON_URL_LENGTH];

        public string GetServerName()
        {
            fixed (char* p = ServerName)
                return new string(p);
        }
        public string GetServerIconUrl()
        {
            fixed (char* p = ServerIconUrl)
                return new string(p);
        }

        public void SetServerName(string serverName)
        {
            serverName = serverName.ToLength(MAX_SERVER_NAME_LENGTH);
            for (var i = 0; i < serverName.Length; i++)
                ServerName[i] = serverName[i];
        }

        public void SetServerIconUrl(string url)
        {
            url = url.ToLength(MAX_SERVER_ICON_URL_LENGTH);
            for (var i = 0; i < url.Length; i++)
                ServerIconUrl[i] = url[i];
        }
        public static MsgVServer Create(int serverId, string name, string url, DateTime created, DateTime lastActivity)
        {
            Span<MsgVServer> span = stackalloc MsgVServer[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Length = (short)sizeof(MsgVServer);
            ptr.Id = PacketType.MsgVServer;
            ptr.UniqueId = serverId;
            ptr.Created = created.Ticks;
            ptr.LastActivity = lastActivity.Ticks;
            ptr.SetServerName(name);
            ptr.SetServerIconUrl(url);
            return ptr;
        }

        public static implicit operator byte[](MsgVServer msg)
        {
            Span<byte> buffer = stackalloc byte[sizeof(MsgVServer)];
            fixed (byte* p = buffer)
                *(MsgVServer*)p = *&msg;
            return buffer.ToArray();
        }
        public static implicit operator MsgVServer(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgVServer*)p;
        }
    }
}
