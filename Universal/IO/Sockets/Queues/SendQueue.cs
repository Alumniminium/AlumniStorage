using Universal.IO.Sockets.Client;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System;

namespace Universal.IO.Sockets.Queues
{

    public static class SendQueue
    {
        private static readonly BlockingCollection<(SocketAsyncEventArgs, byte[] packet, bool dontCompress)> Queue = new BlockingCollection<(SocketAsyncEventArgs, byte[] packet, bool dontCompress)>();
        private static Thread _workerThread;

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
                connection.SendSync.WaitOne();

                var packet = e.packet;
                var size = packet.Length;
                if (connection.UseCompression && !e.dontCompress)
                    size = Compress(ref packet);

                e.Item1.SetBuffer(packet, 0, size);
                if (!connection.Socket.SendAsync(e.Item1))
                    connection.SendSync.Set();
            }
        }
        private static int Compress(ref byte[] packet)
        {
            using (var ms = new MemoryStream())
            using (var cp = new DeflateStream(ms, CompressionMode.Compress))
            {
                ms.Seek(2, SeekOrigin.Begin);
                cp.Write(packet);
                cp.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes((short)ms.Length));
                packet = ms.ToArray();
                return packet.Length;
            }
        }
    }
}
