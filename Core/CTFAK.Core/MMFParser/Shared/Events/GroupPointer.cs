using CTFAK.Memory;

namespace CTFAK.MMFParser.Shared.Events;

public class GroupPointer : ParameterCommon
{
    public short Id;
    public int Pointer;

    public override void Read(ByteReader reader)
    {
        Pointer = reader.ReadInt32();
        Id = reader.ReadInt16();
    }

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(Pointer);
        Writer.WriteInt32(Id);
    }
}