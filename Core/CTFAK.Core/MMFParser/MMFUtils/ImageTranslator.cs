using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace CTFAK.MMFParser.Translation;

public static class ImageTranslator
{
    public static int GetPadding(int width, int pointSize, int bytes = 2, bool gaylord = false)
    {
        if (gaylord) return (bytes - width * pointSize % bytes) % bytes;

        var pad = bytes - width * pointSize % bytes;
        if (pad == bytes) return 0;

        return (int)Math.Ceiling(pad / (float)pointSize);
    }
    
    public static byte[] Normal24BitMaskedToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        byte[] colorArray = new byte[width * height * 4];
        int stride = width * 4;
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

        if (alpha)
        {
            int aPad = GetPadding(width, 1, 4);
            int aStride = width * 4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorArray[(y * aStride) + (x * 4) + 3] = imageData[position];
                    position += 1;
                }
                position += aPad;
            }
        }

        return colorArray;
    }
    public static byte[] Normal16BitToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        byte[] colorArray = new byte[width * height * 4];
        int stride = width * 4;
        int pad = GetPadding(width, 2);
        int position = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                UInt16 newShort = (ushort) (imageData[position] | imageData[position + 1] << 8);
                byte r = (byte) ((newShort & 63488) >> 11);
                byte g = (byte) ((newShort & 2016) >> 5);
                byte b = (byte) ((newShort & 31));

                r=(byte) (r << 3);
                g=(byte) (g << 2);
                b=(byte) (b << 3);
                colorArray[(y * stride) + (x * 4) + 2] = r;
                colorArray[(y * stride) + (x * 4) + 1] = g;
                colorArray[(y * stride) + (x * 4) + 0] = b;
                colorArray[(y * stride) + (x * 4) + 3] = 255;
                position += 2;
            }

            position += pad * 2;
        }
        if (alpha)
        {
            int aPad = GetPadding(width, 1, 4);
            int aStride = width * 4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorArray[(y * aStride) + (x * 4) + 3] = imageData[position];
                    position += 1;
                }
                position += aPad;
            }
        }

        return colorArray;
    }
    public static byte[] Normal15BitToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        byte[] colorArray = new byte[width * height * 4];
        int stride = width * 4;
        int pad = GetPadding(width, 2);
        int position = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                UInt16 newShort = (ushort) (imageData[position] | imageData[position + 1] << 8);
                byte r = (byte) ((newShort & 31744) >> 10);
                byte g = (byte) ((newShort & 992) >> 5);
                byte b = (byte) ((newShort & 31));

                r=(byte) (r << 3);
                g=(byte) (g << 3);
                b=(byte) (b << 3);
                colorArray[(y * stride) + (x * 4) + 2] = r;
                colorArray[(y * stride) + (x * 4) + 1] = g;
                colorArray[(y * stride) + (x * 4) + 0] = b;
                colorArray[(y * stride) + (x * 4) + 3] = 255;
                position += 2;
            }

            position += pad * 2;
        }
        if (alpha)
        {
            int aPad = GetPadding(width, 1, 4);
            int aStride = width * 4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorArray[(y * aStride) + (x * 4) + 3] = imageData[position];
                    position += 1;
                }
                position += aPad;
            }
        }
        
        return colorArray;

    }
    public static byte[] Normal8BitToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        return null;
    }
    public static byte[] AndroidMode0ToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        var colorArray = new byte[width * height * 4];
        var stride = width * 4;
        var position = 0;
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

        return colorArray;
    }
    public static byte[] AndroidMode1ToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        var colorArray = new byte[width * height * 4];
        var stride = width * 4;
        var position = 0;
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

        return colorArray;
    }
    public static byte[] AndroidMode2ToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        var colorArray = new byte[width * height * 4];
        var stride = width * 4;
        var position = 0;
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
        return colorArray;
    }
    public static byte[] AndroidMode3ToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        var colorArray = new byte[width * height * 4];
        var stride = width * 4;
        var position = 0;
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

        }
        return colorArray;
    }
    public static byte[] AndroidMode4ToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        var colorArray = new byte[width * height * 4];
        var stride = width * 4;
        var position = 0;
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
        return colorArray;
    }
    public static byte[] AndroidMode5ToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        return null;
    }
    public static byte[] TwoFivePlusToRGBA(byte[] imageData, int width, int height, bool alpha)
    {
        byte[] colorArray = new byte[width * height * 4];
        int stride = width * 4;
        int pad = GetPadding(width, 4);
        int position = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorArray[(y * stride) + (x * 4) + 0] = imageData[position];
                colorArray[(y * stride) + (x * 4) + 1] = imageData[position + 1];
                colorArray[(y * stride) + (x * 4) + 2] = imageData[position + 2];
                colorArray[(y * stride) + (x * 4) + 3] = imageData[position + 3];
                position += 4;
            }

            position += pad * 4;
        }

        return colorArray;
    }
    public static byte[] RGBAToRGBMasked(byte[] imageData, int width, int height, bool alpha)
    {
        byte[] colorArray = new byte[width * height * 6];
        int stride = width * 4;
        int pad = GetPadding(width, 3);
        int position = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorArray[position + 0] = imageData[(y * stride) + (x * 4)];
                colorArray[position + 1] = imageData[(y * stride) + (x * 4) + 1];
                colorArray[position + 2] = imageData[(y * stride) + (x * 4) + 2];
                colorArray[position + 3] = 255;
                position += 3;
            }

            position += pad * 3;
        }
        
        if (alpha)
        {
            int aPad = GetPadding(width, 1,4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorArray[position] = imageData[(y * stride) + (x * 4) + 3];
                    position += 1;
                }

                position += aPad;
            }
        }
        Array.Resize(ref colorArray,position);
        return colorArray;


    }

 
}
