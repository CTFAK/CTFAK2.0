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
        public ObjectInstance(ByteReader reader) : base(reader) { }
        public override void Read()
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
        public VirtualRect(ByteReader reader) : base(reader) { }
        public override void Read()
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

        public Frame(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            while(true)
            {
                var newChunk = new Chunk(reader);
                var chunkData = newChunk.Read();
                var chunkReader = new ByteReader(chunkData);
                if (newChunk.Id == 32639) break;
                if (reader.Tell() >= reader.Size()) break;
                switch (newChunk.Id)
                {
                    
                    case 13109:
                        var frameName = new StringChunk(chunkReader);
                        frameName.Read();
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
                            var objInst = new ObjectInstance(chunkReader);
                            objInst.Read();
                            objects.Add(objInst);
                        }
                        break;
                    case 13117:
                        events = new Events(chunkReader);
                        events.Read();
                        //File.WriteAllBytes($"FNAFWorldTest\\{name}",chunkReader.ReadBytes());
                        break;
                    case 13121:
                        layers = new Layers(chunkReader);
                        layers.Read();
                        break;
                    case 13111:
                        var pal = new FramePalette(chunkReader);
                        pal.Read();
                        palette = pal.Items;
                        break;
                    case 13115:
                        fadeIn = new Transition(chunkReader);
                        fadeIn.Read();
                        break;
                    case 13116:
                        fadeOut = new Transition(chunkReader);
                        fadeOut.Read();
                        break;
                    case 13122:
                        virtualRect = new VirtualRect(chunkReader);
                        virtualRect.Read();
                        break;

                        


                }
            }

            Logger.Log($"Frame Found: {name}, {width}x{height}", true, ConsoleColor.Green);
        }
        

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
    public class Layers : ChunkLoader
    {
        public List<Layer> Items;

        public Layers(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {
            Items = new List<Layer>();
            var count = reader.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                Layer item = new Layer(reader);
                item.Read();
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


        public Layer(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
        {
            Flags.flag = reader.ReadUInt32();
            XCoeff = reader.ReadSingle();
            YCoeff = reader.ReadSingle();
            NumberOfBackgrounds = reader.ReadInt32();
            BackgroudIndex = reader.ReadInt32();
            Name = reader.ReadWideString();
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

        public FramePalette(ByteReader reader) : base(reader)
        {
        }



        public override void Read()
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
