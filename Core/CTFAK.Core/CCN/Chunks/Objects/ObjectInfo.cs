using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks.Objects
{
    public class ObjectInfo : ChunkLoader
    {
        public int handle;
        public string name;
        public ChunkLoader properties;
        public int ObjectType;
        public int Flags;
        public int Reserved;
        public int InkEffect;
        public int InkEffectValue;
        public Color rgbCoeff;
        public byte blend;

        public ShaderData shaderData = new();
        //public int shaderId;
        //public List<ByteReader> effectItems;

        public class ShaderParameter
        {
            public string Name;
            public int ValueType;
            public object Value;
        }
        public class ShaderData
        {
            public bool hasShader = false;
            public string name;
            public int ShaderHandle;
            public List<ShaderParameter> parameters = new();
        }

        public override void Read(ByteReader reader)
        {
            while (true)
            {
                var newChunk = new Chunk();
                var chunkData = newChunk.Read(reader);
                var chunkReader = new ByteReader(chunkData);
                if (CTFAKCore.parameters.Contains("-onlyimages"))
                {
                    if (newChunk.Id != 17476 && // Header
                        newChunk.Id != 17477 && // Name
                        newChunk.Id != 17478)   // Properties
                        continue;
                }
                if (newChunk.Id == 32639) break;
                //Logger.Log("Object Chunk ID " + newChunk.Id);
                switch (newChunk.Id)
                {
                    case 17476:
                        handle = chunkReader.ReadInt16();
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
                            rgbCoeff = Color.FromArgb(0, b, g, r);
                            blend = chunkReader.ReadByte();
                        }
                        else
                        {
                            var flag = chunkReader.ReadByte();
                            chunkReader.Skip(2);
                            InkEffectValue = chunkReader.ReadByte();
                            chunkReader.Skip(3);
                        }

                        if (Settings.Old || Settings.gameType == Settings.GameType.MMF2)
                        {
                            rgbCoeff = Color.White;
                            blend = 255;
                        }
                        break;
                    case 17477:
                        name = chunkReader.ReadYuniversal();
                        break;
                    case 17478:        
                        if (ObjectType == 0) properties = new Quickbackdrop();
                        else if (ObjectType == 1) properties = new Backdrop();
                        else properties = new ObjectCommon(this);

                        properties?.Read(chunkReader);
                        break;
                    case 17480:
                        try
                        {
                            var shaderHandle = chunkReader.ReadInt32();
                            var numberOfParams = chunkReader.ReadInt32();
                            var shdr = CTFAKCore.currentReader.getGameData().shaders.ShaderList[shaderHandle];
                            shaderData.name = shdr.Name;
                            shaderData.ShaderHandle = shaderHandle;
                            shaderData.hasShader = true;

                            for (int i = 0; i < numberOfParams; i++)
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
                                shaderData.parameters.Add(new ShaderParameter()
                                {
                                    Name = param.Name,
                                    ValueType = param.Type,
                                    Value = paramValue
                                });
                            }
                        }
                        catch
                        {
                            shaderData.hasShader = false;
                        }
                        break;
                    default:
                        Logger.Log("No Reader for ObjectInfo Chunk " + newChunk.Id);
                        if (CTFAKCore.parameters.Contains("-dumpnewchunks"))
                            File.WriteAllBytes("UnkChunks\\ObjectInfo\\" + newChunk.Id + ".bin", chunkReader.ReadBytes());
                        break;
                }
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
