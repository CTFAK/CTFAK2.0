using CTFAK.Memory;
using CTFAK.Utils;
using Ionic.Zlib;
using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CTFAK.Attributes;

namespace CTFAK.CCN.Chunks.Banks
{
    [ChunkLoader(26214,"ImageBank")]
    public class ImageBank : ChunkLoader
    {
        public static event SaveHandler OnImageLoaded;
        public Dictionary<int, Image> Items = new Dictionary<int, Image>();
        public static int realGraphicMode = 4;
        public override void Read(ByteReader reader)
        {
            if (Core.parameters.Contains("-noimg")) return;

            if (Settings.android)
            {
                var maxHandle = reader.ReadInt16();
                var count = reader.ReadInt16();
                for (int i = 0; i < count; i++)
                {
                    var newImg = new Image();
                    newImg.Read(reader);
                    Items.Add(newImg.Handle, newImg);
                }
            }
            else
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    
                    var newImg = new Image();
                    newImg.Read(reader);
                    OnImageLoaded?.Invoke(i,count);
                    Items.Add(newImg.Handle, newImg);
                }
                
            }

            foreach (var task in Image.imageReadingTasks)
            {
                task.Wait();
            }
        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class Image : ChunkLoader
    {
        public Bitmap realBitmap;

        public Bitmap bitmap
        {
            get
            {
                if (realBitmap == null)
                {
                    byte[] colorArray = null;
                    colorArray = new byte[Width * Height * 4];

                    IntPtr resultAllocated = Marshal.AllocHGlobal(Width * Height * 4);
                    IntPtr imageAllocated = Marshal.AllocHGlobal(imageData.Length);

                    Marshal.Copy(imageData, 0, imageAllocated, imageData.Length);
                    switch (GraphicMode)
                    {
                        case 4:
                            NativeLib.ReadPoint(resultAllocated, Width, Height, Flags["Alpha"] ? 1 : 0,
                                imageData.Length, imageAllocated, Transparent);
                            ImageBank.realGraphicMode = 4;
                            break;
                        case 6:
                            NativeLib.ReadFifteen(resultAllocated, Width, Height, Flags["Alpha"] ? 1 : 0,
                                imageData.Length, imageAllocated, Transparent);
                            ImageBank.realGraphicMode = 3;
                            break;
                        case 7:
                            NativeLib.ReadSixteen(resultAllocated, Width, Height, Flags["Alpha"] ? 1 : 0,
                                imageData.Length, imageAllocated, Transparent);
                            ImageBank.realGraphicMode = 2;
                            break;
                        case 16:
                            int stride = Width * 4;
                            int pad = GetPadding(Width, 4);
                            int position = 0;
                            for (int y = 0; y < Height; y++)
                            {
                                for (int x = 0; x < Width; x++)
                                {
                                    var bytes = BitConverter.GetBytes(Transparent);
                                    colorArray[(y * stride) + (x * 4) + 0] = imageData[position + 0];
                                    colorArray[(y * stride) + (x * 4) + 1] = imageData[position + 1];
                                    colorArray[(y * stride) + (x * 4) + 2] = imageData[position + 2];

                                    if (Flags["Alpha"] && !Core.parameters.Contains("-noalpha"))
                                    {
                                        colorArray[(y * stride) + (x * 4) + 3] = imageData[position + 3];
                                    }
                                    else
                                    {
                                        //colorArray[(y * stride) + (x * 4) + 3] = 255;
                                        if (imageData[position] == bytes[0] && imageData[position + 1] == bytes[1] &&
                                            imageData[position + 2] == bytes[2])
                                            colorArray[(y * stride) + (x * 4) + 3] = 255; //bytes[3];
                                    }

                                    position += 4;
                                }

                                position += pad * 4;
                            }

                            ImageBank.realGraphicMode = 1;
                            break;
                    }

                    if (GraphicMode != 16)
                    {
                        Marshal.Copy(resultAllocated, colorArray, 0, colorArray.Length);
                    }

                    Marshal.FreeHGlobal(resultAllocated);
                    Marshal.FreeHGlobal(imageAllocated);
                    //colorArray = ReadPoint(imageData, width, height, 4).Item1;
                    realBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                    BitmapData bmpData = realBitmap.LockBits(new Rectangle(0, 0,
                            realBitmap.Width,
                            realBitmap.Height),
                        ImageLockMode.WriteOnly,
                        realBitmap.PixelFormat);

                    IntPtr pNative = bmpData.Scan0;
                    Marshal.Copy(colorArray, 0, pNative, colorArray.Length);

                    realBitmap.UnlockBits(bmpData);
                    //realBitmap.Save($"Images\\{Handle}.png");
                    //Logger.Log("Trying again");
                    if (Settings.twofiveplus)
                        ImageBank.realGraphicMode = 4;
                }

                return realBitmap;
            }
        }

        public void FromBitmap(Bitmap bmp)
        {
            Width = bmp.Width;
            Height = bmp.Height;
            if (!Core.parameters.Contains("-noalpha"))
                Flags["Alpha"] = true;
            GraphicMode = 4;

            var bitmapData = bmp.LockBits(new Rectangle(0, 0,
                    bmp.Width,
                    bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int copyPad = GetPadding(Width, 4);
            var length = bitmapData.Height * bitmapData.Stride + copyPad * 4;

            byte[] bytes = new byte[length];
            int stride = bitmapData.Stride;
            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bmp.UnlockBits(bitmapData);

            imageData = new byte[Width * Height * 6];
            int position = 0;
            int pad = GetPadding(Width, 3);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int newPos = (y * stride) + (x * 3);
                    imageData[position] = bytes[newPos];
                    imageData[position + 1] = bytes[newPos + 1];
                    imageData[position + 2] = bytes[newPos + 2];
                    position += 3;
                }

                position += 3 * pad;
            }

            try
            {
                var bitmapDataAlpha = bmp.LockBits(new Rectangle(0, 0,
                        bmp.Width,
                        bmp.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);
                int copyPadAlpha = GetPadding(Width, 1);
                var lengthAlpha = bitmapDataAlpha.Height * bitmapDataAlpha.Stride + copyPadAlpha * 4;

                byte[] bytesAlpha = new byte[lengthAlpha];
                int strideAlpha = bitmapDataAlpha.Stride;
                // Copy bitmap to byte[]
                Marshal.Copy(bitmapDataAlpha.Scan0, bytesAlpha, 0, lengthAlpha);
                bmp.UnlockBits(bitmapDataAlpha);

                int aPad = GetPadding(Width, 1, 4);
                int alphaPos = position;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        imageData[alphaPos] = bytesAlpha[(y * strideAlpha) + (x * 4) + 3];
                        alphaPos += 1;
                    }

                    alphaPos += aPad;
                }
            }
            catch
            {
            } /*(Exception ex){Console.WriteLine(ex);}*/
        }

