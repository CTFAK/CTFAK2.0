using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static CTFAK.CCN.Chunks.Objects.ObjectInfo;

namespace CTFAK.CCN.Chunks.Frame
{
    public class ObjectInstance : ChunkLoader
    {
        public ushort handle;
        public ushort objectInfo;
        public int x;
        public int y;
        public short parentType;
        public short layer;
        public short instance;
        public short parentHandle;
        public override void Read(ByteReader reader)
        {
            handle = reader.ReadUInt16();
            objectInfo = reader.ReadUInt16();

            if (Settings.Old)
            {
                y = reader.ReadInt16();
                x = reader.ReadInt16();
            }
            else
            {
                x = reader.ReadInt32();
                y = reader.ReadInt32();
            }
            parentType = reader.ReadInt16();
            parentHandle = reader.ReadInt16();
            if (Settings.Old || Settings.F3) return;
            layer = reader.ReadInt16();
            instance = reader.ReadInt16();
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
    public class VirtualRect : ChunkLoader
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public override void Read(ByteReader reader)
        {
            left = reader.ReadInt32();
            top = reader.ReadInt32();
            right = reader.ReadInt32();
            bottom = reader.ReadInt32();
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
    public class Frame : ChunkLoader
    {
        public string name;
        public int width;
        public int height;
        public Color background;
        public Events events;
        public BitDict flags = new BitDict(new string[]
        {
            "DisplayTitle",
            "GrabDesktop",
            "KeepDisplay",
            "Unk1",
            "Unk2",
            "HandleCollision",
            "Unk3",
            "Unk4",
            "ResizeAtStart",
            "Unk5",
            "Unk6",
            "Unk7",
            "Unk8",
            "Unk9",
            "Unk10",
            "TimeMovements",
            "Unk11",
            "Unk12",
            "DontInclude",
            "DontEraseBG"
        });
        public List<ObjectInstance> objects = new();
        public Layers layers;
        public List<Color> palette;
        public Transition fadeIn;
        public Transition fadeOut;
        public VirtualRect virtualRect;
        public ShaderData shaderData = new();
        public int InkEffect;
        public int InkEffectValue;
        public short Effect;
        public short EffectParam;
        public Color RGBCoeff;
        public byte blend;
        public int randomSeed;
        public int movementTimer = 60;

        public override void Read(ByteReader reader)
        {
            while (true)
            {
                if (reader.Tell() >= reader.Size()) break;
                var newChunk = new Chunk();
                var chunkData = newChunk.Read(reader);
                var chunkReader = new ByteReader(chunkData);
                if (CTFAKCore.parameters.Contains("-onlyimages"))
                {
                    if (newChunk.Id != 13109 && // Name
                        newChunk.Id != 13112)   // Object Instances
                        continue;
                }
                //Logger.Log("Reading Frame Chunk: " + newChunk.Id);
                if (ChunkList.ChunkNames.TryGetValue(newChunk.Id, out string chunkName))
                    Logger.Log($"Reading Chunk {newChunk.Id} ({chunkName})");
                else
                    Logger.Log($"Reading Chunk {newChunk.Id}");
                if (newChunk.Id == 32639) break;
                switch (newChunk.Id)
                {
                    case 13108: // Header
                        if (Settings.Old)
                        {
                            width = chunkReader.ReadInt16();
                            height = chunkReader.ReadInt16();
                            background = chunkReader.ReadColor();
                            flags.flag = chunkReader.ReadUInt16();
                        }
                        else
                        {
                            width = chunkReader.ReadInt32();
                            height = chunkReader.ReadInt32();
                            background = chunkReader.ReadColor();
                            flags.flag = chunkReader.ReadUInt32();
                        }
                        break;
                    case 13109: // Name
                        var frameName = new StringChunk();
                        frameName.Read(chunkReader);
                        name = frameName.value;
                        break;
                    case 13110: // Password
                        break;
                    case 13111: // Frame Palette
                        var pal = new FramePalette();
                        pal.Read(chunkReader);
                        palette = pal.Items;
                        break;
                    case 13112: // Object Instances
                        var count = chunkReader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            var objInst = new ObjectInstance();
                            objInst.Read(chunkReader);
                            objects.Add(objInst);
                        }
                        break;
                    case 13113:
                    case 13115: // Fade In
                        fadeIn = new Transition();
                        fadeIn.Read(chunkReader);
                        break;
                    case 13114:
                    case 13116: // Fade Out
                        fadeOut = new Transition();
                        fadeOut.Read(chunkReader);
                        break;
                    case 13117: // Events
                        events = new Events();
                        if (!CTFAKCore.parameters.Contains("-noevnt"))
                            events.Read(chunkReader);
                        break;
                    /*case 13118: // Play Header
                        break;
                    case 13119: // Additional Items
                        break;
                    case 13120: // Additional Items Instances
                        break;*/
                    case 13121: // Layers
                        layers = new Layers();
                        layers.Read(chunkReader);
                        break;
                    case 13122: // Virtual Size
                        virtualRect = new VirtualRect();
                        virtualRect.Read(chunkReader);
                        break;
                    /*case 13123: // Demo File Path
                        break;*/
                    case 13124: // Random Seed
                        randomSeed = chunkReader.ReadInt16();
                        break;
                    case 13125: // Layer Effects
                        var starte = chunkReader.Tell();
                        for (int i = 0; i < layers.Items.Count; i++)
                        {
                            layers.Items[i].Effect = chunkReader.ReadInt16();
                            layers.Items[i].EffectParam = chunkReader.ReadInt16();
                            layers.Items[i].RGBCoeff = chunkReader.ReadColor();
                            layers.Items[i].InkEffect = chunkReader.ReadInt32();
                            var numberOfParams = chunkReader.ReadInt32();
                            var offset = chunkReader.ReadInt32();
                            if (CTFAKCore.parameters.Contains("-chunk_info"))
                            {
                                Logger.Log("Effect: " + layers.Items[i].Effect);
                                Logger.Log("Effect Parameter: " + layers.Items[i].EffectParam);
                                Logger.Log("RGB Coefficient: " + layers.Items[i].RGBCoeff);
                                Logger.Log("Ink Effect: " + layers.Items[i].InkEffect);
                                Logger.Log("Number Of Parameters: " + numberOfParams);
                                Logger.Log("Offset: " + offset);
                            }
                            if (layers.Items[i].InkEffect == -1 || layers.Items[i].Effect <= 0)
                                continue;
                            layers.Items[i].shaderData.hasShader = true;
                            var returnOffset = chunkReader.Tell();
                            chunkReader.Seek(offset);
                            var shdr = CTFAKCore.currentReader.getGameData().shaders.ShaderList[layers.Items[i].InkEffect];
                            if (CTFAKCore.parameters.Contains("-chunk_info"))
                                Logger.Log("Shader Name: " + shdr.Name);
                            layers.Items[i].shaderData.name = shdr.Name;
                            layers.Items[i].shaderData.ShaderHandle = layers.Items[i].InkEffect;

                            for (int ii = 0; ii < numberOfParams; ii++)
                            {
                                var param = shdr.Parameters[ii];
                                object paramValue;
                                switch (param.Type)
                                {
                                    case 0:
                                    case 2:
                                    case 3: //Image Handle
                                        paramValue = chunkReader.ReadInt32();
                                        break;
                                    case 1:
                                        paramValue = chunkReader.ReadSingle();
                                        break;
                                    default:
                                        paramValue = "unknownType";
                                        break;
                                }
                                layers.Items[i].shaderData.parameters.Add(new ObjectInfo.ShaderParameter()
                                {
                                    Name = param.Name,
                                    ValueType = param.Type,
                                    Value = paramValue
                                });
                            }
                            chunkReader.Seek(returnOffset);
                        }
                        break;
                    case 13126: // Options
                        var frameAlpha = chunkReader.ReadInt32();
                        var keyTimeOut = chunkReader.ReadInt32();
                        break;
                    case 13127: // Movement Timer Base
                        movementTimer = chunkReader.ReadInt32();
                        break;
                    /*case 13128: // Mosaic Image Table
                        break;*/
                    case 13129: // Frame Effects
                        Effect = chunkReader.ReadInt16();
                        EffectParam = chunkReader.ReadInt16();
                        RGBCoeff = chunkReader.ReadColor();
                        InkEffect = chunkReader.ReadInt32();
                        var FrmNumberOfParams = chunkReader.ReadInt32();
                        if (CTFAKCore.parameters.Contains("-chunk_info"))
                        {
                            Logger.Log("Effect: " + Effect);
                            Logger.Log("Effect Parameter: " + EffectParam);
                            Logger.Log("RGB Coefficient: " + RGBCoeff);
                            Logger.Log("Ink Effect: " + InkEffect);
                            Logger.Log("Number Of Parameters: " + FrmNumberOfParams);
                        }
                        if (InkEffect == -1 || Effect <= 0)
                            continue;
                        shaderData.hasShader = true;
                        var FrmShdr = CTFAKCore.currentReader.getGameData().shaders.ShaderList[InkEffect];
                        shaderData.name = FrmShdr.Name;
                        if (CTFAKCore.parameters.Contains("-chunk_info"))
                            Logger.Log("Shader Name: " + FrmShdr.Name);
                        shaderData.ShaderHandle = InkEffect;

                        for (int i = 0; i < FrmNumberOfParams; i++)
                        {
                            if (FrmShdr.Parameters.Count <= i)
                                continue;
                            var param = FrmShdr.Parameters[i];
                            object paramValue;
                            switch (param.Type)
                            {
                                case 0:
                                case 2:
                                case 3: //Image Handle
                                    paramValue = chunkReader.ReadInt32();
                                    break;
                                case 1:
                                    paramValue = chunkReader.ReadSingle();
                                    break;
                                default:
                                    paramValue = "unknownType";
                                    break;
                            }
                            shaderData.parameters.Add(new ObjectInfo.ShaderParameter()
                            {
                                Name = param.Name,
                                ValueType = param.Type,
                                Value = paramValue
                            });
                        }
                        break;
                    case 13130: // iPhone Settings
                        var Joystick = chunkReader.ReadInt16();
                        var iPhoneOptions = chunkReader.ReadInt16();
                        break;
                    /*case 13131: // Unknown
                        break;
                    case 13132: // Unknown
                        break;*/
                    default:
                        Logger.Log("No Reader for Frame Chunk " + newChunk.Id);
                        if (CTFAKCore.parameters.Contains("-dumpnewchunks"))
                            File.WriteAllBytes("UnkChunks\\Frame\\" + newChunk.Id + ".bin", chunkReader.ReadBytes());
                        break;
                }
            }

            Logger.Log($"Frame Found: {name}, {width}x{height}, {objects.Count} objects.", true, ConsoleColor.Green);
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
            for (int i = 0; i < count; i++)
            {
                Layer item = new Layer();
                item.Read(reader);
                Items.Add(item);
            }

        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Items.Count);
            foreach (Layer layer in Items)
            {
                layer.Write(Writer);
            }
        }



    }

