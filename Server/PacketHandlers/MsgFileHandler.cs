using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server.Entities;
using Universal.IO;
using Universal.IO.FastConsole;
using Universal.Packets;

namespace Server
{
    internal class MsgFileHandler
    {
        public static Dictionary<string, string> Tokens = new Dictionary<string, string>();
        internal static void Process(User user, byte[] packet)
        {
            var msgFile = (MsgFile)packet;
            var token = msgFile.GetToken();
            var kvp = user.Tokens.FirstOrDefault(n => n.Value == token);
            var path = "/tmp/" + kvp.Key;

            if (string.IsNullOrEmpty(kvp.Key))
                user.Disconnect("No token");

            var mode = msgFile.CreateFile ? FileMode.Create : FileMode.Append;

            using (var filestream = new FileStream(path, mode))
            {
                var chunk = msgFile.GetChunk();
                filestream.Write(chunk);
                Log(path, filestream);
                if (filestream.Position == msgFile.FileSize)
                {
                    user.Tokens.Remove(kvp.Key);
                    Tokens.Remove(kvp.Key);
                    Log(path, filestream);
                }
            }
        }

        private static void Log(string path, FileStream filestream)
        {
            int count = 0;
            double size = filestream.Position;
            while (size > 1000)
            {
                size = size / 1024;
                count++;
            }
            FConsole.WriteLine($"File {path} ({size.ToString("###.##")} {(FormatEnum)count}) received!");
        }
    }
}