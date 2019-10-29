using System.Buffers;
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
                    Completed(null, receiveArgs);
            }
            else
                Disconnect("ClientSocket.Connected() e.SocketError != SocketError.Success");

            RecycleArgs(e);
        }

        public void Receive()
        {
            var e = GetSaea();
            e.UserToken = this;
            e.Completed += Completed;

            if (!Socket.ReceiveAsync(e))
                Completed(null, e);
        }

        internal void Completed(object sender, SocketAsyncEventArgs e)
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
            var e = SaeaPool.Get();
            e.UserToken = this;
            e.Completed += Completed;
            SendQueue.Add(e, packet);
        }
        public void Sent(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                Disconnect("ClientSocket.Sent() e.SocketError != SocketError.Success");
            //if (e.BytesTransferred != BitConverter.ToInt32(e.Buffer, 0))
            //    Socket.SendAsync(e);
            RecycleArgs(e);
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
            e.RemoteEndPoint = null;
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
