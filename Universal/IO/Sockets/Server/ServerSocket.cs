using System.Net;
using System.Net.Sockets;
using System.Threading;
using Universal.IO.Sockets.Client;

namespace Universal.IO.Sockets.Server
{

    public static class ServerSocket
    {
        internal static Socket Socket;
        internal static SocketAsyncEventArgs AcceptArgs;
        internal static readonly AutoResetEvent AcceptSync = new AutoResetEvent(true);

        public static void Start(ushort port, int bufferSize = 500_500)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false,
                UseOnlyOverlappedIO = true
            };
            Socket.Bind(new IPEndPoint(IPAddress.Any, port));
            Socket.Listen(100);

            AcceptArgs = new SocketAsyncEventArgs();
            AcceptArgs.Completed += Accepted;
            AcceptArgs.UserToken = new ClientSocket(bufferSize);
            StartAccepting();
        }

        private static void StartAccepting()
        {
            AcceptSync.WaitOne();
            if (!Socket.AcceptAsync(AcceptArgs))
                Accepted(null, AcceptArgs);
        }

        private static void Accepted(object sender, SocketAsyncEventArgs e)
        {
            var connection = (ClientSocket)e.UserToken;
            var args = connection.GetReceiveArgs();
            ((ClientSocket)args.UserToken).Socket = e.AcceptSocket;
            connection.Socket.ReceiveAsync(args);
            e.AcceptSocket = null;
            e.UserToken = new ClientSocket();
            AcceptSync.Set();
            StartAccepting();
        }
        private static void CloseClientSocket(SocketAsyncEventArgs e)
        {
            var token = (ClientSocket)e.UserToken;
            token?.Socket?.Dispose();
        }
    }
}