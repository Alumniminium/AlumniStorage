using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Universal.Packets;
using Universal.Packets.Enums;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public unsafe class CastingTests
    {
        public MsgLogin Allocate_Stackalloc()
        {
            var ptr = stackalloc MsgLogin[1];
            ptr->Header.Length = sizeof(MsgLogin);
            ptr->Header.Id = PacketType.MsgLogin;
            ptr->Header.Compressed = true;
            ptr->SetUsername("user");
            ptr->SetPassword("pass");
            return *ptr;
        }
        public MsgLogin Allocate_Stackalloc_Span()
        {
            Span<MsgLogin> span = stackalloc MsgLogin[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Header.Id = PacketType.MsgLogin;
            ptr.Header.Length = sizeof(MsgLogin);
            ptr.Header.Compressed = true;
            ptr.SetUsername("user");
            ptr.SetPassword("pass");
            return ptr;
        } 
        public MsgLogin Allocate_New()
        {
            var ptr = new MsgLogin {Header = {Id = PacketType.MsgLogin, Length = sizeof(MsgLogin), Compressed = true}};
            ptr.SetUsername("user");
            ptr.SetPassword("pass");
            return ptr;
        }
    }
}
