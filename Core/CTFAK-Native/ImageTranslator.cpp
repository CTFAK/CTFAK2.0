#include <Windows.h>
#include <stdio.h>
#include <math.h>
#include <cstdint>
#define DllExport  __declspec(dllexport) 
int GetPadding(int width, int pointSize, int bytes = 2)
{
    int pad = bytes - ((width * pointSize) % bytes);
    if (pad == bytes)
    {
        return 0;
    }

    return ceil((double)((float)pad / (float)pointSize));
}
extern "C" {
	DllExport void ReadPoint(char* result, int width, int height, int alpha,int size, char* imageData,int transparentColor)
	{

            int stride = width * 4;
            int pad = GetPadding(width, 3);
            int position = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int newPos = (y * stride) + (x * 4);
                    result[ newPos+ 0] = imageData[position];
                    result[newPos + 1] = imageData[position + 1];
                    result[newPos + 2] = imageData[position + 2];
                    result[newPos + 3] = 255;
                    if (!alpha)
                    {
                        char t1 = (transparentColor & 0x000000ff);
                        char t2 = (transparentColor & 0x0000ff00) >> 8;
                        char t3 = (transparentColor & 0x00ff0000) >> 16;
                        char t4 = (transparentColor & 0xff000000) >> 24;
                        char d1 = imageData[position];
                        char d2 = imageData[position + 1];
                        char d3 = imageData[position + 2];
                        if (t1 == d3 && t2 == d2 && d3 == d1) result[(y * stride) + (x * 4) + 3] = 0;
                    }
                    position += 3;
                }

                position += pad * 3;
            }
            if (alpha)
            {
                int alphaSize = size - position;
                alphaSize = size - alphaSize;

                int aPad = GetPadding(width, 1, 4);
                int aStride = width * 4;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        result[(y * aStride) + (x * 4) + 3] = imageData[alphaSize];
                        alphaSize += 1;
                    }
                    alphaSize += aPad;
                }
            }
	}
    DllExport void ReadPointBGRA(char* result, int width, int height, int alpha, int size, char* imageData, int transparentColor)
    {
        int stride = width * 4;
        int pad = GetPadding(width, 3);
        int position = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int newPos = (y * stride) + (x * 4);
                result[newPos + 0] = imageData[position + 2];
                result[newPos + 1] = imageData[position + 1];
                result[newPos + 2] = imageData[position + 0];
                result[newPos + 3] = 255;
                if (!alpha)
                {
                    char t1 = (transparentColor & 0x000000ff);
                    char t2 = (transparentColor & 0x0000ff00) >> 8;
                    char t3 = (transparentColor & 0x00ff0000) >> 16;
                    char t4 = (transparentColor & 0xff000000) >> 24;
                    char d1 = imageData[position + 2];
                    char d2 = imageData[position + 1];
                    char d3 = imageData[position];
                    if (t1 == d3 && t2 == d2 && d3 == d1) result[(y * stride) + (x * 4) + 3] = 0;
                }
                position += 3;
            }

            position += pad * 3;
        }
        if (alpha)
        {
            int alphaSize = size - position;
            alphaSize = size - alphaSize;

            int aPad = GetPadding(width, 1, 4);
            int aStride = width * 4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[(y * aStride) + (x * 4) + 3] = imageData[alphaSize];
                    alphaSize += 1;
                }
                alphaSize += aPad;
            }
        }
    }
    DllExport void ReadFifteen(char* result, int width, int height, int alpha, int size, char* imageData, int transparentColor)
    {

        int stride = width * 4;
        int pad = GetPadding(width, 2);
        int position = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                UINT16 newShort = (UINT16)(imageData[position] | imageData[position + 1] << 8);
                byte r = (byte)((newShort & 31744) >> 10);
                byte g = (byte)((newShort & 992) >> 5);
                byte b = (byte)((newShort & 31));

                r = (byte)(r << 3);
                g = (byte)(g << 3);
                b = (byte)(b << 3);
                result[(y * stride) + (x * 4) + 2] = r;
                result[(y * stride) + (x * 4) + 1] = g;
                result[(y * stride) + (x * 4) + 0] = b;
                result[(y * stride) + (x * 4) + 3] = 255;
                position += 2;
            }

            position += pad * 2;
        
        }

        if (alpha)
        {
            int alphaSize = size - position;
            alphaSize = size - alphaSize;

            int aPad = GetPadding(width, 1, 4);
            int aStride = width * 4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[(y * aStride) + (x * 4) + 3] = imageData[alphaSize];
                    alphaSize += 1;
                }
                alphaSize += aPad;
            }
        }


    }
    DllExport void ReadSixteen(char* result, int width, int height, int alpha, int size, char* imageData, int transparentColor)
    {

        int stride = width * 4;
        int pad = GetPadding(width, 2);
        int position = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                UINT16 newShort = (UINT16)(imageData[position] | imageData[position + 1] << 8);
                byte r = (byte)((newShort & 63488) >> 11);
                byte g = (byte)((newShort & 2016) >> 5);
                byte b = (byte)((newShort & 31));

                r = (byte)(r << 3);
                g = (byte)(g << 2);
                b = (byte)(b << 3);
                result[(y * stride) + (x * 4) + 2] = r;
                result[(y * stride) + (x * 4) + 1] = g;
                result[(y * stride) + (x * 4) + 0] = b;
                result[(y * stride) + (x * 4) + 3] = 255;
                position += 2;
            }

            position += pad * 2;
        }
        
        if (alpha)
        {
            int alphaSize = size - position;
            alphaSize = size - alphaSize;

            int aPad = GetPadding(width, 1, 4);
            int aStride = width * 4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[(y * aStride) + (x * 4) + 3] = imageData[alphaSize];
                    alphaSize += 1;
                }
                alphaSize += aPad;
            }
        }
    }
    DllExport void TranslateToRGBMasked(char* result, int width, int height, int alpha, int size, char* imageData, int transparentColor, int colorMode)
    {

    }
    DllExport void TranslateToRGBA(char* result, int width, int height, int alpha, int size, char* imageData, int transparentColor, int colorMode)
    {
        ReadPoint(result, width, height, alpha, size, imageData, transparentColor);
    }
    DllExport void TranslateToBGRA(char* result, int width, int height, int alpha, int size, char* imageData, int transparentColor, int colorMode)
    {
        ReadPointBGRA(result, width, height, alpha, size, imageData, transparentColor);
    }
}