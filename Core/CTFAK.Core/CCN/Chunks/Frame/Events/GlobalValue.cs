using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    class GlobalValue : Short
    {


        public GlobalValue(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            base.Read();           
        }
        public override string ToString()
        {
            if(Value>26) return $"GlobalValue{Value}";
            return $"GlobalValue{Convert.ToChar(Value).ToString().ToUpper()}";
        }
    }
}
