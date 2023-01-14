using CTFAK.Memory;

namespace CTFAK.MMFParser.Shared.Events;

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

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(Timer);
        Writer.WriteInt32(Loops);
        Writer.WriteInt16(Comparsion);
    }

    public override string ToString()
    {
        return $"Time time: {Timer} loops: {Loops}";
    }
}