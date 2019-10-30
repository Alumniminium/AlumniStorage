using System;
using System.Net;
using System.Net.Sockets;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Pools;

namespace Universal.IO.Sockets.Server
{
    public static class ServerSocket
    {
        public static Action<ClientSocket, byte[]> OnPacket;
        internal static Socket Socket;
        public static int BufferSize { get; internal set; }
        public static void Start(ushort port, int bufferSize = ushort.MaxValue)
        {
            BufferSize = bufferSize;

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false,
                UseOnlyOverlappedIO = true
            };
            Socket.Bind(new IPEndPoint(IPAddress.Any, port));
            Socket.Listen(100);

            StartAccepting();
        }

        private static void StartAccepting()
        {
            var acceptArgs = SaeaPool.Get();
            acceptArgs.Completed += Accepted;
            if (!Socket.AcceptAsync(acceptArgs))
                Accepted(null, acceptArgs);
        }

        private static void Accepted(object sender, SocketAsyncEventArgs e)
        {
            var receiveArgs = SaeaPool.Get();
            receiveArgs.UserToken = new ClientSocket(BufferSize);
            ((ClientSocket)receiveArgs.UserToken).Socket = e.AcceptSocket;
            ((ClientSocket)receiveArgs.UserToken).IsConnected = true;
            ((ClientSocket)receiveArgs.UserToken).OnPacket += OnPacket;
            ((ClientSocket)receiveArgs.UserToken).Receive();

            e.Completed -= Accepted;
            e.AcceptSocket = null;
            SaeaPool.Return(e);
            StartAccepting();
        }
    }
}