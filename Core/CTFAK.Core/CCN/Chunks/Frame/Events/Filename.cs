using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class Filename:StringParam
    {


        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUnicode(Value);
        }
    }
}