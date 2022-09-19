using System.Runtime.InteropServices;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class KeyParameter:ParameterCommon
    {
        public ushort Key;



        public override void Read(ByteReader reader)
        {
            Key = reader.ReadUInt16();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUInt16(Key);
        }

        public override string ToString()
        {
            return "Key-" + Key;
        }
    }
}