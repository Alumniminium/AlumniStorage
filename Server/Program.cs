using System;
using Server.PacketHandlers;
using Universal.IO.Sockets.Queues;
using Universal.IO.Sockets.Server;

namespace Server
{
    internal static class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello World!");
            ReceiveQueue.OnPacket += PacketRouter.Handle;
            ServerSocket.Start(65533);

            while (true)
                Console.ReadLine();
        }
    }
}
