using System.Collections.Generic;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;

namespace CTFAK.MFA;

public class MFAItemFolder : ChunkLoader
{
    public bool isRetard;
    public List<uint> Items = new();
    public string Name;
    public uint UnkHeader;

    public override void Read(ByteReader reader)
    {
        UnkHeader = reader.ReadUInt32();
        if (UnkHeader == 0x70000004)
        {
            isRetard = false;
            Name = reader.AutoReadUnicode();
            Items = new List<uint>();
            var count = reader.ReadUInt32();
            for (var i = 0; i < count; i++) Items.Add(reader.ReadUInt32());
        }
        else
        {
            isRetard = true;
            Name = null;
            Items = new List<uint>();
            Items.Add(reader.ReadUInt32());
        }
    }

    public override void Write(ByteWriter Writer)
    {
        if (isRetard)
        {
            Writer.WriteInt32(0x70000005);
            Writer.WriteInt32((int)Items[0]);
        }
        else
        {
            Writer.WriteInt32(0x70000004);
            if (Name == null) Name = "";
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32(Items.Count);
            foreach (var item in Items) Writer.WriteUInt32(item);
        }
    }
}