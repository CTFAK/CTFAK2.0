using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    class AppName:StringChunk
    {
    }
    public class AppAuthor : StringChunk
    {
    }

    class ExtPath : StringChunk
    {
    }

    public class EditorFilename : StringChunk
    {
    }

    public class TargetFilename : StringChunk
    {
    }

    class AppDoc : StringChunk
    {
    }

    class AboutText : StringChunk
    {
    }

    public class Copyright : StringChunk
    {
    }

    class DemoFilePath : StringChunk
    {
    }
}
