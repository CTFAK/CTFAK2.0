using System;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.Common.Events;

public class ParameterCommon : ChunkLoader
{
    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException("Unexcepted parameter: " + GetType().Name);
    }

    public override void Read(ByteReader reader)
    {
    }
}