namespace CTFAK.MMFParser.Shared.Events;

public class AlterableValue : Short
{
    public override string ToString()
    {
        return $"AlterableValue{Value.ToString().ToUpper()}";
    }
}