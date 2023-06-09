using System.Collections.Generic;
using System.Drawing;
using CTFAK.Memory;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class MFACounter : ObjectLoader
{
    public Color Color1;
    public Color Color2;
    public uint CountFlags;
    public int CountType;
    public uint DisplayType;
    public uint Font;
    public int Height;
    public List<int> Images;
    public int Maximum;
    public int Minimum;
    public int Value;
    public uint VerticalGradient;
    public int Width;

    public override void Read(ByteReader reader)
    {
        base.Read(reader);
        Value = reader.ReadInt32();
        Minimum = reader.ReadInt32();
        Maximum = reader.ReadInt32();
        DisplayType = reader.ReadUInt32();
        CountFlags = reader.ReadUInt32();
        Color1 = reader.ReadColor();
        Color2 = reader.ReadColor();
        VerticalGradient = reader.ReadUInt32();
        CountType = reader.ReadInt32();
        Width = reader.ReadInt32();
        Height = reader.ReadInt32();
        Images = new List<int>();
        var imageCount = reader.ReadUInt32();
        for (var i = 0; i < imageCount; i++) Images.Add((int)reader.ReadUInt32());

        Font = reader.ReadUInt32();
    }

    public override void Write(ByteWriter writer)
    {
        base.Write(writer);
        writer.WriteInt32(Value);
        writer.WriteInt32(Minimum);
        writer.WriteInt32(Maximum);
        writer.WriteUInt32(DisplayType);
        writer.WriteUInt32(CountFlags);
        writer.WriteColor(Color1);
        ;
        writer.WriteColor(Color2);
        ;
        writer.WriteUInt32(VerticalGradient);
        writer.WriteInt32(CountType);
        writer.WriteInt32(Width);
        writer.WriteInt32(Height);
        writer.WriteInt32(Images?.Count ?? 0);
        if (Images != null)
            foreach (var item in Images)
                writer.WriteUInt32((uint)item);
        else writer.WriteInt32(0);
        writer.WriteUInt32(Font);
    }
}