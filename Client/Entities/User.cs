using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Monitoring;
using Universal.Packets;

namespace Client.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public ClientSocket Socket { get; set; }
        public ConcurrentDictionary<int, string> Tokens;

        public User()
        {
            Tokens = new ConcurrentDictionary<int, string>();
        }

        public void OnDisconnect()
        {

        }

        public void Send(byte[] packet)
        {
            NetworkMonitor.Log(BitConverter.ToInt32(packet, 0), TrafficMode.Out);
            Socket?.Send(packet);
        }
        public async ValueTask SendFile(string path, int tokenId)
        {
            var token = Tokens[tokenId];
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                var fileSize = fileStream.Length;
                var chunk = new byte[MsgFile.MAX_CHUNK_SIZE];

                while (fileStream.Position != fileStream.Length)
                {
                    var firstRead = fileStream.Position == 0;
                    var readBytes = await fileStream.ReadAsync(chunk, 0, MsgFile.MAX_CHUNK_SIZE);
                    var msgFile = MsgFile.Create(token, fileSize, readBytes, chunk, firstRead);
                    Send(msgFile);
                }
            }

            Tokens.TryRemove(tokenId, out _);
        }
        public override string ToString() => $"UserId: {Id} | Name: {Username} | Password: {Password} | Online: {Socket != null && Socket.IsConnected}";
    }
}