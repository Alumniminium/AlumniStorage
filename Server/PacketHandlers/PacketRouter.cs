using System;
using Server.Entities;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Monitoring;
using Universal.Packets.Enums;

namespace Server.PacketHandlers
{
    public static class PacketRouter
    {
        public static void Handle(ClientSocket clientSocket, byte[] packet)
        {
            NetworkMonitor.Log(BitConverter.ToInt32(packet, 0), TrafficMode.In);
            var packetId = (PacketType)packet[5];
            var user = (User)clientSocket.StateObject;
            switch (packetId)
            {
                case PacketType.MsgHandshake:
                    MsgDiffieHandler.Process(clientSocket,packet);
                    break;
                case PacketType.MsgLogin:
                    MsgLoginHandler.Process(clientSocket, packet);
                    break;
                case PacketType.MsgFile:
                    MsgFileHandler.Process(user, packet);
                    break;
                case PacketType.MsgToken:
                    MsgTokenHandler.Process(user, packet);
                    break;
                case PacketType.MsgBench:
                    MsgBenchHandler.Process(clientSocket, packet);
                    break;
                default:
                    Console.WriteLine("Unknown Packet Id " + packetId);
                    break;
            }
        }
    }
}
