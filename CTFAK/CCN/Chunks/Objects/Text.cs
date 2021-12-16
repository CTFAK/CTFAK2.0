using System.Collections.Generic;
using System.Drawing;
using System.Xml.Schema;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Objects
{
    public class Text : ChunkLoader
    {
        public int Width;
        public int Height;
        public List<Paragraph> Items = new List<Paragraph>();

        public Text(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {

                var currentPos = reader.Tell();
                var size = reader.ReadInt32();
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
                List<int> itemOffsets = new List<int>();
                var offCount = reader.ReadInt32();
                for (int i = 0; i < offCount; i++)
                {
                    itemOffsets.Add(reader.ReadInt32());
                }
            foreach (int itemOffset in itemOffsets)
            {
                reader.Seek(currentPos + itemOffset);
                var par = new Paragraph(reader);
                par.Read();
                Items.Add(par);
            }



        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }


    }

    public class Paragraph : ChunkLoader
    {
        public ushort FontHandle;
        public BitDict Flags = new BitDict(new string[]{
            "HorizontalCenter",
            "RightAligned",
            "VerticalCenter",
            "BottomAligned",
            "None", "None", "None", "None",
            "Correct",
            "Relief"});
        public string Value;
        public Color Color;

        public Paragraph(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {

                FontHandle = reader.ReadUInt16();
                Flags.flag = reader.ReadUInt16();
                Color = reader.ReadColor();
                Value = reader.ReadWideString();
            

        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }


    }
}