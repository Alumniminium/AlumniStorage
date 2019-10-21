using System;
using System.IO;
using Client.Entities;
using Universal.IO.Sockets.Client;

public static class PacketRouter
{
    public static void Handle(ClientSocket clientSocket, byte[] packet)
    {
        var packetId = packet[4];
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
            // Make the chunks as big as possible, let the TCP Stack do the rest
            // Preserve 128 + 24 bytes for Header information (128 bytes for filename, 24 bytes for header info).
            var chunk = new byte[user.Buffer.SendBuffer.Length - (128 + 40)];

            while (fileStream.Position != fileStream.Length)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(memoryStream))
                    {
                        // [Offset 0] Advance by 2 bytes, first two bytes are packet size, calculated at the end.
                        writer.Seek(4, SeekOrigin.Current);
                        // [Offset 2] Packet Id, 3000 = FileTransfer
                        writer.Write((byte)3);
                        // [Offset 4]
                        writer.Write(Path.GetFileName(fileName));
                        // [Offset 6 + FileName Length] FileName to be Created (1) / Appended (0) (6-8 = Length Prefix, 9-N = string)
                        writer.Write(fileStream.Position == 0);
                        //Read the payload (file contents chunk) into a buffer
                        var readBytes = fileStream.Read(chunk, 0, chunk.Length);
                        // [Offset 6 + FileName Length] Write the total file size
                        writer.Write(fileSize);
                        // [Offset 10 + FileName Length] Write size contained in this packet
                        writer.Write((int)readBytes);
                        // [Offset 12 + FileName Length] Write payload buffer
                        writer.Write(chunk, 0, readBytes);
                        var pos = writer.BaseStream.Position;
                        writer.Seek(0, SeekOrigin.Begin);
                        // [Offset 0] Write the full packet size to the first two bytes
                        writer.Write((int)pos);
                        //Console.WriteLine("Pos:"+pos);
                        // Get the complete packet out of the stream
                        var buffer = memoryStream.ToArray();
                        // Send it
                        user.Send(buffer);
                    }
                }
            }
        }
    }
}