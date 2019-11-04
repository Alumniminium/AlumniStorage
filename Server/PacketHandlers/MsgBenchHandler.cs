using Universal.IO.Sockets.Client;
using Universal.Packets;

namespace Server.PacketHandlers
{
    internal static class MsgBenchHandler
    {
        public static void Process(ClientSocket user, byte[] packet)
        {
            var msgBench = (MsgBench)packet;
            msgBench = MsgBench.Create(new byte[64], false);
            user.Send(msgBench);
        }
    }
}