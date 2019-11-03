using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using Universal.IO.Sockets.Client;
using System.Buffers;
using Universal.IO.Sockets.Pools;

namespace Universal.IO.Sockets.Queues
{
    public static class ReceiveQueue
    {
        private static readonly Thread WorkerThread;
        private const int MIN_HEADER_SIZE = 4;
        private const int COMPRESSION_FLAG_OFFSET = 4;
        private static readonly ChannelWriter<SocketAsyncEventArgs> Writer;
        private static readonly ChannelReader<SocketAsyncEventArgs> Reader;

        static ReceiveQueue()
        {
            var channel = Channel.CreateUnbounded<SocketAsyncEventArgs>(new UnboundedChannelOptions() { SingleReader = true });
            Reader = channel.Reader;
            Writer = channel.Writer;
            WorkerThread = new Thread(WorkLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            WorkerThread.Start();
        }
        public static void Add(SocketAsyncEventArgs e) => Writer.TryWrite(e);
        public static async void WorkLoop()
        {
            while (await Reader.WaitToReadAsync())
            {
                while (Reader.TryRead(out var e))
                {
                    var clientSocket = (ClientSocket)e.UserToken;
                    AssemblePacket(clientSocket, e);

                    clientSocket.RecycleSaea(e);
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


                if (connection.Buffer.BytesProcessed == 0)
                    break;
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
        private static void Merge(ClientSocket connection, SocketAsyncEventArgs e)
        {
            var count = Math.Min(e.BytesTransferred - connection.Buffer.BytesProcessed, connection.Buffer.BytesRequired - connection.Buffer.BytesInBuffer);
            var destOffset = connection.Buffer.BytesInBuffer;
            var recOffset = connection.Buffer.BytesProcessed;
            var sourceSlice = e.Buffer.AsSpan().Slice(recOffset, count);
            var destinationSlice = connection.Buffer.MergeBuffer.AsSpan().Slice(destOffset);

            sourceSlice.CopyTo(destinationSlice);

            connection.Buffer.BytesInBuffer += count;
            connection.Buffer.BytesProcessed += count;
        }
        private static void FinishPacket(ClientSocket connection)
        {
            if (connection.Buffer.MergeBuffer[COMPRESSION_FLAG_OFFSET] == 1)
                connection.Buffer.Decompress();

            var packet = ArrayPool<byte>.Shared.Rent(connection.Buffer.BytesRequired);
            connection.Buffer.MergeBuffer.AsSpan().Slice(0, connection.Buffer.BytesRequired).CopyTo(packet);

            if (connection.Encryption)
            {
                connection.Crypto.IV = packet.AsSpan().Slice(connection.Buffer.BytesRequired - 4).ToArray();
                packet = connection.Crypto.CreateDecryptor().TransformFinalBlock(packet, 4, connection.Buffer.BytesRequired - 4);
            }
            connection.OnPacket?.Invoke(connection, packet);

            ArrayPool<byte>.Shared.Return(packet);

            connection.Buffer.BytesInBuffer = 0;
        }
        public static void Die()
        {
            WorkerThread.Join(1000);
        }
    }
}