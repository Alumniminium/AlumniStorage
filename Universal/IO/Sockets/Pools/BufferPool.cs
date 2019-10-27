using System.Net.Sockets;
using System.Collections.Concurrent;
using Universal.IO.FastConsole;

namespace Universal.IO.Sockets.Pools
{
    public static class SaeaPool
    {
        private static ConcurrentQueue<SocketAsyncEventArgs> _queue = new ConcurrentQueue<SocketAsyncEventArgs>();

        static SaeaPool()
        {
            Fill();
        }

        private static void Fill()
        {
            for (var i = 0; i < 1; i++)
            {
                _queue.Enqueue(new SocketAsyncEventArgs());
            }
        }

        public static SocketAsyncEventArgs Get()
        {
            if (_queue.IsEmpty)
                Fill();
            if (_queue.TryDequeue(out var e))
                return e;
            FConsole.WriteLine("Fucking queue starved.");

            return Get();
        }

        public static void Return(SocketAsyncEventArgs e) => _queue.Enqueue(e);
    }
}
