using System;
using Server.PacketHandlers;
using Universal.Exceptions;
using Universal.IO.FastConsole;
using Universal.IO.Sockets.Server;

namespace Server
{
    internal static class Program
    {
        static void Main()
        {
            GlobalExceptionHandler.Setup();
            FConsole.WriteLine("Hello World!");
            ServerSocket.OnPacket += PacketRouter.Handle;
            ServerSocket.Start(65533);

            while (true)
                Console.ReadLine();
        }
    }
}
