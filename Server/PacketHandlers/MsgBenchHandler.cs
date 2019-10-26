using Server.Entities;
using Universal.Packets;
using System.Linq;

namespace Server
{
    internal class MsgBenchHandler
    {
        public static void Process(User user, byte[] packet)
        {
            var msgBench = (MsgBench)packet;
            var array = msgBench.GetArray();
            array.Reverse();

            msgBench = MsgBench.Create(array, false);
            user.Send(msgBench);
        }
    }
}