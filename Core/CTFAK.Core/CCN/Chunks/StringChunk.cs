using CTFAK.Memory;
using CTFAK.Utils;
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
            value = reader.ReadYuniversal();
            if (value == null) value = "";
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
    class AppName:StringChunk
    {
        public override void Read(ByteReader reader)
        {
            var start = reader.Tell();
            var str = reader.ReadAscii();
            if (str.Length + 1 == reader.Size())
            {
                Settings.Unicode = false;
                value = str;
            }
            else
            {
                reader.Seek(start);
                value = reader.ReadWideString();
                Settings.Unicode = true;
            }
        }
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
