using System.Buffers;
using System.Reflection.Emit;
using Universal.IO.Sockets.Client;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System;

namespace Universal.IO.Sockets.Queues
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

    public static class SendQueue
    {
        private static readonly BlockingCollection<SendQueueItem> Queue = new BlockingCollection<SendQueueItem>();
        private static Thread _workerThread;
        private static int COMPRESSION_FLAG_OFFSET = 4;

        static SendQueue()
        {
            _workerThread = new Thread(WorkLoop) { IsBackground = true };
            _workerThread.Start();
        }

        public static void Add(SocketAsyncEventArgs e, byte[] packet) => Queue.Add(new SendQueueItem(e, packet));

        private static void WorkLoop()
        {
            foreach (var item in Queue.GetConsumingEnumerable())
            {
                var connection = (ClientSocket)item.Args.UserToken;
                var packet = item.Packet;
                var size = item.Size;

                connection.SendSync.WaitOne();

                Buffer.BlockCopy(packet, 0, connection.Buffer.SendBuffer, 0, size);
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
