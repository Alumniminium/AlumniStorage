using System;
using System.IO;
using Client.Entities;
using Universal.IO;
using Universal.Packets;

internal class MsgFileHandler
{
    internal static void Process(User user, MsgFile msgFile)
    {
        var CurrentFileName = "/tmp/" + msgFile.GetToken();

        var mode = FileMode.Create;
        if (File.Exists(CurrentFileName))
            mode = FileMode.Append;
        if (msgFile.CreateFile)
            mode = FileMode.Truncate;

        var chunk = msgFile.GetChunk();
        using (var filestream = new FileStream(CurrentFileName, mode))
        {
            filestream.Write(chunk);
            if (filestream.Position == msgFile.FileSize)
            {
                int count = 0;
                double size = filestream.Position;
                while (size > 1000)
                {
                    size = size / 1024;
                    count++;
                }
                Console.WriteLine($"File {CurrentFileName} ({size.ToString("###.##")} {(FormatEnum)count}) received!");
            }
        }
    }
}