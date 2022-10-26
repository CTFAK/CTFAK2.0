using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTFAK.Attributes;

namespace CTFAK.CCN.Chunks
{
    [ChunkLoader(0x222B,"FrameHandles")]
    public class FrameHandles : ChunkLoader
    {
        public Dictionary<int, int> Items;
        
        public override void Read(ByteReader reader)
        {

            var len = reader.Size() / 2;
            Items = new Dictionary<int, int>();
            for (int i = 0; i < len; i++)
            {
                var handle = reader.ReadInt16();
                Items.Add(i, handle);
            }

        }

        public override void Write(ByteWriter Writer)
        {
            foreach (KeyValuePair<int, int> item in Items)
            {
                Writer.WriteInt16((short)item.Value);
            }
        }


    }
}
