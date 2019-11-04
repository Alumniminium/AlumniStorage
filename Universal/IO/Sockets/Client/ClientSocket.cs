using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Universal.IO.FastConsole;
using Universal.IO.Sockets.Cryptography;
using Universal.IO.Sockets.Queues;
using Universal.Packets;

namespace Universal.IO.Sockets.Client
{
    public class ClientSocket
    {
        public Action OnConnected, OnDisconnect;
        public Action<ClientSocket, byte[]> OnPacket;
        internal Socket Socket;
        public object StateObject;
        public bool IsConnected;
        internal readonly NeutralBuffer Buffer;
        internal readonly AutoResetEvent SendSync = new AutoResetEvent(true);
        internal readonly SocketAsyncEventArgs ReceiveArgs;
        internal readonly SocketAsyncEventArgs SendArgs;
        public DiffieHellman Diffie;
        public Crypto Crypto;

        public ClientSocket(int bufferSize, object stateObject = null)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false,
                UseOnlyOverlappedIO = true
            };
            Buffer = new NeutralBuffer(bufferSize);

            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.Completed += Completed;
            ReceiveArgs.SetBuffer(Buffer.ReceiveBuffer);
            ReceiveArgs.UserToken = this;

            SendArgs = new SocketAsyncEventArgs();
            SendArgs.Completed += Completed;
            SendArgs.SetBuffer(Buffer.SendBuffer);
            SendArgs.UserToken = this;

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
                var connectArgs = new SocketAsyncEventArgs();
                connectArgs.Completed += Completed;
                connectArgs.UserToken = this;

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
                ReceiveArgs.SetBuffer(Buffer.ReceiveBuffer, 0, Buffer.ReceiveBuffer.Length);
                if (!Socket.ReceiveAsync(ReceiveArgs))
                    Completed(null, ReceiveArgs);
            }
            catch (Exception ex)
            {
                Disconnect($"ClientSocket.Receive() if (!Socket.ReceiveAsync(e)) -> {ex.Message} {ex.StackTrace}");
            }
        }
        public unsafe void Send(byte[] packet)
        {
            SendSync.WaitOne();

            if (Crypto != null)
            {
                var iv = Crypto.SetRandomIV();
                var size = BitConverter.ToInt32(packet, 0);
                var header = packet.AsSpan().Slice(0, sizeof(MsgHeader));
                var data = packet.AsSpan().Slice(sizeof(MsgHeader), size - sizeof(MsgHeader));

                var encryptedData = Crypto.Encrypt(data.ToArray());
                BitConverter.GetBytes(encryptedData.Length + sizeof(MsgHeader)).CopyTo(header);
                iv.CopyTo(header.Slice(sizeof(MsgHeader)-16,16));

                var newPacket = new byte[encryptedData.Length + sizeof(MsgHeader)];
                header.CopyTo(newPacket);
                encryptedData.CopyTo(newPacket.AsSpan().Slice(sizeof(MsgHeader)));
                SendQueue.Add(SendArgs, newPacket, newPacket.Length);
            }
            else
                SendQueue.Add(SendArgs, packet, BitConverter.ToInt32(packet,0));
        }

        internal void Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Disconnect("ClientSocket.Completed() e.SocketError != SocketError.Success");
                return;
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ReceiveQueue.Add(this);
                    break;
                case SocketAsyncOperation.Send:
                    SendSync.Set();
                    break;
                case SocketAsyncOperation.Connect:
                    IsConnected = true;
                    OnConnected?.Invoke();
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
    }
}
