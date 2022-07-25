using System;
using System.Collections.Generic;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;

namespace CTFAK.CCN.Chunks
{
    public class GlobalValues:ChunkLoader
    {
        public List<object> Items = new List<object>();
        public GlobalValues(ByteReader reader) : base(reader)
        {
        }

        public override void Read()
        {
            var count = reader.ReadInt16();
            List<ByteReader> tempReaders = new List<ByteReader>();
            for (int i = 0; i < count; i++)
            {
                tempReaders.Add(new ByteReader(reader.ReadBytes(4)));
            }

            foreach (var glob in tempReaders)
            {
                var type = reader.ReadByte();
                if (type == 2)
                {
                    Items.Add(glob.ReadSingle());
                }
                else if (type == 0)
                {
                    Items.Add(glob.ReadInt32());
                }
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
    public class GlobalStrings:ChunkLoader
    {
        public List<string> Items = new List<string>();
        public GlobalStrings(ByteReader reader) : base(reader)
        {
        }

        public override void Read()
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var str = reader.ReadWideString();
                Items.Add(str);
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}