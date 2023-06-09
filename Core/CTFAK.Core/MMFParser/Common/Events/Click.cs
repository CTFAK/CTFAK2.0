using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Click : ParameterCommon
{
    public byte Button;
    public byte IsDouble;

    public override void Read(ByteReader reader)
    {
        Button = reader.ReadByte();
        IsDouble = reader.ReadByte();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt8(Button);
        writer.WriteInt8(IsDouble);
    }

    public override string ToString()
    {
        return $"{Button}-{IsDouble}";
    }
}