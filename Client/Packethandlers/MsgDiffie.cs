using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Security;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Crypto;
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
            clientSocket.Crypto = AesManaged.Create();
            clientSocket.Crypto.Key = clientSocket.Diffie.Key;
            clientSocket.Crypto.IV = CryptoRandom.NextBytes(16);
            clientSocket.Send(MsgLogin.Create("asd", "asdasd", true, MsgLoginType.Login));
        }
    }
}