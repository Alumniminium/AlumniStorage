using System;
using System.IO;
using Client.Entities;
using Universal.IO.Sockets.Client;
using Universal.Packets;

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
                break;
            case 3:
                ReceiveFile(user, packet);
                break;
            default:
                Console.WriteLine("Unknown Packet Id " + packetId);
                break;
        }
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
                bool firstRead = fileStream.Position==0;
                var readBytes = fileStream.Read(chunk, 0, chunk.Length);
                var msgFile = MsgFile.Create(fileName, fileSize, readBytes, chunk, firstRead);
                user.Send(msgFile);
            }
        }
    }
}