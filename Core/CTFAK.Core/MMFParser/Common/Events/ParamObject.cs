using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

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

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16((short)ObjectInfoList);
        writer.WriteInt16((short)ObjectInfo);
        writer.WriteInt16((short)ObjectType);
    }

    public override string ToString()
    {
        return $"Object {ObjectInfoList} {ObjectInfo} {ObjectType}";
    }
}