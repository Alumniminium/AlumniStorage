using System;
using System.Net.Sockets;

namespace Universal.IO.Sockets.Queues
{
    public class SendQueueItem
    {
        public readonly SocketAsyncEventArgs Args;
        public readonly byte[] Packet;
        public readonly int Size;
        public SendQueueItem(SocketAsyncEventArgs args, byte[] packet)
        {
            Args = args;
            Size = BitConverter.ToInt32(packet, 0);
            Packet = packet;
        }
    }
}