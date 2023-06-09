using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.Utils;

namespace CTFAK.MMFParser.Common.Banks;

public class MusicBank : ChunkLoader
{
    public List<MusicFile> Items = new();
    public int NumOfItems;
    public int References = 0;

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Read(ByteReader reader)
    {
        Items = new List<MusicFile>();
        // if (!Settings.DoMFA)return;
        NumOfItems = reader.ReadInt32();
        for (var i = 0; i < NumOfItems; i++)
        {
            if (Settings.Android) continue;
            var item = new MusicFile();
            item.Read(reader);
            Items.Add(item);
        }
    }
}

public class MusicFile : ChunkLoader
{
    private uint _flags;
    public int Checksum;
    public byte[] Data;
    public int Handle;
    public string Name;
    public int References;

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Read(ByteReader reader)
    {
        var compressed = true;
        Handle = reader.ReadInt32();
        if (compressed) reader = Decompressor.DecompressAsReader(reader, out var decompressed);

        Checksum = reader.ReadInt32();
        References = reader.ReadInt32();
        var size = reader.ReadUInt32();
        _flags = reader.ReadUInt32();
        var reserved = reader.ReadInt32();
        var nameLen = reader.ReadInt32();
        Name = reader.ReadWideString(nameLen);
        Data = reader.ReadBytes((int)(size - nameLen));
    }

    public void Save(string filename)
    {
        File.WriteAllBytes(filename, Data);
    }
}