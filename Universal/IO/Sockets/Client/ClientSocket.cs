using System.Buffers;
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
            var endPoint = new IPEndPoint(IPAddress.Parse(host), port);

            if (IsConnected)
                Disconnect("ConnectAsync() IsConnected == true");

            var connectArgs = GetSaea();
            connectArgs.RemoteEndPoint = endPoint;

            if (!Socket.ConnectAsync(connectArgs))
                Completed(null, connectArgs);
        }

        private void Connected(object o, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                IsConnected = true;
                OnConnected?.Invoke();
                var receiveArgs = GetSaea();
                if (!Socket.ReceiveAsync(receiveArgs))
                    Received(null, receiveArgs);
            }
            else
                Disconnect("ClientSocket.Connected() e.SocketError != SocketError.Success");

            e.RemoteEndPoint = null;
            SaeaPool.Return(e);
        }

        public void Receive()
        {
            var e = GetSaea();
            if (!Socket.ReceiveAsync(e))
                Received(null, e);
        }

        private void Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    Received(sender, e);
                    break;
                case SocketAsyncOperation.Send:
                    Sent(sender, e);
                    break;
                case SocketAsyncOperation.Connect:
                    Connected(sender, e);
                    break;
            }
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

            var size = BitConverter.ToInt32(packet, 0);
            var copy = ArrayPool<byte>.Shared.Rent(size);
            packet.AsSpan().CopyTo(copy);

            SendQueue.Add(SendArgs, copy);
        }
        public void Sent(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                Disconnect("ClientSocket.Sent() e.SocketError != SocketError.Success");

            e.SetBuffer(null);
            e.UserToken = null;
            SaeaPool.Return(e);
        }
        internal SocketAsyncEventArgs GetSaea()
        {
            var e = SaeaPool.Get();
            e.SetBuffer(Buffer.ReceiveBuffer, 0, Buffer.ReceiveBuffer.Length);
            e.Completed += Completed;
            e.UserToken = this;
            return e;
        }
        internal void RecycleArgs(SocketAsyncEventArgs e)
        {
            e.SetBuffer(null);
            e.Completed -= Completed;
            e.UserToken = null;
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
