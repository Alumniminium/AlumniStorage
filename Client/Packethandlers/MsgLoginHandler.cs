using System;
using Client.Entities;
using Universal.IO.Sockets.Client;
using Universal.Packets;

namespace Client.Packethandlers
{
    internal static class MsgLoginHandler
    {
        internal static void Process(ClientSocket clientSocket, byte[] packet)
        {
            var msgLogin = (MsgLogin)packet;

            if (clientSocket.StateObject == null)
            {
                var username = msgLogin.GetUsername();
                var password = msgLogin.GetPassword();
                clientSocket.StateObject = new User();
                ((User)clientSocket.StateObject).Username = username;
                ((User)clientSocket.StateObject).Password = password;
                ((User)clientSocket.StateObject).Socket = clientSocket;
            }
            else
                clientSocket.OnDisconnect -= ((User)clientSocket.StateObject).OnDisconnect;

            ((User)clientSocket.StateObject).Id = msgLogin.UniqueId;

            clientSocket.OnDisconnect += ((User)clientSocket.StateObject).OnDisconnect;
            ((User)clientSocket.StateObject).Socket.StateObject = ((User)clientSocket.StateObject);

            Console.WriteLine($"MsgLogin: {((User)clientSocket.StateObject).Username} authenticated with UniqueId {((User)clientSocket.StateObject).Id}");
        }
    }
}