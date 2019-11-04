using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Monitoring;
using Universal.Packets;

namespace Server.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public ClientSocket Socket { get; set; }
        public Dictionary<string, string> Tokens { get; set; }

        public User()
        {
            Tokens = new Dictionary<string, string>();
        }

        public void Send(byte[] packet)
        {
            NetworkMonitor.Log(BitConverter.ToInt32(packet, 0), TrafficMode.Out);
            Socket?.Send(packet);
        }
        public void SendFile(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                var fileName = Path.GetFileName(path);
                var fileSize = fileStream.Length;

                while (fileStream.Position != fileStream.Length)
                {
                    var firstRead = fileStream.Position == 0;
                    var chunk = ArrayPool<byte>.Shared.Rent(MsgFile.MAX_CHUNK_SIZE);
                    var readBytes = fileStream.Read(chunk, 0, chunk.Length);
                    var msgFile = MsgFile.Create(fileName, fileSize, readBytes, chunk, firstRead);
                    Send(msgFile);
                    ArrayPool<byte>.Shared.Return(chunk);
                }
            }
        }

        public void Disconnect(string reason) => Socket.Disconnect(reason: reason);
        public override string ToString() => $"UserId: {Id} | Name: {Username} | Password: {Password} | Online: {Socket != null && Socket.IsConnected}";
    }
}