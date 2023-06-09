using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Position : ParameterCommon
{
    public int Angle;
    public int Direction;
    public short Flags;
    public int Layer;
    public int ObjectInfoList;
    public int ObjectInfoParent;
    public int Slope;
    public int TypeParent;
    public int X;
    public int Y;

    public override void Read(ByteReader reader)
    {
        ObjectInfoParent = reader.ReadInt16();
        Flags = reader.ReadInt16();
        X = reader.ReadInt16();
        Y = reader.ReadInt16();
        Slope = reader.ReadInt16();
        Angle = reader.ReadInt16();
        Direction = reader.ReadInt32();
        TypeParent = reader.ReadInt16();
        ObjectInfoList = reader.ReadInt16();
        Layer = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16((short)ObjectInfoParent);
        writer.WriteInt16(Flags);
        writer.WriteInt16((short)X);
        writer.WriteInt16((short)Y);
        writer.WriteInt16((short)Slope);
        writer.WriteInt16((short)Angle);
        writer.WriteInt32(Direction);
        writer.WriteInt16((short)TypeParent);
        writer.WriteInt16((short)ObjectInfoList);
        writer.WriteInt16((short)Layer);
    }

    public override string ToString()
    {
        return $"Object X:{X} Y:{Y} Angle:{Angle} Direction:{Direction} Parent:{ObjectInfoList}";
    }
}