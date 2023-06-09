using CTFAK.Memory;

namespace CTFAK.MMFParser.Common.Events;

public class Every : ParameterCommon
{
    public int Compteur;
    public int Delay;

    public override void Read(ByteReader reader)
    {
        Delay = reader.ReadInt32();
        Compteur = reader.ReadInt32();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Delay);
        writer.WriteInt32(Compteur);
    }

    public override string ToString()
    {
        return $"Every {Delay / 1000} sec";
    }
}