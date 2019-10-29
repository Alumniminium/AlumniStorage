using System;
using System.IO;
using Client.Entities;
using Universal.IO;
using Universal.Packets;

namespace Client.Packethandlers
{
    internal static class MsgFileHandler
    {
        internal static void Process(User user, MsgFile msgFile)
        {
            var currentFileName = "/tmp/" + msgFile.GetToken();

            var mode = FileMode.Create;
            if (File.Exists(currentFileName))
                mode = FileMode.Append;
            if (msgFile.CreateFile)
                mode = FileMode.Truncate;

            var chunk = msgFile.GetChunk();
            using (var filestream = new FileStream(currentFileName, mode))
            {
                filestream.Write(chunk);
                if (filestream.Position == msgFile.FileSize)
                {
                    var count = 0;
                    double size = filestream.Position;
                    while (size > 1000)
                    {
                        size = size / 1024;
                        count++;
                    }
                    Console.WriteLine($"File {currentFileName} ({size.ToString("###.##")} {(FormatEnum)count}) received!");
                }
            }
        }
    }
}