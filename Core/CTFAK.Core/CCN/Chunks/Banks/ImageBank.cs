using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zlib;
using Joveler.Compression.ZLib;

namespace CTFAK.CCN.Chunks.Banks
{
    public class ImageBank : ChunkLoader
    {
        public static event Core.SaveHandler OnImageLoaded;
        public Dictionary<int, Image> Items = new Dictionary<int, Image>();
        public ImageBank(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            if (Settings.android)
            {
                var maxHandle = reader.ReadInt16();
                var count = reader.ReadInt16();
                for (int i = 0; i < count; i++)
                {
                    var newImg = new Image(reader);
                    newImg.Read();
                    Items.Add(newImg.Handle, newImg);
                }
            }
            else
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var newImg = new Image(reader);
                    newImg.Read();
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
                if(realBitmap==null)
                {

                    byte[] colorArray = null;
                    colorArray = new byte[width * height * 4];

                    IntPtr resultAllocated = Marshal.AllocHGlobal(width * height * 4);
                    IntPtr imageAllocated = Marshal.AllocHGlobal(imageData.Length);

                    Marshal.Copy(imageData, 0, imageAllocated, imageData.Length);
                    switch (graphicMode)
                    {
                        case 4:          
                            NativeLib.ReadPoint(resultAllocated, width, height, Flags["Alpha"] ? 1 : 0, imageData.Length, imageAllocated, transparent);
                            break;
                        case 6:
                            NativeLib.ReadFifteen(resultAllocated, width, height, Flags["Alpha"] ? 1 : 0, imageData.Length, imageAllocated, transparent);
                            break;
                        case 7:
                            NativeLib.ReadSixteen(resultAllocated, width, height, Flags["Alpha"] ? 1 : 0, imageData.Length, imageAllocated, transparent);
                            break;
                    }
                    Marshal.Copy(resultAllocated, colorArray, 0, colorArray.Length);
                    Marshal.FreeHGlobal(resultAllocated);
                    Marshal.FreeHGlobal(imageAllocated);
                    //colorArray = ReadPoint(imageData, width, height, 4).Item1;
                    realBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                        BitmapData bmpData = realBitmap.LockBits(new Rectangle(0, 0,
                                realBitmap.Width,
                                realBitmap.Height),
                            ImageLockMode.WriteOnly,
                            realBitmap.PixelFormat);

                        IntPtr pNative = bmpData.Scan0;
                        Marshal.Copy(colorArray, 0, pNative, colorArray.Length);

                    realBitmap.UnlockBits(bmpData);
                        //bmp.Save($"Images\\{Handle}.png");
                        //Logger.Log("Trying again");
                    
                    
                }
                return realBitmap;

            }
        }

