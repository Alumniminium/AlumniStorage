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
            var decompressed = QuickLZ.decompress(chunk.ToArray());
            Array.Copy(decompressed,0,MergeBuffer,0,decompressed.Length);
        }

        internal int Compress(int size)
        {
            var chunk = SendBuffer.AsSpan().Slice(0,size);
            var compressedChunk = QuickLZ.compress(chunk.ToArray(),3);
            var packet = new byte[compressedChunk.Length +4];
            var sizeBytes = BitConverter.GetBytes(packet.Length);
            Array.Copy(sizeBytes,0,packet,0,sizeBytes.Length);
            Array.Copy(compressedChunk,0,packet,4,compressedChunk.Length);
            Array.Copy(packet,0,SendBuffer,0,packet.Length);
            return packet.Length;
        }        
    }
}