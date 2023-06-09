using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Common.Banks;

[ChunkLoader(26215, "FontBank")]
public class FontBank : ChunkLoader
{
    public bool Compressed;
    public bool Debug;
    public List<FontItem> Items = new();

    public override void Read(ByteReader reader)
    {
        if (!Settings.isMFA) return;
        if (Settings.Old) return; //TODO FIX FIX FIX
        var count = reader.ReadInt32();
        var offset = 0;
        if (Settings.Build > 284 && !Debug) offset = -1;

        Items = new List<FontItem>();
        for (var i = 0; i < count; i++)
        {
            if (Settings.Android) continue;
            var item = new FontItem();
            item.Compressed = Compressed;
            item.Read(reader);
            item.Handle += (uint)offset;
            Items.Add(item);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Items.Count);
        foreach (var item in Items) item.Write(writer);
    }
}

public class FontItem : ChunkLoader
{
    public int Checksum;
    public bool Compressed;
    public uint Handle;
    public int References;
    public LogFont Value;

    public override void Read(ByteReader reader)
    {
        Handle = reader.ReadUInt32();
        ByteReader dataReader = null;
        if (Compressed)
            dataReader = Decompressor.DecompressAsReader(reader, out var decompSize);
        else dataReader = reader;

        var currentPos = dataReader.Tell();
        Checksum = dataReader.ReadInt32();
        References = dataReader.ReadInt32();
        var size = dataReader.ReadInt32();
        Value = new LogFont();
        Value.Read(dataReader);
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt32(Handle);
        var compressedWriter = new ByteWriter(new MemoryStream());
        compressedWriter.WriteInt32(Checksum);
        compressedWriter.WriteInt32(References);
        compressedWriter.WriteInt32(0);
        Value.Write(compressedWriter);
        if (Compressed) writer.WriteBytes(Decompressor.CompressBlock(compressedWriter.GetBuffer()));
        else writer.WriteWriter(compressedWriter);
    }
}

public class LogFont : ChunkLoader
{
    private byte _charSet;
    private byte _clipPrecision;
    private int _escapement;
    private string _faceName;
    private int _height;
    private byte _italic;
    private int _orientation;
    private byte _outPrecision;
    private byte _pitchAndFamily;
    private byte _quality;
    private byte _strikeOut;
    private byte _underline;
    private int _weight;
    private int _width;

    public override void Read(ByteReader reader)
    {
        _height = reader.ReadInt32();
        _width = reader.ReadInt32();
        _escapement = reader.ReadInt32();
        _orientation = reader.ReadInt32();
        _weight = reader.ReadInt32();
        _italic = reader.ReadByte();
        _underline = reader.ReadByte();
        _strikeOut = reader.ReadByte();
        _charSet = reader.ReadByte();
        _outPrecision = reader.ReadByte();
        _clipPrecision = reader.ReadByte();
        _quality = reader.ReadByte();
        _pitchAndFamily = reader.ReadByte();
        _faceName = reader.ReadWideString(32);
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(_height);
        writer.WriteInt32(_width);
        writer.WriteInt32(_escapement);
        writer.WriteInt32(_orientation);
        writer.WriteInt32(_weight);
        writer.WriteInt8(_italic);
        writer.WriteInt8(_underline);
        writer.WriteInt8(_strikeOut);
        writer.WriteInt8(_charSet);
        writer.WriteInt8(_outPrecision);
        writer.WriteInt8(_clipPrecision);
        writer.WriteInt8(_quality);
        writer.WriteInt8(_pitchAndFamily);
        writer.WriteUnicode(_faceName);
    }
}

[ChunkLoader(8793, "TrueTypeFontMetas")]
public class TrueTypeMeta : ChunkLoader
{
    public List<TTM> TTFMetas = new();

    public override void Read(ByteReader reader)
    {
        var end = reader.Tell() + reader.Size();
        while (reader.Tell() < end)
        {
            var newTTM = new TTM();
            newTTM.Read(reader);
            TTFMetas.Add(newTTM);
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }

    public class TTM
    {
        private bool Bold;
        private string FontName;
        private int FontSize;
        private int FontStyle;
        private bool Italic;
        private int ScriptType;
        private bool Strikeout;
        private bool Underline;

        public void Read(ByteReader reader)
        {
            FontSize = reader.ReadInt32();
            FontSize = -(FontSize + 6);
            reader.ReadInt32(); //Idk Yet
            reader.ReadInt32(); //Idk Yet
            reader.ReadInt32(); //Idk Yet
            FontStyle = reader.ReadByte();
            Bold = reader.ReadByte() > 01;
            reader.ReadInt16(); //Idk Yet
            Italic = reader.ReadByte() != 00;
            Underline = reader.ReadByte() != 00;
            Strikeout = reader.ReadByte() != 00;
            ScriptType = reader.ReadByte();
            if (ScriptType > 0)
                ScriptType = 179 - ScriptType;
            FontName = reader.ReadWideString(32).TrimEnd((char)0);
            reader.ReadInt32(); //Idk Yet
        }
    }
}

[ChunkLoader(8793, "TrueTypeFonts")]
public class TrueTypeFonts : ChunkLoader
{
    public List<byte[]> Fonts = new();

    public override void Read(ByteReader reader)
    {
        var end = reader.Tell() + reader.Size();
        while (reader.Tell() < end)
            Fonts.Add(Decompressor.Decompress(reader, out var decomp));
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}