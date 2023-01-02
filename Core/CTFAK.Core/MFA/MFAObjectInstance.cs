using CTFAK.CCN.Chunks;
using CTFAK.Memory;

namespace CTFAK.MFA;

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

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(X);
        Writer.WriteInt32(Y);
        Writer.WriteUInt32(Layer);
        Writer.WriteInt32(Handle);
        Writer.WriteUInt32(Flags);
        Writer.WriteUInt32(ParentType);
        Writer.WriteUInt32(ItemHandle);
        Writer.WriteInt32((int)ParentHandle);
    }
}