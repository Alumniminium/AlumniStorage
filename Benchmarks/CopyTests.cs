using System;
using BenchmarkDotNet.Attributes;
using Universal.Extensions;
using System.Runtime.CompilerServices;

namespace Benchmarks
{
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public unsafe class CopyTests
    {
        private MyStruct _structSource;
        private byte[] _source;
        private byte[] _destination;

        [Params(32, 500)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            _source = new byte[N];
            _destination = new byte[N];
            new Random(42).NextBytes(_source);
        }
        [Benchmark]
        public byte[] UnsafeCopy()
        {
            fixed(byte* ptr = _structSource.FixedSource)
            return Unsafe.Read<byte[]>(ptr);
        }
        [Benchmark]
        public Span<byte> UnsafeSpanCopy()
        {
            fixed (byte* b = _structSource.FixedSource)
                return new Span<byte>(b, 100_000);
        }
        
        public byte[] SpanCopy()
        {
            _source.AsSpan().CopyTo(_destination);
            return _destination;
        }
        public byte[] VectorizedCopy()
        {
            _source.VectorizedCopy(0, _destination, 0, _destination.Length);
            return _destination;
        }
        public byte[] ArrayCopy()
        {
            Array.Copy(_source, 0, _destination, 0, _destination.Length);
            return _destination;
        }

        public byte[] BufferCopy()
        {
            Buffer.BlockCopy(_source, 0, _destination, 0, _destination.Length);
            return _destination;
        }
        public byte[] UnsafeForCopy()
        {
            fixed (byte* ps = _source)
            fixed (byte* pd = _destination)
            {
                for (var i = 0; i < _destination.Length; i++)
                {
                    pd[i] = ps[i];
                }
            }
            return _destination;
        }
        public byte[] ForCopy()
        {
            for (var i = 0; i < _destination.Length; i++)
            {
                _destination[i] = _source[i];
            }
            return _destination;
        }
    }
}
