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
        private Bitmap realBitmap;
        public Bitmap bitmap
        {
            get
            {
                if(realBitmap==null)
                {
                    IntPtr resultAllocated = Marshal.AllocHGlobal(width * height * 4);
                    IntPtr imageAllocated = Marshal.AllocHGlobal(imageData.Length);


                    Marshal.Copy(imageData, 0, imageAllocated, imageData.Length);

                    NativeLib.ConvertImage(resultAllocated, width, height, Flags["Alpha"] ? 1 : 0, imageData.Length, imageAllocated, transparent);

                    byte[] colorArray = new byte[width * height * 4];
                    Marshal.Copy(resultAllocated, colorArray, 0, colorArray.Length);
                    Marshal.FreeHGlobal(resultAllocated);
                    Marshal.FreeHGlobal(imageAllocated);

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
                        //bmp.Save($"Images\\{Handle}.png");
                        //Logger.Log("Trying again");
                    }
                    
                }
                return realBitmap;

            }
        }
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
        private int checksum;
        private int references;
        private byte[] imageData;
        private short HotspotX;
        private short HotspotY;
        private short ActionX;
        private short ActionY;
        private int transparent;

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
