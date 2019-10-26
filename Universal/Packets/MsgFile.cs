using System.Buffers;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct MsgFile
    {
        public const int TOKEN_LENGTH = 32;
        public const int MAX_CHUNK_SIZE = 500_000;
        [FieldOffset(0)]
        public int Length;
        [FieldOffset(4)]
        public bool Compressed;
        [FieldOffset(5)]
        public PacketType Id;
        [FieldOffset(6)]
        public bool CreateFile;
        [FieldOffset(7)]
        public long FileSize;
        [FieldOffset(15)]
        public int ChunkSize;
        [FieldOffset(19)]
        public fixed char Token[TOKEN_LENGTH];
        [FieldOffset(83)]
        public fixed byte Chunk[MAX_CHUNK_SIZE];

        public string GetToken()
        {
            fixed (char* p = Token)
                return new string(p,0,TOKEN_LENGTH);
        }
        public void SetToken(string token)
        {
            token = token.ToLength(TOKEN_LENGTH);
            for (var i = 0; i < token.Length; i++)
                Token[i] = token[i];
        }
        public byte[] GetChunk()
        {
            var buffer = ArrayPool<byte>.Shared.Rent(ChunkSize);
            fixed (byte* pSource = Chunk, pTarget = buffer)
            {
                for (int i = 0; i < ChunkSize; i++)
                    pTarget[i] = pSource[i];
            }
            return buffer;
        }
        public void SetChunk(byte[] chunk)
        {    
            fixed (byte* pSource = chunk, pTarget = Chunk)
            {
                for (int i = 0; i < chunk.Length; i++)
                    pTarget[i] = pSource[i];
            }
        }

        public static MsgFile Create(string token, long size, int chunkSize, byte[] chunk, bool create)
        {
            MsgFile* ptr = stackalloc MsgFile[1];
            
            ptr->Length = sizeof(MsgFile);
            ptr->Compressed = false;
            ptr->Id = PacketType.MsgFile;

            ptr->FileSize = size;
            ptr->ChunkSize = chunkSize;
            ptr->CreateFile = create;

            ptr->SetToken(token);
            ptr->SetChunk(chunk);

            return *ptr;
        }
        public static implicit operator byte[](MsgFile msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgFile));
            fixed (byte* p = buffer)
                *(MsgFile*)p = *&msg;
            return buffer;
        }
        public static implicit operator MsgFile(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgFile*)p;
        }
    }
}
