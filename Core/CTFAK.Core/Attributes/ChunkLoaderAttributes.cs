using System;
using CTFAK.Utils;

namespace CTFAK.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ChunkLoaderAttribute:Attribute
    {
        public int chunkId;
        public string chunkName;

        public ChunkLoaderAttribute(int chunkId, string chunkName)
        {
            this.chunkId = chunkId;
            this.chunkName = chunkName;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class LoaderReadAttribute : Attribute
    {
        public Settings.GameType allowedGameTypes;

        public LoaderReadAttribute(Settings.GameType allowedGameTypes)
        {
            this.allowedGameTypes = allowedGameTypes;
        }
    }
}