        public static int GetPadding(int width, int pointSize, int bytes = 2)
        {
            int pad = bytes - ((width * pointSize) % bytes);
            if (pad == bytes)
            {
                return 0;
            }

            return (int)Math.Ceiling(pad / (float)pointSize);
        }

        public bool IsMFA;

        public BitDict Flags = new BitDict(new[]
        {
            "RLE",
            "RLEW",
            "RLET",
            "LZX",
            "Alpha",
            "ACE",
            "Mac"
        });

        public int Handle;
        public int Width;
        public int Height;
        public byte GraphicMode;
        public int Checksum;
        public int references;
        public byte[] imageData;
        public short HotspotX;
        public short HotspotY;
        public short ActionX;
        public short ActionY;
        public int Transparent;

        public static List<Task> imageReadingTasks = new List<Task>();

        public override void Read(ByteReader reader)
        {
            if (Settings.twofiveplus && !IsMFA)
            {
                Handle = reader.ReadInt32();
                Handle -= 1;
                var checksum = reader.ReadInt32();
                var references = reader.ReadInt32();
                //Flags.flag = reader.ReadUInt32();
                var unk3 = reader.ReadInt32();
                var dataSize = reader.ReadInt32();
                Width = reader.ReadInt16(); //width
                Height = reader.ReadInt16(); //height
                reader.ReadByte(); //color mode
                Flags.flag = reader.ReadByte();

                var unk6 = reader.ReadInt16();
                HotspotX = reader.ReadInt16();
                HotspotY = reader.ReadInt16();
                ActionX = reader.ReadInt16();
                ActionY = reader.ReadInt16();
                Transparent = reader.ReadInt32();
                var decompressedSize = reader.ReadInt32();
                var rawImg = reader.ReadBytes(dataSize - 4);
                if (!Core.parameters.Contains("-noalpha"))
                    Flags["Alpha"] = true;
                byte[] target = new byte[decompressedSize];
                LZ4Codec.Decode(rawImg, target);
                imageData = target;
                GraphicMode = 16;
                var bmp = bitmap;
                var newImg = new Image();
                newImg.FromBitmap(bmp);
                imageData = newImg.imageData;
                GraphicMode = 4;
                Flags = newImg.Flags;
            }
            else if (Settings.gameType == Settings.GameType.NORMAL && !Settings.android)
            {
                Handle = reader.ReadInt32();
                if (!IsMFA && Settings.Build >= 284) Handle -= 1;

                byte[] newImageData = null;
                if (!IsMFA)
                {
                    Int32 decompSize = reader.ReadInt32();
                    Int32 compSize = reader.ReadInt32();
                    newImageData = reader.ReadBytes(compSize);
                }

                ByteReader imageReader;
                var imageReadingTask = new Task(() =>
                {
                    if (IsMFA)
                    {
                        imageReader = reader;
                    }
                    else
                    {
                        imageReader = new ByteReader(ZlibStream.UncompressBuffer(newImageData));
                    }

                    Checksum = imageReader.ReadInt32();
                    references = imageReader.ReadInt32();
                    var size = imageReader.ReadInt32();
                    if (IsMFA) imageReader = new ByteReader(imageReader.ReadBytes(size + 20));
                    Width = imageReader.ReadInt16();
                    Height = imageReader.ReadInt16();

                    GraphicMode = imageReader.ReadByte();
                    Flags.flag = imageReader.ReadByte();
                    imageReader.Skip(2);
                    HotspotX = imageReader.ReadInt16();
                    HotspotY = imageReader.ReadInt16();
                    ActionX = imageReader.ReadInt16();
                    ActionY = imageReader.ReadInt16();
                    Transparent = imageReader.ReadInt32();
                    //Logger.Log($"Loading image {Handle} with size {width}x{height}");

                    if (Flags["LZX"])
                    {
                        uint decompressedSize = imageReader.ReadUInt32();

                        imageData = Decompressor.DecompressBlock(imageReader,
                            (int)(imageReader.Size() - imageReader.Tell()),
                            (int)decompressedSize);
                    }
                    else
                        imageData = imageReader.ReadBytes((int)(size));
                });
                imageReadingTasks.Add(imageReadingTask);
                if (IsMFA)
                    imageReadingTask.RunSynchronously();
                else
                    imageReadingTask.Start();
                //imageReader = IsMFA ? reader :Decompressor.DecompressAsReader(reader, out var a);
            }
            else if (Settings.android)
            {
                Handle = reader.ReadInt16();
                var unk = (uint)reader.ReadInt32();

                Width = reader.ReadInt16(); //width
                Height = reader.ReadInt16(); //height
                HotspotX = reader.ReadInt16();
                HotspotY = reader.ReadInt16();
                ActionX = reader.ReadInt16();
                ActionY = reader.ReadInt16();
                var size = reader.ReadInt32();
                var thefuk1 = reader.PeekByte();
                Logger.Log($"Loading image {Handle}, size {Width}x{Height}, mode: {unk}");
                ByteReader imageReader;

                if (thefuk1 == 255)
                {
                    imageData = reader.ReadBytes(size);
                    //return;
                }
                else
                {
                    imageReader = new ByteReader(Decompressor.DecompressBlock(reader, size));
                    imageReader.Seek(0);
                    imageData = imageReader.ReadBytes();
                }

                var newImage = ImageHelper.DumpImage(Handle, imageData, Width, Height, unk);
                imageData = newImage.imageData;
                if (!Core.parameters.Contains("-noalpha"))
                    Flags["Alpha"] = true;
                GraphicMode = 4;
            }
            else if (Settings.Old)
            {
                Handle = reader.ReadInt32();
                var imageReader = new ByteReader(Decompressor.DecompressOld(reader));
                Checksum = imageReader.ReadInt16();
                references = imageReader.ReadInt32();
                var size = imageReader.ReadInt32();
                Width = imageReader.ReadInt16();
                Height = imageReader.ReadInt16();
                GraphicMode = imageReader.ReadByte();
                Flags.flag = imageReader.ReadByte();
                HotspotX = imageReader.ReadInt16();
                HotspotY = imageReader.ReadInt16();
                ActionX = imageReader.ReadInt16();
                ActionY = imageReader.ReadInt16();
                if (Flags["LZX"])
                {
                    uint decompressedSize = imageReader.ReadUInt32();

                    imageData = Decompressor.DecompressBlock(imageReader,
                        (int)(imageReader.Size() - imageReader.Tell()),
                        (int)decompressedSize);
                }
                else imageData = imageReader.ReadBytes((int)(size));
            }

        }

