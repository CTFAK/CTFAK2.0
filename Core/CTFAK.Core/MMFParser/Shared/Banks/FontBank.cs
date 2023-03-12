using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Shared.Banks;

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

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteUInt32(Handle);
        var compressedWriter = new ByteWriter(new MemoryStream());
        compressedWriter.WriteInt32(Checksum);
        compressedWriter.WriteInt32(References);
        compressedWriter.WriteInt32(0);
        Value.Write(compressedWriter);
        if (Compressed) Writer.WriteBytes(Decompressor.CompressBlock(compressedWriter.GetBuffer()));
        else Writer.WriteWriter(compressedWriter);
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

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(_height);
        Writer.WriteInt32(_width);
        Writer.WriteInt32(_escapement);
        Writer.WriteInt32(_orientation);
        Writer.WriteInt32(_weight);
        Writer.WriteInt8(_italic);
        Writer.WriteInt8(_underline);
        Writer.WriteInt8(_strikeOut);
        Writer.WriteInt8(_charSet);
        Writer.WriteInt8(_outPrecision);
        Writer.WriteInt8(_clipPrecision);
        Writer.WriteInt8(_quality);
        Writer.WriteInt8(_pitchAndFamily);
        Writer.WriteUnicode(_faceName);
    }
}

[ChunkLoader(8793, "TrueTypeFontMetas")]
public class TrueTypeMeta : ChunkLoader
{
    public List<TTM> TTFMetas = new List<TTM>();

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

    public class TTM
    {
        string FontName;
        int FontSize;
        int FontStyle;
        bool Bold;
        bool Italic;
        bool Underline;
        bool Strikeout;
        int ScriptType;

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

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
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
            Fonts.Add(Decompressor.Decompress(reader, out int decomp));
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}