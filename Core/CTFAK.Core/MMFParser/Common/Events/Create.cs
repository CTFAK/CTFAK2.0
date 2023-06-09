using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Create : ParameterCommon
{
    public int ObjectInfo;
    public int ObjectInstances;
    public Position Position;

    public override void Read(ByteReader reader)
    {
        Position = new Position();
        Position.Read(reader);
        ObjectInstances = reader.ReadUInt16();
        ObjectInfo = reader.ReadUInt16();
        // Reader.Skip(4);
    }

    public override void Write(ByteWriter writer)
    {
        Position.Write(writer);
        writer.WriteUInt16((ushort)ObjectInstances);
        writer.WriteUInt16((ushort)ObjectInfo);
        // Writer.Skip(4);
    }

    public override string ToString()
    {
        return $"Create obj instance:{ObjectInstances} info:{ObjectInfo} pos:({Position})";
    }
}