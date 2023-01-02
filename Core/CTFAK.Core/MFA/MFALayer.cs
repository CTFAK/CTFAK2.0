using CTFAK.CCN.Chunks;
using CTFAK.Memory;

namespace CTFAK.MFA;

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

    public override void Write(ByteWriter Writer)
    {
        Writer.AutoWriteUnicode(Name);
        Writer.WriteInt32((int)Flags.flag);
        Writer.WriteSingle(XCoefficient);
        Writer.WriteSingle(YCoefficient);
    }

    public override void Read(ByteReader reader)
    {
        Name = reader.AutoReadUnicode();
        Flags.flag = (uint)reader.ReadInt32();
        XCoefficient = reader.ReadSingle();
        YCoefficient = reader.ReadSingle();
    }
}