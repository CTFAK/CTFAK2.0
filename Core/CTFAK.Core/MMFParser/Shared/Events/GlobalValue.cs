using System;

namespace CTFAK.MMFParser.Shared.Events;

internal class GlobalValue : Short
{
    public override string ToString()
    {
        if (Value > 26) return $"GlobalValue{Value}";
        return $"GlobalValue{Convert.ToChar(Value).ToString().ToUpper()}";
    }
}