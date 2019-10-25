using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Universal.Extensions;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgLogin
    {
        public const int MAX_USERNAME_LENGTH = 32;
        public const int MAX_PASSWORD_LENGTH = 32;

        public int Length{get;set;}
        public PacketType Id{get;set;}
        public bool Compressed{get;set;}
        public int UniqueId { get; set; }
        public MsgLoginType Type { get; set; }

        public fixed char Username[MAX_USERNAME_LENGTH];
        public fixed char Password[MAX_PASSWORD_LENGTH];

        public string GetUsername()
        {
            fixed (char* p = Username)
                return new string(p);
        }
        public string GetPassword()
        {
            fixed (char* p = Password)
                return new string(p);
        }

        public void SetUsername(string username)
        {
            username = username.ToLength(MAX_PASSWORD_LENGTH);
            for (var i = 0; i < username.Length; i++)
                Username[i] = username[i];
        }
        public void SetPassword(string password)
        {
            password = password.ToLength(MAX_PASSWORD_LENGTH);
            for (var i = 0; i < password.Length; i++)
                Password[i] = password[i];
        }
        public static MsgLogin Create(string user, string pass, bool compression, MsgLoginType type)
        {
            MsgLogin* ptr = stackalloc MsgLogin[1];
            ptr->Length = sizeof(MsgLogin);
            ptr->Compressed = compression;
            ptr->Id = PacketType.MsgLogin;
            ptr->Type = type;
            ptr->SetUsername(user);
            ptr->SetPassword(pass);
            return *ptr;
        }
        
        public static implicit operator byte[](MsgLogin msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgLogin));
            MemoryMarshal.Write<MsgLogin>(buffer,ref msg);
            return buffer;
        }
        public static implicit operator MsgLogin(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgLogin*)p;
        }
    }
}
