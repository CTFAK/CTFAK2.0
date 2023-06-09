using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.CCN.Chunks.Objects;

public class ShaderParameter
{
    public string Name;
    public object Value;
    public int ValueType;
}

public class ShaderData
{
    public bool HasShader;
    public string Name;
    public List<ShaderParameter> Parameters = new();
    public int ShaderHandle;
}

public class ObjectInfo : ChunkLoader
{
    public byte Blend;
    public int Flags;
    public int Handle;
    public int InkEffect;
    public int InkEffectValue;
    public string Name;
    public int ObjectType;
    public ChunkLoader Properties;
    public int Reserved;
    public Color RgbCoeff;

    public ShaderData ShaderData = new();

    public override void Read(ByteReader reader)
    {
        while (true)
        {
            var newChunk = new Chunk();
            var chunkData = newChunk.Read(reader);
            var chunkReader = new ByteReader(chunkData);

            if (newChunk.Id == 32639) break;
            //Logger.Log("Object Chunk ID " + newChunk.Id);
            switch (newChunk.Id)
            {
                case 17477:
                    Name = chunkReader.ReadUniversal();
                    break;
                case 17476:
                    Handle = chunkReader.ReadInt16();
                    ObjectType = chunkReader.ReadInt16();
                    Flags = chunkReader.ReadInt16();
                    chunkReader.Skip(2);
                    InkEffect = chunkReader.ReadByte();
                    if (InkEffect != 1)
                    {
                        chunkReader.Skip(3);
                        var r = chunkReader.ReadByte();
                        var g = chunkReader.ReadByte();
                        var b = chunkReader.ReadByte();
                        RgbCoeff = Color.FromArgb(0, r, g, b);
                        Blend = chunkReader.ReadByte();
                    }
                    else
                    {
                        var flag = chunkReader.ReadByte();
                        chunkReader.Skip(2);
                        InkEffectValue = chunkReader.ReadByte();
                    }

                    if (Settings.Old)
                    {
                        RgbCoeff = Color.White;
                        Blend = 255;
                    }

                    break;
                case 17478:
                    Properties = ObjectType switch
                    {
                        0 => new Quickbackdrop(),
                        1 => new Backdrop(),
                        _ => new ObjectCommon(this)
                    };
                    Properties?.Read(chunkReader);

                    break;

                case 17480:
                    ShaderData.HasShader = true;
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
                                paramValue = chunkReader.ReadInt32(); //image handle
                                break;
                            default:
                                paramValue = "unknownType";
                                break;
                        }

                        ShaderData.Parameters.Add(new ShaderParameter
                            { Name = param.Name, ValueType = param.Type, Value = paramValue });
                    }

                    break;
            }
        }

        if (CTFAKCore.Parameters.Contains("-debug"))
            Logger.Log($"Found object: {Name} - {(Constants.ObjectType)ObjectType}");
        if (string.IsNullOrEmpty(Name))
            Name = $"{(Constants.ObjectType)ObjectType} {Handle}";
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }

    //public int shaderId;
    //public List<ByteReader> effectItems;
}