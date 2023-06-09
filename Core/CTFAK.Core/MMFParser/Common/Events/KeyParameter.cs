using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class KeyParameter : ParameterCommon
{
    public ushort Key;

    public override void Read(ByteReader reader)
    {
        Key = reader.ReadUInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt16(Key);
    }

    public override string ToString()
    {
        return "Key-" + Key;
    }
}