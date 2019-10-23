using Universal.IO.Sockets.Client;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System;
using Universal.Extensions;

namespace Universal.IO.Sockets.Queues
{

    public static class SendQueue
    {
        private static readonly BlockingCollection<(SocketAsyncEventArgs, byte[] packet, bool dontCompress)> Queue = new BlockingCollection<(SocketAsyncEventArgs, byte[] packet, bool dontCompress)>();
        private static Thread _workerThread;
        private static int COMPRESSION_FLAG_OFFSET = 4;

        static SendQueue()
        {
            _workerThread = new Thread(WorkLoop) { IsBackground = true };
            _workerThread.Start();
        }

        public static void Add(SocketAsyncEventArgs e, byte[] packet, bool dontCompress) => Queue.Add((e, packet, dontCompress));

        private static void WorkLoop()
        {
            foreach (var e in Queue.GetConsumingEnumerable())
            {
                var connection = (ClientSocket)e.Item1.UserToken;
                var packet = e.packet;
                var size = packet.Length;

                connection.SendSync.WaitOne();
                
                packet.VectorizedCopy(0, connection.Buffer.SendBuffer, 0, size);
                if (packet[COMPRESSION_FLAG_OFFSET] == 1)
                    size = connection.Buffer.Compress(size);


                e.Item1.SetBuffer(connection.Buffer.SendBuffer, 0, size);
                if (!connection.Socket.SendAsync(e.Item1))
                    connection.SendSync.Set();
            }
        }
    }
}
