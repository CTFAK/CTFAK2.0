namespace CTFAK.MMFParser.Common.Events;

public class AlterableValue : Short
{
    public override string ToString()
    {
        return $"AlterableValue{Value.ToString().ToUpper()}";
    }
}