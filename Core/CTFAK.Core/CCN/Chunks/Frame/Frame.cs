using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTFAK.Attributes;

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
        public short flags;
        public short parentHandle;
        public Random rnd = new Random(1337);
        public override void Read(ByteReader reader)
        {
            handle = (ushort)reader.ReadInt16();
            objectInfo = (ushort)reader.ReadInt16();
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
            if (Settings.Old) return;
            layer = reader.ReadInt16();
            flags = reader.ReadInt16();
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
    [ChunkLoader(0x3333,"Frame")]
    public class Frame : ChunkLoader
    {
        public string name;
        public int width;
        public int height;
        public Color background;
        public Events events;
        public BitDict flags = new BitDict(new string[]
        {
            "XCoefficient",
            "YCoefficient",
            "DoNotSaveBackground",
            "Wrap",
            "Visible",
            "WrapHorizontally",
            "WrapVertically","","","","","","","","","",
            "Redraw",
            "ToHide",
            "ToShow"

        });
        public List<ObjectInstance> objects = new List<ObjectInstance>();
        public Layers layers;
        public List<Color> palette;
        public Transition fadeIn;
        public Transition fadeOut;
        public VirtualRect virtualRect;

        public override void Read(ByteReader reader)
        {
            while(true)
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
                        if (string.IsNullOrEmpty(frameName.value))
                            name = "CORRUPTED FRAME";
                        else
                            name = frameName.value;
                        break;
                    case 13108:
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
                    case 13112:
                        var count = chunkReader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            var objInst = new ObjectInstance();
                            objInst.Read(chunkReader);
                            objects.Add(objInst);
                        }
                        break;
                    case 13117:
                        if (Core.parameters.Contains("-noevnt"))
                            events = new Events();
                        else
                        {
                            events = new Events();
                            events.Read(chunkReader);
                        }
                        break;
                    case 13121:
                        layers = new Layers();
                        layers.Read(chunkReader);
                        break;
                    case 13111:
                        var pal = new FramePalette();
                        pal.Read(chunkReader);
                        palette = pal.Items;
                        break;
                    case 13115:
                        fadeIn = new Transition();
                        fadeIn.Read(chunkReader);
                        break;
                    case 13116:
                        fadeOut = new Transition();
                        fadeOut.Read(chunkReader);
                        break;
                    case 13122:
                        virtualRect = new VirtualRect();
                        virtualRect.Read(chunkReader);
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
        });

        public float XCoeff;
        public float YCoeff;
        public int NumberOfBackgrounds;
        public int BackgroudIndex;
        
        public override void Read(ByteReader reader)
        {
            Flags.flag = reader.ReadUInt32();
            XCoeff = reader.ReadSingle();
            YCoeff = reader.ReadSingle();
            NumberOfBackgrounds = reader.ReadInt32();
            BackgroudIndex = reader.ReadInt32();
            Name = reader.ReadUniversal();
            if (Settings.android)
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
