using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server.Entities;
using Universal.IO;
using Universal.IO.FastConsole;
using Universal.IO.Sockets.Client;
using Universal.Packets;

namespace Server
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
                    ProcessLogin(clientSocket, packet);
                    break;
                case 2:
                    ReceiveFile(user, packet);
                    break;
                default:
                    Console.WriteLine("Unknown Packet Id " + packetId);
                    break;
            }
        }

        private static void ProcessLogin(ClientSocket userSocket, byte[] packet)
        {
            var msgLogin = (MsgLogin)packet;
            var username = msgLogin.GetUsername();
            var password = msgLogin.GetPassword();
            FConsole.WriteLine($"MsgLogin: {username} with password {password} (compressed: {msgLogin.Header.Compressed}) requesting login.");

            var user = new User
            {
                Socket = userSocket,
                Username = username,
                Password = password,
                Id = 1
            };
            user.Socket.OnDisconnect += user.OnDisconnect;
            user.Socket.StateObject = user;

            msgLogin.UniqueId = user.Id;

            user.Send(msgLogin);
        }
        public static void SendFile(ClientSocket user, string path)
        {
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                var fileName = Path.GetFileName(path);
                var fileSize = fileStream.Length;
                var chunk = new byte[MsgFile.MAX_CHUNK_SIZE];

                while (fileStream.Position != fileStream.Length)
                {
                    bool firstRead = fileStream.Position == 0;
                    var readBytes = fileStream.Read(chunk, 0, chunk.Length);
                    var msgFile = MsgFile.Create(fileName, fileSize, readBytes, chunk, firstRead);
                    user.Send(msgFile);
                }
            }
        }
        private static void ReceiveFile(User user, byte[] packet)
        {
            var msgFile = (MsgFile)packet;
            user.CurrentFileName = "/tmp/" + msgFile.GetFileName();

            var mode = FileMode.Create;
            if (File.Exists(user.CurrentFileName))
                mode = FileMode.Append;
            if (msgFile.CreateFile)
                mode = FileMode.Truncate;

            using (var filestream = new FileStream(user.CurrentFileName, mode))
            {
                var chunk = msgFile.GetChunk();
                filestream.Write(chunk);

                if (filestream.Position == msgFile.FileSize)
                {
                    int count = 0;
                    double size = filestream.Position;
                    while (size > 1000)
                    {
                        size = size / 1024;
                        count++;
                    }
                    Console.WriteLine($"File {user.CurrentFileName} ({size.ToString("###.##")} {(FormatEnum)count}) received!");
                }
            }

        }
    }
}
