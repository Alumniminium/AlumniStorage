using System.Buffers;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgFile
    {
        public const int TOKEN_LENGTH = 33;
        public const int MAX_CHUNK_SIZE = 500_000;
        public int Length{get;set;}
        public bool Compressed{get;set;}
        public PacketType Id{get;set;}
        public bool CreateFile;
        public long FileSize;
        public int ChunkSize;
        public fixed char Token[TOKEN_LENGTH];
        public fixed byte Chunk[MAX_CHUNK_SIZE];

        public string GetToken()
        {
            fixed (char* p = Token)
                return new string(p);
        }
        public void SetToken(string token)
        {
            token = token.ToLength(TOKEN_LENGTH);
            for (var i = 0; i < token.Length; i++)
                Token[i] = token[i];
        }
        public Span<byte> GetChunk()
        {
            fixed (byte* b = Chunk)
                return new Span<byte>(b, ChunkSize);
        }
        public void SetChunk(byte[] chunk)
        {
            fixed (byte* b = Chunk)
                Unsafe.Copy(b, ref chunk);
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
            MemoryMarshal.Write<MsgFile>(buffer,ref msg);
            return buffer;
        }
        public static implicit operator MsgFile(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgFile*)p;
        }
    }
}
