using System;
using System.Drawing;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Objects
{
    public enum Obstacle
    {
        None = 0,
        Solid = 1,
        Platform = 2,
        Ladder = 3,
        Transparent = 4
    }

    public enum Collision
    {
        Fine = 0,
        Box = 1
    }

    public class BackdropLoader:ChunkLoader
    {
        public int Size;
        public Obstacle ObstacleType;
        public Collision CollisionType;
        public int Width;
        public int Height;
        public int Image;

        public override void Read(ByteReader reader)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Backdrop : BackdropLoader
    {
        public override void Read(ByteReader reader)
        {
            Size = reader.ReadInt32();
            ObstacleType = (Obstacle)reader.ReadInt16();
            CollisionType = (Collision)reader.ReadInt16();
            if (!Settings.Old)
            {
                Width = reader.ReadInt32();
                Height = reader.ReadInt32(); 
            }
                
            Image = reader.ReadInt16();
        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Quickbackdrop : BackdropLoader
    {
        public Shape Shape;

        public override void Read(ByteReader reader)
        {
            Size = reader.ReadInt32();
            ObstacleType = (Obstacle)reader.ReadInt16();
            CollisionType = (Collision)reader.ReadInt16();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Shape = new Shape();
            Shape.Read(reader);
        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Shape : ChunkLoader
    {
        public short BorderSize;
        public Color BorderColor;
        public short ShapeType;
        public short FillType;
        public short LineFlags;
        public Color Color1;
        public Color Color2;
        public short GradFlags;
        public short Image = 15;

        public override void Read(ByteReader reader)
        {
            BorderSize = reader.ReadInt16();
            BorderColor = reader.ReadColor();
            ShapeType = reader.ReadInt16();
            FillType = reader.ReadInt16();
            if (ShapeType == 1)
            {
                LineFlags = reader.ReadInt16();
            }
            else if (FillType == 1)
            {
                Color1 = reader.ReadColor();
            }
            else if (FillType == 2)
            {
                Color1 = reader.ReadColor();
                Color2 = reader.ReadColor();
                GradFlags = reader.ReadInt16();
            }
            // else if(FillType==3)
            // {
            Image = reader.ReadInt16();
            // }
        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }
    }
}