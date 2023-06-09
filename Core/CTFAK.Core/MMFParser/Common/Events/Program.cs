using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

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

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(Flags);
        writer.WriteAscii(Filename);
        writer.WriteAscii(Command);
    }
}