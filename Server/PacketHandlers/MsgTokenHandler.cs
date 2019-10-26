using System;
using Server.Entities;
using Universal.IO.FastConsole;
using Universal.Packets;

namespace Server
{
    internal class MsgTokenHandler
    {
        public static void Process(User user, byte[] packet)
        {
            var msgToken = (MsgToken)packet;
            var path = msgToken.GetToken;

            var token = "";
            if (!user.Tokens.ContainsKey(path))
            {
                token = Guid.NewGuid().ToString().Replace("-", "");
                user.Tokens.Add(path, token);
            }
            else
                token = user.Tokens[path];

            FConsole.WriteLine("Token Created: " + token + " for: " + path);

            msgToken = MsgToken.Create(token, 0, false);
            user.Send(msgToken);
        }
    }
}