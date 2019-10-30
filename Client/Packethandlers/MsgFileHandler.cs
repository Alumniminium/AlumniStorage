using System.Collections.Concurrent;
using System;
using System.IO;
using Client.Entities;
using Universal.IO;
using Universal.Packets;
using Universal.IO.FastConsole;

namespace Client.Packethandlers
{
    internal static class MsgFileHandler
    {
        internal static void Process(User user, MsgFile msgFile)
        {
            var currentFileName = "/tmp/" + msgFile.GetToken();

            var mode = FileMode.Append;
            if (msgFile.CreateFile)
                mode = FileMode.Truncate;

            var chunk = msgFile.GetChunk();
        }
    }
}