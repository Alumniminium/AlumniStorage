using System;
using System.Buffers;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Universal.Packets;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class AllocTests
    {
        public MsgLogin CachedMsg;
        public byte[] CachedArray;
        [GlobalSetup]
        public void Setup()
        {
            CachedMsg = MsgLogin.Create("user", "pass", true, Universal.Packets.Enums.MsgLoginType.Login);
            CachedArray = CachedMsg;
        }

        public void Stackalloc()
        {
            MsgLogin msg = MsgLogin.Create("user", "pass", true, Universal.Packets.Enums.MsgLoginType.Login);
        }

        public void Span()
        {
            MsgLogin msg = MsgLogin.CreateSpan("user", "pass", true, Universal.Packets.Enums.MsgLoginType.Login);
        }

        [Benchmark]
        public unsafe byte[] DeserializeUnsafe()
        {
            var buffer = ArrayPool<byte>.Shared.Rent(CachedMsg.Header.Length);
            var cachedMsg = CachedMsg;
            fixed (byte* p = buffer)
                *(MsgLogin*)p = *&cachedMsg;

            return buffer;
        }
        [Benchmark]
        public unsafe byte[] DeserializeMemoryMarhsal()
        {
            var buffer = ArrayPool<byte>.Shared.Rent(CachedMsg.Header.Length);
            MemoryMarshal.TryWrite(buffer, ref CachedMsg);
            return buffer;
        }
    }
}
