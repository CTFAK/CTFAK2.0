using System.IO;

namespace Dumper
{
    public class Utils
    {
        public static string ClearName(string ogName)
        {
            string str;
            try
            {
                str = string.Join("", ogName.Split(Path.GetInvalidFileNameChars()));
                str = str.Replace("?", "");
            }
            catch
            {
                str = "CORRUPTED FRAME";
            }
            return str;
        }
    }
}
