using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks.Banks
{
    public class FontBank : ChunkLoader
    {
        public bool Compressed;
        public bool Debug;
        public List<FontItem> Items;




        public override void Read()
        {

          
            var count = reader.ReadInt32();
            int offset = 0;
            if (Settings.Build > 284 && !Debug) offset = -1;

            Items = new List<FontItem>();
            for (int i = 0; i < count; i++)
            {
                if (Settings.android) continue;
                var item = new FontItem(reader);
                item.Compressed = Compressed;
                item.Read();
                item.Handle += (uint)offset;
                Items.Add(item);
            }


        }
        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(Items.Count);
            foreach (FontItem item in Items)
            {
                item.Write(writer);
            }

        }
        public FontBank(ByteReader reader) : base(reader)
        {
        }


    }
    public class FontItem : ChunkLoader
    {
        public bool Compressed;
        public uint Handle;
        public int Checksum;
        public int References;
        public LogFont Value;

        public FontItem(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {
            Handle = reader.ReadUInt32();
            ByteReader dataReader = null;
            if (Compressed)
            {
                dataReader = Decompressor.DecompressAsReader(reader, out var decompSize);
            }
            else dataReader = reader;
            
            var currentPos = dataReader.Tell();
            Checksum = dataReader.ReadInt32();
            References = dataReader.ReadInt32();
            var size = dataReader.ReadInt32();
            Value = new LogFont(dataReader);
            Value.Read();


        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUInt32(Handle);
            var compressedWriter = new ByteWriter(new MemoryStream());
            compressedWriter.WriteInt32(Checksum);
            compressedWriter.WriteInt32(References);
            compressedWriter.WriteInt32(0);
            Value.Write(compressedWriter);
            if (Compressed) Writer.WriteBytes(Decompressor.compress_block(compressedWriter.GetBuffer()));
            else Writer.WriteWriter(compressedWriter);
        }



    }

    public class LogFont : ChunkLoader
    {
        private int _height;
        private int _width;
        private int _escapement;
        private int _orientation;
        private int _weight;
        private byte _italic;
        private byte _underline;
        private byte _strikeOut;
        private byte _charSet;
        private byte _outPrecision;
        private byte _clipPrecision;
        private byte _quality;
        private byte _pitchAndFamily;
        private string _faceName;

        public LogFont(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {
            _height = reader.ReadInt32();
            _width = reader.ReadInt32();
            _escapement = reader.ReadInt32();
            _orientation = reader.ReadInt32();
            _weight = reader.ReadInt32();
            _italic = reader.ReadByte();
            _underline = reader.ReadByte();
            _strikeOut = reader.ReadByte();
            _charSet = reader.ReadByte();
            _outPrecision = reader.ReadByte();
            _clipPrecision = reader.ReadByte();
            _quality = reader.ReadByte();
            _pitchAndFamily = reader.ReadByte();
            _faceName = reader.ReadWideString(32);
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(_height);
            Writer.WriteInt32(_width);
            Writer.WriteInt32(_escapement);
            Writer.WriteInt32(_orientation);
            Writer.WriteInt32(_weight);
            Writer.WriteInt8(_italic);
            Writer.WriteInt8(_underline);
            Writer.WriteInt8(_strikeOut);
            Writer.WriteInt8(_charSet);
            Writer.WriteInt8(_outPrecision);
            Writer.WriteInt8(_clipPrecision);
            Writer.WriteInt8(_quality);
            Writer.WriteInt8(_pitchAndFamily);
            Writer.WriteUnicode(_faceName);
        }


    }
}
