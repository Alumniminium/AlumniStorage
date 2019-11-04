using System.Security.Cryptography;
using System.Text;
using Universal.IO.Sockets.Client;
using Universal.IO.Sockets.Cryptography;
using Universal.Packets;

namespace Server.PacketHandlers
{
    public static class MsgDiffieHandler
    {
        public static void Process(ClientSocket clientSocket, MsgDH packet)
        {
            var b = packet.GetPayload();
            clientSocket.Diffie = new DiffieHellman(256).GenerateResponse(Encoding.ASCII.GetString(b));
            clientSocket.Send(MsgDH.Create(clientSocket.Diffie.ToString()));
            clientSocket.Crypto = new Crypto(clientSocket.Diffie.Key);
        }
    }
}