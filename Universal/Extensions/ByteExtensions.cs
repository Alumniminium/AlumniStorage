using Universal.Packets.Enums;

namespace Universal.Extensions
{
    public static class ByteExtensions
    {
        public static PacketType GetPacketType(this byte[] packet) => (PacketType)packet[2];
    }
}
