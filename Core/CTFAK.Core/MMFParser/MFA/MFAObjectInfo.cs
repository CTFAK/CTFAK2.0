using System;
using System.IO;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.MFA.MFAObjectLoaders;

namespace CTFAK.MMFParser.MFA;

public class MFAObjectInfo : ChunkLoader
{
    public int AntiAliasing;
    public MFAChunkList Chunks;
    public int Flags;
    public MFAObjectFlags FlagWriter;
    public int Handle;
    public int IconHandle;
    public int IconType;
    public int InkEffect;
    public uint InkEffectParameter;
    public ChunkLoader Loader;
    public string Name;
    public int ObjectType;
    public int Transparent;

    public override void Write(ByteWriter writer)
    {
        //Debug.Assert(ObjectType==2);
        writer.WriteInt32(ObjectType);
        writer.WriteInt32(Handle);
        writer.AutoWriteUnicode(Name);
        writer.WriteInt32(Transparent);
        writer.WriteInt32(InkEffect);
        writer.WriteUInt32(InkEffectParameter);
        writer.WriteInt32(AntiAliasing);
        writer.WriteInt32(Flags);
        writer.WriteInt32(1);
        writer.WriteInt32(IconHandle);

        if (FlagWriter != null)
            FlagWriter.Write(writer);

        Chunks.Write(writer);
        Loader.Write(writer);
    }


    public override void Read(ByteReader reader)
    {
        ObjectType = reader.ReadInt32();
        Handle = reader.ReadInt32();
        Name = reader.AutoReadUnicode();
        Transparent = reader.ReadInt32();

        InkEffect = reader.ReadInt32();
        InkEffectParameter = reader.ReadUInt32();
        AntiAliasing = reader.ReadInt32();

        Flags = reader.ReadInt32();

        IconType = reader.ReadInt32();
        if (IconType == 1)
            IconHandle = reader.ReadInt32();
        else throw new InvalidDataException("invalid icon");
        Chunks = new MFAChunkList();
        Chunks.Log = true;
        Chunks.Read(reader);

        if (ObjectType >= 32) //extension base
            Loader = new MFAExtensionObject();
        else if (ObjectType == 0)
            Loader = new MFAQuickBackdrop();
        else if (ObjectType == 1)
            Loader = new MFABackdrop();
        else if (ObjectType == 2)
            Loader = new MFAActive();
        else if (ObjectType == 3)
            Loader = new MFAText();
        else if (ObjectType == 7)
            Loader = new MFACounter();

        else throw new NotImplementedException("Unsupported object: " + ObjectType);
        Loader.Read(reader);
    }
}