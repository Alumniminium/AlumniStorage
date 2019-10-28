using System;
using System.Net;
using System.Net.Sockets;
using Universal.IO.FastConsole;
using Universal.IO.Sockets.Pools;
using Universal.IO.Sockets.Queues;
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

        public ClientSocket(int bufferSize = 2_000_000, object stateObject = null)
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
            var endPoint = new IPEndPoint(IPAddress.Parse(host), port);

            if (IsConnected)
                Disconnect("ConnectAsync() IsConnected == true");

            var connectArgs = SaeaPool.Get();
            //connectArgs.UserToken = new ClientSocket();
            connectArgs.RemoteEndPoint = endPoint;
            connectArgs.Completed += Connected;
            if (!Socket.ConnectAsync(connectArgs))
                Connected(null, connectArgs);
        }

        private void Connected(object o, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                IsConnected = true;
                OnConnected?.Invoke();
                var receiveArgs = GetReceiveArgs();
                if (!Socket.ReceiveAsync(receiveArgs))
                    Received(null, receiveArgs);
            }
            else
                Disconnect("ClientSocket.Connected() e.SocketError != SocketError.Success");

            e.RemoteEndPoint = null;
            e.Completed -= Connected;
            SaeaPool.Return(e);
        }

        internal void RecycleArgs(SocketAsyncEventArgs e)
        {
            e.SetBuffer(null);
            e.Completed -= Received;
            e.UserToken = null;
            SaeaPool.Return(e);
        }

        public void Receive()
        {
            var e = GetReceiveArgs();
            if (!Socket.ReceiveAsync(e))
                Received(null, e);
        }

        internal SocketAsyncEventArgs GetReceiveArgs()
        {
            var ReceiveArgs = SaeaPool.Get();
            ReceiveArgs.SetBuffer(Buffer.ReceiveBuffer, 0, Buffer.ReceiveBuffer.Length);
            ReceiveArgs.Completed += Received;
            ReceiveArgs.UserToken = this;
            return ReceiveArgs;
        }

        public void Received(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                ReceiveQueue.Add(e);
            else
                Disconnect("ClientSocket.Received() if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)");
        }
        public void Send(byte[] packet)
        {
            var SendArgs = SaeaPool.Get();
            SendArgs.UserToken = this;
            SendArgs.Completed += Sent;
            SendQueue.Add(SendArgs, packet);
        }
        public void Sent(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                Disconnect("ClientSocket.Sent() e.SocketError != SocketError.Success");
            //SendSync.Set();
            e.SetBuffer(null);
            e.UserToken = null;
            e.Completed -= Sent;
            SaeaPool.Return(e);
        }
        public string GetIp() => ((IPEndPoint)Socket?.RemoteEndPoint)?.Address?.ToString();

        public void Disconnect(string reason)
        {
            FConsole.WriteLine("Disconnecting: " + reason);
            IsConnected = false;
            OnDisconnect?.Invoke();
        }
    }
}
