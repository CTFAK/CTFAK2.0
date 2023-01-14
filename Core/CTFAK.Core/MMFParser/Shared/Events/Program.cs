using CTFAK.Memory;

namespace CTFAK.MMFParser.Shared.Events;

public class Program : ParameterCommon
{
    public string Command;
    public string Filename;
    public short Flags;

    public override void Read(ByteReader reader)
    {
        Flags = reader.ReadInt16();
        Filename = reader.ReadAscii(260);
        Command = reader.ReadAscii();
    }

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt16(Flags);
        Writer.WriteAscii(Filename);
        Writer.WriteAscii(Command);
    }
}