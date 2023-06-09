using System;
using System.Collections.Generic;
using CTFAK.Memory;

namespace CTFAK.MMFParser.CCN.Chunks.Objects;

public class AlterableValues : ChunkLoader
{
    public int Flags;
    public List<int> Items = new();

    public override void Read(ByteReader reader)
    {
        var count = reader.ReadInt16();
        for (var i = 0; i < count; i++)
            try
            {
                Items.Add(reader.ReadInt32());
            }
            catch
            {
                break;
            }

        try
        {
            Flags = reader.ReadInt32();
        }
        catch
        {
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class AlterableStrings : ChunkLoader
{
    public List<string> Items = new();

    public override void Read(ByteReader reader)
    {
        var count = reader.ReadInt16();
        for (var i = 0; i < count; i++)
            try
            {
                Items.Add(reader.ReadWideString());
            }
            catch
            {
                break;
            }
        //Logger.Log($"Reading AltStr {i}: {Items[i]}");
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}