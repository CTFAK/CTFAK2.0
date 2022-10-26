using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public override void Write(ByteWriter Writer)
        {
            
        }


    }
}
