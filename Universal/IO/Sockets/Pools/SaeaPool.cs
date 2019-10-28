using System.Net.Sockets;
using System.Collections.Concurrent;
using Universal.IO.FastConsole;
using System;

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
            for (var i = 0; i < 128; i++)
            {
                _queue.Enqueue(new SocketAsyncEventArgs());
            }
        }

        public static SocketAsyncEventArgs Get()
        {
            if (_queue.IsEmpty)
            {
                FConsole.WriteLine("Fucking queue starved.");
                Fill();
            }
            if (_queue.TryDequeue(out var e))
                return e;
            Get(f =>
            {
                new SocketAsyncEventArgs()


            });
            return Get();
        }
        public static SocketAsyncEventArgs Get(Func<SocketAsyncEventArgs> f)
        {
            return f.Invoke();
            if (_queue.IsEmpty)
            {
                FConsole.WriteLine("Fucking queue starved.");
                Fill();
            }
            if (_queue.TryDequeue(out var e))
                return e;

            return Get();
        }

        public static void Return(SocketAsyncEventArgs e) => _queue.Enqueue(e);
    }
}
