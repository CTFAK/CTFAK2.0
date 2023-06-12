using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks;

[ChunkLoader(0x2223, "AppHeader")]
public class AppHeader : ChunkLoader
{
    public Color BorderColor;
    public Controls Controls;

    public BitDict Flags = new(new[]
    {
        "HeadingMaximized",
        "NoHeading",
        "FitInsideBars",
        "MachineIndependentSpeed",
        "ResizeDisplay",
        "MusicOn",
        "SoundOn",
        "DontDisplayMenu",
        "MenuBar",
        "MaximizedOnBoot",
        "MultiSamples",
        "ChangeResolutionMode",
        "SwitchToFromFullscreen",
        "Protected",
        "Copyright",
        "OneFile"
    });

    public int FrameRate;
    public short GraphicsMode;
    public int InitialLives;
    public int InitialScore;

    public BitDict NewFlags = new(new[]
    {
        "SamplesOverFrames",
        "RelocFiles",
        "RunFrame",
        "PlaySamplesWhenUnfocused",
        "NoMinimizeBox",
        "NoMaximizeBox",
        "NoThickFrame",
        "DoNotCenterFrame",
        "IgnoreInputOnScreensaver",
        "DisableClose",
        "HiddenAtStart",
        "VisualThemes",
        "VSync",
        "RunWhenMinimized",
        "MDI",
        "RunWhileResizing"
    });

    public int NumberOfFrames;

    public BitDict OtherFlags = new(new[]
    {
        "DebuggerShortcuts",
        "Unknown1",
        "Unknown2",
        "DontShareSubData",
        "Unknown3",
        "Unknown4",
        "Unknown5",
        "ShowDebugger",
        "Unknown6",
        "Unknown7",
        "Unknown8",
        "Unknown9",
        "Unknown10",
        "Unknown11",
        "Direct3D9or11",
        "Direct3D8or11"
    });

    public int Size;
    public int WindowHeight;
    public int WindowsMenuIndex;
    public int WindowWidth;


    public override void Read(ByteReader reader)
    {
        if (!Settings.Old) Size = reader.ReadInt32();
        Flags.Flag = reader.ReadUInt16();
        NewFlags.Flag = reader.ReadUInt16();
        GraphicsMode = reader.ReadInt16();
        OtherFlags.Flag = reader.ReadUInt16();
        WindowWidth = reader.ReadInt16();
        WindowHeight = reader.ReadInt16();
        InitialScore = (int)(reader.ReadUInt32() ^ 0xffffffff);
        InitialLives = (int)(reader.ReadUInt32() ^ 0xffffffff);
        Controls = new Controls();
        if (Settings.Old) reader.Skip(56);
        else Controls.Read(reader);
        BorderColor = reader.ReadColor();
        NumberOfFrames = reader.ReadInt32();
        if (Settings.Old) return;
        FrameRate = reader.ReadInt32();
        WindowsMenuIndex = reader.ReadInt32();
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class Controls : ChunkLoader
{
    public List<PlayerControl> Items;

    public override void Read(ByteReader reader)
    {
        Items = new List<PlayerControl>();
        for (var i = 0; i < 4; i++)
        {
            var item = new PlayerControl(reader);
            Items.Add(item);
            item.Read();
        }
    }

    public override void Write(ByteWriter writer)
    {
        foreach (var control in Items) control.Write(writer);
    }
}

public class PlayerControl
{
    private readonly ByteReader _reader;
    private int _controlType;
    private Keys _keys;

    public PlayerControl(ByteReader reader)
    {
        _reader = reader;
    }

    public void Read()
    {
        _keys = new Keys(_reader);
        _controlType = _reader.ReadInt16();
        _keys.Read();
    }

    public void Write(ByteWriter writer)
    {
        writer.WriteInt16((short)_controlType);
        _keys.Write(writer);
    }
}

public class Keys
{
    private readonly ByteReader _reader;
    private short _button1;
    private short _button2;
    private short _button3;
    private short _button4;
    private short _down;
    private short _left;
    private short _right;
    private short _up;

    public Keys(ByteReader reader)
    {
        _reader = reader;
    }

    public void Read()
    {
        _up = _reader.ReadInt16();
        _down = _reader.ReadInt16();
        _left = _reader.ReadInt16();
        _right = _reader.ReadInt16();
        _button1 = _reader.ReadInt16();
        _button2 = _reader.ReadInt16();
        //if (Settings.GameType == GameType.OnePointFive) return;
        _button3 = _reader.ReadInt16();
        _button4 = _reader.ReadInt16();
    }

    public void Write(ByteWriter writer)
    {
        writer.WriteInt16(_up);
        writer.WriteInt16(_down);
        writer.WriteInt16(_left);
        writer.WriteInt16(_right);
        writer.WriteInt16(_button1);
        writer.WriteInt16(_button2);
        writer.WriteInt16(_button3);
        writer.WriteInt16(_button4);
    }
}