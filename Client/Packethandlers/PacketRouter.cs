using System;
using Client.Entities;
using Universal.IO.Sockets.Client;

namespace Client.Packethandlers
{
    public static class PacketRouter
    {
        public static void Handle(ClientSocket clientSocket, byte[] packet)
        {
            var packetId = packet[5];
            var user = (User)clientSocket.StateObject;
            switch (packetId)
            {
                case 1:
                    MsgLoginHandler.Process(clientSocket, packet);
                    break;
                case 2:
                    MsgFileHandler.Process(user, packet);
                    break;
                case 3:
                    MsgTokenHandler.Process(user, packet);
                    break;
                case 10:
                    MsgBenchHandler.Process(user, packet);
                    break;
                default:
                    Console.WriteLine("Unknown Packet Id " + packetId);
                    break;
            }
        }
    }
}