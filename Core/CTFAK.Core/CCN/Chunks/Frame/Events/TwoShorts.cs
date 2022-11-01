using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class TwoShorts:ParameterCommon
    {
        public short Value1;
        public short Value2;



        public override void Read(ByteReader reader)
        {
            Value1 = reader.ReadInt16();
            Value2 = reader.ReadInt16();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16(Value1);
            Writer.WriteInt16(Value2);
        }

        public override string ToString()
        {
            return $"Shorts: {Value1} and {Value2}";
        }
    }
}