using System.IO;
using System.IO.Compression;

namespace Universal.IO.Sockets
{
    public class NeutralBuffer
    {
        public readonly byte[] ReceiveBuffer;
        public readonly byte[] SendBuffer;
        public readonly byte[] MergeBuffer;

        public int BytesInBuffer { get; set; }
        public int BytesRequired { get; set; }
        public int BytesProcessed { get; set; }

        public NeutralBuffer(int receiveBufferSize = 4_000_000, int sendBufferSize = 4_000_000)
        {
            ReceiveBuffer = new byte[receiveBufferSize];
            SendBuffer = new byte[sendBufferSize];
            MergeBuffer = new byte[receiveBufferSize];
        }
    }
}