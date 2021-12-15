using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    class Short : ParameterCommon
    {
        public short Value;

        public Short(ByteReader reader) : base(reader) { }
        public override void Read()
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
