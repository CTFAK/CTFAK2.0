using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks
{
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
            foreach (int item in Items.Values)
                Writer.WriteInt16((short)item);
        }
    }
}
