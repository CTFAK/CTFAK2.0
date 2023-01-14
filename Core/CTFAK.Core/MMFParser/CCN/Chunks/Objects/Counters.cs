using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.Memory;

namespace CTFAK.MMFParser.CCN.Chunks.Objects;

public class Counter : ChunkLoader
{
    public int Initial;
    public int Maximum;
    public int Minimum;
    public short Size;

    public override void Read(ByteReader reader)
    {
        Size = reader.ReadInt16();
        Initial = reader.ReadInt32();
        Minimum = reader.ReadInt32();
        Maximum = reader.ReadInt32();
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class Counters : ChunkLoader
{
    private readonly int _floatDecimalsMask = 0xF000;
    private readonly int _floatDecimalsShift = 12;
    private readonly int _floatDigitsMask = 0xF0;
    private readonly int _floatDigitsShift = 4;
    private readonly int _floatPad = 0x0800;
    private readonly int _formatFloat = 0x0200;
    private readonly int _intDigitsMask = 0xF;
    private readonly int _useDecimals = 0x0400;
    public bool AddNulls;
    public int Decimals;
    public ushort DisplayType;
    public ushort Flags;
    public int FloatDigits;
    public ushort Font;
    public bool FormatFloat;
    public List<int> Frames;
    public uint Height;
    public int IntegerDigits;
    public bool Inverse;
    public ushort Player;
    public Shape Shape;
    public uint Size;
    public bool UseDecimals;
    public uint Width;

    public override void Read(ByteReader reader)
    {
        Size = reader.ReadUInt32();
        Width = reader.ReadUInt32();
        Height = reader.ReadUInt32();
        Player = reader.ReadUInt16();
        DisplayType = reader.ReadUInt16();
        Flags = reader.ReadUInt16();

        IntegerDigits = Flags & _intDigitsMask;
        FormatFloat = (Flags & _formatFloat) != 0;
        FloatDigits = (Flags & _floatDigitsMask) >> (_floatDigitsShift + 1);
        UseDecimals = (Flags & _useDecimals) != 0;
        Decimals = (Flags & _floatDecimalsMask) >> _floatDecimalsShift;
        AddNulls = (Flags & _floatPad) != 0;

        Inverse = ByteFlag.GetFlag(Flags, 8);
        Font = reader.ReadUInt16();
        if (DisplayType == 0) return;

        if (DisplayType == 1 || DisplayType == 4 || DisplayType == 50)
        {
            Frames = new List<int>();
            var count = reader.ReadInt16();
            for (var i = 0; i < count; i++) Frames.Add(reader.ReadUInt16());
        }
        else if (DisplayType == 2 || DisplayType == 3 || DisplayType == 5)
        {
            Frames = new List<int> { 0 };
            Shape = new Shape();
            Shape.Read(reader);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt32(Size);
        writer.WriteUInt32(Width);
        writer.WriteUInt32(Height);
        writer.WriteUInt16(Player);
        writer.WriteUInt16(DisplayType);
        writer.WriteUInt16(Flags);
        writer.WriteUInt16(Font);
        if (DisplayType == 0) return;

        if (DisplayType == 1 || DisplayType == 4 || DisplayType == 50)
        {
            Frames = new List<int>();
            writer.WriteInt16(0);
        }
        else if (DisplayType == 2 || DisplayType == 3 || DisplayType == 5)
        {
            writer.WriteInt16(0);
            writer.WriteColor(Color.FromArgb(0, 255, 255, 255));
            writer.WriteInt16(1);
            writer.WriteInt16(1);
            writer.WriteInt16(2);
            writer.WriteInt16(0);
            writer.WriteColor(Color.FromArgb(0, 255, 255, 255));
            writer.WriteInt16(0);
        }
    }
}