using System.IO;
using System.IO.Compression;

namespace Universal.IO.Sockets
{
    public class NeutralBuffer
    {
        public readonly MemoryStream ReceiveMemoryStream;
        public readonly MemoryStream SendMemoryStream;
        public readonly DeflateStream ReceiveDeflateStream;
        public readonly DeflateStream SendDeflateStream;
        public readonly byte[] ReceiveBuffer;
        public readonly byte[] SendBuffer;
        public readonly byte[] MergeBuffer;

        public int BytesInBuffer { get; set; }
        public int BytesRequired { get; set; }
        public int BytesProcessed { get; set; }

        public NeutralBuffer(int receiveBufferSize = 1_000_000, int sendBufferSize = 1_000_000)
        {
            ReceiveBuffer = new byte[receiveBufferSize];
            SendBuffer = new byte[sendBufferSize];
            MergeBuffer = new byte[receiveBufferSize];
            ReceiveMemoryStream = new MemoryStream(ReceiveBuffer);
            SendMemoryStream = new MemoryStream(SendBuffer);
            ReceiveDeflateStream = new DeflateStream(ReceiveMemoryStream, CompressionMode.Decompress);
            SendDeflateStream = new DeflateStream(SendMemoryStream, CompressionMode.Compress);
        }
    }
}