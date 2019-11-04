using System.Security.Cryptography;
using System.Text;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Cryptography;
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
            clientSocket.Crypto = new Crypto(clientSocket.Diffie.Key);
            clientSocket.Send(MsgLogin.Create("asd", "asdasd", false, MsgLoginType.Login));
        }
    }
}