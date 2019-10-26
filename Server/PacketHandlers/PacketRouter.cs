using System;
using System.Buffers;
using System.IO;
using Server.Entities;
using Universal.IO.Sockets.Client;
using Universal.Packets;
using Universal.Packets.Enums;

namespace Server
{
    public static class PacketRouter
    {
        public static void Handle(ClientSocket clientSocket, byte[] packet)
        {
            var packetId = (PacketType)packet[5];
            var user = (User)clientSocket.StateObject;
            switch (packetId)
            {
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
                    MsgBenchHandler.Process(user, packet);
                    break;
                default:
                    Console.WriteLine("Unknown Packet Id " + packetId);
                    break;
            }
        }        
    }
}
