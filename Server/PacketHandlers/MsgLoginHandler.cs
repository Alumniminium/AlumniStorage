using Server.Entities;
using Universal.IO.FastConsole;
using Universal.IO.Sockets.Client;
using Universal.Packets;

namespace Server.PacketHandlers
{
    public class MsgLoginHandler
    {
        internal static void Process(ClientSocket userSocket, byte[] packet)
        {
            var msgLogin = (MsgLogin)packet;
            var username = msgLogin.GetUsername();
            var password = msgLogin.GetPassword();
            FConsole.WriteLine($"MsgLogin: {username} with password {password} (compressed: {msgLogin.Compressed}) requesting login.");

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
    }
}