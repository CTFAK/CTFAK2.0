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
        public StringChunk(ByteReader reader) : base(reader) { }
        public string value="";
        public override void Read()
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
        public AppName(ByteReader reader) : base(reader) { }

    }
    public class AppAuthor : StringChunk
    {
        public AppAuthor(ByteReader reader) : base(reader)
        {
        }


    }

    class ExtPath : StringChunk
    {
        public ExtPath(ByteReader reader) : base(reader)
        {
        }


    }

    public class EditorFilename : StringChunk
    {
        public EditorFilename(ByteReader reader) : base(reader)
        {
        }


    }

    public class TargetFilename : StringChunk
    {
        public TargetFilename(ByteReader reader) : base(reader)
        {
        }

    }

    class AppDoc : StringChunk
    {
        public AppDoc(ByteReader reader) : base(reader)
        {
        }


    }

    class AboutText : StringChunk
    {
        public AboutText(ByteReader reader) : base(reader)
        {
        }


    }

    public class Copyright : StringChunk
    {
        public Copyright(ByteReader reader) : base(reader)
        {
        }


    }

    class DemoFilePath : StringChunk
    {
        public DemoFilePath(ByteReader reader) : base(reader)
        {
        }


    }
}
