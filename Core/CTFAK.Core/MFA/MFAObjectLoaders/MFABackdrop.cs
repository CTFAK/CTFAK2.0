using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class MFABackdrop : MFABackgroundLoader
    {
        public int Handle;




        public override void Read(ByteReader reader)
        {
            base.Read(reader);
            Handle = reader.ReadInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteInt32(Handle);
        }
    }
    public class MFAQuickBackdrop : MFABackgroundLoader
    {
        public int Width;
        public int Height;
        public int Shape;
        public int BorderSize;
        public Color BorderColor;
        public int FillType;
        public Color Color1;
        public Color Color2;
        public int Flags;
        public int Image;





        public override void Read(ByteReader reader)
        {
            base.Read(reader);
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Shape = reader.ReadInt32();
            BorderSize = reader.ReadInt32();
            BorderColor = reader.ReadColor();

            FillType = reader.ReadInt32();
            Color1 = reader.ReadColor();
            Color2 = reader.ReadColor();
            Flags = reader.ReadInt32();
            Image = reader.ReadInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteInt32(Width);
            Writer.WriteInt32(Height);
            Writer.WriteInt32(Shape);
            Writer.WriteInt32(BorderSize);
            Writer.WriteColor(BorderColor);

            Writer.WriteInt32(FillType);
            Writer.WriteColor(Color1);
            Writer.WriteColor(Color2);
            Writer.WriteInt32(Flags);
            Writer.WriteInt32(Image);
        }
    }

    public class MFABackgroundLoader : ChunkLoader
    {
        public uint ObstacleType;
        public uint CollisionType;
        public override void Read(ByteReader reader)
        {
            ObstacleType = reader.ReadUInt32();
            CollisionType = reader.ReadUInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteUInt32(ObstacleType);
            Writer.WriteUInt32(CollisionType);
        }


    }
}
