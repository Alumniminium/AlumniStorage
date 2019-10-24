using System.Buffers;
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

            for (int i = 0; i < 4; i++)
                MergeBuffer[i] = chunk[i];

            Buffer.BlockCopy(decompressed, 0, MergeBuffer, MsgHeader.SIZE, decompressed.Length);
        }

        internal int Compress(int size)
        {
            var compressedChunk = QuickLZ.compress(SendBuffer.AsSpan(MsgHeader.SIZE, size - MsgHeader.SIZE).ToArray(), size - MsgHeader.SIZE, 3, ArrayPool<byte>.Shared.Rent(400 + size - MsgHeader.SIZE).SelfSetToDefaults());
            var compressedSize = compressedChunk.Length;
            var sizeBytes = BitConverter.GetBytes(compressedSize + MsgHeader.SIZE);
            Buffer.BlockCopy(sizeBytes, 0, SendBuffer, 0, sizeBytes.Length);
            Buffer.BlockCopy(compressedChunk, 0, SendBuffer, MsgHeader.SIZE, compressedSize);
            ArrayPool<byte>.Shared.Return(compressedChunk);
            return compressedSize + MsgHeader.SIZE;
        }
    }
}