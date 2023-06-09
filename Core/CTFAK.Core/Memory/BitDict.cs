using System;
using System.Collections.Generic;
using System.Linq;

namespace CTFAK.Memory;

public static class ByteFlag
{
    public static bool GetFlag(uint flagbyte, int pos)
    {
        var mask = (uint)(1 << pos);
        var result = flagbyte & mask;
        return result == mask;
    }
}

public class BitDict
{
    public readonly string[] Keys;

    public BitDict(string[] keys)
    {
        Keys = keys;
    }

    public uint Flag { get; set; }

    public bool this[string key]
    {
        get => GetFlag(key);
        set => SetFlag(key, value);
    }

    public bool GetFlag(string key)
    {
        var pos = Array.IndexOf(Keys, key);
        if (pos >= 0) return (Flag & (uint)Math.Pow(2, pos)) != 0;
        return false;
    }

    public void SetFlag(string key, bool value)
    {
        if (value)
            Flag |= (uint)Math.Pow(2, Array.IndexOf(Keys, key));
        else
            Flag &= ~(uint)Math.Pow(2, Array.IndexOf(Keys, key));
    }

    public static string ToDebugString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        return string.Join(";\n", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray());
        // return string.Join("\n", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray());
    }

    public override string ToString()
    {
        var actualKeys = new Dictionary<string, bool>();
        foreach (var key in Keys) actualKeys[key] = this[key];
        return ToDebugString(actualKeys);
    }
}