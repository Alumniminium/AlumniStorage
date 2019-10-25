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
            using (var ms = new MemoryStream(MergeBuffer))
            using (var bs = new BrotliStream(ms, CompressionMode.Decompress))
            {
                ms.Seek(sizeof(MsgHeader),SeekOrigin.Begin);
                bs.Read(MergeBuffer,sizeof(MsgHeader),BytesRequired-sizeof(MsgHeader));
            }
        }
        internal int Compress(int size)
        {
            using (var ms = new MemoryStream(SendBuffer))
            using (var bs = new BrotliStream(ms, CompressionMode.Compress))
            {
                ms.Seek(sizeof(MsgHeader),SeekOrigin.Begin);
                bs.Write(SendBuffer,sizeof(MsgHeader),size-sizeof(MsgHeader));
                bs.Flush();                
                size=(int)ms.Position+1;
            }
            
            Span<byte> header = BitConverter.GetBytes(size);
            header.CopyTo(SendBuffer.AsSpan().Slice(0,4));
            return size;
        }
    }
}