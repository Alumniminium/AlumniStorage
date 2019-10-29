using System.Diagnostics;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Queues;
using Universal.Packets;
using Universal.Packets.Enums;
using Client.Entities;
using Universal.IO.FastConsole;
using System.Threading.Tasks;

namespace Client
{
    public static class Program
    {
        public static string ServerHostname = "localhost";
        public static ushort ServerPort = 65533;
        public static ClientSocket Client = new ClientSocket(500_500);
        public static Stopwatch Stopwatch = new Stopwatch();
        public static async Task Main()
        {
            ServerHostname = "localhost";
            Client.OnConnected += Connected;
            ReceiveQueue.OnPacket += PacketRouter.Handle;
            Client.OnDisconnect += Disconnected;
            Thread.Sleep(1000);
            Client.ConnectAsync(ServerHostname, ServerPort);

            while (true)
            {
                var msg = Console.ReadLine();

                switch (msg)
                {
                    case "ping":
                        byte[] array = new byte[100_000];
                        var random = new Random();
                        for (int i = 0; i < array.Length; i++)
                        {
                            array[i] = (byte)random.Next(0, 255);
                        }
                        var msgBench = MsgBench.Create(array, false);
                        Client.Send(msgBench);
                        break;
                    case "send":
                        var user = (User)Client.StateObject;
                        FConsole.WriteLine("Requesting Token...");
                        user.Send(MsgToken.Create("transcoder", 0, true));
                        while (!user.Tokens.ContainsKey(0))
                            Thread.Sleep(1);
                        FConsole.WriteLine("Uploading... using " + user.Tokens[0]);
                        await user.SendFile(@"/home/alumni/transcoder", 0);
                        FConsole.WriteLine("Done.");
                        break;
                    default:
                        break;
                }
            }
        }

        private static void Disconnected()
        {
            Thread.Sleep(1000);
            Console.WriteLine("Socket disconnected!");
            Client = new ClientSocket(500_500);
            Client.OnConnected += Connected;
            Client.OnDisconnect += Disconnected;
            Client.ConnectAsync(ServerHostname, ServerPort);
        }

        private static void Connected()
        {
            Console.WriteLine("Socket Connected!");
            Client.Send(MsgLogin.Create("asd", "asdasd", true, MsgLoginType.Login));
        }
    }
}
