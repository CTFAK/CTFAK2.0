using System;
using System.Collections.Generic;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class MFAObjectFlags : ChunkLoader
{
    public List<ObjectFlag> Items = new();

    public override void Read(ByteReader reader)
    {
        throw new NotImplementedException();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt8(57);
        writer.WriteInt32(14 + Items.Count * 12);
        writer.WriteInt32(Items.Count);
        foreach (var item in Items) item.Write(writer);
        writer.WriteInt8(60);
        writer.WriteInt32(4 + Items.Count * 4);
        writer.WriteInt32(Items.Count);
        for (var i = 0; i < Items.Count; i++)
            writer.WriteInt32(i);
    }
}

public class ObjectFlag : ChunkLoader
{
    public string Name;
    public bool Value;

    public override void Read(ByteReader reader)
    {
        throw new NotImplementedException();
    }

    public override void Write(ByteWriter writer)
    {
        writer.AutoWriteUnicode(Name);
        writer.WriteInt32(0);
        if (Value)
            writer.WriteInt32(1);
        else
            writer.WriteInt32(0);
    }
}