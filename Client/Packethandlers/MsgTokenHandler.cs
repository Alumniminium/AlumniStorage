using Client.Entities;
using Universal.Packets;

namespace Client.Packethandlers
{
    internal class MsgTokenHandler
    {
        public static void Process(User user, MsgToken msgToken)
        {
            if (user.Tokens.ContainsKey(msgToken.UniqueId))
                user.Tokens[msgToken.UniqueId] = msgToken.GetToken;
            else
                user.Tokens.TryAdd(msgToken.UniqueId, msgToken.GetToken);
        }
    }
}