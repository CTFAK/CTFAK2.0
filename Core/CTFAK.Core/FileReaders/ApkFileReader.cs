using CTFAK.CCN;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace CTFAK.FileReaders
{
    public class ApkFileReader
    {
        public static string ExtractCCN(string apkPath)
        {
            File.Delete(Path.GetTempPath() + "application.ccn");
            using (ZipArchive archive = ZipFile.OpenRead(apkPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name == "application.ccn")
                    {
                        entry.ExtractToFile(Path.GetTempPath() + "application.ccn");
                        break;
                    }
                }
            }
            if (File.Exists(Path.GetTempPath() + "application.ccn"))
                return Path.GetTempPath() + "application.ccn";
            else
                return apkPath;
        }
    }
}
