using System.Security.Cryptography;
using System.Text;
using Universal.IO.Sockets.Client;
using Universal.Packets;
using Universal.Packets.Enums;

namespace Client.Packethandlers
{
    public static class MsgDiffieHandler
    {
        public static void Process(ClientSocket clientSocket, MsgDH packet)
        {
            var b = packet.GetPayload();
            clientSocket.Diffie.HandleResponse(Encoding.ASCII.GetString(b));
            clientSocket.Crypto = Aes.Create();
            clientSocket.Crypto.Key = clientSocket.Diffie.Key;
            clientSocket.Diffie.Dispose();
            clientSocket.Send(MsgLogin.Create("asd", "asdasd", false, MsgLoginType.Login));
        }
    }
}