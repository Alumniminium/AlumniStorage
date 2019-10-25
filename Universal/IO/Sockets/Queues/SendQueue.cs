using System.Linq;
using System.Buffers;
using System.Reflection.Emit;
using Universal.IO.Sockets.Client;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Threading.Channels;

namespace Universal.IO.Sockets.Queues
{
    public static class SendQueue
    {
        public class SendQueueItem
        {
            public SocketAsyncEventArgs Args;
            public byte[] Packet;
            public int Size => BitConverter.ToInt32(Packet, 0);
            public SendQueueItem(SocketAsyncEventArgs args, byte[] packet)
            {
                Args = args;
                Packet = packet;
            }
        }
        private static Thread _workerThread;
        private static int COMPRESSION_FLAG_OFFSET = 4;
        private static ChannelWriter<SendQueueItem> _writer;
        private static ChannelReader<SendQueueItem> _reader;
        static SendQueue()
        {
            var channel = Channel.CreateUnbounded<SendQueueItem>(new UnboundedChannelOptions() { SingleReader = true });
            _reader = channel.Reader;
            _writer = channel.Writer;
            _workerThread = new Thread(WorkLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            _workerThread.Start();
        }
        public static void Add(SocketAsyncEventArgs e, byte[] packet) => _writer.TryWrite(new SendQueueItem(e, packet));
        public static async void WorkLoop()
        {
            while (await _reader.WaitToReadAsync())
            {
                while (_reader.TryRead(out var item))
                {
                    var connection = (ClientSocket)item.Args.UserToken;
                    var packet = item.Packet;
                    var size = item.Size;

                    connection.SendSync.WaitOne();

                    packet.AsSpan().Slice(0,Math.Min(packet.Length,size)).CopyTo(connection.Buffer.SendBuffer);
                    ArrayPool<byte>.Shared.Return(packet);

                    if (connection.Buffer.SendBuffer[COMPRESSION_FLAG_OFFSET] == 1)
                        size = connection.Buffer.Compress(size);

                    item.Args.SetBuffer(connection.Buffer.SendBuffer, 0, size);
                    if (!connection.Socket.SendAsync(item.Args))
                        connection.SendSync.Set();
                }
            }
        }
    }
}
