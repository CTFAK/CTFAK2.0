using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN.Chunks.Objects;
using CTFAK.MMFParser.Common.Events;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks.Frame;

public class ObjectInstance : ChunkLoader
{
    public short Flags;
    public ushort Handle;
    public short Layer;
    public ushort ObjectInfo;
    public short ParentHandle;
    public short ParentType;
    public int X;
    public int Y;

    public override void Read(ByteReader reader)
    {
        Handle = (ushort)reader.ReadInt16();
        ObjectInfo = (ushort)reader.ReadInt16();
        if (Settings.Old)
        {
            Y = reader.ReadInt16();
            X = reader.ReadInt16();
        }
        else
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
        }

        ParentType = reader.ReadInt16();
        ParentHandle = reader.ReadInt16();

        if (Settings.Old || Settings.F3 /*either clickteam is being funny or my eyes are lying to me*/) return;
        Layer = reader.ReadInt16();
        Flags = reader.ReadInt16();
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class VirtualRect : ChunkLoader
{
    public int Bottom;
    public int Left;
    public int Right;
    public int Top;

    public override void Read(ByteReader reader)
    {
        Left = reader.ReadInt32();
        Top = reader.ReadInt32();
        Right = reader.ReadInt32();
        Bottom = reader.ReadInt32();
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

[ChunkLoader(0x3333, "Frame")]
public class Frame : ChunkLoader
{
    public Color Background;
    public byte Blend;
    public Events Events;
    public Transition FadeIn;
    public Transition FadeOut;

    public BitDict Flags = new(new[]
    {
        "XCoefficient",
        "YCoefficient",
        "DoNotSaveBackground",
        "Wrap",
        "Visible",
        "WrapHorizontally",
        "WrapVertically", "", "", "", "", "", "", "", "", "",
        "Redraw",
        "ToHide",
        "ToShow"
    });

    public int Height;
    public int InkEffect;
    public int InkEffectValue;
    public Layers Layers;
    public string Name;
    public List<ObjectInstance> Objects = new();
    public List<Color> Palette;
    public Color RgbCoeff;
    public ShaderData ShaderData = new();
    public VirtualRect VirtualRect;

    public int Width;


    public override void Read(ByteReader reader)
    {
        while (true)
        {
            var newChunk = new Chunk();
            var chunkData = newChunk.Read(reader);
            var chunkReader = new ByteReader(chunkData);
            if (newChunk.Id == 32639) break;
            if (reader.Tell() >= reader.Size()) break;
            switch (newChunk.Id)
            {
                case 13109:
                    var frameName = new StringChunk();
                    frameName.Read(chunkReader);
                    if (string.IsNullOrEmpty(frameName.Value))
                        Name = "CORRUPTED FRAME";
                    else
                        Name = frameName.Value;
                    break;
                case 13108:
                    if (Settings.Old)
                    {
                        Width = chunkReader.ReadInt16();
                        Height = chunkReader.ReadInt16();
                        Background = chunkReader.ReadColor();
                        Flags.Flag = chunkReader.ReadUInt16();
                    }
                    else
                    {
                        Width = chunkReader.ReadInt32();
                        Height = chunkReader.ReadInt32();
                        Background = chunkReader.ReadColor();
                        Flags.Flag = chunkReader.ReadUInt32();
                    }

                    break;
                case 13112:

                    var count = chunkReader.ReadInt32();
                    for (var i = 0; i < count; i++)
                        //while (reader.Tell()<reader.Size())
                    {
                        var objInst = new ObjectInstance();
                        objInst.Read(chunkReader);
                        Objects.Add(objInst);
                    }

                    break;
                case 13117:
                    if (CTFAKCore.Parameters.Contains("-noevnt"))
                    {
                        Events = new Events();
                    }
                    else
                    {
                        Events = new Events();
                        Events.Read(chunkReader);
                    }

                    break;
                case 13121:
                    Layers = new Layers();
                    Layers.Read(chunkReader);
                    break;
                case 13111:
                    var pal = new FramePalette();
                    pal.Read(chunkReader);
                    Palette = pal.Items;
                    break;
                case 13115:
                    FadeIn = new Transition();
                    FadeIn.Read(chunkReader);
                    break;
                case 13116:
                    FadeOut = new Transition();
                    FadeOut.Read(chunkReader);
                    break;
                case 13122:
                    VirtualRect = new VirtualRect();
                    VirtualRect.Read(chunkReader);
                    break;
                case 13125: // Layer Effects
                    var start = chunkReader.Tell();
                    var end = start + chunkReader.Size();
                    if (start == end) break;

                    var current = 0;
                    while (true)
                    {
                        if (chunkReader.Tell() == end ||
                            Layers.Items.Count <= current) break;
                        var layer = Layers.Items[current];
                        layer.InkEffect = chunkReader.ReadByte();
                        chunkReader.Skip(3);
                        if (layer.InkEffect != 1)
                        {
                            var b = chunkReader.ReadByte();
                            var g = chunkReader.ReadByte();
                            var r = chunkReader.ReadByte();
                            layer.RgbCoeff = Color.FromArgb(0, r, g, b);
                            layer.Blend = chunkReader.ReadByte();
                        }
                        else
                        {
                            layer.InkEffectValue = chunkReader.ReadByte();
                        }

                        layer.ShaderData.HasShader = true;
                        try
                        {
                            var shaderHandle = chunkReader.ReadInt32();
                            var numberOfParams = chunkReader.ReadInt32();
                            var shdr = CTFAKCore.CurrentReader.GetGameData().Shaders.ShaderList[shaderHandle];
                            layer.ShaderData.Name = shdr.Name;
                            layer.ShaderData.ShaderHandle = shaderHandle;

                            for (var i = 0; i < numberOfParams; i++)
                            {
                                var param = shdr.Parameters[i];
                                object paramValue;
                                switch (param.Type)
                                {
                                    case 0:
                                        paramValue = chunkReader.ReadInt32();
                                        break;
                                    case 1:
                                        paramValue = chunkReader.ReadSingle();
                                        break;
                                    case 2:
                                        paramValue = chunkReader.ReadInt32();
                                        break;
                                    case 3:
                                        paramValue = chunkReader.ReadInt32(); //Image Handle
                                        break;
                                    default:
                                        paramValue = "unknownType";
                                        break;
                                }

                                layer.ShaderData.Parameters.Add(new Objects.ShaderParameter
                                {
                                    Name = param.Name,
                                    ValueType = param.Type,
                                    Value = paramValue
                                });
                            }
                            //Logger.Log($"Shader Handle: {shaderHandle}\nShader Name: {shdr.Name}\nNumber of Params: {numberOfParams}");
                        }
                        catch // No Shader Found
                        {
                            layer.ShaderData.HasShader = false;
                            current++;
                            break;
                        }

                        chunkReader.Seek(chunkReader.Tell() + 4);
                        current++;
                    }

                    break;
                case 13129: // Frame Effects
                    InkEffect = chunkReader.ReadInt32();
                    if (InkEffect != 1)
                    {
                        var b = chunkReader.ReadByte();
                        var g = chunkReader.ReadByte();
                        var r = chunkReader.ReadByte();
                        RgbCoeff = Color.FromArgb(0, r, g, b);
                        Blend = chunkReader.ReadByte();
                    }
                    else
                    {
                        InkEffectValue = chunkReader.ReadByte();
                    }

                    ShaderData.HasShader = true;
                    try
                    {
                        var shaderHandle = chunkReader.ReadInt32();
                        var numberOfParams = chunkReader.ReadInt32();
                        var shdr = CTFAKCore.CurrentReader.GetGameData().Shaders.ShaderList[shaderHandle];
                        ShaderData.Name = shdr.Name;
                        ShaderData.ShaderHandle = shaderHandle;

                        for (var i = 0; i < numberOfParams; i++)
                        {
                            var param = shdr.Parameters[i];
                            object paramValue;
                            switch (param.Type)
                            {
                                case 0:
                                    paramValue = chunkReader.ReadInt32();
                                    break;
                                case 1:
                                    paramValue = chunkReader.ReadSingle();
                                    break;
                                case 2:
                                    paramValue = chunkReader.ReadInt32();
                                    break;
                                case 3:
                                    paramValue = chunkReader.ReadInt32(); //Image Handle
                                    break;
                                default:
                                    paramValue = "unknownType";
                                    break;
                            }

                            ShaderData.Parameters.Add(new Objects.ShaderParameter
                            {
                                Name = param.Name,
                                ValueType = param.Type,
                                Value = paramValue
                            });
                        }
                        //Logger.Log($"Shader Handle: {shaderHandle}\nShader Name: {shdr.Name}\nNumber of Params: {numberOfParams}");
                    }
                    catch // No Shader Found
                    {
                        ShaderData.HasShader = false;
                    }

                    break;
            }
        }

        Logger.Log($"<color=green>Frame Found: {Name}, {Width}x{Height}, {Objects.Count} objects.</color>");
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class Layers : ChunkLoader
{
    public List<Layer> Items;

    public override void Read(ByteReader reader)
    {
        Items = new List<Layer>();
        var count = reader.ReadUInt32();
        for (var i = 0; i < count; i++)
        {
            var item = new Layer();
            item.Read(reader);
            Items.Add(item);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Items.Count);
        foreach (var layer in Items) layer.Write(writer);
    }
}

public class Layer : ChunkLoader
{
    public int BackgroudIndex;
    public byte Blend;

    public BitDict Flags = new(new[]
    {
        "XCoefficient",
        "YCoefficient",
        "DoNotSaveBackground",
        "",
        "Visible",
        "WrapHorizontally",
        "WrapVertically",
        "", "", "", "",
        "", "", "", "", "",
        "Redraw",
        "ToHide",
        "ToShow"
    });

    public int InkEffect;
    public int InkEffectValue;
    public string Name;
    public int NumberOfBackgrounds;
    public Color RgbCoeff;
    public ShaderData ShaderData = new();

    public float XCoeff;
    public float YCoeff;

    public override void Read(ByteReader reader)
    {
        Flags.Flag = reader.ReadUInt32();
        XCoeff = reader.ReadSingle();
        YCoeff = reader.ReadSingle();
        NumberOfBackgrounds = reader.ReadInt32();
        BackgroudIndex = reader.ReadInt32();
        Name = reader.ReadUniversal();
        if (Settings.Android)
        {
            XCoeff = 1;
            YCoeff = 1;
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32((int)Flags.Flag);
        writer.WriteSingle(XCoeff);
        writer.WriteSingle(YCoeff);
        writer.WriteInt32(NumberOfBackgrounds);
        writer.WriteInt32(BackgroudIndex);
        writer.WriteUnicode(Name);
    }
}

public class FramePalette : ChunkLoader
{
    public List<Color> Items;

    public override void Read(ByteReader reader)
    {
        Items = new List<Color>();
        for (var i = 0; i < 257; i++) Items.Add(reader.ReadColor());
    }

    public override void Write(ByteWriter writer)
    {
        foreach (var item in Items) writer.WriteColor(item);
    }
}