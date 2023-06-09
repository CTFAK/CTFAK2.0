using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Short : ParameterCommon
{
    public short Value;

    public override void Read(ByteReader reader)
    {
        Value = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(Value);
    }

    public override string ToString()
    {
        return $"{GetType().Name} value: {Value}";
    }
}