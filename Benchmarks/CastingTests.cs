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
        public unsafe MsgLogin Allocate_Stackalloc()
        {
            MsgLogin* ptr = stackalloc MsgLogin[1];
            ptr->Length = sizeof(MsgLogin);
            ptr->Id = PacketType.MsgLogin;
            ptr->Compressed = true;
            ptr->Type = 0;
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
            ptr.Type = 0;
            ptr.SetUsername("user");
            ptr.SetPassword("pass");
            return ptr;
        } 
        public MsgLogin Allocate_New()
        {
            MsgLogin ptr = new MsgLogin();
            ptr.Id = PacketType.MsgLogin;
            ptr.Length = sizeof(MsgLogin);
            ptr.Compressed = true;
            ptr.Type = 0;
            ptr.SetUsername("user");
            ptr.SetPassword("pass");
            return ptr;
        }
    }
}
