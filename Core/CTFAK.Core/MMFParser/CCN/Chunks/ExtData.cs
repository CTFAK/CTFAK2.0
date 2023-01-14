using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks;

public class ExtData : ChunkLoader
{
    public byte[] Data;
    public string Name;

    public override void Read(ByteReader reader)
    {
        Name = reader.ReadAscii();
        Data = reader.ReadBytes();
        Logger.Log($"Found file {Name}, {Data.Length}");
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}