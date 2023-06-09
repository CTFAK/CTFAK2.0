using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Zone : ParameterCommon
{
    public short X1;
    public short X2;
    public short Y1;
    public short Y2;

    public override void Read(ByteReader reader)
    {
        X1 = reader.ReadInt16();
        Y1 = reader.ReadInt16();
        X2 = reader.ReadInt16();
        Y2 = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16(X1);
        writer.WriteInt16(Y1);
        writer.WriteInt16(X2);
        writer.WriteInt16(Y2);
    }

    public override string ToString()
    {
        return $"Zone ({X1}x{Y1})x({X2}x{Y2})";
    }
}