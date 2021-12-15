using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MFA
{
    public class MFAFrame : ChunkLoader
    {
        public string Name = "ERROR";
        public int SizeX;
        public int SizeY;
        public Color Background;
        public int MaxObjects;
        public List<MFAObjectInfo> Items = new List<MFAObjectInfo>();
        public int Handle;
        public int LastViewedX;
        public int LastViewedY;
        public List<MFAItemFolder> Folders = new List<MFAItemFolder>();
        public List<MFAObjectInstance> Instances = new List<MFAObjectInstance>();
        public BitDict Flags = new BitDict(new string[]
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

        public string Password = "";
        public string UnkString = "";
        public List<Color> Palette;
        public int StampHandle;
        public int ActiveLayer;
        public List<MFALayer> Layers = new List<MFALayer>();
        public MFAEvents Events;
        public MFAChunkList Chunks;
        public MFATransition FadeIn;
        public MFATransition FadeOut;
        public int PaletteSize;

        public MFAFrame(ByteReader reader) : base(reader)
        {
        }


        public override void Write(ByteWriter Writer)
        {
            Writer.WriteInt32(Handle);
            Writer.AutoWriteUnicode(Name);
            Writer.WriteInt32(SizeX);
            Writer.WriteInt32(SizeY);
            Writer.WriteColor(Background);
            Writer.WriteUInt32(Flags.flag);
            Writer.WriteInt32(MaxObjects);
            Writer.AutoWriteUnicode(Password);
            Writer.AutoWriteUnicode(UnkString);
            Writer.WriteInt32(LastViewedX);
            Writer.WriteInt32(LastViewedY);
            Writer.WriteInt32(Palette.Count);//WTF HELP 

            foreach (var item in Palette)
            {
                Writer.WriteColor(item);
            }

            Writer.WriteInt32(StampHandle);
            Writer.WriteInt32(ActiveLayer);
            Writer.WriteInt32(Layers.Count);
            foreach (var layer in Layers)
            {
                layer.Write(Writer);
            }

            if (FadeIn != null)
            {
                Writer.WriteInt8(1);
                FadeIn.Write(Writer);
            }
            else Writer.Skip(1);

            if (FadeOut != null)
            {
                Writer.WriteInt8(1);
                FadeOut.Write(Writer);
            }
            else Writer.Skip(1);


            Writer.WriteInt32(Items.Count);
            foreach (var item in Items)
            {
                item.Write(Writer);
            }

            Writer.WriteInt32(Folders.Count);
            foreach (var item in Folders)
            {
                item.Write(Writer);
            }

            Writer.WriteInt32(Instances.Count);
            foreach (var item in Instances)
            {
                item.Write(Writer);
            }


            Events.Write(Writer);

            Chunks.Write(Writer);
        }



        public override void Read()
        {
            Handle = reader.ReadInt32();
            Name = reader.AutoReadUnicode();
            SizeX = reader.ReadInt32();
            SizeY = reader.ReadInt32();
            Background = reader.ReadColor();
            Flags.flag = reader.ReadUInt32();

            MaxObjects = reader.ReadInt32();
            Password = reader.AutoReadUnicode();
            UnkString = reader.AutoReadUnicode();

            LastViewedX = reader.ReadInt32();
            LastViewedY = reader.ReadInt32();

            PaletteSize = reader.ReadInt32();
            Palette = new List<Color>();
            for (int i = 0; i < 256; i++)
            {
                Palette.Add(reader.ReadColor());
            }

            StampHandle = reader.ReadInt32();
            ActiveLayer = reader.ReadInt32();
            int layersCount = reader.ReadInt32();
            for (int i = 0; i < layersCount; i++)
            {
                var layer = new MFALayer(reader);
                layer.Read();
                Layers.Add(layer);
            }

            if (reader.ReadByte() == 1)
            {
                FadeIn = new MFATransition(reader);
                FadeIn.Read();
            }

            if (reader.ReadByte() == 1)
            {
                FadeOut = new MFATransition(reader);
                FadeOut.Read();
            }
            var frameItemsCount = reader.ReadInt32();
            for (int i = 0; i < frameItemsCount; i++)
            {
                var frameitem = new MFAObjectInfo(reader);
                frameitem.Read();

                Items.Add(frameitem);
            }
            var folderCount = reader.ReadInt32();
            for (int i = 0; i < folderCount; i++)
            {
                var folder = new MFAItemFolder(reader);
                folder.Read();
                Folders.Add(folder);
            }
            var instancesCount = reader.ReadInt32();
            for (int i = 0; i < instancesCount; i++)
            {
                var inst = new MFAObjectInstance(reader);
                inst.Read();
                Instances.Add(inst);
            }

            // var unkCount = reader.ReadInt32();
            // for (int i = 0; i < unkCount; i++)
            // {
            // UnkBlocks.Add(reader.ReadBytes(32));
            // }

            Events = new MFAEvents(reader);
            Events.Read();


            Chunks = new MFAChunkList(reader);
            // Chunks.Log = true;
            Chunks.Read();

        }
    }
}