using System;
using Server.Entities;
using Universal.IO.Sockets.Client;
using Universal.Packets;

namespace Server.PacketHandlers
{
    internal static class MsgBenchHandler
    {
        public static void Process(ClientSocket user, byte[] packet)
        {
            var msgBench = (MsgBench)packet;
            var array = msgBench.GetArray();
            array.Reverse();

            msgBench = MsgBench.Create(new byte[100_000], false);
            user.Send(msgBench);
        }
    }
}