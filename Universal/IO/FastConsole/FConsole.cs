using System;

namespace Universal.IO.FastConsole
{
    public static class FConsole
    {
        public static string Title { get => Console.Title; set => Console.Title = value; }

        public static void WriteLine(string msg, ConsoleColor color = ConsoleColor.White)
        {
            FastConsoleThread.Add(msg, color);
        }
        public static void WriteLine(object msg)
        {
            FastConsoleThread.Add(msg + Environment.NewLine, ConsoleColor.Blue);
        }

        public static void WriteLine(Exception msg)
        {
            FastConsoleThread.Add(msg + Environment.NewLine, ConsoleColor.Red);
        }

        public static string ReadLine() => Console.ReadLine();
    }
}