using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Click:ParameterCommon
    {
        public byte IsDouble;
        public byte Button;



        public override void Read(ByteReader reader)
        {
            Button = reader.ReadByte();
            IsDouble = reader.ReadByte();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt8(Button);
            Writer.WriteInt8(IsDouble);
            
        }

        public override string ToString()
        {
            return $"{Button}-{IsDouble}";
        }
    }
}