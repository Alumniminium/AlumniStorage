using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Universal.IO.FastConsole;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Pools;
using Universal.IO.Sockets.Queues;

namespace Universal.IO.Sockets.Server
{

    public static class ServerSocket
    {
        internal static Socket Socket;
        internal static SocketAsyncEventArgs AcceptArgs;
        internal static readonly AutoResetEvent AcceptSync = new AutoResetEvent(true);

        public static void Start(ushort port, int bufferSize= 500_500)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
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
            ((ClientSocket)connection.ReceiveArgs.UserToken).Socket = e.AcceptSocket;

            try
            {
                if (!e.AcceptSocket.ReceiveAsync(connection.ReceiveArgs))
                    Received(null, connection.ReceiveArgs);
            }
            catch (Exception exception)
            {
                FConsole.WriteLine(exception);
                CloseClientSocket(e);
                AcceptSync.Set();
            }
            e.AcceptSocket = null;
            e.UserToken = new ClientSocket();
            AcceptSync.Set();
            StartAccepting();
        }

        internal static void Received(object sender, SocketAsyncEventArgs e)
        {
            var client = (ClientSocket)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                try
                {
                    ReceiveQueue.Add(e);
                    client.ReceiveSync.WaitOne();
                    if (!client.Socket.ReceiveAsync(e))
                        Received(null, e);
                }
                catch (Exception exception)
                {
                    FConsole.WriteLine(exception);
                    CloseClientSocket(e);
                }
            }
            else
                CloseClientSocket(e);
        }
        private static void CloseClientSocket(SocketAsyncEventArgs e)
        {
            var token = (ClientSocket)e.UserToken;
            token?.Socket?.Dispose();
        }
    }
}