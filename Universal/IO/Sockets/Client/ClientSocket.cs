using System.Threading;
using System;
using System.Net;
using System.Net.Sockets;
using Universal.IO.FastConsole;
using Universal.IO.Sockets.Pools;
using Universal.IO.Sockets.Queues;
using System.Linq;

namespace Universal.IO.Sockets.Client
{
    public class ClientSocket
    {
        public Action OnConnected, OnDisconnect;
        internal Socket Socket;
        public object StateObject;
        public bool IsConnected;
        public int BufferSize => Buffer.MergeBuffer.Length;
        internal readonly NeutralBuffer Buffer;

        public Semaphore SendSync = new Semaphore(1, 1);

        public ClientSocket(int bufferSize, object stateObject = null)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false,
                UseOnlyOverlappedIO = true
            };

            Buffer = new NeutralBuffer(bufferSize);
            StateObject = stateObject;
        }

        public void ConnectAsync(string host, ushort port)
        {
            var ipList = Dns.GetHostAddresses(host).Where(i => i.AddressFamily == AddressFamily.InterNetwork).ToArray();
            var endPoint = new IPEndPoint(ipList.First(), port);

            if (IsConnected)
                Disconnect("ConnectAsync() IsConnected == true");

            FConsole.WriteLine($"Connecting to {host} at {ipList.First()} on port {port}");
            var connectArgs = SaeaPool.Get();
            connectArgs.Completed += Completed;
            connectArgs.RemoteEndPoint = endPoint;

            try
            {
                if (!Socket.ConnectAsync(connectArgs))
                    Completed(null, connectArgs);
            }
            catch (Exception ex)
            {
                Disconnect($"ConnectAsync() if (!Socket.ConnectAsync(connectArgs)) -> {ex.Message} {ex.StackTrace}");
            }
        }

        public void Receive()
        {
            var e = SaeaPool.Get();
            e.SetBuffer(Buffer.ReceiveBuffer, 0, Buffer.ReceiveBuffer.Length);
            e.UserToken = this;
            e.Completed += Completed;
            try
            {
                if (!Socket.ReceiveAsync(e))
                    Completed(null, e);
            }
            catch (Exception ex)
            {
                Disconnect($"Receive() if (!Socket.ReceiveAsync(e)) -> {ex.Message} {ex.StackTrace}");
            }
        }
        public void Send(byte[] packet)
        {
            var e = SaeaPool.Get();
            e.UserToken = this;
            e.Completed += Completed;
            SendQueue.Add(e, packet);
        }
        internal void Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Disconnect("ClientSocket.Sent() e.SocketError != SocketError.Success");
                return;
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ReceiveQueue.Add(e);
                    break;
                case SocketAsyncOperation.Send:
                    SendCompleted(e);
                    break;
                case SocketAsyncOperation.Connect:
                    IsConnected = true;
                    OnConnected?.Invoke();
                    Receive();
                    break;
            }
        }

        private void SendCompleted(SocketAsyncEventArgs e)
        {
            e.SetBuffer(null);
            e.Completed -= Completed;
            e.UserToken = null;
            SaeaPool.Return(e);
            SendSync.Release();
        }

        public string GetIp() => ((IPEndPoint)Socket?.RemoteEndPoint)?.Address?.ToString();

        public void Disconnect(string reason)
        {
            FConsole.WriteLine("Disconnecting: " + reason);
            IsConnected = false;
            Socket.Dispose();
            OnDisconnect?.Invoke();
        }
    }
}
