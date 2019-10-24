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
        public static Dictionary<string, string> Tokens = new Dictionary<string, string>();
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
                case 3:
                    MsgTokenHandler(user, packet);
                    break;
                case 10:
                    Pong(user, packet);
                    break;
                default:
                    Console.WriteLine("Unknown Packet Id " + packetId);
                    break;
            }
        }
        private static void MsgTokenHandler(User user, byte[] packet)
        {
            var msgToken = (MsgToken)packet;
            var path = msgToken.GetPath();

            var token = "";
            if (!user.Tokens.ContainsKey(path))
            {
                if(!Tokens.ContainsKey(path))
                {
                    token = Guid.NewGuid().ToString().Replace("-","");
                    Tokens.Add(path,token);
                    user.Tokens.Add(path,token);
                }
            }
            else
                token = user.Tokens[path];
            Console.WriteLine("Token Created: "+token+" for: "+path);
            msgToken = MsgToken.Create(token,path,true,0);
            user.Send(msgToken);
        }
        private static void Pong(User user, byte[] packet)
        {
            var msgBench = (MsgBench)packet;
            var array = msgBench.GetArray();
            array = array.Reverse().ToArray();

            msgBench = MsgBench.Create(array, true);
            user.Send(msgBench);
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
            var token = msgFile.GetToken();
            Console.WriteLine("Token: "+token);
            var kvp = user.Tokens.FirstOrDefault(n=> n.Value == token);
            var path = "/tmp/"+kvp.Key;
            Console.WriteLine("Path: "+path);
            if(string.IsNullOrEmpty(kvp.Key))
                user.Disconnect("No token");

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
