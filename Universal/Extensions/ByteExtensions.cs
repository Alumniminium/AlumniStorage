using System.Runtime.CompilerServices;
using Universal.Packets.Enums;

namespace Universal.Extensions
{
    public static class ByteExtensions
    {
        public static PacketType GetPacketType(this byte[] packet) => (PacketType)packet[2];
        public static unsafe T ToMsg<T>(this byte[] msg)
        {
            fixed (byte* p = msg)
                return Unsafe.Read<T>(p);
        }
    }
}
