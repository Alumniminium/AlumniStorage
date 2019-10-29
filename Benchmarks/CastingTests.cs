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
            ptr->Length = sizeof(MsgLogin);
            ptr->Id = PacketType.MsgLogin;
            ptr->Compressed = true;
            ptr->SetUsername("user");
            ptr->SetPassword("pass");
            return *ptr;
        }
        public MsgLogin Allocate_Stackalloc_Span()
        {
            Span<MsgLogin> span = stackalloc MsgLogin[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Id = PacketType.MsgLogin;
            ptr.Length = sizeof(MsgLogin);
            ptr.Compressed = true;
            ptr.SetUsername("user");
            ptr.SetPassword("pass");
            return ptr;
        } 
        public MsgLogin Allocate_New()
        {
            var ptr = new MsgLogin
            {
                Id = PacketType.MsgLogin, 
                Length = sizeof(MsgLogin), 
                Compressed = true
            };
            ptr.SetUsername("user");
            ptr.SetPassword("pass");
            return ptr;
        }
    }
}
