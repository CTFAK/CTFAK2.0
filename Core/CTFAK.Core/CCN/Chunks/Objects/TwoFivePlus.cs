using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.Attributes;
using CTFAK.Memory;
using Ionic.Zlib;

namespace CTFAK.CCN.Chunks.Objects;

public class TwoFilePlusContainer
{
    public static TwoFilePlusContainer instance;
    public Dictionary<int, ObjectInfo> objectsContainer = new();

    public TwoFilePlusContainer()
    {
        instance = this;
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
            var objectData = TwoFilePlusContainer.instance.objectsContainer[current];

            if (objectData.ObjectType == 0)
                objectData.properties = new Quickbackdrop();
            else if (objectData.ObjectType == 1)
                objectData.properties = new Backdrop();
            else
                objectData.properties = new ObjectCommon(null);

            objectData.properties.Read(decompressedReader);
            TwoFilePlusContainer.instance.objectsContainer[current] = objectData;
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
            TwoFilePlusContainer.instance.objectsContainer[ncurrent].name = reader.ReadUniversal();
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
        new TwoFilePlusContainer();
        while (true)
        {
            if (reader.Tell() >= reader.Size()) break;
            var newObject = new ObjectInfo();
            newObject.handle = reader.ReadInt16();
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
                newObject.rgbCoeff = Color.FromArgb(0, b, g, r);
                newObject.blend = reader.ReadByte();
            }
            else
            {
                var flag = reader.ReadByte();
                reader.Skip(2);
                newObject.InkEffectValue = reader.ReadByte();
                reader.Skip(3);
            }

            TwoFilePlusContainer.instance.objectsContainer.Add(newObject.handle, newObject);
        }
    }

    public override void Write(ByteWriter Writer)
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

            var obj = TwoFilePlusContainer.instance.objectsContainer[current];
            obj.shaderData.hasShader = true;

            var shaderHandle = reader.ReadInt32();
            var numberOfParams = reader.ReadInt32();
            var shdr = Core.currentReader.getGameData().Shaders.ShaderList[shaderHandle];
            obj.shaderData.name = shdr.Name;
            obj.shaderData.ShaderHandle = shaderHandle;

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

                obj.shaderData.parameters.Add(new ObjectInfo.ShaderParameter
                    { Name = param.Name, ValueType = param.Type, Value = paramValue });
            }

            reader.Seek(paramStart + size);
            current++;
        }
    }

    public override void Write(ByteWriter Writer)
    {
    }
}