        public void FromBitmap(Bitmap bmp)
        {
            width = bmp.Width;
            height = bmp.Height;
            Flags["Alpha"] = true;
            graphicMode = 4;

            var bitmapData = bmp.LockBits(new Rectangle(0, 0,
                    bmp.Width,
                    bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int copyPad = GetPadding(width, 4);
            var length = bitmapData.Height * bitmapData.Stride+copyPad*4;

            byte[] bytes = new byte[length];
            int stride = bitmapData.Stride;
            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bmp.UnlockBits(bitmapData);

            
            
            imageData = new byte[width * height * 6];
            int position = 0;
            int pad = GetPadding(width, 3);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int newPos = (y * stride) + (x*3);
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
                int copyPadAlpha = GetPadding(width, 1);
                var lengthAlpha = bitmapDataAlpha.Height * bitmapDataAlpha.Stride+copyPadAlpha*4;

                byte[] bytesAlpha = new byte[lengthAlpha];
                int strideAlpha = bitmapDataAlpha.Stride;
                // Copy bitmap to byte[]
                Marshal.Copy(bitmapDataAlpha.Scan0, bytesAlpha, 0, lengthAlpha);
                bmp.UnlockBits(bitmapDataAlpha);
            
                int aPad = GetPadding(width, 1, 4);
                int alphaPos = position;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        imageData[alphaPos] = bytesAlpha[(y * strideAlpha) + (x*4)+3];
                        alphaPos += 1;
                    }

                    alphaPos += aPad;
                }
            }catch(Exception ex){Console.WriteLine(ex);}
            
        }
        
        public static int GetPadding(int width, int pointSize, int bytes = 2)
        {
            return (bytes - ((width * pointSize) % bytes)) % bytes;
        }
        public bool IsMFA;
        public BitDict Flags = new BitDict(new string[]
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
        public int width;
        public int height;
        public byte graphicMode;
        public int checksum;
        public int references;
        public byte[] imageData;
        public short HotspotX;
        public short HotspotY;
        public short ActionX;
        public short ActionY;
        public int transparent;
        

        public Image(ByteReader reader):base(reader)
        {

        }

        public static List<Task> imageReadingTasks = new List<Task>();
        public override void Read()
        {
            if (Settings.twofiveplus&&!IsMFA)
            {
                Handle = reader.ReadInt32();

                var unk2 = reader.ReadInt32();
                var unk = reader.ReadInt32();
                //Flags.flag = reader.ReadUInt32();
                var unk3 = reader.ReadInt32();
                var dataSize = reader.ReadInt32();
                width = reader.ReadInt16(); //width
                height = reader.ReadInt16(); //height
                var unk6 = reader.ReadInt16();
                var unk7 = reader.ReadInt16();
                HotspotX = reader.ReadInt16();
                HotspotY = reader.ReadInt16();
                ActionX = reader.ReadInt16();
                ActionY = reader.ReadInt16();
                transparent = reader.ReadInt32();
                var decompressedSize = reader.ReadInt32();
                var rawImg = reader.ReadBytes(dataSize - 4);
                byte[] target = new byte[decompressedSize];
                //LZ4Codec.Decode(rawImg, target);
                graphicMode = 16;
            }
            else if(Settings.gameType==Settings.GameType.NORMAL)
            {
                Handle = reader.ReadInt32();
                if (!IsMFA && Settings.Build >= 284) Handle -= 1;

                byte[] newImageData=null;
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
                    
                        
                        checksum = imageReader.ReadInt32();
                        references = imageReader.ReadInt32();
                        var size = imageReader.ReadInt32();
                        if (IsMFA) imageReader = new ByteReader(imageReader.ReadBytes(size + 20));
                        width = imageReader.ReadInt16();
                        height = imageReader.ReadInt16();

                        graphicMode = imageReader.ReadByte();
                        Flags.flag = imageReader.ReadByte();
                        imageReader.Skip(2);
                        HotspotX = imageReader.ReadInt16();
                        HotspotY = imageReader.ReadInt16();
                        ActionX = imageReader.ReadInt16();
                        ActionY = imageReader.ReadInt16();
                        transparent = imageReader.ReadInt32();
                        //Logger.Log($"Loading image {Handle} with size {width}x{height}");

                        if (Flags["LZX"])
                        {
                            uint decompressedSize = imageReader.ReadUInt32();

                            imageData = Decompressor.DecompressBlock(imageReader,
                                (int)(imageReader.Size() - imageReader.Tell()),
                                (int)decompressedSize);
                        }
                        else imageData = imageReader.ReadBytes((int)(size));
                    });
                    imageReadingTasks.Add(imageReadingTask);
                    if(IsMFA) imageReadingTask.RunSynchronously();
                    else imageReadingTask.Start();
                //imageReader = IsMFA ? reader :Decompressor.DecompressAsReader(reader, out var a);

                
            }
            else if (Settings.android)
            {
                Handle = reader.ReadInt16();
                var unk = (uint)reader.ReadInt32();

                width = reader.ReadInt16(); //width
                height = reader.ReadInt16(); //height
                HotspotX = reader.ReadInt16();
                HotspotY = reader.ReadInt16();
                ActionX = reader.ReadInt16();
                ActionY = reader.ReadInt16();
                var size = reader.ReadInt32();
                var thefuk1 = reader.PeekByte();
                Logger.Log($"Loading image {Handle}, size {width}x{height}, mode: {unk}");
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
                var newImage = ImageHelper.DumpImage(Handle, imageData, width, height, unk);
                imageData = newImage.imageData;
                Flags["Alpha"] = true;
                graphicMode = 4;
                
            }
            else if (Settings.Old)
            {
                Handle = reader.ReadInt32();
                var imageReader = new ByteReader(Decompressor.DecompressOld(reader));
                checksum = imageReader.ReadInt16();
                references = imageReader.ReadInt32();
                var size = imageReader.ReadInt32();
                width = imageReader.ReadInt16();
                height = imageReader.ReadInt16();
                graphicMode = imageReader.ReadByte();
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
                    writer.WriteInt32(checksum);//4
                    writer.WriteInt32(references);//8
                    writer.WriteUInt32((uint)compressedImg.Length + 4);//12
                    writer.WriteInt16((short)width);//14
                    writer.WriteInt16((short)height);//16
                    writer.WriteInt8((byte)graphicMode);//17
                    writer.WriteInt8((byte)Flags.flag);//18
                    writer.WriteInt16(0);//20
                    writer.WriteInt16((short)HotspotX);//22
                    writer.WriteInt16((short)HotspotY);//24
                    writer.WriteInt16((short)ActionX);//26
                    writer.WriteInt16((short)ActionY);//28
                    writer.WriteInt32(transparent);//32

                    writer.WriteInt32(imageData.Length);//36
                    writer.WriteBytes(compressedImg);
                    
                    //writer.WriteWriter(chunk);



                    // writer.WriteInt32(Handle-1);//FNAC3 FIX
            var chunkSize = 36 + compressedImg.Length;
            stopwatch.Stop();
            //Console.WriteLine($"Image: {Handle}, decompressed/compressed ratio: {((float)compressedImg.Length)/imageData.Length}, time: {stopwatch.ElapsedMilliseconds}, level:{Decompressor.compressionLevel}");
            //Console.ReadKey();
            return (int)(chunkSize+4+start);
        }
        public override void Write(ByteWriter writer)
        {
            
        }
    }
}
