using System.Drawing;
using CTFAK.Memory;

namespace CTFAK.MMFParser.Shared.Events;

internal class Colour : ParameterCommon
{
    public Color Value;

    public override void Read(ByteReader reader)
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