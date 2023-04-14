using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class GlobalValue : Short
    {



        public override string ToString()
        {
            if(Value>26) return $"GlobalValue{Value}";
            return $"GlobalValue{Convert.ToChar(Value).ToString().ToUpper()}";
        }
    }
}
