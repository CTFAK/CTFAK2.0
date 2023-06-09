using System.Collections.Generic;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA.MFAObjectLoaders;

public class Behaviours : ChunkLoader
{
    private readonly List<Behaviour> _items = new();

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(_items.Count);
        foreach (var behaviour in _items) behaviour.Write(writer);
    }

    public override void Read(ByteReader reader)
    {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var item = new Behaviour();
            item.Read(reader);
            _items.Add(item);
        }
    }
}

internal class Behaviour : ChunkLoader
{
    public byte[] Data;
    public string Name = "ERROR";

    public override void Write(ByteWriter writer)
    {
        writer.AutoWriteUnicode(Name);
        writer.WriteUInt32((uint)Data.Length);
        writer.WriteBytes(Data);
    }

    public override void Read(ByteReader reader)
    {
        Name = reader.AutoReadUnicode();

        Data = reader.ReadBytes((int)reader.ReadUInt32());
    }
}