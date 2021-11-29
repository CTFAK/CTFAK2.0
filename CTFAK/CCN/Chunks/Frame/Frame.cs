using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public short parentHandle;
        public ObjectInstance(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            handle = (ushort)reader.ReadInt16();
            objectInfo = (ushort)reader.ReadInt16();

            x = reader.ReadInt32();
            y = reader.ReadInt32();
            parentType = reader.ReadInt16();
            parentHandle = reader.ReadInt16();
            layer = reader.ReadInt16();
            var res = reader.ReadInt16();
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

        public Frame(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            while(true)
            {
                var newChunk = new Chunk(reader);
                var chunkData = newChunk.Read();
                var chunkReader = new ByteReader(chunkData);
                if (newChunk.Id == 32639) break;
                switch(newChunk.Id)
                {
                    case 13109:
                        var frameName = new StringChunk(chunkReader);
                        frameName.Read();
                        name = frameName.value;
                        break;
                    case 13108:
                        width = chunkReader.ReadInt32();
                        height = chunkReader.ReadInt32();
                        background = chunkReader.ReadColor();
                        flags.flag = chunkReader.ReadUInt32();
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
                }
            }

            Logger.Log($"Frame Found: {name}, {width}x{height}", true, ConsoleColor.Green);
        }
        

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
