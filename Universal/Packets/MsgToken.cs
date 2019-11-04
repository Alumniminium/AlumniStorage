using System.Buffers;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgToken
    {
        public const int MAX_TOKEN_LENGTH = 32;

        public MsgHeader Header;
        public int UniqueId { get; set; }
        public fixed char Token[MAX_TOKEN_LENGTH];

        public string GetToken
        {
            get
            {
                fixed (char* p = Token)
                    return new string(p);
            }
        }

        public void SetToken(string token)
        {
            token = token.ToLength(MAX_TOKEN_LENGTH);
            for (var i = 0; i < token.Length; i++)
                Token[i] = token[i];
        }

        public static MsgToken Create(string token, int uniqueId, bool compression)
        {
            var ptr = stackalloc MsgToken[1];
            ptr->Header.Length = sizeof(MsgToken);
            ptr->Header.Compressed = compression;
            ptr->Header.Id = PacketType.MsgToken;
            ptr->UniqueId = uniqueId;
            ptr->SetToken(token);

            return *ptr;
        }
        public static implicit operator byte[](MsgToken msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgToken));
            MemoryMarshal.Write(buffer, ref msg);
            return buffer;
        }
        public static implicit operator MsgToken(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgToken*)p;
        }
    }
}
