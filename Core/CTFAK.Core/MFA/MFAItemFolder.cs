using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA
{
    public class MFAItemFolder : ChunkLoader
    {
        public List<uint> Items=new List<uint>();
        public string Name;
        public uint UnkHeader;
        public bool isRetard;

        public override void Read(ByteReader reader)
        {
            UnkHeader = reader.ReadUInt32();
            if (UnkHeader == 0x70000004)
            {
                isRetard = false;
                Name = reader.AutoReadUnicode();
                Items = new List<uint>();
                var count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    Items.Add(reader.ReadUInt32());
                }
            }
            else
            {
                isRetard = true;
                Name = null;
                Items = new List<uint>();
                Items.Add(reader.ReadUInt32());
            }
        }

        public override void Write(ByteWriter Writer)
        {
            if (isRetard)
            {
                Writer.WriteInt32(0x70000005);
                Writer.WriteInt32((int)Items[0]);
            }
            else
            {
                Writer.WriteInt32(0x70000004);
                if (Name == null) Name = "";
                Writer.AutoWriteUnicode(Name);
                Writer.WriteInt32(Items.Count);
                foreach (var item in Items)
                {
                    Writer.WriteUInt32(item);
                }
            }
        }

    }
}
