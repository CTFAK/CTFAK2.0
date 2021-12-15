using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    class ParamObject : ParameterCommon
    {
        public int ObjectInfoList;
        public int ObjectInfo;
        public int ObjectType;
        public ParamObject(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            
            ObjectInfoList = reader.ReadInt16();
            ObjectInfo = reader.ReadUInt16();
            ObjectType = reader.ReadInt16();           
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16((short) ObjectInfoList);
            Writer.WriteInt16((short)ObjectInfo);
            Writer.WriteInt16((short)ObjectType);
            
        }

        public override string ToString()
        {
            return $"Object {ObjectInfoList} {ObjectInfo} {ObjectType}";
        }
    }
}
