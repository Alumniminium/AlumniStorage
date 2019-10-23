using System;
using System.IO;
using System.IO.Compression;
using Universal.Extensions;
using Universal.Packets;

namespace Universal.IO.Sockets
{
    public unsafe class NeutralBuffer
    {
        internal readonly byte[] ReceiveBuffer;
        internal byte[] SendBuffer;
        internal readonly byte[] MergeBuffer;

        internal int BytesInBuffer { get; set; }
        internal int BytesRequired { get; set; }
        internal int BytesProcessed { get; set; }
        public NeutralBuffer(int bufferSize = 1_000)
        {
            ReceiveBuffer = new byte[bufferSize];
            SendBuffer = new byte[bufferSize];
            MergeBuffer = new byte[bufferSize];
        }
        internal void Decompress()
        {
            var chunk = MergeBuffer.AsSpan().Slice(MsgHeader.SIZE, BytesRequired - MsgHeader.SIZE);
            var decompressed = QuickLZ.decompress(chunk);
            Buffer.BlockCopy(chunk.Slice(0, 4).ToArray(), 0, MergeBuffer, 0, 4);
            Buffer.BlockCopy(decompressed, 0, MergeBuffer, MsgHeader.SIZE, decompressed.Length);
        }

        internal int Compress(int size)
        {
            var compressedChunk = QuickLZ.compress(SendBuffer.AsSpan(MsgHeader.SIZE, size - MsgHeader.SIZE).ToArray(), size - MsgHeader.SIZE, 3);
            var sizeBytes = BitConverter.GetBytes(compressedChunk.Length + MsgHeader.SIZE);
            Buffer.BlockCopy(sizeBytes, 0, SendBuffer, 0, sizeBytes.Length);
            Buffer.BlockCopy(compressedChunk, 0, SendBuffer, MsgHeader.SIZE, compressedChunk.Length);
            return compressedChunk.Length + MsgHeader.SIZE;
        }
    }
}