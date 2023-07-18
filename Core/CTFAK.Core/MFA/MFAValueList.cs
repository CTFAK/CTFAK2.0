using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA
{
    public class MFAValueList : ChunkLoader
    {
        public List<ValueItem> Items = new List<ValueItem>();

        public override void Read(ByteReader reader)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var item = new ValueItem();
                item.Read(reader);
                Items.Add(item);
            }
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Items.Count);
            foreach (var item in Items) item.Write(Writer);
        }
    }
    public class ValueItem : ChunkLoader
    {
        public object Value;
        public string Name = "";



        public override void Read(ByteReader reader)
        {
            Name = reader.AutoReadUnicode();
            var type = reader.ReadInt32();
            switch (type)
            {
                case 2://string
                    Value = reader.AutoReadUnicode();
                    break;
                case 0://int
                    Value = reader.ReadInt32();
                    break;
                case 1://double
                    Value = reader.ReadSingle();
                    break;
            }
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Name);
            if (Value is string)
            {
                Writer.WriteInt32(2);
                Writer.AutoWriteUnicode((string)Value);
            }
            else if (Value is int)
            {
                Writer.WriteInt32(0);
                Writer.WriteInt32((int)Value);
            }
            else if (Value is double || Value is float)
            {
                Writer.WriteInt32(1);
                Writer.WriteSingle((float)Value);
            }
        }
    }
}
