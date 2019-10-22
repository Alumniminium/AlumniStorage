using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server.Entities;
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
                    break;
                case 3:
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

        private static void ReceiveFile(User user, byte[] packet)
        {
            using (var reader = new BinaryReader(new MemoryStream(packet)))
            {
                var packetSize = reader.ReadInt32();
                var compressed = reader.ReadBoolean();
                var packetId = reader.ReadByte();
                var fileName = reader.ReadString();
                user.CurrentFileName = fileName.Trim();
                var createNew = reader.ReadBoolean();
                var fileSize = reader.ReadInt64();
                var fileChunkSize = reader.ReadInt32();

                var chunk = new byte[fileChunkSize];
                reader.Read(chunk, 0, fileChunkSize);

                using (var filestream = new FileStream("/tmp/" + fileName, createNew ? FileMode.Create : FileMode.Append))
                {
                    filestream.Write(chunk, 0, chunk.Length);

                    if (filestream.Position == fileSize)
                    {
                        //file complete
                        Console.WriteLine("File received.");
                    }
                }
            }
        }
        public static void SendFile(User user, string path)
        {
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                var fileName = Path.GetFileName(path);
                var fileSize = fileStream.Length;
                var chunk = new byte[100_000 - 164];

                while (fileStream.Position != fileStream.Length)
                {
                    using (var memoryStream = new MemoryStream())
                    using (var writer = new BinaryWriter(memoryStream))
                    {
                        writer.Seek(4, SeekOrigin.Current);
                        writer.Write((byte)1);
                        writer.Write((byte)3);
                        writer.Write(Path.GetFileName(fileName));
                        writer.Write(fileStream.Position == 0);
                        var readBytes = fileStream.Read(chunk, 0, chunk.Length);
                        writer.Write(fileSize);
                        writer.Write((int)readBytes);
                        writer.Write(chunk, 0, readBytes);
                        var pos = writer.BaseStream.Position;
                        writer.Seek(0, SeekOrigin.Begin);
                        writer.Write((int)pos);
                        var buffer = memoryStream.ToArray();
                        user.Send(buffer);
                    }
                }
            }
        }
    }
}
