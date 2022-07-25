using System.IO;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks
{
    public class BinaryFile:ChunkLoader
    {
        public string name;
        public byte[] data;
        public BinaryFile(ByteReader reader) : base(reader)
        {
        }

        public override void Read()
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

    public class BinaryFiles:ChunkLoader
    {
        public BinaryFiles(ByteReader reader) : base(reader)
        {
        }

        public override void Read()
        {
        }

        public override void Write(ByteWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}