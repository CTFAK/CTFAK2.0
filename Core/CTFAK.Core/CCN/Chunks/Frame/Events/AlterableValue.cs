using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
    public class AlterableValue : Short
    {


        public override string ToString()
        {
            return $"AlterableValue{Value.ToString().ToUpper()}";
        }
    }
}
