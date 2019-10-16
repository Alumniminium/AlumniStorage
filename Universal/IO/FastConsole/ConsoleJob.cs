using System;

namespace Universal.IO.FastConsole
{
    public class ConsoleJob
    {
        public string Text;
        public ConsoleColor Color;

        public ConsoleJob(string text, ConsoleColor color)
        {
            Text = text;
            Color = color;
        }
    }
}