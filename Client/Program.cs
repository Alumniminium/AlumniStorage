using System.Diagnostics;
using System;
using System.Threading;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Queues;
using Universal.Packets;
using Universal.Packets.Enums;
using Client.Entities;
using Universal.IO.FastConsole;
using System.Threading.Tasks;
using Client.Packethandlers;
using Universal.IO.Sockets.Crypto;

namespace Client
{
    public static class Program
    {
        public static string ServerHostname = "localhost";
        public static ushort ServerPort = 65533;
        public static ClientSocket Client = new ClientSocket(ushort.MaxValue);
        public static Stopwatch Stopwatch = new Stopwatch();
        public static async Task Main()
        {
            FConsole.WriteLine("Host: ");
            ServerHostname = Console.ReadLine().Trim();
            Client.OnConnected += Connected;
            Client.OnPacket += PacketRouter.Handle;
            Client.OnDisconnect += Disconnected;
            Client.ConnectAsync(ServerHostname, ServerPort);

            while (true)
            {
                var msg = Console.ReadLine();

                switch (msg)
                {
                    case "login":
                        Client.Send(MsgLogin.Create("asd", "asdasd", true, MsgLoginType.Login));
                        break;
                    case "ping":
                        var array = new byte[MsgBench.MAX_ARRAY_LENGTH];
                        var random = new Random();
                        for (var i = 0; i < array.Length; i++)
                        {
                            array[i] = (byte)random.Next(0, 255);
                        }
                        Stopwatch.Start();
                        for (var i = 0; i < 1000; i++)
                        {
                            var msgBench = MsgBench.Create(array, true);
                            Client.Send(msgBench);
                        }
                        break;
                    case "send":
                        var user = (User)Client.StateObject;
                        FConsole.WriteLine("Requesting Token...");
                        user.Send(MsgToken.Create("transcoder", 0, true));
                        while (!user.Tokens.ContainsKey(0))
                            Thread.Sleep(1);
                        FConsole.WriteLine("Uploading... using " + user.Tokens[0]);
                        Stopwatch.Start();
                        await user.SendFile(@"/home/alumni/transcoder", 0);
                        Stopwatch.Stop();
                        FConsole.WriteLine($"Done. {Stopwatch.Elapsed.TotalSeconds.ToString("##.##")}");
                        break;
                }
            }
        }

        private static void Disconnected()
        {
            Thread.Sleep(1000);
            FConsole.WriteLine("Socket disconnected!");
            Client = new ClientSocket(ushort.MaxValue);
            Client.OnConnected += Connected;
            Client.OnDisconnect += Disconnected;
            Client.ConnectAsync(ServerHostname, ServerPort);
        }

        private static void Connected()
        {
            FConsole.WriteLine("Socket Connected! Logging in...");
            Client.Diffie = new DiffieHellman(256).GenerateRequest();
            var msgDh = MsgDH.Create(Client.Diffie.ToString());
            Client.Send(msgDh);
        }
    }
}
