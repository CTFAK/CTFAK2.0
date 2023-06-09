using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks.Objects;

public class Text : ChunkLoader
{
    public int Height;
    public List<Paragraph> Items = new();
    public int Width;

    public override void Read(ByteReader reader)
    {
        if (Settings.Old)
        {
            var currentPos = reader.Tell();
            var size = reader.ReadInt32();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            var itemOffsets = new List<int>();
            var offCount = reader.ReadInt16();
            for (var i = 0; i < offCount; i++) itemOffsets.Add(reader.ReadInt16());
            foreach (var itemOffset in itemOffsets)
            {
                reader.Seek(currentPos + itemOffset);
                var par = new Paragraph();
                par.Read(reader);
                Items.Add(par);
            }
        }
        else
        {
            var currentPos = reader.Tell();
            var size = reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            var itemOffsets = new List<int>();
            var offCount = reader.ReadInt32();
            for (var i = 0; i < offCount; i++) itemOffsets.Add(reader.ReadInt32());
            foreach (var itemOffset in itemOffsets)
            {
                reader.Seek(currentPos + itemOffset);
                var par = new Paragraph();
                par.Read(reader);
                Items.Add(par);
            }
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class Paragraph : ChunkLoader
{
    public Color Color;

    public BitDict Flags = new(new[]
    {
        "HorizontalCenter",
        "RightAligned",
        "VerticalCenter",
        "BottomAligned",
        "None", "None", "None", "None",
        "Correct",
        "Relief"
    });

    public ushort FontHandle;
    public string Value;

    public override void Read(ByteReader reader)
    {
        if (Settings.Old)
        {
            var size = reader.ReadUInt16();
            FontHandle = reader.ReadUInt16();
            Color = reader.ReadColor();
            Flags.Flag = reader.ReadUInt16();
            Value = reader.ReadUniversal();
        }
        else
        {
            FontHandle = reader.ReadUInt16();
            Flags.Flag = reader.ReadUInt16();
            Color = reader.ReadColor();
            Value = reader.ReadUniversal();
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}