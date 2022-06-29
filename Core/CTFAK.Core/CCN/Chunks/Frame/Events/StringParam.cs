using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class StringParam:ParameterCommon
    {
        public string Value;

        public StringParam(ByteReader reader) : base(reader)
        {
            
        }

        public override void Read()
        {
            Value = reader.ReadAscii();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteAscii(Value);
        }

        public override string ToString()
        {
            return $"String: {Value}";
        }
    }
}