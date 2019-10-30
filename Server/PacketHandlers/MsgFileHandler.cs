using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server.Entities;
using Universal.IO;
using Universal.IO.FastConsole;
using Universal.Packets;

namespace Server.PacketHandlers
{
    internal class MsgFileHandler
    {
        public static Dictionary<string, string> Tokens = new Dictionary<string, string>();
        public static ConcurrentDictionary<string, FileStream> Streams = new ConcurrentDictionary<string, FileStream>();
        internal static void Process(User user, byte[] packet)
        {
            var msgFile = (MsgFile)packet;
            var token = msgFile.GetToken();
            var kvp = user.Tokens.FirstOrDefault(n => n.Value == token);
            var path = "/dev/null";// + kvp.Key;

            if (string.IsNullOrEmpty(kvp.Key))
                user.Disconnect("No token");

            var mode = msgFile.CreateFile ? FileMode.Create : FileMode.Append;

            if (mode == FileMode.Append)
            {
                if (!Streams.TryGetValue(path, out var stream))
                {
                    stream = new FileStream(path, mode);
                    Streams.TryAdd(path, stream);
                }
                var chunk = msgFile.GetChunk();
                stream.Write(chunk);
                Log(path, stream);
                if (stream.Position == msgFile.FileSize)
                {
                    user.Tokens.Remove(kvp.Key);
                    Tokens.Remove(kvp.Key);
                    Log(path, stream);
                    Streams.TryRemove(path, out _);
                    stream.Dispose();
                }
            }
        }
        private static void Log(string path, FileStream filestream)
        {
            var count = 0;
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