using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks.Objects
{
    public class ObjectInfo : ChunkLoader
    {
        public int handle;
        public string name;
        public ObjectInfo(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            while (true)
            {
                var newChunk = new Chunk(reader);
                var chunkData = newChunk.Read();
                var chunkReader = new ByteReader(chunkData);
                if (newChunk.Id == 32639) break;
                switch (newChunk.Id)
                {
                    case 17477:
                        name = chunkReader.ReadWideString();
                        break;
                    case 17476:
                        handle = chunkReader.ReadInt16();
                        break;


                }
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
