using System.Collections.Generic;
using System.Drawing;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;

namespace CTFAK.MFA.MFAObjectLoaders;

public class MFAText : ObjectLoader
{
    public Color Color;
    public uint Flags;
    public uint Font;
    public uint Height;
    public List<MFAParagraph> Items;
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
        Items = new List<MFAParagraph>();
        var parCount = reader.ReadUInt32();
        for (var i = 0; i < parCount; i++)
        {
            var par = new MFAParagraph();
            par.Read(reader);
            Items.Add(par);
        }
    }

    public override void Write(ByteWriter Writer)
    {
        base.Write(Writer);
        Writer.WriteUInt32(Width);
        Writer.WriteUInt32(Height);
        Writer.WriteUInt32(Font);
        Writer.WriteColor(Color);
        Writer.WriteUInt32(Flags);
        Writer.WriteInt32(0);
        Writer.WriteUInt32((uint)Items.Count);
        foreach (var paragraph in Items) paragraph.Write(Writer);
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

    public override void Write(ByteWriter Writer)
    {
        Writer.AutoWriteUnicode(Value);
        Writer.WriteUInt32(Flags);
    }
}