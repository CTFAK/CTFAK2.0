using System;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class ParameterCommon : ChunkLoader
{
    public override void Write(ByteWriter Writer)
    {
        throw new NotImplementedException("Unexcepted parameter: " + GetType().Name);
    }

    public override void Read(ByteReader reader)
    {
    }
}