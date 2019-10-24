using System.Linq;
using System;
using System.IO;
using Client.Entities;
using Universal.IO;
using Universal.IO.Sockets.Client;
using Universal.Packets;
using Client;

public static class PacketRouter
{
    public static void Handle(ClientSocket clientSocket, byte[] packet)
    {
        var packetId = packet[5];
        var user = (User)clientSocket.StateObject;
        switch (packetId)
        {
            case 1:
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

    private static void Pong(User user, byte[] packet)
    {
        Program.Stopwatch.Stop();
        Console.WriteLine("Took: "+Program.Stopwatch.Elapsed.TotalMilliseconds);
    }

    private static void MsgTokenHandler(User user, byte[] packet)
    {
        var msgToken = (MsgToken)packet;
        var token = msgToken.GetToken();
        var path = msgToken.GetPath();

        if (user.Tokens.ContainsKey(path))
            user.Tokens[path] = token;
        else
            user.Tokens.Add(path, token);
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
}