using System.Runtime.InteropServices;
using Universal.Packets.Enums;

namespace Universal.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgHeader
    {
        public static int SIZE=sizeof(MsgHeader);
        public int Length { get; set; }
        public bool Compressed { get; set; }
        public PacketType Id { get; set; }
    }
}
