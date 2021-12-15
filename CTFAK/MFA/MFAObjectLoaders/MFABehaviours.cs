using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class Behaviours : ChunkLoader
    {
        List<Behaviour> _items = new List<Behaviour>();
        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(_items.Count);
            foreach (Behaviour behaviour in _items)
            {
                behaviour.Write(Writer);
            }
        }



        public override void Read()
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var item = new Behaviour(reader);
                item.Read();
                _items.Add(item);
            }
        }
        public Behaviours(ByteReader reader) : base(reader) { }
    }
    class Behaviour : ChunkLoader
    {
        public string Name = "ERROR";
        public byte[] Data;
        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Name);
            Writer.WriteUInt32((uint)Data.Length);
            Writer.WriteBytes(Data);
        }



        public override void Read()
        {
            Name = reader.AutoReadUnicode();

            Data = reader.ReadBytes((int)reader.ReadUInt32());

        }
        public Behaviour(ByteReader reader) : base(reader) { }
    }
}
