using System;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.Shared.Events;

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