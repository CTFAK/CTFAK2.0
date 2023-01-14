using System.Collections.Generic;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MFA.MFAObjectLoaders;

public class Behaviours : ChunkLoader
{
    private readonly List<Behaviour> _items = new();

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(_items.Count);
        foreach (var behaviour in _items) behaviour.Write(Writer);
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

    public override void Write(ByteWriter Writer)
    {
        Writer.AutoWriteUnicode(Name);
        Writer.WriteUInt32((uint)Data.Length);
        Writer.WriteBytes(Data);
    }

    public override void Read(ByteReader reader)
    {
        Name = reader.AutoReadUnicode();

        Data = reader.ReadBytes((int)reader.ReadUInt32());
    }
}