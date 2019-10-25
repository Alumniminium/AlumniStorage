using System.Buffers;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Universal.Packets;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public unsafe class AllocTests
    {
        public MsgLogin CachedMsg;
        public byte[] CachedArray;

        [GlobalSetup]
        public void Setup()
        {
            CachedMsg = MsgLogin.Create("user", "pass", true, Universal.Packets.Enums.MsgLoginType.Login);
            CachedArray = CachedMsg;
        }        
        [Benchmark]
        public byte[] Cast_Safe_TryWrite()
        {
            MemoryMarshal.TryWrite(CachedArray, ref CachedMsg);
            return CachedArray;
        }        
        [Benchmark]
        public byte[] Cast_Safe_Write()
        {
            MemoryMarshal.Write(CachedArray, ref CachedMsg);
            return CachedArray;
        }
        [Benchmark]
        public byte[] Cast_Unsafe_Pointer()
        {
            var cachedMsg = CachedMsg;
            fixed (byte* p = CachedArray)
                *(MsgLogin*)p = *&cachedMsg;

            return CachedArray;
        }
    }
}
