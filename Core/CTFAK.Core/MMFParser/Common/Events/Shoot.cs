using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Shoot : ParameterCommon
{
    public ushort ObjectInfo;
    public ushort ObjectInstance;
    public Position ShootPos;
    public short ShootSpeed;

    public override void Read(ByteReader reader)
    {
        ShootPos = new Position();
        ShootPos.Read(reader);
        ObjectInstance = reader.ReadUInt16();
        ObjectInfo = reader.ReadUInt16();
        reader.Skip(4);
        ShootSpeed = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        ShootPos.Write(writer);
        writer.WriteUInt16(ObjectInstance);
        writer.WriteUInt16(ObjectInfo);
        writer.Skip(4);
        writer.WriteInt16(ShootSpeed);
    }

    public override string ToString()
    {
        return $"Shoot {ShootPos.X}x{ShootPos.Y}";
    }
}