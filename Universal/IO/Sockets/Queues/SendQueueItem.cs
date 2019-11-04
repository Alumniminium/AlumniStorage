using System.Net.Sockets;

namespace Universal.IO.Sockets.Queues
{
    public class SendQueueItem
    {
        public readonly SocketAsyncEventArgs Args;
        public readonly byte[] Packet;
        public readonly int Size;
        public SendQueueItem(SocketAsyncEventArgs args, byte[] packet, int size)
        {
            Args = args;
            Size = size;
            Packet = packet;
        }
    }
}