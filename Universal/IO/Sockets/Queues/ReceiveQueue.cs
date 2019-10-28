using System.Linq;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Pools;
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
                    var clientSocket = (ClientSocket)e.UserToken;
                    AssemblePacket(clientSocket, e);
                    clientSocket.RecycleArgs(e);
                    clientSocket.Receive();
                }
            }
        }
        private static void AssemblePacket(ClientSocket connection, SocketAsyncEventArgs e)
        {
            while (true)
            {
                if (connection.Buffer.BytesInBuffer == 0)
                    StartNewPacket(e, connection);
                if (connection.Buffer.BytesInBuffer > 0)
                    ReadHeader(e, connection);

                MergeUnsafe(connection, e);

                if (connection.Buffer.BytesInBuffer == connection.Buffer.BytesRequired && connection.Buffer.BytesRequired > 4)
                    FinishPacket(connection);
                if (connection.Buffer.BytesProcessed != e.BytesTransferred)
                    continue;

                break;
            }
            connection.Buffer.BytesProcessed = 0;
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
                MergeUnsafe(connection, e);
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
        private static unsafe void MergeUnsafe(ClientSocket connection, SocketAsyncEventArgs e)
        {
            var _count = e.BytesTransferred - connection.Buffer.BytesProcessed;
            var _destOffset = connection.Buffer.BytesInBuffer;
            var _recOffset = connection.Buffer.BytesProcessed;

            e.Buffer.AsSpan().Slice(_recOffset, _count).CopyTo(connection.Buffer.MergeBuffer.AsSpan().Slice(_destOffset));
            connection.Buffer.BytesInBuffer += _count;
            connection.Buffer.BytesProcessed += _count;
        }
    }
}