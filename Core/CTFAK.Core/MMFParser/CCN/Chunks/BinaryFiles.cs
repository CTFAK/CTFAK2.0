using System.Collections.Generic;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MMFParser.MFA;

namespace CTFAK.MMFParser.CCN.Chunks;

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
    public List<BinaryFile> Files = new();

    public override void Read(ByteReader reader)
    {
        Count = reader.ReadInt32();
        Files = new List<BinaryFile>();
        for (var i = 0; i < Count; i++)
        {
            var file = new BinaryFile();
            file.Read(reader);
            Files.Add(file);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Files.Count);
        foreach (var item in Files)
            item.Write(writer);
    }
}