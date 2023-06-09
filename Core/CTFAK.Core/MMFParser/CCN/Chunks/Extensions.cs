using System.Collections.Generic;
using System.IO;
using CTFAK.Attributes;
using CTFAK.Memory;

namespace CTFAK.MMFParser.CCN.Chunks;

[ChunkLoader(8756, "Extensions")]
public class Extensions : ChunkLoader
{
    public List<Extension> Items;
    internal ushort PreloadExtensions;

    public override void Read(ByteReader reader)
    {
        var count = reader.ReadUInt16();
        PreloadExtensions = reader.ReadUInt16();
        Items = new List<Extension>();
        for (var i = 0; i < count; i++)
        {
            var ext = new Extension();
            ext.Read(reader);
            Items.Add(ext);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt16((short)Items.Count);
        writer.WriteInt16((short)PreloadExtensions);
        foreach (var item in Items) item.Write(writer);
    }
}

//[ChunkLoader(2234,"Extensions")] // this might have a purpose and I'm too scared to remove it
public class Extension : ChunkLoader
{
    public string Ext;
    public short Handle;
    public int MagicNumber;
    public string Name = "";
    public string SubType = "";
    public int VersionLs;
    public int VersionMs;

    public override void Read(ByteReader reader)
    {
        var currentPosition = reader.Tell();
        var size = reader.ReadInt16();
        if (size < 0) size = (short)(size * -1);
        Handle = reader.ReadInt16();
        MagicNumber = reader.ReadInt32();
        VersionLs = reader.ReadInt32();
        VersionMs = reader.ReadInt32();
        var extName = reader.ReadUniversal();
        if (extName.Length == 0)
        {
            reader.Seek(currentPosition + size);
            return;
        }

        string[] arr;
        arr = extName.Split('.');
        if (!extName.Contains(".")) return;
        Name = arr[0];
        Ext = arr[1];
        SubType = reader.ReadUniversal();
        reader.Seek(currentPosition + size);
    }

    public override void Write(ByteWriter writer)
    {
        var newWriter = new ByteWriter(new MemoryStream());
        newWriter.WriteInt16(Handle);
        newWriter.WriteInt32(MagicNumber);
        newWriter.WriteInt32(VersionLs);
        newWriter.WriteInt32(VersionMs);
        newWriter.WriteUnicode(string.Join(".", Name, Ext));
        newWriter.WriteUnicode(SubType);
        writer.WriteInt16((short)(newWriter.Size() + 2));
        writer.WriteWriter(newWriter);
    }
}