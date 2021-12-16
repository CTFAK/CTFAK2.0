using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dumper
{
    public class Utils
    {
        public static string ClearName(string ogName)
        {
            var str = string.Join("_", ogName.Split(Path.GetInvalidFileNameChars()));
            str = str.Replace("?", "");
            return str;
        }
    }
}
