using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.Memory;
using HarmonyLib;
using Image = CTFAK.CCN.Chunks.Banks.Image;

namespace AndroidImageReader
{
    [HarmonyPatch(typeof(Image),nameof(Image.bitmap),MethodType.Getter)]
    class BitmapExport_Patch
    {
        public static void Postfix(Image __instance,ref Bitmap __result)
        {
            __result = __instance.realBitmap;
        }
        public static bool Prefix(ref Image __instance)
        {


                var width = __instance.width;
                var height = __instance.height;
                var imageData = __instance.imageData;
                byte[] colorArray = new byte[width * height * 4];
                int stride = width * 4;

                int position = 0;
                if (__instance.graphicMode == 0)
                {
                    int pad = Image.GetPadding(width, 4);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {



                            colorArray[(y * stride) + (x * 4) + 2] = imageData[position];
                            colorArray[(y * stride) + (x * 4) + 1] = imageData[position + 1];
                            colorArray[(y * stride) + (x * 4) + 0] = imageData[position + 2];
                            colorArray[(y * stride) + (x * 4) + 3] = imageData[position + 3];




                            position += 4;
                        }

                        position += pad * 3;
                    }
                }
                else if (__instance.graphicMode == 1)
                {
                    int pad = Image.GetPadding(width, 3);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            UInt16 newShort = (ushort)(imageData[position] | imageData[position + 1] << 8);

                            byte a = (byte)((newShort & 0xf) >> 0);
                            byte r = (byte)((newShort & 0xF000) >> 12);
                            byte g = (byte)((newShort & 0xF00) >> 8);
                            byte b = (byte)((newShort & 0xf0) >> 4);

                            r = (byte)(r << 4);
                            g = (byte)(g << 4);
                            b = (byte)(b << 4);
                            a = (byte)(a << 4);
                            //r done
                            //g partially done


                            colorArray[(y * stride) + (x * 4) + 2] = r;
                            colorArray[(y * stride) + (x * 4) + 1] = g;
                            colorArray[(y * stride) + (x * 4) + 0] = b;
                            colorArray[(y * stride) + (x * 4) + 3] = a;

                            position += 2;
                        }

                        position += pad;// * 2;
                    }
                }
                else if (__instance.graphicMode == 2)
                {
                    int pad = Image.GetPadding(width, 3);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            UInt16 newShort = (ushort)(imageData[position] | imageData[position + 1] << 8);

                            byte a = (byte)((newShort & 0xf) >> 0);
                            byte r = (byte)((newShort & 0xf800) >> 11);
                            byte g = (byte)((newShort & 0x7c0) >> 6);
                            byte b = (byte)((newShort & 0x3e) >> 1);

                            r = (byte)(r << 3);
                            g = (byte)(g << 3);
                            b = (byte)(b << 3);
                            a = (byte)(a << 4);
                            //r done
                            //g partially done


                            colorArray[(y * stride) + (x * 4) + 2] = r;
                            colorArray[(y * stride) + (x * 4) + 1] = g;
                            colorArray[(y * stride) + (x * 4) + 0] = b;
                            colorArray[(y * stride) + (x * 4) + 3] = 255;
                            if (newShort == 0) colorArray[(y * stride) + (x * 4) + 3] = 0;

                            position += 2;
                        }

                        position += pad * 2;
                    }
                }
                else if (__instance.graphicMode == 3)
                {
                    int pad = Image.GetPadding(width, 3, 4);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {



                            colorArray[(y * stride) + (x * 4) + 2] = imageData[position];
                            colorArray[(y * stride) + (x * 4) + 1] = imageData[position + 1];
                            colorArray[(y * stride) + (x * 4) + 0] = imageData[position + 2];
                            colorArray[(y * stride) + (x * 4) + 3] = 255;

                            position += 3;
                        }
                        position += pad;

                        /*if (height * width * 3 != imageData.Length && height * width * 3 + height * 3 != imageData.Length && height * width * 3 + height != imageData.Length)
                        {
                            position += 2;
                        }
                        else if (height * width * 3 + height * 3 == imageData.Length)
                        {
                            position += 3;
                        }
                        else if (height * width * 3 + height == imageData.Length)
                        {
                            position++;
                        }*/


                    }


                }
                else if (__instance.graphicMode == 4)
                {
                    int pad = Image.GetPadding(width, 3);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            UInt16 newShort = (ushort)(imageData[position] | imageData[position + 1] << 8);

                            byte r = (byte)((newShort & 0xF800) >> 11);
                            byte g = (byte)((newShort & 0x7E0) >> 5);
                            byte b = (byte)((newShort & 0x1F));

                            r = (byte)(r << 3);
                            g = (byte)(g << 2);
                            b = (byte)(b << 3);
                            //r done
                            //g partially done


                            colorArray[(y * stride) + (x * 4) + 2] = r;
                            colorArray[(y * stride) + (x * 4) + 1] = g;
                            colorArray[(y * stride) + (x * 4) + 0] = b;
                            colorArray[(y * stride) + (x * 4) + 3] = 255;

                            position += 2;
                        }

                        position += pad * 2;
                    }
                }
                else if (__instance.graphicMode == 5)
                {
                    __instance.realBitmap = new Bitmap(Bitmap.FromStream(new MemoryStream(imageData)));
                }
                else Console.WriteLine("BROKEN COLOR MODE " + __instance.graphicMode);


                __instance.realBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                BitmapData bmpData = __instance.realBitmap.LockBits(new Rectangle(0, 0,
                         __instance.realBitmap.Width,
                         __instance.realBitmap.Height),
                    ImageLockMode.WriteOnly,
                     __instance.realBitmap.PixelFormat);

                IntPtr pNative = bmpData.Scan0;
                Marshal.Copy(colorArray, 0, pNative, colorArray.Length);

                __instance.realBitmap.UnlockBits(bmpData);

            
            return false;

        }


    }







    [HarmonyPatch(typeof(ImageBank), "Read")]
    class ImageBank_Patch
    {
        public static bool Prefix(ref ImageBank __instance)
        {
            var reader = __instance.reader;
            var maxHandle = reader.ReadInt16();
            var count = reader.ReadInt16();
            for (int i = 0; i < count; i++)
            {
                var newImg = new Image(reader);
                newImg.Read();
                __instance.Items.Add(newImg.Handle, newImg);
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(Image),"Read")]
    class Image_Patch
    {
        public static bool Prefix(ref Image __instance)
        {
            var reader = __instance.reader;
            __instance.Handle = reader.ReadInt16();
            __instance.graphicMode= (byte)reader.ReadInt32();

            __instance.width = reader.ReadInt16(); //width
            __instance.height = reader.ReadInt16(); //height
            __instance.HotspotX = reader.ReadInt16();
            __instance.HotspotY = reader.ReadInt16();
            __instance.ActionX = reader.ReadInt16();
            __instance.ActionY = reader.ReadInt16();
            var size = reader.ReadInt32();
            var thefuk1 = reader.PeekByte();
            ByteReader imageReader;

            if (thefuk1 == 255)
            {
                __instance.imageData = reader.ReadBytes(size);
                //return;
            }
            else
            {
                imageReader = new ByteReader(Decompressor.DecompressBlock(reader, size));
                imageReader.Seek(0);
                __instance.imageData = imageReader.ReadBytes();
            }
            return false;
        }
    }
}
