using System.Collections.Generic;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA;

public class MFAValueList : ChunkLoader
{
    public List<ValueItem> Items = new();

    public override void Read(ByteReader reader)
    {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var item = new ValueItem();
            item.Read(reader);
            Items.Add(item);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(Items.Count);
        foreach (var item in Items) item.Write(writer);
    }
}

public class ValueItem : ChunkLoader
{
    public string Name;
    public object Value;

    public override void Read(ByteReader reader)
    {
        Name = reader.AutoReadUnicode();
        var type = reader.ReadInt32();
        switch (type)
        {
            case 2: //string
                Value = reader.AutoReadUnicode();
                break;
            case 0: //int
                Value = reader.ReadInt32();
                break;
            case 1: //double
                Value = reader.ReadSingle();
                break;
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.AutoWriteUnicode(Name);
        if (Value is string)
        {
            writer.WriteInt32(2);
            writer.AutoWriteUnicode((string)Value);
        }
        else if (Value is int)
        {
            writer.WriteInt32(0);
            writer.WriteInt32((int)Value);
        }
        else if (Value is double || Value is float)
        {
            writer.WriteInt32(1);
            writer.WriteSingle((float)Value);
        }
    }
}