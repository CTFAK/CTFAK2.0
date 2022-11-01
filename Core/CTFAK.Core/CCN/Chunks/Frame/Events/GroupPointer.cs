using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class GroupPointer:ParameterCommon
    {
        public int Pointer;
        public short Id;



        public override void Read(ByteReader reader)
        {
            Pointer = reader.ReadInt32();
            Id = reader.ReadInt16();
            
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Pointer);
            Writer.WriteInt32(Id);
        }
    }
}