﻿using System.Drawing;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MFA.MFAObjectLoaders;

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
    public Color BorderColor;
    public int BorderSize;
    public Color Color1;
    public Color Color2;
    public int FillType;
    public int Flags;
    public int Height;
    public int Image;
    public int Shape;
    public int Width;

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
    public uint CollisionType;
    public uint ObstacleType;

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