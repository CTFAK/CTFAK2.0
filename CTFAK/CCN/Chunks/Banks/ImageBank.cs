using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
        public static int GetPadding(int width, int pointSize, int bytes = 2)
        {
            int pad = bytes - ((width * pointSize) % bytes);
            if (pad == bytes)
            {
                return 0;
            }

            return (int)Math.Ceiling((double)((float)pad / (float)pointSize));
        }
        public bool IsMFA;
        BitDict Flags = new BitDict(new string[]
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

        public Image(ByteReader reader):base(reader)
        {

        }
        public override void Read()
        {
            Handle = reader.ReadInt32();
            if (!IsMFA && Settings.Build >= 284) Handle -= 1;
            ByteReader imageReader;
            imageReader = Decompressor.DecompressAsReader(reader, out var a);
            var checksum = imageReader.ReadInt32();
            var references = imageReader.ReadInt32();
            var size = imageReader.ReadInt32();
            //TODO: MFA
            width = imageReader.ReadInt16();
            height = imageReader.ReadInt16();

            graphicMode = imageReader.ReadByte();
            Flags.flag = imageReader.ReadByte();
            imageReader.Skip(2);
            var HotspotX = imageReader.ReadInt16();
            var HotspotY = imageReader.ReadInt16();
            var ActionX = imageReader.ReadInt16();
            var ActionY = imageReader.ReadInt16();
            var _transparent = imageReader.ReadColor();
            //Logger.Log($"Loading image {Handle} with size {width}x{height}");
            byte[] imageData;
            if (Flags["LZX"])
            {
                uint decompressedSize = imageReader.ReadUInt32();

                imageData = Decompressor.DecompressBlock(imageReader,
                    (int)(imageReader.Size() - imageReader.Tell()),
                    (int)decompressedSize);
            }
            else imageData = imageReader.ReadBytes((int)(size));

            IntPtr resultAlloted = Marshal.AllocHGlobal(width * height * 4);
            IntPtr imageAllocated = Marshal.AllocHGlobal(size);
            
            
            Marshal.Copy(imageData, 0, imageAllocated, imageData.Length);

            NativeLib.ConvertImage(resultAlloted, width, height,Flags["Alpha"]?1:0,size,imageAllocated);

            byte[] colorArray = new byte[width * height * 4];
            Marshal.Copy(resultAlloted, colorArray, 0, colorArray.Length);
            /*int stride = width * 4;
            int pad = GetPadding(width, 3);
            int position = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorArray[(y * stride) + (x * 4) + 0] = imageData[position];
                    colorArray[(y * stride) + (x * 4) + 1] = imageData[position + 1];
                    colorArray[(y * stride) + (x * 4) + 2] = imageData[position + 2];
                    colorArray[(y * stride) + (x * 4) + 3] = 255;
                    position += 3;
                }

                position += pad * 3;
            }
            if (Flags["Alpha"])
            {
                int alphaSize = size - position;
                alphaSize = size - alphaSize;

                int aPad = GetPadding(width, 1, 4);
                byte[,] alpha = new byte[width, height];
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        alpha[j, i] = imageData[alphaSize];
                        alphaSize += 1;
                    }

                    alphaSize += aPad;
                }

                int aStride = width * 4;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        colorArray[(y * aStride) + (x * 4) + 3] = alpha[x, y];
                    }
                }
            }*/
            using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,
                        bmp.Width,
                        bmp.Height),
                    ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                IntPtr pNative = bmpData.Scan0;
                Marshal.Copy(colorArray, 0, pNative, colorArray.Length);

                bmp.UnlockBits(bmpData);
                bmp.Save($"Images\\{Handle}.png");
                //Logger.Log("Trying again");
            }

        }

        public override void Write(ByteWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
