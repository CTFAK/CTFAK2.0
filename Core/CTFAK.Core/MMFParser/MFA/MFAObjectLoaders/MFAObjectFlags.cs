using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using System;
using System.Collections.Generic;

namespace CTFAK.MFA
{
    public class MFAObjectFlags : ChunkLoader
    {
        public List<ObjectFlag> Items = new List<ObjectFlag>();

        public override void Read(ByteReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt8(57);
            Writer.WriteInt32(14 + (Items.Count * 12));
            Writer.WriteInt32(Items.Count);
            foreach (var item in Items) item.Write(Writer);
            Writer.WriteInt8(60);
            Writer.WriteInt32(4 + (Items.Count * 4));
            Writer.WriteInt32(Items.Count);
            for (int i = 0; i < Items.Count; i++)
                Writer.WriteInt32(i);
        }
    }
    public class ObjectFlag : ChunkLoader
    {
        public string Name;
        public bool Value;

        public override void Read(ByteReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32(0);
            if (Value)
                Writer.WriteInt32(1);
            else
                Writer.WriteInt32(0);
        }
    }
}