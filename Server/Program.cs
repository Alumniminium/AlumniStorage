using System;
using Server.Entities;
using Universal.IO.Sockets.Queues;
using Universal.IO.Sockets.Server;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ReceiveQueue.OnPacket += PacketRouter.Handle;
            ServerSocket.Start(65533);

            while (true)
                Console.ReadLine();
        }
    }
}
