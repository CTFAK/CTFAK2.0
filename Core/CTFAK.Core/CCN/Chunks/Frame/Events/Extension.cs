using CTFAK.Memory;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class Extension : ParameterCommon
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

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt16((short)(Data.Length + 6));
        Writer.WriteInt16(Type);
        Writer.WriteInt16(Code);
        Writer.WriteBytes(Data);
    }
}