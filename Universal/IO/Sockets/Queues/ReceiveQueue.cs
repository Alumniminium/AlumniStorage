using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using Universal.IO.Sockets.Client;
using System.Buffers;

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
            while (connection.Buffer.BytesProcessed != e.BytesTransferred)
            {
                if (connection.Buffer.BytesInBuffer == 0)
                    StartNewPacket(e, connection);
                if (connection.Buffer.BytesInBuffer > 0)
                    ReadHeader(e, connection);

                Merge(connection, e);

                if (connection.Buffer.BytesInBuffer == connection.Buffer.BytesRequired && connection.Buffer.BytesRequired > 4)
                    FinishPacket(connection);
            }
            connection.Buffer.BytesProcessed = 0;
        }

        private static void StartNewPacket(SocketAsyncEventArgs e, ClientSocket connection)
        {
            var bytesLeft = e.BytesTransferred - connection.Buffer.BytesProcessed;
            if (bytesLeft >= MIN_HEADER_SIZE)
                connection.Buffer.BytesRequired = BitConverter.ToInt32(e.Buffer, connection.Buffer.BytesProcessed);
        }

        private static void ReadHeader(SocketAsyncEventArgs e, ClientSocket connection)
        {
            if (connection.Buffer.BytesInBuffer < MIN_HEADER_SIZE)
                Merge(connection, e);
            else
                connection.Buffer.BytesRequired = BitConverter.ToInt32(connection.Buffer.MergeBuffer, 0);
        }
        private static unsafe void Merge(ClientSocket connection, SocketAsyncEventArgs e)
        {
            var _count = Math.Min(e.BytesTransferred - connection.Buffer.BytesProcessed, connection.Buffer.BytesRequired - connection.Buffer.BytesInBuffer);
            var _destOffset = connection.Buffer.BytesInBuffer;
            var _recOffset = connection.Buffer.BytesProcessed;
            var sourceSlice = e.Buffer.AsSpan().Slice(_recOffset, _count);
            var destinationSlice = connection.Buffer.MergeBuffer.AsSpan().Slice(_destOffset);

            sourceSlice.CopyTo(destinationSlice);

            connection.Buffer.BytesInBuffer += _count;
            connection.Buffer.BytesProcessed += _count;
        }
        private static void FinishPacket(ClientSocket connection)
        {
            if (connection.Buffer.MergeBuffer[COMPRESSION_FLAG_OFFSET] == 1)
                connection.Buffer.Decompress();

            var packet = ArrayPool<byte>.Shared.Rent(connection.Buffer.BytesRequired);
            connection.Buffer.MergeBuffer.AsSpan().Slice(0, connection.Buffer.BytesRequired).CopyTo(packet);
            OnPacket?.Invoke(connection, connection.Buffer.MergeBuffer);
            ArrayPool<byte>.Shared.Return(packet);

            connection.Buffer.BytesInBuffer = 0;
        }
    }
}