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
        public List<MusicFile> Items = new List<MusicFile>();

        public override void Read(ByteReader reader)
        {
            NumOfItems = reader.ReadInt32();
            if (Settings.Android) return;
            for (int i = 0; i < NumOfItems; i++)
            {
                var item = new MusicFile();
                item.Read(reader);
                Items.Add(item);
            }
        }

        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException();
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

        public override void Read(ByteReader reader)
        {
            Handle = reader.ReadInt32();
            reader = Decompressor.DecompressAsReader(reader, out int decompressed);

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

        public override void Write(ByteWriter Writer)
        {
            throw new NotImplementedException();
        }
    }
}
