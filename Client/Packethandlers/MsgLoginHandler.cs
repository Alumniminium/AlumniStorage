using System;
using Client.Entities;
using Universal.IO.Sockets.Client;
using Universal.Packets;

internal class MsgLoginHandler
{
    internal static void Process(ClientSocket clientSocket, byte[] packet)
    {
        var msgLogin = (MsgLogin)packet;
        var username = msgLogin.GetUsername();
        var password = msgLogin.GetPassword();
        Console.WriteLine($"MsgLogin: {username} with password {password} (compressed: {msgLogin.Compressed}) requesting login.");

        var user = new User
        {
            Socket = clientSocket,
            Username = username,
            Password = password,
            Id = msgLogin.UniqueId
        };
        user.Socket.OnDisconnect += user.OnDisconnect;
        user.Socket.StateObject = user;
    }
}