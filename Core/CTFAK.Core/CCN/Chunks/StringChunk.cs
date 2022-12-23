using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTFAK.Attributes;

namespace CTFAK.CCN.Chunks
{
    public class StringChunk : ChunkLoader
    {
        public string value="";
        public override void Read(ByteReader reader)
        {
            value = reader.ReadUniversal();
            if (value == null) value = "";
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
    [ChunkLoader(0x2224,"AppName")]
    class AppName:StringChunk
    {
    }
    [ChunkLoader(0x2225,"AppAuthor")]
    public class AppAuthor : StringChunk
    {
    }
    [ChunkLoader(0x2227,"ExtPath")]
    class ExtPath : StringChunk
    {
    }
    [ChunkLoader(0x222E,"EditorFilename")]
    public class EditorFilename : StringChunk
    {
    }
    [ChunkLoader(0x222F,"TargetFilename")]
    public class TargetFilename : StringChunk
    {
    }
    class AppDoc : StringChunk
    {
    }
    class AboutText : StringChunk
    {
    }
    [ChunkLoader(0x223B,"Copyright")]
    public class Copyright : StringChunk
    {
    }
    class DemoFilePath : StringChunk
    {
    }
}
