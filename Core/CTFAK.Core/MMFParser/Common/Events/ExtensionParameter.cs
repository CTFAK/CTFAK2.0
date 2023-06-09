using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class ExtensionParameter : ParameterCommon
{
    public short Code;
    public byte[] Data;
    public short Size;
    public short Type;

    public override void Read(ByteReader reader)
    {
        Size = reader.ReadInt16();
        Type = reader.ReadInt16();
        Code = reader.ReadInt16();
        Data = reader.ReadBytes(Size);
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16((short)(Data.Length + 6));
        writer.WriteInt16(Type);
        writer.WriteInt16(Code);
        writer.WriteBytes(Data);
    }
}