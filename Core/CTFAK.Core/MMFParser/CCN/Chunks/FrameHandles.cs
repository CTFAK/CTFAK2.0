using System.Collections.Generic;
using CTFAK.Attributes;
using CTFAK.Memory;

namespace CTFAK.MMFParser.CCN.Chunks;

[ChunkLoader(0x222B, "FrameHandles")]
public class FrameHandles : ChunkLoader
{
    public Dictionary<int, int> Items;

    public override void Read(ByteReader reader)
    {
        var len = reader.Size() / 2;
        Items = new Dictionary<int, int>();
        for (var i = 0; i < len; i++)
        {
            var handle = reader.ReadInt16();
            Items.Add(i, handle);
        }
    }

    public override void Write(ByteWriter writer)
    {
        foreach (var item in Items) writer.WriteInt16((short)item.Value);
    }
}