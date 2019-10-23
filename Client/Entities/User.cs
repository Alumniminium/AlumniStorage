using System.Net;
using Universal.IO.Sockets.Client;

namespace Client.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public ClientSocket Socket { get; set; }
        public string CurrentFileName { get; set; }

        public User()
        {
        }

        public void OnDisconnect()
        {

        }

        public void Send(byte[] packet) => Socket?.Send(packet);
        public string GetIp() => ((IPEndPoint)Socket.Socket.RemoteEndPoint).Address.ToString();

        public override string ToString() => $"UserId: {Id} | Name: {Username} | Password: {Password} | Online: {Socket != null && Socket.IsConnected}";
    }
}