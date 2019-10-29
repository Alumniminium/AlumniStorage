using System.Net.Sockets;
using System.Collections.Concurrent;
using Universal.IO.FastConsole;

namespace Universal.IO.Sockets.Pools
{
    public static class SaeaPool
    {
        private static readonly ConcurrentQueue<SocketAsyncEventArgs> Queue = new ConcurrentQueue<SocketAsyncEventArgs>();

        static SaeaPool()
        {
            Fill();
        }

        private static void Fill()
        {
            for (var i = 0; i < 3; i++)
            {
                Queue.Enqueue(new SocketAsyncEventArgs());
            }
        }

        public static SocketAsyncEventArgs Get()
        {
            while (true)
            {
                if (Queue.IsEmpty)
                {
                    FConsole.WriteLine("Fucking queue starved.");
                    Fill();
                }

                if (Queue.TryDequeue(out var e)) 
                    return e;
            }
        }

        public static void Return(SocketAsyncEventArgs e) => Queue.Enqueue(e);
    }
}
