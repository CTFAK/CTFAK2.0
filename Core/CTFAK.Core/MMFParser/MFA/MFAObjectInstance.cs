using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA;

public class MFAObjectInstance : ChunkLoader
{
    public uint Flags;
    public int Handle;
    public uint ItemHandle;
    public uint Layer;
    public uint ParentHandle;
    public uint ParentType;
    public int X;
    public int Y;

    public override void Read(ByteReader reader)
    {
        X = reader.ReadInt32();
        Y = reader.ReadInt32();
        Layer = reader.ReadUInt32();
        Handle = reader.ReadInt32();
        Flags = reader.ReadUInt32();
        ParentType = reader.ReadUInt32();
        ItemHandle = reader.ReadUInt32();
        ParentHandle = (uint)reader.ReadInt32();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(X);
        writer.WriteInt32(Y);
        writer.WriteUInt32(Layer);
        writer.WriteInt32(Handle);
        writer.WriteUInt32(Flags);
        writer.WriteUInt32(ParentType);
        writer.WriteUInt32(ItemHandle);
        writer.WriteInt32((int)ParentHandle);
    }
}