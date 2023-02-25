using System;
using System.Collections.Generic;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Objects
{
    public class AlterableValues:ChunkLoader
    {
        public List<int> Items = new List<int>();
        public int Flags = 0;

        public override void Read(ByteReader reader)
        {
            var count = reader.ReadInt16();
            for (int i = 0; i < count; i++)
            {
                try
                {
                    Items.Add(reader.ReadInt32());
                }
                catch
                {
                    break;
                }
                //Logger.Log($"Reading AltVal {i}: {Items[i]}");
            }
            try
            {
                Flags = reader.ReadInt32();
            }
            catch {}
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }

    public class AlterableStrings : ChunkLoader
    {
        public List<string> Items = new List<string>();

        public override void Read(ByteReader reader)
        {
            var count = reader.ReadInt16();
            for (int i = 0; i < count; i++)
            {
                try
                {
                    Items.Add(reader.ReadWideString());
                }
                catch
                {
                    break;
                }
                //Logger.Log($"Reading AltStr {i}: {Items[i]}");
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
