using System.Drawing;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    class Colour : ParameterCommon
    {
        public Color Value;

        public Colour(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            var bytes = reader.ReadBytes(4);
            Value = Color.FromArgb(bytes[0], bytes[1], bytes[2]);
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt8(Value.R);
            Writer.WriteInt8(Value.G);
            Writer.WriteInt8(Value.B);
            Writer.WriteInt8(255);
        }
    }
}
