using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

internal class Float : ParameterCommon
{
    public float Value;

    public override void Read(ByteReader reader)
    {
        Value = reader.ReadSingle();
    }

    public override string ToString()
    {
        return $"{GetType().Name} value: {Value}";
    }
}