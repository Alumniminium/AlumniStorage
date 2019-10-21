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

namespace Client
{
    public static class Program
    {
        public static string ServerHostname = "localhost";
        //public static ushort ServerPort = 65533;
        public static ushort ServerPort = 70;
        public static ClientSocket Client = new ClientSocket(null);

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
                Client = new ClientSocket(null);
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
                    case "send":
                        Client.Send(MsgLogin.Create("asd", "asdasd", "asd@a.sd", false, MsgLoginType.Login));
                        //PacketRouter.SendFile(Client, "/home/alumni/Downloads/ct.exe");
                        PacketRouter.SendFile(Client, @"C:\Users\domi\Desktop\2u5bccouzxp31.png");
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
