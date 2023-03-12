using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CTFAK.Core.CCN.Chunks
{
    [ChunkLoader(8757, "AppIcon")]
    public class AppIcon : ChunkLoader
    {
        public Bitmap Icon;
        public List<Color> Palette;

        public override void Read(ByteReader reader)
        {
            reader.Seek(reader.PeekInt32());
            Palette = new();
            for (int i = 0; i < 16 * 16; i++)
            {
                var b = reader.ReadByte();
                var g = reader.ReadByte();
                var r = reader.ReadByte();
                var a = reader.ReadByte();
                var newColor = Color.FromArgb(255, r, g, b);
                Palette.Add(newColor);
            }

            Icon = new Bitmap(16, 16);

            for (int h = 0; h < Icon.Height; h++)
                for (int w = 0; w < Icon.Width; w++)
                    Icon.SetPixel(w, Icon.Width - 1 - h, Palette[reader.ReadByte()]);
            
            return; //idk wtf im doing with the mask
            var BitmapSize = Icon.Width * Icon.Height;
            for (int i = 0; i < BitmapSize / 8; i++)
            {
                byte Mask = reader.ReadByte();
                for (int j = 0; j < 8; j++)
                {
                    int y = (int)Math.Round((double)i / 16, MidpointRounding.ToZero) - 1 + (j * 2);
                    int x = j * 2;
                    if ((0x1 & (Mask >> (8 - j))) != 0)
                    {
                        Color get = Icon.GetPixel(x, y);
                        Icon.SetPixel(x, y, Color.FromArgb(0, get.R, get.G, get.B));
                    }
                    else
                    {
                        Color get = Icon.GetPixel(x, y);
                        Icon.SetPixel(x, y, Color.FromArgb(255, get.R, get.G, get.B));
                    }
                }
            }
            return;
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [ChunkLoader(8768, "ExeOnly")]
    public class ExeOnly : ChunkLoader
    {
        public bool exeOnly = false;
        public override void Read(ByteReader reader)
        {
            exeOnly = reader.ReadByte() != 0;
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [ChunkLoader(8773, "ExtendedHeader")]
    public class ExtendedHeader : ChunkLoader
    {
        public BitDict Flags = new BitDict(new string[]
        {
            "KeepScreenRatio",
            "Unknown1",
            "AntiAliasingWhenResizing",
            "Unknown2",
            "Unknown3",
            "RightToLeftReading",
            "Unknown4",
            "RightToLeftLayout",
            "Unknown5",
            "Unknown6",
            "Unknown7",
            "Unknown8",
            "Unknown9",
            "Unknown10",
            "Unknown11",
            "Unknown12",
            "Unknown13",
            "Unknown14",
            "Unknown15",
            "Unknown16",
            "Unknown17",
            "Unknown18",
            "DontOptimizeStrings",
            "Unknown19",
            "Unknown20",
            "Unknown21",
            "DontIgnoreDestroy",
            "DisableIME",
            "ReduceCPUUsage",
            "Unknown22",
            "PremultipliedAlpha",
            "OptimizePlaySample"
        });

        public BitDict CompressionFlags = new BitDict(new string[]
        {
            "CompressionLevelMax",
            "CompressSounds",
            "IncludeExternalFiles",
            "NoAutoImageFilters",
            "NoAutoSoundFilters",
            "Unknown3",
            "Unknown4",
            "Unknown5",
            "DontDisplayBuildWarning",
            "OptimizeImageSize",
        });

        public BitDict ViewFlags = new BitDict(new string[]
        {
            "Unknown1"
        });

        public BitDict NewFlags = new BitDict(new string[]
        {
            "Unknown1"
        });

        public int BuildType;
        public int ScreenRatio;
        public int ScreenAngle;

        public override void Read(ByteReader reader)
        {
            Flags.flag = (uint)reader.ReadInt32();
            BuildType = reader.ReadInt32();
            CompressionFlags.flag = (uint)reader.ReadInt32();
            ScreenRatio = reader.ReadInt16();
            ScreenAngle = reader.ReadInt16();
            ViewFlags.flag = (uint)reader.ReadInt16();
            NewFlags.flag = (uint)reader.ReadInt16();
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [ChunkLoader(17664, "ImageShapes")]
    public class ImageShapes : ChunkLoader
    {
        public int Count;
        public List<ImageShape> Shapes = new List<ImageShape>();

        public override void Read(ByteReader reader)
        {
            Count = reader.ReadInt32();
            for (int i = 0; i < Count; i++)
            {
                var shape = new ImageShape();
                shape.Read(reader);
                Shapes.Add(shape);
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageShape : ChunkLoader
    {
        public int[] xArray;
        public int[] yArray;
        public short Image;
        public int Count;

        public override void Read(ByteReader reader)
        {
            Image = (short)reader.ReadInt32();
            Count = reader.ReadInt32();
            if (Count != 0)
            {
                xArray = new int[Count];
                yArray = new int[Count];
                for (int i = 0; i < Count; i++)
                {
                    xArray[i] = reader.ReadInt32();
                    yArray[i] = reader.ReadInt32();
                }
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}