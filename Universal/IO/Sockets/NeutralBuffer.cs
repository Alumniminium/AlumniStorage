using System;
using System.IO;
using System.IO.Compression;
using Universal.Extensions;

namespace Universal.IO.Sockets
{
    public class NeutralBuffer
    {
        internal readonly byte[] ReceiveBuffer;
        internal readonly byte[] SendBuffer;
        internal readonly byte[] MergeBuffer;

        internal int BytesInBuffer { get; set; }
        internal int BytesRequired { get; set; }
        internal int BytesProcessed { get; set; }
        private const int HEADER_SIZE=4;
        public NeutralBuffer(int bufferSize = 1_000_000)
        {
            ReceiveBuffer = new byte[bufferSize];
            SendBuffer = new byte[bufferSize];
            MergeBuffer = new byte[bufferSize];
        }
        internal void Decompress()
        {
            var chunk = MergeBuffer.AsSpan().Slice(4,BytesRequired);
            var decompressed = QuickLZ.decompress(chunk);
            Array.Copy(decompressed,0,MergeBuffer,0,decompressed.Length);
        }

        internal int Compress(int size)
        {
            var compressedChunk = QuickLZ.compress(SendBuffer,size,3);
            var sizeBytes = BitConverter.GetBytes(compressedChunk.Length +4);
            Array.Copy(sizeBytes,0,SendBuffer,0,sizeBytes.Length);
            Array.Copy(compressedChunk,0,SendBuffer,4,compressedChunk.Length);
            return compressedChunk.Length +4;
        }        
    }
}