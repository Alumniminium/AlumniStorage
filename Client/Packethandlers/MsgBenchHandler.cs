using Client.Entities;
using Universal.IO.FastConsole;
using Universal.Packets;

namespace Client.Packethandlers
{
    public static class MsgBenchHandler
    {
        public static void Process(User user, MsgBench packet)
        {
            Program.Stopwatch.Stop();
            FConsole.WriteLine("Took: " + Program.Stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            Program.Stopwatch.Restart();
        }
    }
}