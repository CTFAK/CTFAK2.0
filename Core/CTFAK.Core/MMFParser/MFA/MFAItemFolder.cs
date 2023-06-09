using System.Collections.Generic;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.MMFParser.MFA;

public class MFAItemFolder : ChunkLoader
{
    public bool IsHiddenFolder;
    public List<uint> Items = new();
    public string Name;
    public uint UnkHeader;

    public override void Read(ByteReader reader)
    {
        UnkHeader = reader.ReadUInt32();
        if (UnkHeader == 0x70000004)
        {
            IsHiddenFolder = false;
            Name = reader.AutoReadUnicode();
            Items = new List<uint>();
            var count = reader.ReadUInt32();
            for (var i = 0; i < count; i++) Items.Add(reader.ReadUInt32());
        }
        else
        {
            IsHiddenFolder = true;
            Name = null;
            Items = new List<uint>();
            Items.Add(reader.ReadUInt32());
        }
    }

    public override void Write(ByteWriter writer)
    {
        if (IsHiddenFolder)
        {
            writer.WriteInt32(0x70000005);
            writer.WriteInt32((int)Items[0]);
        }
        else
        {
            writer.WriteInt32(0x70000004);
            if (Name == null) Name = "";
            writer.AutoWriteUnicode(Name);
            writer.WriteInt32(Items.Count);
            foreach (var item in Items) writer.WriteUInt32(item);
        }
    }
}