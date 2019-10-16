using System.Collections.Concurrent;
using Universal.IO.FastConsole;

namespace Universal.IO.Sockets.Pools
{
    public class BufferPool
    {
        public static BufferPool Instance { get; set; } = new BufferPool();
        private readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();

        public BufferPool()
        {
            Fill();
        }

        private void Fill()
        {
            for (var i = 0; i < 128; i++)
            {
                _queue.Enqueue(new byte[320]);
            }
        }

        public byte[] Get()
        {
            if (_queue.IsEmpty)
                Fill();
            if (_queue.TryDequeue(out var buffer))
                return buffer;
            FConsole.WriteLine("Fucking queue starved.");
            return new byte[320];
        }

        public void Return(byte[] buffer) => _queue.Enqueue(buffer);
    }
}
