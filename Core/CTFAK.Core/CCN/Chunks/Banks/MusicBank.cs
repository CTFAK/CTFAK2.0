﻿using CTFAK.Memory;
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
        public List<MusicFile> Items=new List<MusicFile>();

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Items.Count);
            foreach (var item in Items)
                item.Write(Writer); // Music: Done!
        }


        public override void Read(ByteReader reader)
        {

            Items = new List<MusicFile>();
            // if (!Settings.DoMFA)return;
            NumOfItems = reader.ReadInt32();
            for (int i = 0; i < NumOfItems; i++)
            {
                if (Settings.android) continue;
                var item = new MusicFile();
                item.Read(reader);
                Items.Add(item);
            }
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
            Writer.WriteUInt32((uint)Handle); // Write handle.
            Writer.WriteInt32(Checksum); // Write checksum (1)
            Writer.WriteInt32(References);
            Writer.WriteInt32(Data.Length + (Name.Length * 2));
            Writer.WriteUInt32(_flags); // Flags? 
            Writer.WriteInt32(0); // Reserved 4 bytes, 0x00000000 (?)
            Writer.WriteInt32(Name.Length); // Write name length.
            Writer.WriteUnicode(Name); // Write name.
            Writer.WriteBytes(Data); // Write data.
            
            // -
            // 
            // -
        }




        public override void Read(ByteReader reader)
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




    }
}
