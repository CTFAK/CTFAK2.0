using System.Collections.Generic;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MFA;

namespace CTFAK.CCN.Chunks;

public class BinaryFile : ChunkLoader
{
    public byte[] Data;
    public string Name;


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
    public int Count;
    public List<BinaryFile> Files;

    public override void Read(ByteReader reader)
    {
        Count = reader.ReadInt32();
        Files = new List<BinaryFile>();
        for (var i = 0; i < Count; i++)
        {
            var File = new BinaryFile();
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