        public static List<Task> imageWritingTasks = new List<Task>();

        public int WriteNew(ByteWriter writer)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var start = writer.Tell();

            byte[] compressedImg = null;
            Flags["LZX"] = true;

            compressedImg = Decompressor.compress_block(imageData);

            writer.WriteInt32(Handle);
            writer.WriteInt32(Checksum); //4
            writer.WriteInt32(references); //8
            writer.WriteUInt32((uint)compressedImg.Length + 4); //12
            writer.WriteInt16((short)Width); //14
            writer.WriteInt16((short)Height); //16
            writer.WriteInt8((byte)GraphicMode); //17
            writer.WriteInt8((byte)Flags.flag); //18
            writer.WriteInt16(0); //20
            writer.WriteInt16((short)HotspotX); //22
            writer.WriteInt16((short)HotspotY); //24
            writer.WriteInt16((short)ActionX); //26
            writer.WriteInt16((short)ActionY); //28
            writer.WriteInt32(Transparent); //32
            writer.WriteInt32(imageData.Length); //36
            writer.WriteBytes(compressedImg);
            //writer.WriteWriter(chunk);
            // writer.WriteInt32(Handle-1);//FNAC3 FIX

            var chunkSize = 36 + compressedImg.Length;
            stopwatch.Stop();
            //Console.WriteLine($"Image: {Handle}, decompressed/compressed ratio: {((float)compressedImg.Length)/imageData.Length}, time: {stopwatch.ElapsedMilliseconds}, level:{Decompressor.compressionLevel}");
            //Console.ReadKey();
            return (int)(chunkSize + 4 + start);
        }

        public override void Write(ByteWriter writer)
        {

        }
    }
}
