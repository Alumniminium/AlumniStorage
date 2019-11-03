using System.Security.Cryptography;
using System.Threading;
using System;
using System.Net;
using System.Net.Sockets;
using Universal.IO.FastConsole;
using Universal.IO.Sockets.Pools;
using Universal.IO.Sockets.Queues;
using System.Linq;
using Universal.IO.Sockets.Crypto;
using System.Buffers;

namespace Universal.IO.Sockets.Client
{
    public class ClientSocket
    {
        public Action OnConnected, OnDisconnect;
        public Action<ClientSocket, byte[]> OnPacket;
        internal Socket Socket;
        public object StateObject;
        public bool IsConnected;
        public int BufferSize => Buffer.MergeBuffer.Length;
        internal readonly NeutralBuffer Buffer;
        internal readonly AutoResetEvent SendSync = new AutoResetEvent(true);
        public DiffieHellman Diffie;
        public bool Encryption => Crypto != null;
        public Aes Crypto;

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
                Disconnect("ClientSocket.ConnectAsync() IsConnected == true");

            FConsole.WriteLine($"Connecting to {host} at {ipList.First()} on port {port}");
            try
            {
                var connectArgs = RentSaea();
                connectArgs.RemoteEndPoint = endPoint;
                if (!Socket.ConnectAsync(connectArgs))
                    Completed(null, connectArgs);
            }
            catch (Exception ex)
            {
                Disconnect($"ClientSocket.ConnectAsync() if (!Socket.ConnectAsync(connectArgs)) -> {ex.Message} {ex.StackTrace}");
            }
        }

        public void Receive()
        {
            try
            {
                var e = RentSaea();
                e.SetBuffer(Buffer.ReceiveBuffer, 0, Buffer.ReceiveBuffer.Length);
                if (!Socket.ReceiveAsync(e))
                    Completed(null, e);
            }
            catch (Exception ex)
            {
                Disconnect($"ClientSocket.Receive() if (!Socket.ReceiveAsync(e)) -> {ex.Message} {ex.StackTrace}");
            }
        }

        public void Send(byte[] packet)
        {
            SendSync.WaitOne();
            var e = RentSaea();

            if (Encryption)
            {
                var lengthBytes = BitConverter.GetBytes(packet.Length + 16);
                lengthBytes.AsSpan().CopyTo(packet);
                var encryptedIV = ArrayPool<byte>.Shared.Rent(packet.Length + 16);
                var encrypted = Crypto.CreateEncryptor().TransformFinalBlock(packet, 4, packet.Length - 4);
                encrypted.AsSpan().CopyTo(encryptedIV.AsSpan(4));
                Crypto.IV.AsSpan().CopyTo(encryptedIV.AsSpan().Slice(packet.Length));

                ArrayPool<byte>.Shared.Return(packet);
                SendQueue.Add(e, encryptedIV, packet.Length + 16);
            }
            else
                SendQueue.Add(e, packet, packet.Length);
        }

        internal void Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                RecycleSaea(e);
                Disconnect("ClientSocket.Completed() e.SocketError != SocketError.Success");
                return;
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ReceiveQueue.Add(e);
                    break;
                case SocketAsyncOperation.Send:
                    RecycleSaea(e);
                    SendSync.Set();
                    break;
                case SocketAsyncOperation.Connect:
                    IsConnected = true;
                    OnConnected?.Invoke();
                    RecycleSaea(e);
                    Receive();
                    break;
            }
        }
        public string GetIp() => ((IPEndPoint)Socket?.RemoteEndPoint)?.Address?.ToString();

        public void Disconnect(string reason)
        {
            FConsole.WriteLine("Disconnecting: " + reason);
            IsConnected = false;
            Socket?.Dispose();
            OnDisconnect?.Invoke();
        }

        private SocketAsyncEventArgs RentSaea()
        {
            var e = SaeaPool.Get();
            e.UserToken = this;
            e.Completed += Completed;
            return e;
        }
        internal void RecycleSaea(SocketAsyncEventArgs e)
        {
            e.SetBuffer(null);
            e.Completed -= Completed;
            e.UserToken = null;
            SaeaPool.Return(e);
        }
    }
}
