using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.Attributes;
using CTFAK.Memory;
using Ionic.Zlib;

namespace CTFAK.MMFParser.CCN.Chunks.Objects;

public class TwoFilePlusContainer
{
    public static TwoFilePlusContainer Instance;
    public Dictionary<int, ObjectInfo> ObjectsContainer = new();

    public TwoFilePlusContainer()
    {
        Instance = this;
    }
}

[ChunkLoader(8790, "TwoFivePlusProperties")]
public class TwoFilePlusProps : ChunkLoader
{
    public override void Read(ByteReader reader)
    {
        var start = reader.Tell();
        var end = start + reader.Size();
        if (start == end) return;
        reader.ReadInt32();

        var current = 0;
        while (reader.Tell() <= end)
        {
            var currentPosition = reader.Tell();
            var chunkSize = reader.ReadInt32();
            var data = reader.ReadBytes(chunkSize);
            var decompressed = ZlibStream.UncompressBuffer(data);
            var decompressedReader = new ByteReader(decompressed);
            var objectData = TwoFilePlusContainer.Instance.ObjectsContainer[current];

            if (objectData.ObjectType == 0)
                objectData.Properties = new Quickbackdrop();
            else if (objectData.ObjectType == 1)
                objectData.Properties = new Backdrop();
            else
                objectData.Properties = new ObjectCommon(null);

            objectData.Properties.Read(decompressedReader);
            TwoFilePlusContainer.Instance.ObjectsContainer[current] = objectData;
            reader.Seek(currentPosition + chunkSize + 8);
            current++;
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

[ChunkLoader(8788, "TwoFivePlusNames")]
public class TwoFivePlusNames : ChunkLoader
{
    public override void Read(ByteReader reader)
    {
        var nstart = reader.Tell();

        var nend = nstart + reader.Size();
        //reader.ReadInt32();
        var ncurrent = 0;
        while (reader.Tell() < nend)
        {
            TwoFilePlusContainer.Instance.ObjectsContainer[ncurrent].Name = reader.ReadUniversal();
            ncurrent++;
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

[ChunkLoader(8787, "TwoFivePlusHeaders")]
public class TwoFivePlusHeaders : ChunkLoader
{
    public override void Read(ByteReader reader)
    {
        new TwoFilePlusContainer(); // This is quite stupid, I don't like that, but I wanna refactor everything else before starting to actually rewrite everything
        while (true)
        {
            if (reader.Tell() >= reader.Size()) break;
            var newObject = new ObjectInfo();
            newObject.Handle = reader.ReadInt16();
            newObject.ObjectType = reader.ReadInt16();
            newObject.Flags = reader.ReadInt16();
            reader.Skip(2);
            newObject.InkEffect = reader.ReadByte();
            if (newObject.InkEffect != 1)
            {
                reader.Skip(3);
                var r = reader.ReadByte();
                var g = reader.ReadByte();
                var b = reader.ReadByte();
                newObject.RgbCoeff = Color.FromArgb(0, b, g, r);
                newObject.Blend = reader.ReadByte();
            }
            else
            {
                var flag = reader.ReadByte();
                reader.Skip(2);
                newObject.InkEffectValue = reader.ReadByte();
                reader.Skip(3);
            }

            TwoFilePlusContainer.Instance.ObjectsContainer.Add(newObject.Handle, newObject);
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

[ChunkLoader(8789, "TwoFivePlusShaders")]
public class TwoFivePlusShaders : ChunkLoader
{
    public override void Read(ByteReader reader)
    {
        var start = reader.Tell();
        var end = start + reader.Size();
        if (start == end) return;

        var current = 0;
        while (true)
        {
            var paramStart = reader.Tell() + 4;
            if (reader.Tell() == end) return;
            var size = reader.ReadInt32();
            if (size == 0)
            {
                current++;
                continue;
            }

            var obj = TwoFilePlusContainer.Instance.ObjectsContainer[current];
            obj.ShaderData.HasShader = true;

            var shaderHandle = reader.ReadInt32();
            var numberOfParams = reader.ReadInt32();
            var shdr = CTFAKCore.CurrentReader.GetGameData().Shaders.ShaderList[shaderHandle];
            obj.ShaderData.Name = shdr.Name;
            obj.ShaderData.ShaderHandle = shaderHandle;

            for (var i = 0; i < numberOfParams; i++)
            {
                var param = shdr.Parameters[i];
                object paramValue;
                switch (param.Type)
                {
                    case 0:
                        paramValue = reader.ReadInt32();
                        break;
                    case 1:
                        paramValue = reader.ReadSingle();
                        break;
                    case 2:
                        paramValue = reader.ReadInt32();
                        break;
                    case 3:
                        paramValue = reader.ReadInt32();
                        break;
                    default:
                        paramValue = "unknownType";
                        break;
                }

                obj.ShaderData.Parameters.Add(new ShaderParameter
                    { Name = param.Name, ValueType = param.Type, Value = paramValue });
            }

            reader.Seek(paramStart + size);
            current++;
        }
    }

    public override void Write(ByteWriter writer)
    {
    }
}