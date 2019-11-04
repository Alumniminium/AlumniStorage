using Universal.IO.Sockets.Client;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Threading.Channels;

namespace Universal.IO.Sockets.Queues
{
    public static class SendQueue
    {
        private const int COMPRESSION_FLAG_OFFSET = 4;
        private static readonly Thread WorkerThread;
        private static readonly ChannelWriter<SendQueueItem> Writer;
        private static readonly ChannelReader<SendQueueItem> Reader;
        static SendQueue()
        {
            var channel = Channel.CreateUnbounded<SendQueueItem>(new UnboundedChannelOptions() { SingleReader = true });
            Reader = channel.Reader;
            Writer = channel.Writer;
            WorkerThread = new Thread(WorkLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            WorkerThread.Start();
        }
        public static void Add(SocketAsyncEventArgs e, byte[] packet, int size) => Writer.TryWrite(new SendQueueItem(e, packet, size));
        public static async void WorkLoop()
        {
            while (await Reader.WaitToReadAsync())
            {
                while (Reader.TryRead(out var item))
                {
                    var connection = (ClientSocket)item.Args.UserToken;
                    var packet = item.Packet;
                    var size = item.Size;

                    packet.AsSpan().Slice(0, item.Size).CopyTo(connection.Buffer.SendBuffer);
                    //ArrayPool<byte>.Shared.Return(packet);

                    if (connection.Buffer.SendBuffer[COMPRESSION_FLAG_OFFSET] == 1)
                        size = connection.Buffer.Compress(size);

                    item.Args.SetBuffer(connection.Buffer.SendBuffer, 0, size);

                    if (!connection.Socket.SendAsync(item.Args))
                        connection.Completed(null, item.Args);
                }
            }
        }
    }
}
