namespace Universal.Packets.Enums
{
    public enum PacketType : byte
    {
        MsgHandshake = 0,
        MsgLogin = 1,
        MsgFile = 2,
        MsgToken = 3,
        MsgBench = 10,
    }
}