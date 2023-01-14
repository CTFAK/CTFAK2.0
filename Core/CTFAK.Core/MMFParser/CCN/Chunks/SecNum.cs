using System;
using CTFAK.Attributes;
using CTFAK.Memory;

namespace CTFAK.MMFParser.CCN.Chunks;

[ChunkLoader(8759, "SecNum")]
public class SecNum : ChunkLoader
{
    public override void Read(ByteReader reader)
    {
        // I removed the implementation for that because Clickteam asked me to
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}