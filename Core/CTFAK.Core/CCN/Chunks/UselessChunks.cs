using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CTFAK.Core.CCN.Chunks
{
    public class AppIcon
    {
        public static Bitmap Icon;
        public static List<Color> Palette;

        public static Bitmap ReadIcon(ByteReader reader)
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

            return Icon; //idk wtf im doing with the mask
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
            return Icon;
        }
    }

    public class ExeOnly
    {
        public static bool ReadBool(ByteReader reader)
        {
            return reader.ReadByte() != 0;
        }
    }

    public class ExtendedHeader : ChunkLoader
    {
        // The flags can add up (cringe)
        public byte FlagScreen; // 1 = Keep Screen Ratio, 4 = Anti Alias, 32 = Right to Left Reading, 128 = Right to Left Layout
        public byte FlagOptimizeStr; // 0 = On, 64 = Off
        public byte FlagCompression; // 4 = Default, 64 = Premultiplied Alpha, 128 = Optimize Play Sample
        public byte FlagCompression2; // 1, Maximum Compression, 2 = Compress Sounds, 4 = Include External Files, 20 = Default
        public byte FlagDumb; // 1 = Don't display build warning, // 2 = Optimize image size in RAM

        public int BuildType;
        public int ScreenRatio;
        public int ScreenAngle;
        public int MoreFlags;

        //Flags to Bools
        public bool KeepScreenRatio;
        public bool AntiAlias;
        public bool ReadRighttoLeft;
        public bool LayoutRighttoLeft;
        public bool OptimizeStrings;
        public bool PremultipliedAlpha;
        public bool OptimizePlaySample;
        public bool MaximumCompression;
        public bool CompressSound;
        public bool IncludeExternalFiles;
        public bool DisplayBuildWarning;
        public bool OptimizeImageRAM;

        public override void Read(ByteReader reader)
        {
            FlagScreen = reader.ReadByte();
            reader.ReadByte(); // Unknown Flag
            FlagOptimizeStr = reader.ReadByte();
            FlagCompression = reader.ReadByte();

            BuildType = reader.ReadInt32();

            FlagCompression2 = reader.ReadByte();
            FlagDumb = reader.ReadByte();
            reader.ReadByte(); // Unknown Flag
            reader.ReadByte(); // Unknown Flag

            ScreenRatio = reader.ReadInt16();
            ScreenAngle = reader.ReadInt16();
            MoreFlags = reader.ReadInt32();

            //Flags to Bools
            var KSR = 0;
            if (FlagScreen == 1 || FlagScreen > 1 && FlagScreen % 4 != 1)
                KSR = 1;
            KeepScreenRatio = KSR == 1;

            var AA = 0;
            if (FlagScreen == 4 + KSR || FlagScreen > 32 && (FlagScreen - KSR) % 32 != 1)
                AA = 4;
            AntiAlias = AA == 4;

            ReadRighttoLeft = FlagScreen == 32 + KSR + AA || FlagScreen >= 128 + 32 + KSR + AA;

            ReadRighttoLeft = FlagScreen >= 128;

            OptimizeStrings = FlagOptimizeStr != 64;

            PremultipliedAlpha = FlagCompression - 4 == 64 || FlagCompression - 4 > 128;

            OptimizePlaySample = FlagCompression - 4 >= 128;

            var MC = 0;
            if (FlagCompression2 - 20 == 1 || FlagCompression2 - 20 > 1 && FlagCompression2 - 20 % 2 != 1)
                MC = 1;
            MaximumCompression = MC == 1;

            CompressSound = FlagCompression2 - 20 == 2 + MC || FlagCompression2 - 20 >= 4 + 2 + MC;

            IncludeExternalFiles = FlagCompression2 - 20 >= 4;

            DisplayBuildWarning = FlagDumb == 1 || FlagDumb > 1 && FlagDumb % 2 != 1;

            OptimizeImageRAM = FlagDumb >= 2;
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }

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
