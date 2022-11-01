using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Short : ParameterCommon
    {
        public short Value;

        public override void Read(ByteReader reader)
        {
            Value = reader.ReadInt16();
            
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16(Value);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} value: {Value}";
        }
    }
}
