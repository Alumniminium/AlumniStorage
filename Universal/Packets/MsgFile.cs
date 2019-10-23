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
        public const int MAX_NAME_LENGTH = 128;
        public const int MAX_CHUNK_SIZE = 500_000;
        public MsgHeader Header;
        public bool CreateFile;
        public long FileSize;
        public int ChunkSize;
        public fixed char FileName[MAX_NAME_LENGTH];
        public fixed byte Chunk[MAX_CHUNK_SIZE];

        public string GetFileName()
        {
            fixed (char* p = FileName)
                return new string(p);
        }
        public void SetFileName(string fileName)
        {
            fileName = fileName.ToLength(MAX_NAME_LENGTH);
            for (var i = 0; i < fileName.Length; i++)
                FileName[i] = fileName[i];
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

        public static MsgFile Create(string fileName, long size, int chunkSize, byte[] chunk, bool create)
        {
            Span<MsgFile> span = stackalloc MsgFile[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Header = new MsgHeader
            {
                Length = sizeof(MsgFile),
                Compressed = true,
                Id = PacketType.MsgFile,
            };
            ptr.SetFileName(fileName);
            ptr.SetChunk(chunk);
            ptr.FileSize = size;
            ptr.ChunkSize = chunkSize;
            ptr.CreateFile = create;
            return ptr;
        }
        public static implicit operator byte[](MsgFile msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(msg.Header.Length);
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
