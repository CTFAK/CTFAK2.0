using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Extension:ParameterCommon
    {
        public short Size;
        public short Type;
        public short Code;
        public byte[] Data;

        public Extension(ByteReader reader) : base(reader)
        {
        }

        public override void Read()
        {
            Size = reader.ReadInt16();
            Type = reader.ReadInt16();
            Code = reader.ReadInt16();
            Data = reader.ReadBytes((Size-20>0?Size-20:0));

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt16((short) (Data.Length+6));
            Writer.WriteInt16(Type);
            Writer.WriteInt16(Code);
            Writer.WriteBytes(Data);
        }
    }
}