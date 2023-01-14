using CTFAK.Memory;

namespace CTFAK.MMFParser.Shared.Events;

public class ParamObject : ParameterCommon
{
    public int ObjectInfo;
    public int ObjectInfoList;
    public int ObjectType;

    public override void Read(ByteReader reader)
    {
        ObjectInfoList = reader.ReadInt16();
        ObjectInfo = reader.ReadUInt16();
        ObjectType = reader.ReadInt16();
    }

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt16((short)ObjectInfoList);
        Writer.WriteInt16((short)ObjectInfo);
        Writer.WriteInt16((short)ObjectType);
    }

    public override string ToString()
    {
        return $"Object {ObjectInfoList} {ObjectInfo} {ObjectType}";
    }
}