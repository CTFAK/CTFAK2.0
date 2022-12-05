using System.Collections.Generic;
using System.IO;
using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks
{
    public class BinaryFile:ChunkLoader
    {
        public string name;
        public byte[] data;


        public override void Read(ByteReader reader)
        {
            name = reader.ReadUniversal(reader.ReadInt16());
            data = reader.ReadBytes(reader.ReadInt32());
        }

        public override void Write(ByteWriter writer)
        {
            writer.AutoWriteUnicode(name);
        }
    }

    public class BinaryFiles:ChunkLoader
    {
        public List<BinaryFile> files;
        public int count;

        public override void Read(ByteReader reader)
        {
            count = reader.ReadInt32();
            files = new();
            for (int i = 0; i < count; i++)
            {
                BinaryFile file = new BinaryFile();
                file.Read(reader);
                files.Add(file);
            }
        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(files.Count);
            foreach (var item in files)
                item.Write(writer);
        }
    }
}