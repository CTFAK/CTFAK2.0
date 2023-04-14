using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Create : ParameterCommon
    {
        public int ObjectInstances;
        public int ObjectInfo;
        public Position Position;

        public override void Read(ByteReader reader)
        {
            Position = new Position();
            Position.Read(reader);
            ObjectInstances = reader.ReadUInt16();
            ObjectInfo = reader.ReadUInt16();
            // Reader.Skip(4);
        }

        public override void Write(ByteWriter Writer)
        {
            Position.Write(Writer);
            Writer.WriteUInt16((ushort) ObjectInstances);
            Writer.WriteUInt16((ushort) ObjectInfo);
            // Writer.Skip(4);
        }

        public override string ToString()
        {
            return $"Create obj instance:{ObjectInstances} info:{ObjectInfo} pos:({Position.ToString()})";
        }
    }
}
