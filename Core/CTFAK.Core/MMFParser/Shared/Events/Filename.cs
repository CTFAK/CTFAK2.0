using CTFAK.Memory;

namespace CTFAK.MMFParser.Shared.Events;

public class Filename : StringParam
{
    public override void Write(ByteWriter Writer)
    {
        Writer.WriteUnicode(Value);
    }
}