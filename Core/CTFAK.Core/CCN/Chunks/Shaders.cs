using System.Collections.Generic;
using System.IO;
using CTFAK.CCN.Chunks;
using CTFAK.Core.Utils;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders
{
    public class Shaders:ChunkLoader
    {
        public Dictionary<int, Shader> ShaderList;

        public override void Read(ByteReader reader)
        {
            var start = reader.Tell();
            var count = reader.ReadInt32();
            List<int> offsets = new List<int>();
            ShaderList = new Dictionary<int, Shader>();
            for (int i = 0; i < count; i++)
            {
                offsets.Add(reader.ReadInt32());
            }

            foreach (int offset in offsets)
            {
                try
                {
                    reader.Seek(start + offset);
                    var shader = new Shader();
                    shader.Read(reader);
                    ShaderList.Add(offsets.IndexOf(offset), shader);
                }
                catch
                {
                    Logger.Log("Invalid Shader.");
                }
            }
        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }


    }
    public class Shader:ChunkLoader
    {
        public List<ShaderParameter> Parameters = new List<ShaderParameter>();
        public string Name;
        public string Data;
        public int BackgroundTexture;
        public override void Read(ByteReader reader)
        {
            var start = reader.Tell();
            var nameOffset = reader.ReadInt32();
            var dataOffset = reader.ReadInt32();
            var parameterOffset = reader.ReadInt32();
            BackgroundTexture = reader.ReadInt32();
            reader.Seek(start+nameOffset);
            Name = reader.ReadAscii();
            reader.Seek(start+dataOffset);
            Data = reader.ReadAscii();
            if (parameterOffset != 0)
            {
                parameterOffset = (int) (parameterOffset + start);
                reader.Seek(parameterOffset);
                var paramCount = reader.ReadInt32();
                
                for (int i = 0; i < paramCount; i++)
                {
                    var newParameter = new ShaderParameter();
                    Parameters.Add(newParameter);
                }

                var typeOffset = reader.ReadInt32();
                var namesOffset = reader.ReadInt32();
                reader.Seek(parameterOffset+typeOffset);
                foreach (var parameter in Parameters)
                {
                    parameter.Type = reader.ReadByte();
                }
                reader.Seek(parameterOffset+namesOffset);
                foreach (ShaderParameter parameter in Parameters)
                {
                    parameter.Name = reader.ReadAscii();
                }
            }
            ShaderGenerator.CreateAndDumpShader(this);
        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }


    }
    public class ShaderParameter:ChunkLoader
    {
        public string Name;
        public int Type;
        public int Value;
        public override void Read(ByteReader reader)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(ByteWriter Writer)
        {
            throw new System.NotImplementedException();
        }


        public string GetValueType()
        {
            switch (Type)
            {
                case 0: return "int";
                case 1: return "float";
                case 2: return "int_float4";
                case 3: return "image";
                default: return "unk";
            }
        }
    }
}