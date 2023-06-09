using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Filename : StringParam
{
    public override void Write(ByteWriter writer)
    {
        writer.WriteUnicode(Value);
    }
}