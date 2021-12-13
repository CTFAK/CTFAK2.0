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
	DllExport void ConvertImage(char* result, int width, int height, int alpha,int size, char* imageData,int transparentColor)
	{

            int stride = width * 4;
            int pad = GetPadding(width, 3);
            int position = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    result[(y * stride) + (x * 4) + 0] = imageData[position];
                    result[(y * stride) + (x * 4) + 1] = imageData[position + 1];
                    result[(y * stride) + (x * 4) + 2] = imageData[position + 2];
                    result[(y * stride) + (x * 4) + 3] = 255;
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
}