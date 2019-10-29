using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Universal.Extensions;
using Universal.Packets;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public unsafe class AllocTests
    {
        public MsgBench CachedMsg;
        public byte[] CachedArray;

        [GlobalSetup]
        public void Setup()
        {
            CachedMsg = MsgBench.Create(new byte[100_000], true);
            CachedArray = CachedMsg;
        }
        [Benchmark]
        public MsgBench ByteToMsgUnsafePointer()
        {
            fixed (byte* p = CachedArray)
                return *(MsgBench*)p;
        }
        [Benchmark]
        public MsgBench ByteToMsg()
        {
            return CachedArray.ToMsg<MsgBench>();
        }
        [Benchmark]
        public MsgBench Unsafe_Read()
        {
            fixed (byte* ptr = CachedArray)
                return Unsafe.Read<MsgBench>(ptr);
        }
        [Benchmark]
        public MsgBench MemoryMarshal_Read()
        {
            return MemoryMarshal.Read<MsgBench>(CachedArray);
        }
    }
}
