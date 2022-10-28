using System.IO;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks
{
    
    public class BinaryFile:ChunkLoader
    {
        public string name;
        public byte[] data;

        public override void Read(ByteReader reader)
        {
            name = reader.ReadAscii(reader.ReadInt16());
            data = reader.ReadBytes(reader.ReadInt32());
            File.WriteAllBytes($"FileDumps\\{name}",data);
        }
        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
    [ChunkLoader(8760,"BinaryFiles")]
    public class BinaryFiles:ChunkLoader
    {
        public override void Read(ByteReader reader)
        {
        }
        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}