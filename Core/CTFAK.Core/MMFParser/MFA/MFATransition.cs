using System.Drawing;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA;

public class MFATransition : ChunkLoader
{
    public Color Color;
    public int Duration;
    public int Flags;
    public string Id;
    public string Module;
    public string Name;
    public byte[] ParameterData;
    public string TransitionId;

    public override void Read(ByteReader reader)
    {
        Module = reader.AutoReadUnicode();
        Name = reader.AutoReadUnicode();
        Id = reader.ReadAscii(4);
        TransitionId = reader.ReadAscii(4);
        Duration = reader.ReadInt32();
        Flags = reader.ReadInt32();
        Color = reader.ReadColor();
        ParameterData = reader.ReadBytes(reader.ReadInt32());
    }

    public override void Write(ByteWriter writer)
    {
        writer.AutoWriteUnicode(Module);
        writer.AutoWriteUnicode(Name);
        writer.WriteAscii(Id);
        writer.WriteAscii(TransitionId);
        writer.WriteInt32(Duration);
        writer.WriteInt32(Flags);
        writer.WriteColor(Color);
        writer.WriteInt32(ParameterData.Length);
        writer.WriteBytes(ParameterData);
    }
}