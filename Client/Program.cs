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

            while (true)
            {
                var msg = Console.ReadLine();

                switch (msg)
                {
                    case null:
                        break;
                    case "login":
                        Client.Send(MsgLogin.Create("asd", "asdasd", true, MsgLoginType.Login));
                        break;
                    case "ping":
                        byte[] array = new byte[5];
                        var random = new Random();
                        for (int i = 0; i < array.Length; i++)
                        {
                            array[i] = (byte)random.Next(0, 255);
                        }
                        Stopwatch.Restart();
                        var msgBench = MsgBench.Create(array, true);
                        Client.Send(msgBench);
                        break;
                    case "send":
                    Console.WriteLine("Logging in...");
                        Client.Send(MsgLogin.Create("asd", "asdasd", true, MsgLoginType.Login));
                        while (Client.StateObject == null)
                            Thread.Sleep(1);
                        var user = (User)Client.StateObject;
                        Console.WriteLine("Requesting Token...");
                        user.Send(MsgToken.Create("", @"transcoder", true, 0));
                        while (!user.Tokens.ContainsKey("transcoder"))
                            Thread.Sleep(1);
                        Console.WriteLine("Uploading... using "+user.Tokens["transcoder"]);
                        PacketRouter.SendFile(user, @"/home/alumni/transcoder", user.Tokens["transcoder"]);
                        Console.WriteLine("Done.");
                        break;
                    default:
                        var buffer = Encoding.UTF8.GetBytes(msg);
                        Client.Send(buffer);
                        break;
                }
            }
        }
    }
}