    public class Layer : ChunkLoader
    {
        public string Name;
        public BitDict Flags = new BitDict(new string[]
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
        }

        );
        public float XCoeff;
        public float YCoeff;
        public int NumberOfBackgrounds;
        public int BackgroudIndex;
        public ShaderData shaderData = new();
        public int InkEffect;
        public int InkEffectValue;
        public short Effect;
        public short EffectParam;
        public Color RGBCoeff;
        public byte blend;

        public override void Read(ByteReader reader)
        {
            Flags.flag = reader.ReadUInt32();
            XCoeff = reader.ReadSingle();
            YCoeff = reader.ReadSingle();
            NumberOfBackgrounds = reader.ReadInt32();
            BackgroudIndex = reader.ReadInt32();
            if (Settings.Fusion3Seed) reader.Skip(6);
            Name = reader.ReadYuniversal();
            if (Settings.Android)
            {
                XCoeff = 1;
                YCoeff = 1;
            }
        }

        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32((int)Flags.flag);
            Writer.WriteSingle(XCoeff);
            Writer.WriteSingle(YCoeff);
            Writer.WriteInt32(NumberOfBackgrounds);
            Writer.WriteInt32(BackgroudIndex);
            Writer.WriteUnicode(Name);
        }



    }

    public class FramePalette : ChunkLoader
    {
        public List<Color> Items;

        public override void Read(ByteReader reader)
        {
            Items = new List<Color>();
            for (int i = 0; i < 257; i++)
            {
                Items.Add(reader.ReadColor());
            }
        }

        public override void Write(ByteWriter Writer)
        {
            foreach (Color item in Items)
            {
                Writer.WriteColor(item);
            }
        }

    }

}
