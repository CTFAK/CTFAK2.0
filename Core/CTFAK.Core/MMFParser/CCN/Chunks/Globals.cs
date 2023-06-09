using System;
using System.Collections.Generic;
using CTFAK.Attributes;
using CTFAK.Memory;

namespace CTFAK.MMFParser.CCN.Chunks;

public class GlobalValues : ChunkLoader
{
    public List<object> Items = new();

    public override void Read(ByteReader reader)
    {

        var count = reader.ReadInt16();
        var tempReaders = new List<ByteReader>();
        for (var i = 0; i < count; i++) tempReaders.Add(new ByteReader(reader.ReadBytes(4)));

        foreach (var glob in tempReaders)
        {
            var type = reader.ReadByte();
            if (type == 2)
                Items.Add(glob.ReadSingle());
            else if (type == 0) Items.Add(glob.ReadInt32());
        }

    }
    

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

[ChunkLoader(8755, "GlobalStrings")]
public class GlobalStrings : ChunkLoader
{
    public List<string> Items = new();

    public override void Read(ByteReader reader)
    {

        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var str = reader.ReadWideString();
            Items.Add(str);
        }

    }


    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}