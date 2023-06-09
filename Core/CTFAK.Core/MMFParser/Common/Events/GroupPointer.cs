using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class GroupPointer : ParameterCommon
{
    public short Id;
    public int Pointer;

    public override void Read(ByteReader reader)
    {
        Pointer = reader.ReadInt32();
        Id = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Pointer);
        writer.WriteInt32(Id);
    }
}