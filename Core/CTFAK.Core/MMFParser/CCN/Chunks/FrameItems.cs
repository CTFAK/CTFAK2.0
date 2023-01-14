using System.Collections.Generic;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN.Chunks.Objects;

namespace CTFAK.MMFParser.CCN.Chunks;

[ChunkLoader(8745, "FrameItems")]
public class FrameItems : ChunkLoader
{
    public Dictionary<int, ObjectInfo> Items = new();

    public override void Read(ByteReader reader)
    {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var newObject = new ObjectInfo();
            newObject.Read(reader);
            Items.Add(newObject.Handle, newObject);
        }
    }

    public override void Write(ByteWriter writer)
    {
    }
}

[ChunkLoader(8767, "FrameItems2")]
public class FrameItems2 : ChunkLoader
{
    public Dictionary<int, ObjectInfo> Items = new();

    public override void Read(ByteReader reader)
    {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var newObject = new ObjectInfo();
            newObject.Read(reader);
            Items.Add(newObject.Handle, newObject);
        }
    }

    public override void Write(ByteWriter writer)
    {
    }
}