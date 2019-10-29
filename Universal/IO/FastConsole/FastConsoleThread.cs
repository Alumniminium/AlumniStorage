using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace Universal.IO.FastConsole
{
    public static class FastConsoleThread
    {
        internal static readonly Thread WorkerThread;
        internal static readonly BlockingCollection<ConsoleJob> Queue = new BlockingCollection<ConsoleJob>();
        internal static readonly StringBuilder Builder = new StringBuilder();
        static FastConsoleThread()
        {
            WorkerThread = new Thread(WorkLoop) { IsBackground = true };
            WorkerThread.Start();
        }

        public static void Add(string msg, ConsoleColor color) => Queue.Add(new ConsoleJob(msg, color));

        private static void WorkLoop()
        {
            foreach (var job in Queue.GetConsumingEnumerable())
            {
                //while (Queue.TryDequeue(out var msg))
                //    Builder.AppendLine(msg);
                //Console.WriteLine(Builder);
                //Builder.Clear();
                Console.ForegroundColor = job.Color;
                Console.WriteLine(job.Text);
                Console.ResetColor();//TODO Benchmark/eval if we need to reset. Prolly expensive.
            }
        }
    }
}