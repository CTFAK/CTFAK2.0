using System.Collections.Generic;
using System.Drawing;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class MFAText : ObjectLoader
{
    public Color Color;
    public uint Flags;
    public uint Font;
    public uint Height;
    public List<MFAParagraph> Items= new List<MFAParagraph>();
    public uint Width;

    public override void Read(ByteReader reader)
    {
        base.Read(reader);
        Width = reader.ReadUInt32();
        Height = reader.ReadUInt32();
        Font = reader.ReadUInt32();
        Color = reader.ReadColor();
        Flags = reader.ReadUInt32();
        reader.ReadUInt32();
        var parCount = reader.ReadUInt32();
        for (var i = 0; i < parCount; i++)
        {
            var par = new MFAParagraph();
            par.Read(reader);
            Items.Add(par);
        }
    }

    public override void Write(ByteWriter writer)
    {
        base.Write(writer);
        writer.WriteUInt32(Width);
        writer.WriteUInt32(Height);
        writer.WriteUInt32(Font);
        writer.WriteColor(Color);
        writer.WriteUInt32(Flags);
        writer.WriteInt32(0);
        writer.WriteUInt32((uint)Items.Count);
        foreach (var paragraph in Items) paragraph.Write(writer);
    }
}

public class MFAParagraph : ChunkLoader
{
    public uint Flags;
    public string Value;

    public override void Read(ByteReader reader)
    {
        Value = reader.AutoReadUnicode();
        Flags = reader.ReadUInt32();
    }

    public override void Write(ByteWriter writer)
    {
        writer.AutoWriteUnicode(Value);
        writer.WriteUInt32(Flags);
    }
}