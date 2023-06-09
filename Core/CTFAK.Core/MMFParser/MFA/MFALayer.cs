using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA;

public class MFALayer : ChunkLoader
{
    public BitDict Flags = new(new[]
        {
            "Visible",
            "Locked",
            "Obsolete",
            "HideAtStart",
            "NoBackground",
            "WrapHorizontally",
            "WrapVertically",
            "PreviousEffect"
        }
    );

    public string Name = "Ass";
    public float XCoefficient;
    public float YCoefficient;

    public override void Write(ByteWriter writer)
    {
        writer.AutoWriteUnicode(Name);
        writer.WriteInt32((int)Flags.Flag);
        writer.WriteSingle(XCoefficient);
        writer.WriteSingle(YCoefficient);
    }

    public override void Read(ByteReader reader)
    {
        Name = reader.AutoReadUnicode();
        Flags.Flag = (uint)reader.ReadInt32();
        XCoefficient = reader.ReadSingle();
        YCoefficient = reader.ReadSingle();
    }
}