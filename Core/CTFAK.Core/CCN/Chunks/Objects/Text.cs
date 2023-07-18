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





        public override void Read(ByteReader reader)
        {
            if (Settings.Old)
            {
                
                var currentPos = reader.Tell();
                var size = reader.ReadInt32();
                Width = reader.ReadInt16();
                Height = reader.ReadInt16();
                List<int> itemOffsets = new List<int>();
                var offCount = reader.ReadInt16();
                for (int i = 0; i < offCount; i++)
                {
                    itemOffsets.Add(reader.ReadInt16());
                }
                foreach (int itemOffset in itemOffsets)
                {
                    reader.Seek(currentPos+itemOffset);
                    var par = new Paragraph();
                    par.Read(reader);
                    Items.Add(par);
                } 
            }
            else
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
                    reader.Seek(currentPos+itemOffset);
                    var par = new Paragraph();
                    par.Read(reader);
                    Items.Add(par);
                } 
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




        public override void Read(ByteReader reader)
        {

            if (Settings.Old)
            {
                var size = reader.ReadUInt16();
                FontHandle = reader.ReadUInt16();
                Color = reader.ReadColor();
                Flags.flag = reader.ReadUInt16();
                Value = reader.ReadYuniversal();

            }
            else
            {
                FontHandle = reader.ReadUInt16();
                Flags.flag = reader.ReadUInt16();
                Color = reader.ReadColor();
                Value = reader.ReadYuniversal();
            }
            

        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }


    }
}