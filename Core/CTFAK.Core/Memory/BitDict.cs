using CTFAK.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CTFAK.Memory
{
    public static class ByteFlag
    {
        public static bool GetFlag(UInt32 flagbyte, int pos)
        {
            UInt32 mask = (uint)(1 << pos);
            UInt32 result = flagbyte & mask;
            return result == mask;
        }
    }
    public class BitDict
    {
        public string[] Keys;
        public uint flag { get; set; }

        public BitDict(string[] keys) => Keys = keys;
        public bool this[string key]
        {
            get => GetFlag(key);
            set => SetFlag(key, value);
        }

        public bool GetFlag(string key)
        {
            int pos = Array.IndexOf(Keys, key);
            if (pos >= 0)
                return (flag & ((uint)Math.Pow(2, pos))) != 0;
            return false;
        }

        public void SetFlag(string key, bool value)
        {
            if (value)
                flag |= (uint)Math.Pow(2, Array.IndexOf(Keys, key));
            else
                flag &= ~(uint)Math.Pow(2, Array.IndexOf(Keys, key));
        }


        public static string ToDebugString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            return string.Join(";\n", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray());
            // return string.Join("\n", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray());
        }

        public override string ToString()
        {
            Dictionary<string, bool> actualKeys = new Dictionary<string, bool>();
            foreach (var key in Keys)
            {
                actualKeys[key] = this[key];
            }
            return ToDebugString(actualKeys);
        }

    }
}