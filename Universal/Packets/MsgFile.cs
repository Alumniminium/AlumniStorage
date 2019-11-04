using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgFile
    {
        public const int TOKEN_LENGTH = 32;
        public const int MAX_CHUNK_SIZE = 65400;

        public MsgHeader Header;
        public bool CreateFile;
        public long FileSize;
        public int ChunkSize;
        public fixed char Token[TOKEN_LENGTH];
        public fixed byte Chunk[MAX_CHUNK_SIZE];

        public string GetToken()
        {
            fixed (char* p = Token)
                return new string(p, 0, TOKEN_LENGTH);
        }
        public void SetToken(string token)
        {
            token = token.ToLength(TOKEN_LENGTH);
            for (var i = 0; i < token.Length; i++)
                Token[i] = token[i];
        }
        public byte[] GetChunk()
        {
            var buffer = new byte[ChunkSize];
            for (var i = 0; i < ChunkSize; i++)
                buffer[i] = Chunk[i];
            return buffer;
        }
        public void SetChunk(byte[] chunk)
        {
            fixed (byte* p = Chunk)
                Unsafe.WriteUnaligned(p, chunk);
        }

        public static MsgFile Create(string token, long size, int chunkSize, byte[] chunk, bool create)
        {
            var ptr = stackalloc MsgFile[1];

            ptr->Header.Length = sizeof(MsgFile);
            ptr->Header.Compressed = false;
            ptr->Header.Id = PacketType.MsgFile;

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
            MemoryMarshal.Write(buffer, ref msg);
            return buffer;
        }
        public static implicit operator MsgFile(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgFile*)p;
        }
    }
}
