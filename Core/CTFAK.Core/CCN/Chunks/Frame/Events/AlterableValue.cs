using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    class AlterableValue : Short
    {

        public AlterableValue(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            base.Read();

        }

        public override void Write(ByteWriter Writer)
        {
            base.Write(Writer);

        }

        public override string ToString()
        {
            return $"AlterableValue{Value.ToString().ToUpper()}";
        }
    }
}
