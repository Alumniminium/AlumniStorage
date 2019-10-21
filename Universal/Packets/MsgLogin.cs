using System;
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
        public const int MAX_EMAIL_LENGTH = 32;

        public int Length { get; private set; }
        public PacketType Id { get; private set; }
        public int UniqueId { get; set; }
        public bool ClientSupportCompression { get; set; }
        public MsgLoginType Type { get; set; }

        public fixed char Username[MAX_USERNAME_LENGTH];
        public fixed char Password[MAX_PASSWORD_LENGTH];
        public fixed char Email[MAX_EMAIL_LENGTH];

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
        public string GetEmail()
        {
            fixed (char* p = Email)
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
        public void SetEmail(string email)
        {
            email = email.ToLength(MAX_EMAIL_LENGTH);
            for (var i = 0; i < email.Length; i++)
                Email[i] = email[i];
        }

        public static MsgLogin Create(string user, string pass, string email, bool compression, MsgLoginType type)
        {
            Span<MsgLogin> span = stackalloc MsgLogin[1];
            ref var ptr = ref MemoryMarshal.GetReference(span);
            ptr.Length = (short)sizeof(MsgLogin);
            ptr.Id = PacketType.MsgLogin;
            ptr.Type = type;
            ptr.ClientSupportCompression = compression;
            ptr.SetUsername(user);
            ptr.SetPassword(pass);
            ptr.SetEmail(email);
            return ptr;
        }
        public static implicit operator byte[](MsgLogin msg)
        {
            Span<byte> buffer = stackalloc byte[sizeof(MsgLogin)];
            fixed (byte* p = buffer)
                *(MsgLogin*)p = *&msg;
            return buffer.ToArray();
        }
        public static implicit operator MsgLogin(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgLogin*)p;
        }
    }
}
