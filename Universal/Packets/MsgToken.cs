using System;
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
        public const int MAX_PATH_LENGTH = 256;
        
        public int Length{get;set;}
        public PacketType Id{get;set;}
        public bool Compressed{get;set;}
        public MsgTokenType Type { get; set; }
        public fixed char Token[MAX_TOKEN_LENGTH];
        public fixed char Path[MAX_PATH_LENGTH];

        public string GetToken()
        {
            fixed (char* p = Token)
                return new string(p);
        }

        public void SetToken(string token)
        {
            token = token.ToLength(MAX_TOKEN_LENGTH);
            for (var i = 0; i < token.Length; i++)
                Token[i] = token[i];
        }
        public string GetPath()
        {
            fixed (char* p = Path)
                return new string(p);
        }

        public void SetPath(string path)
        {
            path = path.ToLength(MAX_PATH_LENGTH);
            for (var i = 0; i < path.Length; i++)
                Path[i] = path[i];
        }
        public static MsgToken Create(string token, string path, bool compression, MsgTokenType type)
        {
            MsgToken* ptr = stackalloc MsgToken[1];
            ptr->Length = sizeof(MsgToken);
            ptr->Compressed = compression;
            ptr->Id = PacketType.MsgToken;
            ptr->Type = type;
            ptr->SetToken(token);
            ptr->SetPath(path);

            return *ptr;
        }
        public static implicit operator byte[](MsgToken msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgToken));
            MemoryMarshal.Write<MsgToken>(buffer, ref msg);
            return buffer;
        }
        public static implicit operator MsgToken(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgToken*)p;
        }
    }
}
