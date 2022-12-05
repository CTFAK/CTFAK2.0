using System.Collections.Generic;
using CTFAK.Memory;
using CTFAK.Attributes;
using CTFAK.MFA;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks
{
    public class BinaryFile : ChunkLoader
    {
        public string Name;
        public byte[] Data;


        public override void Read(ByteReader reader)
        {
            Name = reader.ReadUniversal(reader.ReadInt16());
            Data = reader.ReadBytes(reader.ReadInt32());
        }

        public override void Write(ByteWriter writer)
        {
            writer.AutoWriteUnicode(Name);
        }
    }

    [ChunkLoader(8760, "BinaryFiles")]
    public class BinaryFiles : ChunkLoader
    {
        public List<BinaryFile> Files;
        public int Count;

        public override void Read(ByteReader reader)
        {
            Count = reader.ReadInt32();
            Files = new();
            for (int i = 0; i < Count; i++)
            {
                BinaryFile File = new BinaryFile();
                File.Read(reader);
                Files.Add(File);
            }
        }

        public override void Write(ByteWriter writer)
        {
            writer.WriteInt32(Files.Count);
            foreach (var Item in Files)
                Item.Write(writer);
        }
    }
}