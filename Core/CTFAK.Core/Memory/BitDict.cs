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
        private String[] Keys;
        public UInt32 flag { get; set; }

        public BitDict(String[] keys) => Keys = keys;
        public bool this[String key]
        {
            get => GetFlag(key);
            set => SetFlag(key, value);
        }

        public bool GetFlag(String key)
        {
            Int32 pos = Array.IndexOf(Keys, key);
            if (pos >= 0)
            {
                return (flag & ((UInt32)Math.Pow(2, pos))) != 0;
            }
            return false;
        }

        public void SetFlag(String key, bool value)
        {
            if (value)
            {
                flag |= (uint)Math.Pow(2, Array.IndexOf(Keys, key));
            }
            else
            {
                flag &= (uint)Math.Pow(~2, Array.IndexOf(Keys, key));
            }
        }

        public static string ToDebugString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            return string.Join(";\n", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray());
            // return string.Join("\n", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray());
        }

        public override string ToString()
        {
            Dictionary<String, bool> actualKeys = new Dictionary<string, bool>();
            foreach (var key in Keys)
            {
                actualKeys[key] = this[key];
            }
            return ToDebugString(actualKeys);
        }
    }
}