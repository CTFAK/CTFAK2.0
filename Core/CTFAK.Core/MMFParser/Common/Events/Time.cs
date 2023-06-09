using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Time : ParameterCommon
{
    public short Comparsion;
    public int Loops;
    public int Timer;

    public override void Read(ByteReader reader)
    {
        Timer = reader.ReadInt32();
        Loops = reader.ReadInt32();
        Comparsion = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Timer);
        writer.WriteInt32(Loops);
        writer.WriteInt16(Comparsion);
    }

    public override string ToString()
    {
        return $"Time time: {Timer} loops: {Loops}";
    }
}