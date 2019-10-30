using System;
using Server.Entities;
using Universal.IO.FastConsole;
using Universal.Packets;

namespace Server.PacketHandlers
{
    internal class MsgTokenHandler
    {
        public static void Process(User user, byte[] packet)
        {
            var msgToken = (MsgToken)packet;
            var path = msgToken.GetToken;

            string token;
            if (!user.Tokens.ContainsKey(path))
            {
                token = Guid.NewGuid().ToString().Replace("-", "");
                user.Tokens.Add(path, token);
            }
            else
                token = user.Tokens[path];

            FConsole.WriteLine("Token Created: " + token + " for: " + path);

            msgToken = MsgToken.Create(token, 0, true);
            user.Send(msgToken);
        }
    }
}