using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MFA;

public class MFAChunkList : ChunkLoader //This is used for MFA reading/writing
{
    public List<MFAChunk> Items = new();
    public bool Log = false;
    public byte[] Saved;

    public T GetOrCreateChunk<T>(byte overrideId = 0) where T : MFAChunkLoader, new()
    {
        foreach (var chunk in Items)
            if (chunk.Loader.GetType() == typeof(T))
                return (T)chunk.Loader;
        var newChunk = new MFAChunk(null);
        if (typeof(T) == typeof(ShaderSettings)) newChunk.Id = 45;
        else if (typeof(T) == typeof(FrameVirtualRect)) newChunk.Id = 33;
        if (overrideId != 0) newChunk.Id = overrideId;
        newChunk.Loader = new T();
        Items.Add(newChunk);
        return (T)newChunk.Loader;
    }

    public bool ContainsChunk<T>() where T : MFAChunkLoader
    {
        foreach (var chunk in Items)
            if (chunk.Loader.GetType() == typeof(T))
                return true;
        return false;
    }

    public MFAChunk NewChunk<T>() where T : MFAChunkLoader, new()
    {
        var newChunk = new MFAChunk(null);
        newChunk.Id = 33;
        newChunk.Loader = new T();
        return newChunk;
    }

    public override void Write(ByteWriter Writer)
    {
        foreach (var chunk in Items) chunk.Write(Writer);
        Writer.WriteInt8(0);
    }

    public override void Read(ByteReader reader)
    {
        var start = reader.Tell();
        while (true)
        {
            var newChunk = new MFAChunk(reader);
            newChunk.Read();
            if (newChunk.Id == 0) break;

            Items.Add(newChunk);
        }

        var size = reader.Tell() - start;
        reader.Seek(start);
        Saved = reader.ReadBytes((int)size);
    }
}

public class MFAChunk
{
    public byte[] Data;
    public byte Id;
    public MFAChunkLoader Loader;
    public ByteReader Reader;

    public MFAChunk(ByteReader reader)
    {
        Reader = reader;
    }

    public void Read()
    {
        Id = Reader.ReadByte();

        if (Id == 0) return;
        var size = Reader.ReadInt32();
        Data = Reader.ReadBytes(size);
        var dataReader = new ByteReader(Data);
        switch (Id)
        {
            case 33:
                Loader = new FrameVirtualRect();
                break;
            case 45:
                Loader = new ShaderSettings();
                break;
            default:
                Loader = null;
                Logger.LogWarning($"Missing MFA chunk loader for chunk {Id}");
                break;
        }

        Loader?.Read(dataReader);
    }

    public void Write(ByteWriter writer)
    {
        writer.WriteInt8(Id);
        if (Id == 0) return;
        if (Loader == null)
        {
            writer.WriteInt32(Data.Length);
            writer.WriteBytes(Data);
        }
        else
        {
            var newWriter = new ByteWriter(new MemoryStream());
            Loader.Write(newWriter);
            writer.WriteInt32((int)newWriter.Size());
            writer.WriteWriter(newWriter);
        }
    }
}

public class ShaderSettings : MFAChunkLoader
{
    public byte Blend;
    public Color RGBCoeff;
    public List<MFAShader> Shaders = new();

    public override void Read(ByteReader reader)
    {
        var b = reader.ReadByte();
        var g = reader.ReadByte();
        var r = reader.ReadByte();
        Blend = reader.ReadByte();
        RGBCoeff = Color.FromArgb(Blend, r, g, b);

        var numShaders = reader.ReadInt32();
        for (var i = 0; i < numShaders; i++)
        {
            var newParam = new MFAShader();
            newParam.Read(reader);
            Shaders.Add(newParam);
        }
    }

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt8(RGBCoeff.B);
        Writer.WriteInt8(RGBCoeff.G);
        Writer.WriteInt8(RGBCoeff.R);
        Writer.WriteInt8(Blend);
        Writer.WriteInt32(Shaders.Count);
        foreach (var shdr in Shaders) shdr.Write(Writer);
    }

    public class ShaderParameter : MFAChunkLoader
    {
        public string Name;
        public object Value;
        public int ValueType;

        public override void Read(ByteReader reader)
        {
            //case 0: return "int";
            //case 1: return "float";
            //case 2: return "int_float4";
            //case 3: return "image";
            //default: return "unk";
            Name = reader.AutoReadUnicode();
            ValueType = reader.ReadInt32();
            switch (ValueType)
            {
                case 0:
                    Value = reader.ReadInt32();
                    break;
                case 1:
                    Value = reader.ReadSingle();
                    break;
                case 2:
                    Value = reader.ReadInt32(); //THIS IS ACTUALLY COLOR
                    break;
                case 3:
                    Value = reader.ReadInt32();
                    break;
            }
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32(ValueType);
            switch (ValueType)
            {
                case 0:
                    Writer.WriteInt32((int)Value);
                    break;
                case 1:
                    Writer.WriteSingle((float)Value);
                    break;
                case 2:
                    Writer.WriteInt32((int)Value);
                    break;
                case 3:
                    Writer.WriteInt32((int)Value);
                    break;
            }
        }
    }

    public class MFAShader : MFAChunkLoader
    {
        public string Name;
        public List<ShaderParameter> Parameters = new();

        public override void Read(ByteReader reader)
        {
            Name = reader.AutoReadUnicode();
            var numParams = reader.ReadInt32();
            for (var i = 0; i < numParams; i++)
            {
                var param = new ShaderParameter();
                param.Read(reader);
                Parameters.Add(param);
            }
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32(Parameters.Count);
            foreach (var param in Parameters) param.Write(Writer);
        }
    }
}

public class FrameVirtualRect : MFAChunkLoader
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

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(Left);
        Writer.WriteInt32(Top);
        Writer.WriteInt32(Right);
        Writer.WriteInt32(Bottom);
    }
}

public abstract class MFAChunkLoader
{
    public abstract void Read(ByteReader reader);
    public abstract void Write(ByteWriter Writer);
}