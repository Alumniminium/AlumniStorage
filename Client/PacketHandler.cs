using System.Buffers;
using System;
using System.IO;
using Client.Entities;
using Universal.IO;
using Universal.IO.Sockets.Client;
using Universal.Packets;
using Client;
using Universal.IO.FastConsole;

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

    private static void ProcessLogin(ClientSocket client, byte[] packet)
    {
        var msgLogin = (MsgLogin)packet;
        var username = msgLogin.GetUsername();
        var password = msgLogin.GetPassword();
        Console.WriteLine($"MsgLogin: {username} with password {password} (compressed: {msgLogin.Compressed}) requesting login.");

        var user = new User
        {
            Socket = client,
            Username = username,
            Password = password,
            Id = msgLogin.UniqueId
        };
        user.Socket.OnDisconnect += user.OnDisconnect;
        user.Socket.StateObject = user;
    }
    private static void Pong(User user, byte[] packet)
    {
        Program.Stopwatch.Stop();
        FConsole.WriteLine("Took: " + Program.Stopwatch.Elapsed.TotalMilliseconds);
        Program.Stopwatch.Restart();
        Program.Client.Send(MsgBench.Create(new byte[100_000], false));
    }

    private static void MsgTokenHandler(User user, byte[] packet)
    {
        var msgToken = (MsgToken)packet;

        if (user.Tokens.ContainsKey(msgToken.UniqueId))
            user.Tokens[msgToken.UniqueId] = msgToken.GetToken;
        else
            user.Tokens.TryAdd(msgToken.UniqueId, msgToken.GetToken);
    }

    private static void ReceiveFile(User user, byte[] packet)
    {
        var msgFile = (MsgFile)packet;
        user.CurrentFileName = "/tmp/" + msgFile.GetToken();

        var mode = FileMode.Create;
        if (File.Exists(user.CurrentFileName))
            mode = FileMode.Append;
        if (msgFile.CreateFile)
            mode = FileMode.Truncate;

        var chunk = msgFile.GetChunk();
        using (var filestream = new FileStream(user.CurrentFileName, mode))
        {
            filestream.Write(chunk, 0, msgFile.ChunkSize);
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
        ArrayPool<byte>.Shared.Return(chunk);

    }
    public static void SendFile(User user, string path, int tokenId)
    {
        string token = user.Tokens[tokenId];
        using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
        {
            var fileSize = fileStream.Length;
            var chunk = ArrayPool<byte>.Shared.Rent(MsgFile.MAX_CHUNK_SIZE);

            while (fileStream.Position != fileStream.Length)
            {
                bool firstRead = fileStream.Position == 0;
                var readBytes = fileStream.Read(chunk, 0, chunk.Length);
                var msgFile = MsgFile.Create(token, fileSize, readBytes, chunk, firstRead);
                user.Send(msgFile);
                ArrayPool<byte>.Shared.Return(chunk);
            }
        }

        user.Tokens.TryRemove(tokenId, out _);
    }
}