using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CTFAK.CCN.Chunks.Objects.ObjectInfo;

namespace CTFAK.MFA
{
    public class MFAChunks : ChunkLoader // This is used for MFA reading/writing
    {
        public byte[] Saved;
        public List<MFAChunk> Items = new List<MFAChunk>();
        public bool Log = false;

        public T GetOrCreateChunk<T>() where T : MFAChunkLoader, new()
        {
            foreach (MFAChunk chunk in Items)
            {
                if (chunk.Loader.GetType() == typeof(T))
                {
                    return (T)chunk.Loader;
                }
            }
            var newChunk = new MFAChunk(null);
            if (typeof(T)==typeof(FrameVirtualRect)) newChunk.Id = 33;
            else if (typeof(T) == typeof(LayerShaderSettings)) newChunk.Id = 37;
            else if (typeof(T) == typeof(FrameMovementTimer)) newChunk.Id = 39;
            else if (typeof(T) == typeof(FrameShaderSettings)) newChunk.Id = 40;
            else if (typeof(T) == typeof(ObjectShaderSettings)) newChunk.Id = 45;
            newChunk.Loader = new T();
            Items.Add(newChunk);
            return (T)newChunk.Loader;
        }

        public bool ContainsChunk<T>() where T : MFAChunkLoader
        {
            foreach (MFAChunk chunk in Items)
            {
                if (chunk.Loader.GetType() == typeof(T))
                {
                    return true;
                }
            }
            return false;
        }

        public T NewChunk<T>() where T : MFAChunkLoader, new()
        {
            var newChunk = new MFAChunk(null);
            if (typeof(T) == typeof(FrameVirtualRect)) newChunk.Id = 33;
            else if (typeof(T) == typeof(LayerShaderSettings)) newChunk.Id = 37;
            else if (typeof(T) == typeof(FrameMovementTimer)) newChunk.Id = 39;
            else if (typeof(T) == typeof(FrameShaderSettings)) newChunk.Id = 40;
            else if (typeof(T) == typeof(ObjectShaderSettings)) newChunk.Id = 45;
            newChunk.Loader = new T();
            Items.Add(newChunk);
            return (T)newChunk.Loader;
        }

        public override void Write(ByteWriter Writer)
        {
            foreach (MFAChunk chunk in Items)
                chunk.Write(Writer);
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
                else Items.Add(newChunk);
            }

