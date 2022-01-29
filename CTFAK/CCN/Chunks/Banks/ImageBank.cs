using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks.Banks
{
    public class ImageBank : ChunkLoader
    {
        public Dictionary<int, Image> Items = new Dictionary<int, Image>();
        public ImageBank(ByteReader reader) : base(reader) { }
        public override void Read()
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var newImg = new Image(reader);
                newImg.Read();
                Items.Add(newImg.Handle, newImg);
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
        public static (byte[], int) ReadPoint(byte[] data, int width, int height, int threads)
        {
            byte[] colorArray = new byte[width * height * 4];
            int stride = width * 4;
            int pad = GetPadding(width, 3);
            var tasks = new Task[threads];

            var size = width * height;
            if (size < threads)
            {
                // Log Warning: "Image has less pixels than the amount of threads assigned. Using less threads."
                threads = size;
            }

            var part = size / threads;
            for (var a = 0; a < threads; a++)
            {
                var posX = part * a % width;
                var posY = part * a / width;
                var almostDone = a + 1 == threads;
                var next = part * (a + 1);
                var endX = (almostDone ? width : next % width);
                var endY = (almostDone ? height : next / width);
                tasks[a] = Task.Run(() => ReadPointThreaded(data, colorArray, stride, pad, width, height, posX, posY, endX, endY));
            }
            foreach (var t in tasks)
                t.Wait();

            return (colorArray, 0);
        }

        private static void ReadPointThreaded(byte[] data, byte[] colorArray, int stride, int pad, int width, int height, int startX, int startY, int maxX, int maxY)
        {
            for (int y = startY; y < maxY; y++)
            {
                if (y + 1 == maxY)
                    width = maxX;
                for (int x = startX; x < width; x++)
                {
                    var position = x * 3 + y * width + y * pad * 3;
                    colorArray[(y * stride) + (x * 4) + 0] = data[position];
                    colorArray[(y * stride) + (x * 4) + 1] = data[position + 1];
                    colorArray[(y * stride) + (x * 4) + 2] = data[position + 2];
                    colorArray[(y * stride) + (x * 4) + 3] = 255;
                }
                startX = 0;
            }
        }
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
                var length = bitmapData.Stride * bitmapData.Height;

                byte[] bytes = new byte[length];

                // Copy bitmap to byte[]
                Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
                bmp.UnlockBits(bitmapData);

            imageData = new byte[width * height * 4];
            int position = 0;
            int pad = GetPadding(width, 3);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    imageData[position] = bytes[position];
                    imageData[position+1] = bytes[position+1];
                    imageData[position+2] = bytes[position+2];
                    position += 3;
                }
                
                position += 3 * pad;
            }
            
            int aPad = GetPadding(width, 1, 4);
            int alphaPos = position;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    imageData[alphaPos] = 255;
                    alphaPos += 1;
                }
                alphaPos += aPad;
            }
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
        public override void Read()
        {
            Handle = reader.ReadInt32();
            if (!IsMFA && Settings.Build >= 284) Handle -= 1;
            ByteReader imageReader;
            imageReader = IsMFA ? reader :Decompressor.DecompressAsReader(reader, out var a);
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

            

        }

        public override void Write(ByteWriter writer)
        {
            ByteWriter chunk = new ByteWriter(new MemoryStream());
            chunk.WriteInt32(checksum);
            chunk.WriteInt32(references);
            byte[] compressedImg = null;
            Flags["LZX"] = true;
            if (Flags["LZX"])
            {
                compressedImg = Decompressor.compress_block(imageData);
                chunk.WriteUInt32((uint)compressedImg.Length + 4);
            }
            else
            {
                chunk?.WriteUInt32((uint)(imageData?.Length ?? 0));
            }

            chunk.WriteInt16((short)width);
            chunk.WriteInt16((short)height);
            chunk.WriteInt8((byte)graphicMode);
            chunk.WriteInt8((byte)Flags.flag);
            chunk.WriteInt16(0);
            chunk.WriteInt16((short)HotspotX);
            chunk.WriteInt16((short)HotspotY);
            chunk.WriteInt16((short)ActionX);
            chunk.WriteInt16((short)ActionY);
            chunk.WriteInt32(transparent);
            if (Flags["LZX"])
            {
                chunk.WriteInt32(imageData.Length);
                chunk.WriteBytes(compressedImg);
            }

            else
            {
                chunk.WriteBytes(imageData);
            }

            writer.WriteInt32(Handle);
            // writer.WriteInt32(Handle-1);//FNAC3 FIX
            writer.WriteWriter(chunk);
        }
    }
}
