using Client.Entities;
using Universal.IO.FastConsole;
using Universal.Packets;

namespace Client.Packethandlers
{
    public class MsgBenchHandler
    {

        public static void Process(User user, MsgBench packet)
        {
            Program.Stopwatch.Stop();
            FConsole.WriteLine("Took: " + Program.Stopwatch.Elapsed.TotalMilliseconds);
            Program.Stopwatch.Restart();
            Program.Client.Send(MsgBench.Create(new byte[100], false));
        }

    }
}