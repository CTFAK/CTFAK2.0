using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Banks
{
    public class MusicBank : ChunkLoader
    {
        public int NumOfItems = 0;
        public int References = 0;
        public List<MusicFile> Items;

        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException();
        }


        public override void Read()
        {

            Items = new List<MusicFile>();
            // if (!Settings.DoMFA)return;
            NumOfItems = reader.ReadInt32();
            for (int i = 0; i < NumOfItems; i++)
            {
                if (Settings.android) continue;
                var item = new MusicFile(reader);
                item.Read();
                Items.Add(item);
            }
        }

        public MusicBank(ByteReader reader) : base(reader)
        {
        }


    }

    public class MusicFile : ChunkLoader
    {

        public int Checksum;
        public int References;
        public string Name;
        private uint _flags;
        public byte[] Data;
        public int Handle;

        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException();
        }




        public override void Read()
        {
            var compressed = true;
            Handle = reader.ReadInt32();
            if (compressed)
            {
                reader = Decompressor.DecompressAsReader(reader, out int decompressed);
            }

            Checksum = reader.ReadInt32();
            References = reader.ReadInt32();
            var size = reader.ReadUInt32();
            _flags = reader.ReadUInt32();
            var reserved = reader.ReadInt32();
            var nameLen = reader.ReadInt32();
            Name = reader.ReadWideString(nameLen);
            Data = reader.ReadBytes((int)(size - nameLen));

        }

        public void Save(string filename)
        {
            File.WriteAllBytes(filename, Data);
        }

        public MusicFile(ByteReader reader) : base(reader)
        {
        }


    }
}
