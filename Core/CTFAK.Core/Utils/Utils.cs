using System;
using System.Drawing;
using System.IO;
using CTFAK.Memory;

namespace CTFAK.Utils;

public static class Utils
{
    public static string ClearName(string ogName)
    {
        var str = string.Join("", ogName.Split(Path.GetInvalidFileNameChars()));
        str = str.Replace("?", "");
        return str;
    }


    public static byte[] GetBuffer(this ByteWriter writer)
    {
        var buf = ((MemoryStream)writer.BaseStream).GetBuffer();
        Array.Resize(ref buf, (int)writer.Size());
        return buf;
    }

    public static Bitmap ResizeImage(this Bitmap imgToResize, Size size)
    {
        return new Bitmap(imgToResize, size);
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        var pos = text.IndexOf(search, StringComparison.Ordinal);
        if (pos < 0) return text;

        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    public static string GetHex(this byte[] data, int count = -1, int position = 0)
    {
        var actualCount = count;
        if (actualCount == -1) actualCount = data.Length;
        var temp = "";
        for (var i = 0; i < actualCount; i++)
        {
            temp += data[i].ToString("X2");
            temp += " ";
        }

        return temp;
    }

    public static T[] To1DArray<T>(T[,] input)
    {
        // Step 1: get total size of 2D array, and allocate 1D array.
        var size = input.Length;
        var result = new T[size];

        // Step 2: copy 2D array elements into a 1D array.
        var write = 0;
        for (var i = 0; i <= input.GetUpperBound(0); i++)
        for (var z = 0; z <= input.GetUpperBound(1); z++)
            result[write++] = input[i, z];

        // Step 3: return the new array.
        return result;
    }

    public static string ToPrettySize(this int value)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        var order = 0;
        double len = value;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
// show a single decimal place, and no space.
        var result = string.Format("{0:0.##} {1}", len, sizes[order]);
        return result;
    }
}