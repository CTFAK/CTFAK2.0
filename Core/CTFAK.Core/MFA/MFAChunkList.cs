using System.Collections.Generic;

namespace CTFAK.MFA
{
    public static class MFAChunkList
    {
        public static Dictionary<int, string> ChunkNames = new Dictionary<int, string>()
        {
            { 33, "Frame Virtual Rect" },
            { 37, "Layer Shader Settings" },
            { 39, "Frame Movement Timer" },
            { 40, "Frame Shader Settings" },
            { 45, "Object Shader Settings" },
        };
    }
}