            var size = reader.Tell() - start;
            reader.Seek(start);
            Saved = reader.ReadBytes((int)size);
        }
    }


    public class MFAChunk
    {

        public ByteReader Reader;
        public MFAChunkLoader Loader;
        public byte Id;
        public byte[] Data;

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
            if (MFAChunkList.ChunkNames.TryGetValue(Id, out string chunkName))
                Logger.Log($"Reading MFA Chunk {Id} ({chunkName})");
            //else
                //Logger.Log($"Reading MFA Chunk {Id}");
            var dataReader = new ByteReader(Data);
            switch (Id)
            {
                case 33:
                    Loader = new FrameVirtualRect();
                    break;
                case 37:
                    Loader = new LayerShaderSettings();
                    break;
                case 39:
                    Loader = new FrameMovementTimer();
                    break;
                case 40:
                    Loader = new FrameShaderSettings();
                    break;
                case 45:
                    Loader = new ObjectShaderSettings();
                    break;
                default:
                    Loader = null;
                    //Logger.Log("No Reader for Chunk " + Id);
                    if (CTFAKCore.parameters.Contains("-dumpnewchunks"))
                        File.WriteAllBytes("UnkChunks\\" + Id + ".bin", dataReader.ReadBytes());
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

    public class ObjectShaderSettings : MFAChunkLoader
    {
        public class ShaderParameter : MFAChunkLoader
        {
            public string Name;
            public int ValueType;
            public object Value;
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
                        Value = reader.ReadInt32();//THIS IS ACTUALLY COLOR
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
            public List<ShaderParameter> Parameters = new List<ShaderParameter>();
            public string Name;
            public override void Read(ByteReader reader)
            {
                Name = reader.AutoReadUnicode();
                var numParams = reader.ReadInt32();
                for (int i = 0; i < numParams; i++)
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
                foreach (var param in Parameters)
                {
                    param.Write(Writer);
                }
            }
        }

        public Color RGBCoeff;
        public byte Blend;
        public int HeaderID = 0;
        public int InkEffect;
        public List<MFAShader> Shaders = new();

        public override void Read(ByteReader reader)
        {
            var b = reader.ReadByte();
            var g = reader.ReadByte();
            var r = reader.ReadByte();
            Blend = reader.ReadByte();
            RGBCoeff = Color.FromArgb(Blend, r, g, b);

            var numShaders = reader.ReadInt32();
            for (int i = 0; i < numShaders; i++)
            {
                var newParam = new MFAShader();
                newParam.Read(reader);
                Shaders.Add(newParam);
            }
        }

        public override void Write(ByteWriter Writer)
        {
            if (HeaderID == 2) Writer.WriteInt32(InkEffect);
            Writer.WriteInt8(RGBCoeff.B);
            Writer.WriteInt8(RGBCoeff.G);
            Writer.WriteInt8(RGBCoeff.R);
            Writer.WriteInt8(Blend);
            if (HeaderID == 1) Writer.WriteInt32(9253);
            else
            {
                Writer.WriteInt32(Shaders.Count);
                foreach (var shdr in Shaders)
                    shdr.Write(Writer);
            }
        }
    }

    public class FrameShaderSettings : MFAChunkLoader
    {
        public class ShaderParameter : MFAChunkLoader
        {
            public string Name;
            public int ValueType;
            public object Value;
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
                        Value = reader.ReadInt32();//THIS IS ACTUALLY COLOR
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
            public List<ShaderParameter> Parameters = new List<ShaderParameter>();
            public string Name;
            public override void Read(ByteReader reader)
            {
                Name = reader.AutoReadUnicode();
                var numParams = reader.ReadInt32();
                for (int i = 0; i < numParams; i++)
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
                foreach (var param in Parameters)
                    param.Write(Writer);
            }
        }

        public Color RGBCoeff;
        public byte Blend;
        public int HeaderID = 0;
        public int Effect;
        public List<MFAShader> Shaders = new();

        public override void Read(ByteReader reader)
        {
            var b = reader.ReadByte();
            var g = reader.ReadByte();
            var r = reader.ReadByte();
            Blend = reader.ReadByte();
            RGBCoeff = Color.FromArgb(Blend, r, g, b);

            var numShaders = reader.ReadInt32();
            for (int i = 0; i < numShaders; i++)
            {
                var newParam = new MFAShader();
                newParam.Read(reader);
                Shaders.Add(newParam);
            }
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Effect);
            Writer.WriteColor(RGBCoeff);
            Writer.WriteInt32(Shaders.Count);
            foreach (var shdr in Shaders)
                shdr.Write(Writer);
        }
    }

    public class LayerShaderSettings : MFAChunkLoader
    {
        public class ShaderParameter : MFAChunkLoader
        {
            public string Name;
            public int ValueType;
            public object Value;
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
                        Value = reader.ReadInt32();//THIS IS ACTUALLY COLOR
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
            public List<ShaderParameter> Parameters = new List<ShaderParameter>();
            public string Name;
            public override void Read(ByteReader reader)
            {
                Name = reader.AutoReadUnicode();
                var numParams = reader.ReadInt32();
                for (int i = 0; i < numParams; i++)
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
                foreach (var param in Parameters)
                {
                    param.Write(Writer);
                }
            }
        }

        public class MFALayerShader : MFAChunkLoader
        {
            public int InkInMyBum = 0;
            public Color RGBCoeff = Color.White;
            public List<MFAShader> Shaders = new();

            public override void Read(ByteReader reader)
            {
                InkInMyBum = reader.ReadInt32();
                RGBCoeff = reader.ReadColor();
                var numShaders = reader.ReadInt32();
                for (int i = 0; i < numShaders; i++)
                {
                    var newParam = new MFAShader();
                    newParam.Read(reader);
                    Shaders.Add(newParam);
                }
            }

            public override void Write(ByteWriter Writer)
            {
                Writer.WriteInt32(InkInMyBum);
                Writer.WriteColor(RGBCoeff);
                Writer.WriteInt32(Shaders.Count);
                foreach (var shdr in Shaders)
                    shdr.Write(Writer);
            }
        }

        public List<MFALayerShader> LayerShaders = new();

        public override void Read(ByteReader reader)
        {
            while (reader.Tell() < reader.Size())
            {
                MFALayerShader layer = new MFALayerShader();
                layer.Read(reader);
                LayerShaders.Add(layer);
            }
        }

        public override void Write(ByteWriter Writer)
        {
            foreach (MFALayerShader layer in LayerShaders)
                layer.Write(Writer);
        }
    }

    public class FrameVirtualRect : MFAChunkLoader
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

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

    public class FrameMovementTimer : MFAChunkLoader
    {
        public int Timer;

        public override void Read(ByteReader reader)
        {
            Timer = reader.ReadInt32();
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Timer);
        }
    }

    public abstract class MFAChunkLoader
    {
        public abstract void Read(ByteReader reader);
        public abstract void Write(ByteWriter Writer);
    }
}
