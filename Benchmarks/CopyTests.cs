using System;
using BenchmarkDotNet.Attributes;
using Universal.Extensions;

namespace Benchmarks
{
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class CopyTests
    {
        private byte[] source;
        private byte[] destination;

        [Params(32, 500)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            source = new byte[N];
            destination = new byte[N];
            new Random(42).NextBytes(source);
        }
        [Benchmark]
        public byte[] SpanCopy()
        {
            source.AsSpan().CopyTo(destination);
            return destination;
        }
        [Benchmark]
        public byte[] VectorizedCopy()
        {
            source.VectorizedCopy(0, destination, 0, destination.Length);
            return destination;
        }
        public byte[] ArrayCopy()
        {
            Array.Copy(source, 0, destination, 0, destination.Length);
            return destination;
        }

        public byte[] BufferCopy()
        {
            Buffer.BlockCopy(source, 0, destination, 0, destination.Length);
            return destination;
        }
        public unsafe byte[] UnsafeForCopy()
        {
            fixed (byte* ps = source)
            fixed (byte* pd = destination)
            {
                for (int i = 0; i < destination.Length; i++)
                {
                    pd[i] = ps[i];
                }
            }
            return destination;
        }
        public byte[] ForCopy()
        {
            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = source[i];
            }
            return destination;
        }
    }
}
