using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.Utils
{
    public static class Utils
    {
        public static byte[] GetBuffer(this ByteWriter writer)
        {
            var buf = ((MemoryStream)writer.BaseStream).GetBuffer();
            Array.Resize(ref buf, (int)writer.Size());
            return buf;
        }
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
