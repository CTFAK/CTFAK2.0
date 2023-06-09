using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace CTFAK.Utils;

public static class ImageHelper
{
    public static int GetPadding(int width, int pointSize, int bytes = 2, bool gaylord = false)
    {
        if (gaylord) return (bytes - width * pointSize % bytes) % bytes;

        var pad = bytes - width * pointSize % bytes;
        if (pad == bytes) return 0;

        return (int)Math.Ceiling(pad / (float)pointSize);
    }

    public static Bitmap DumpImage(byte[] imageData, int width, int height, uint unk)
    {
        var colorArray = new byte[width * height * 4];
        var stride = width * 4;

        var position = 0;
        if (unk == 0)
        {
            var pad = GetPadding(width, 4);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    colorArray[y * stride + x * 4 + 2] = imageData[position];
                    colorArray[y * stride + x * 4 + 1] = imageData[position + 1];
                    colorArray[y * stride + x * 4 + 0] = imageData[position + 2];
                    colorArray[y * stride + x * 4 + 3] = imageData[position + 3];
                    position += 4;
                }

                position += pad * 3;
            }
        }
        else if (unk == 1)
        {
            var pad = GetPadding(width, 3);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var newShort = (ushort)(imageData[position] | (imageData[position + 1] << 8));

                    var a = (byte)((newShort & 0xf) >> 0);
                    var r = (byte)((newShort & 0xF000) >> 12);
                    var g = (byte)((newShort & 0xF00) >> 8);
                    var b = (byte)((newShort & 0xf0) >> 4);

                    r = (byte)(r << 4);
                    g = (byte)(g << 4);
                    b = (byte)(b << 4);
                    a = (byte)(a << 4);
                    //r done
                    //g partially done

                    colorArray[y * stride + x * 4 + 2] = r;
                    colorArray[y * stride + x * 4 + 1] = g;
                    colorArray[y * stride + x * 4 + 0] = b;
                    colorArray[y * stride + x * 4 + 3] = a;

                    position += 2;
                }

                position += pad * 2;
            }
        }
        else if (unk == 2)
        {
            var pad = GetPadding(width, 3);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var newShort = (ushort)(imageData[position] | (imageData[position + 1] << 8));

                    var a = (byte)((newShort & 0xf) >> 0);
                    var r = (byte)((newShort & 0xf800) >> 11);
                    var g = (byte)((newShort & 0x7c0) >> 6);
                    var b = (byte)((newShort & 0x3e) >> 1);

                    r = (byte)(r << 3);
                    g = (byte)(g << 3);
                    b = (byte)(b << 3);
                    a = (byte)(a << 4);
                    //r done
                    //g partially done

                    colorArray[y * stride + x * 4 + 2] = r;
                    colorArray[y * stride + x * 4 + 1] = g;
                    colorArray[y * stride + x * 4 + 0] = b;
                    colorArray[y * stride + x * 4 + 3] = 255;
                    if (newShort == 0) colorArray[y * stride + x * 4 + 3] = 0;

                    position += 2;
                }

                position += pad * 2;
            }
        }
        else if (unk == 3)
        {
            var pad = GetPadding(width, 3, 4, true);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    colorArray[y * stride + x * 4 + 2] = imageData[position];
                    colorArray[y * stride + x * 4 + 1] = imageData[position + 1];
                    colorArray[y * stride + x * 4 + 0] = imageData[position + 2];
                    colorArray[y * stride + x * 4 + 3] = 255;
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
        else if (unk == 4)
        {
            var pad = GetPadding(width, 3);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var newShort = (ushort)(imageData[position] | (imageData[position + 1] << 8));

                    var r = (byte)((newShort & 0xF800) >> 11);
                    var g = (byte)((newShort & 0x7E0) >> 5);
                    var b = (byte)(newShort & 0x1F);

                    r = (byte)(r << 3);
                    g = (byte)(g << 2);
                    b = (byte)(b << 3);
                    //r done
                    //g partially done

                    colorArray[y * stride + x * 4 + 2] = r;
                    colorArray[y * stride + x * 4 + 1] = g;
                    colorArray[y * stride + x * 4 + 0] = b;
                    colorArray[y * stride + x * 4 + 3] = 255;

                    position += 2;
                }

                position += pad * 2;
            }
        }
        else if (unk == 5)
        {
            return (Bitmap)Image.FromStream(new MemoryStream(imageData));
            //File.WriteAllBytes($"{AppName}\\{Handle}-{unk}.jpg", imageData);
        }
        else
        {
            Console.WriteLine("BROKEN COLOR MODE " + unk);
        }

        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        var bmpData = bmp.LockBits(new Rectangle(0, 0,
                bmp.Width,
                bmp.Height),
            ImageLockMode.WriteOnly,
            bmp.PixelFormat);

        var pNative = bmpData.Scan0;
        Marshal.Copy(colorArray, 0, pNative, colorArray.Length);

        bmp.UnlockBits(bmpData);
        return bmp;
    }
}