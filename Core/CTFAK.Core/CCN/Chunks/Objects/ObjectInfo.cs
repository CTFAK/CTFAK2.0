using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        

        public ObjectInfo(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            while (true)
            {
                var newChunk = new Chunk(reader);
                var chunkData = newChunk.Read();
                var chunkReader = new ByteReader(chunkData);
                if (newChunk.Id == 32639) break;
                
                switch (newChunk.Id)
                {
                    case 17477:
                        name = chunkReader.ReadUniversal();
                        break;
                    case 17476:
                        handle = chunkReader.ReadInt16();
                        ObjectType = chunkReader.ReadInt16();
                        Flags = chunkReader.ReadInt16();
                        chunkReader.Skip(2);
                        InkEffect = chunkReader.ReadByte();
                        if(InkEffect!=1)
                        {
                            chunkReader.Skip(3);
                            var r = chunkReader.ReadByte();
                            var g = chunkReader.ReadByte();
                            var b = chunkReader.ReadByte();
                            rgbCoeff = Color.FromArgb(0, r, g, b);
                            blend = chunkReader.ReadByte();
                        }
                        else
                        {
                            var flag = chunkReader.ReadByte();
                            chunkReader.Skip(2);
                            InkEffectValue = chunkReader.ReadByte();
                        }
                        
                        
                        break;
                    case 17478:        
                        if (ObjectType == 0) properties = new Quickbackdrop(chunkReader);
                        else if (ObjectType == 1) properties = new Backdrop(chunkReader);
                        else properties = new ObjectCommon(chunkReader, this);

                        properties?.Read();
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
