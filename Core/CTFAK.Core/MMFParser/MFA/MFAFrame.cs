using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.MFA;

namespace CTFAK.MFA;

public class MFAFrame : ChunkLoader
{
    public int ActiveLayer;
    public Color Background;
    public MFAChunkList Chunks;
    public MFAEvents Events;
    public MFATransition FadeIn;
    public MFATransition FadeOut;

    public BitDict Flags = new(new[]
    {
        "GrabDesktop",
        "KeepDisplay",
        "BackgroundCollisions",
        "DisplayFrameTitle",
        "ResizeToScreen",
        "ForceLoadOnCall",
        "NoDisplaySurface",
        "ScreenSaverSetup",
        "TimerBasedMovements",
        "MochiAds",
        "NoGlobalEvents"
    });

    public List<MFAItemFolder> Folders = new();
    public int Handle;
    public List<MFAObjectInstance> Instances = new();
    public List<MFAObjectInfo> Items = new();
    public int LastViewedX;
    public int LastViewedY;
    public List<MFALayer> Layers = new();
    public int MaxObjects;
    public string Name = "ERROR";
    public List<Color> Palette = new Color[256].ToList();
    public int PaletteSize;

    public string Password;
    public int SizeX;
    public int SizeY;
    public int StampHandle;
    public string UnkString = "";

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteInt32(Handle);
        Writer.AutoWriteUnicode(Name);
        Writer.WriteInt32(SizeX);
        Writer.WriteInt32(SizeY);
        Writer.WriteColor(Background);

        Writer.WriteUInt32(Flags.flag);
        Writer.WriteInt32(MaxObjects);

        Writer.WriteInt32(0);
        Writer.WriteInt32(12);
        Writer.Skip(12);


        Writer.WriteInt32(LastViewedX);
        Writer.WriteInt32(LastViewedY);
        Writer.WriteInt32(Palette.Count); //WTF HELP 

        foreach (var item in Palette) Writer.WriteColor(item);

        Writer.WriteInt32(StampHandle);
        Writer.WriteInt32(ActiveLayer);
        Writer.WriteInt32(Layers.Count);
        foreach (var layer in Layers) layer.Write(Writer);

        if (FadeIn != null)
        {
            Writer.WriteInt8(1);
            FadeIn.Write(Writer);
        }
        else
        {
            Writer.Skip(1);
        }

        if (FadeOut != null)
        {
            Writer.WriteInt8(1);
            FadeOut.Write(Writer);
        }
        else
        {
            Writer.Skip(1);
        }


        Writer.WriteInt32(Items.Count);
        foreach (var item in Items) item.Write(Writer);

        Writer.WriteInt32(Folders.Count);
        foreach (var item in Folders) item.Write(Writer);

        Writer.WriteInt32(Instances.Count);
        foreach (var item in Instances) item.Write(Writer);

        Events.Write(Writer);
        Chunks.Write(Writer);
    }


    public override void Read(ByteReader reader)
    {
        Handle = reader.ReadInt32();
        Name = reader.AutoReadUnicode();
        SizeX = reader.ReadInt32();
        SizeY = reader.ReadInt32();
        Background = reader.ReadColor();
        Flags.flag = reader.ReadUInt32();

        MaxObjects = reader.ReadInt32();

        reader.ReadInt32(); //garbage
        var password = reader.ReadBytes(reader.ReadInt32());

        LastViewedX = reader.ReadInt32();
        LastViewedY = reader.ReadInt32();

        PaletteSize = reader.ReadInt32();
        Palette = new List<Color>();
        for (var i = 0; i < PaletteSize; i++) Palette.Add(reader.ReadColor());

        StampHandle = reader.ReadInt32();
        ActiveLayer = reader.ReadInt32();
        var layersCount = reader.ReadInt32();
        for (var i = 0; i < layersCount; i++)
        {
            var layer = new MFALayer();
            layer.Read(reader);
            Layers.Add(layer);
        }

        if (reader.ReadByte() == 1)
        {
            FadeIn = new MFATransition();
            FadeIn.Read(reader);
        }

        if (reader.ReadByte() == 1)
        {
            FadeOut = new MFATransition();
            FadeOut.Read(reader);
        }

        var frameItemsCount = reader.ReadInt32();
        for (var i = 0; i < frameItemsCount; i++)
        {
            var frameitem = new MFAObjectInfo();
            frameitem.Read(reader);

            Items.Add(frameitem);
        }

        var folderCount = reader.ReadInt32();
        for (var i = 0; i < folderCount; i++)
        {
            var folder = new MFAItemFolder();
            folder.Read(reader);
            Folders.Add(folder);
        }

        var instancesCount = reader.ReadInt32();
        for (var i = 0; i < instancesCount; i++)
        {
            var inst = new MFAObjectInstance();
            inst.Read(reader);
            Instances.Add(inst);
        }

        // var unkCount = reader.ReadInt32();
        // for (int i = 0; i < unkCount; i++)
        // {
        // UnkBlocks.Add(reader.ReadBytes(32));
        // }

        Events = new MFAEvents();
        Events.Read(reader);

        Chunks = new MFAChunkList();
        // Chunks.Log = true;
        Chunks.Read(reader);
    }
}