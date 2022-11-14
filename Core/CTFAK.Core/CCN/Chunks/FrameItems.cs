using CTFAK.Memory;
using System.Collections.Generic;
using CTFAK.Attributes;
using CTFAK.CCN.Chunks.Objects;

namespace CTFAK.CCN.Chunks
{
    [ChunkLoader(8745,"FrameItems")]

    public class FrameItems : ChunkLoader
    {
        public Dictionary<int, ObjectInfo> Items=new Dictionary<int, ObjectInfo>();
        
        public override void Read(ByteReader reader)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var newObject = new ObjectInfo();
                newObject.Read(reader);
                Items.Add(newObject.handle,newObject);
            }
        }

        public override void Write(ByteWriter writer)
        {
            
        }
    }
    [ChunkLoader(8767,"FrameItems2")]
    public class FrameItems2 : ChunkLoader
    {
        public Dictionary<int, ObjectInfo> Items=new Dictionary<int, ObjectInfo>();
        
        public override void Read(ByteReader reader)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var newObject = new ObjectInfo();
                newObject.Read(reader);
                Items.Add(newObject.handle,newObject);
            }
        }

        public override void Write(ByteWriter writer)
        {
            
        }
    }
}
