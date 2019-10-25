using System.Diagnostics;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Queues;
using Universal.Packets;
using Universal.Packets.Enums;
using Client.Entities;

namespace Client
{
    public static class Program
    {
        public static string ServerHostname = "localhost";
        public static ushort ServerPort = 65533;
        public static ClientSocket Client = new ClientSocket(500_500);
        public static Stopwatch Stopwatch = new Stopwatch();
        public static void Main()
        {
            ServerHostname = "localhost";

            var ipList = Dns.GetHostAddresses(ServerHostname).Where(i => i.AddressFamily == AddressFamily.InterNetwork).ToArray();
            foreach (var ip in ipList)
                Console.WriteLine(ip.ToString());

            Client.OnConnected += () => Console.WriteLine("Socket Connected!");
            ReceiveQueue.OnPacket += PacketRouter.Handle;

            Client.OnDisconnect += () =>
            {
                Console.WriteLine("Socket disconnected!");
                Client = new ClientSocket();
                Client.ConnectAsync(ipList[0].ToString(), ServerPort);
            };

            Thread.Sleep(1000);
            Client.ConnectAsync(ipList[0].ToString(), ServerPort);

            Client.Send(MsgLogin.Create("asd", "asdasd", true, MsgLoginType.Login));
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
                        Stopwatch.Restart();
                        var msgBench = MsgBench.Create(array, false);
                        Client.Send(msgBench);
                        break;
                    case "send":
                        var user = (User)Client.StateObject;
                        Console.WriteLine("Requesting Token...");
                        user.Send(MsgToken.Create("transcoder", 0,false));
                        while (!user.Tokens.ContainsKey(0))
                            Thread.Sleep(1);
                        Console.WriteLine("Uploading... using "+user.Tokens[0]);
                        PacketRouter.SendFile(user, @"/home/alumni/transcoder", 0);
                        Console.WriteLine("Done.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
