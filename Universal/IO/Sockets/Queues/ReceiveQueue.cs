using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using Universal.IO.Sockets.Client;
using Universal.Packets;

namespace Universal.IO.Sockets.Queues
{
    public static class ReceiveQueue
    {
        private static Thread _workerThread;
        private unsafe static int MIN_HEADER_SIZE = 4;
        private static int COMPRESSION_FLAG_OFFSET = 4;
        public static Action<ClientSocket, byte[]> OnPacket;
        private static ChannelWriter<SocketAsyncEventArgs> _writer;
        private static ChannelReader<SocketAsyncEventArgs> _reader;

        static ReceiveQueue()
        {
            var channel = Channel.CreateUnbounded<SocketAsyncEventArgs>(new UnboundedChannelOptions() { SingleReader = true });
            _reader = channel.Reader;
            _writer = channel.Writer;
            _workerThread = new Thread(WorkLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            _workerThread.Start();
        }
        public static void Add(SocketAsyncEventArgs e) => _writer.TryWrite(e);
        public static async void WorkLoop()
        {
            while (await _reader.WaitToReadAsync())
            {
                while (_reader.TryRead(out var e))
                {
                    AssemblePacket(e);
                    ((ClientSocket)e.UserToken).Receive();
                }
            }
        }
        private static void AssemblePacket(SocketAsyncEventArgs e)
        {
            while (true)
            {
                var connection = (ClientSocket)e.UserToken;

                if (connection.Buffer.BytesInBuffer == 0)
                    StartNewPacket(e, connection);
                if (connection.Buffer.BytesInBuffer > 0)
                    ReadHeader(e, connection);

                MergeUnsafe(e);

                if (connection.Buffer.BytesInBuffer == connection.Buffer.BytesRequired && connection.Buffer.BytesRequired > 4)
                    FinishPacket(connection);
                if (connection.Buffer.BytesProcessed != e.BytesTransferred)
                    continue;

                connection.Buffer.BytesProcessed = 0;
                break;
            }
        }

        private static void StartNewPacket(SocketAsyncEventArgs e, ClientSocket connection)
        {
            var receivedBytes = e.BytesTransferred - connection.Buffer.BytesProcessed;
            if (receivedBytes >= MIN_HEADER_SIZE)
                connection.Buffer.BytesRequired = BitConverter.ToInt32(e.Buffer, connection.Buffer.BytesProcessed);
        }

        private static void ReadHeader(SocketAsyncEventArgs e, ClientSocket connection)
        {
            if (connection.Buffer.BytesInBuffer < MIN_HEADER_SIZE)
                MergeUnsafe(e, true);
            else
                connection.Buffer.BytesRequired = BitConverter.ToInt32(connection.Buffer.MergeBuffer, 0);
        }

        private static void FinishPacket(ClientSocket connection)
        {
            if (connection.Buffer.MergeBuffer[COMPRESSION_FLAG_OFFSET] == 1)
                connection.Buffer.Decompress();

            OnPacket?.Invoke(connection, connection.Buffer.MergeBuffer);
            connection.Buffer.BytesInBuffer = 0;
        }
        private static unsafe void MergeUnsafe(SocketAsyncEventArgs e, bool header = false)
        {
            var connection = (ClientSocket)e.UserToken;
            var _count = e.BytesTransferred - connection.Buffer.BytesProcessed;
            var _destOffset = connection.Buffer.BytesInBuffer;
            var _recOffset = connection.Buffer.BytesProcessed;

            fixed (byte* destination = connection.Buffer.MergeBuffer)
            fixed (byte* source = e.Buffer)
            {
                for (var i = 0; i < _count; i++)
                {
                    destination[i + _destOffset] = source[i + _recOffset];
                    connection.Buffer.BytesInBuffer++;
                    connection.Buffer.BytesProcessed++;

                    if (header && connection.Buffer.BytesInBuffer == MIN_HEADER_SIZE)
                        break;

                    if (!header && connection.Buffer.BytesInBuffer == connection.Buffer.BytesRequired)
                        break;
                }
            }
        }
    }
}