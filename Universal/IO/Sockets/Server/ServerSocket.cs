using System.Net;
using System.Net.Sockets;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Pools;

namespace Universal.IO.Sockets.Server
{

    public static class ServerSocket
    {
        internal static Socket Socket;
        public static int BufferSize { get; internal set; }
        public static void Start(ushort port, int bufferSize = 500_500)
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
            e.UserToken = new ClientSocket(BufferSize);
            ((ClientSocket)e.UserToken).Socket = e.AcceptSocket;
            ((ClientSocket)e.UserToken).Receive();

            e.AcceptSocket = null;
            e.Completed -= Accepted;
            e.UserToken = null;
            SaeaPool.Return(e);
            StartAccepting();
        }
    }
}