using System.Buffers;
using System;

namespace Universal.IO.Sockets
{
    public class NeutralBuffer
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
            var chunk = MergeBuffer.AsSpan().Slice(6, BytesRequired - 6);
            var decompressed = QuickLz.Decompress(chunk);
            var sizeBytes = BitConverter.GetBytes(decompressed.Length + 6);
            sizeBytes.AsSpan().CopyTo(MergeBuffer);
            decompressed.AsSpan().CopyTo(MergeBuffer.AsSpan().Slice(6));
        }

        internal int Compress(int size)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(400 + size - 6);
            var compressedChunk = QuickLz.Compress(SendBuffer.AsSpan(6, size - 6).ToArray(), size - 6, 3, buffer);
            var compressedSize = compressedChunk.Length;
            var sizeBytes = BitConverter.GetBytes(compressedSize + 6);

            sizeBytes.AsSpan().CopyTo(SendBuffer);
            compressedChunk.AsSpan().CopyTo(SendBuffer.AsSpan().Slice(6));

            //ArrayPool<byte>.Shared.Return(compressedChunk);
            ArrayPool<byte>.Shared.Return(buffer);
            return compressedSize + 6;
        }
    }
}