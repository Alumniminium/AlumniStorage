using System;
using BenchmarkDotNet.Attributes;
using Universal.Extensions;
using System.Runtime.CompilerServices;

namespace Benchmarks
{
    public unsafe struct MyStruct
    {
        public fixed byte fixedSource[100_000];
    }
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public unsafe class CopyTests
    {
        private MyStruct StructSource;
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
        public byte[] UnsafeCopy()
        {
            fixed(byte* ptr = StructSource.fixedSource)
            return Unsafe.Read<byte[]>(ptr);
        }
        [Benchmark]
        public Span<byte> UnsafeSpanCopy()
        {
            fixed (byte* b = StructSource.fixedSource)
                return new Span<byte>(b, 100_000);
        }
        
        public byte[] SpanCopy()
        {
            source.AsSpan().CopyTo(destination);
            return destination;
        }
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
