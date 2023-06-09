using System;

namespace CTFAK.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ChunkLoaderAttribute : Attribute
{
    public short ChunkId;
    public string ChunkName;

    public ChunkLoaderAttribute(short chunkId, string chunkName)
    {
        ChunkId = chunkId;
        ChunkName = chunkName;
    }
}