using CTFAK.Memory;

namespace CTFAK.MMFParser.Shared.Events;

public class StringParam : ParameterCommon
{
    public string Value;

    public override void Read(ByteReader reader)
    {
        Value = reader.ReadAscii();
    }

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteAscii(Value);
    }

    public override string ToString()
    {
        return $"String: {Value}";
    }
}