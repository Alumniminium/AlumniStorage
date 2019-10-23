using System;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgFile
    {
        public const int MAX_NAME_LENGTH = 255;
        public const int MAX_CHUNK_SIZE = 1024 * 1024;
        public MsgHeader Header { get; set; }
        public fixed char FileName[MAX_NAME_LENGTH];
        public bool CreateFile;
        public long Size;
        public int ChunkSize;
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
        public byte[] GetChunk()
        {
            byte[] chunk = new byte[ChunkSize];
            for(int i = 0; i<chunk.Length;i++)
                chunk[i]=Chunk[i];
            return chunk;
        }
        public void SetChunk(byte[] chunk)
        {
            for (var i = 0; i < ChunkSize; i++)
                Chunk[i] = chunk[i];
        }

        public static MsgFile Create(string fileName, long size, int chunkSize, byte[] chunk, bool create)
        {
            Span<MsgFile> span = stackalloc MsgFile[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Header = new MsgHeader
            {
                Length = sizeof(MsgFile),
                Compressed = false,
                Id = PacketType.MsgFile,
            };
            ptr.SetFileName(fileName);
            ptr.SetChunk(chunk);
            ptr.Size = size;
            ptr.ChunkSize = chunkSize;
            ptr.CreateFile=create;
            return ptr;
        }
        public static implicit operator byte[](MsgFile msg)
        {
            Span<byte> buffer = stackalloc byte[sizeof(MsgFile)];
            fixed (byte* p = buffer)
                *(MsgFile*)p = *&msg;
            return buffer.ToArray();
        }
        public static implicit operator MsgFile(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgFile*)p;
        }
    }
}
