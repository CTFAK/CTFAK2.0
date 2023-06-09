using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class TwoShorts : ParameterCommon
{
    public short Value1;
    public short Value2;

    public override void Read(ByteReader reader)
    {
        Value1 = reader.ReadInt16();
        Value2 = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(Value1);
        writer.WriteInt16(Value2);
    }

    public override string ToString()
    {
        return $"Shorts: {Value1} and {Value2}";
    }
}