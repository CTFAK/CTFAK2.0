using System;
using CTFAK.Utils;

namespace CTFAK.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ChunkLoaderAttribute : Attribute
{
    public int ChunkId;
    public string ChunkName;

    public ChunkLoaderAttribute(int chunkId, string chunkName)
    {
        this.ChunkId = chunkId;
        this.ChunkName = chunkName;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class LoaderReadAttribute : Attribute
{
    public Settings.GameType AllowedGameTypes;

    public LoaderReadAttribute(Settings.GameType allowedGameTypes = Settings.GameType.NORMAL)
    {
        this.AllowedGameTypes = allowedGameTypes;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class LoaderHandleAttribute : Attribute
{
}