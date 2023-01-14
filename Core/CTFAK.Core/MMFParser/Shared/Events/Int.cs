using CTFAK.Memory;

namespace CTFAK.MMFParser.Shared.Events;

internal class Int : ParameterCommon
{
    public int Value;

    public override void Read(ByteReader reader)
    {
        Value = reader.ReadInt32();
    }

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(Value);
    }
}