using CTFAK.CCN;
using CTFAK.CCN.Chunks.Banks;
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
        public static SoundBank androidSoundBank = new();

        public static string ExtractCCN(string apkPath)
        {
            Settings.gameType = Settings.GameType.ANDROID;
            try
            {
                File.Delete(Path.GetTempPath() + "application.ccn");
            }
            catch { }
            Directory.CreateDirectory(Path.GetTempPath() + "CTFAK\\AndroidSounds");
            using (ZipArchive archive = ZipFile.OpenRead(apkPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name == "application.ccn")
                    {
                        entry.ExtractToFile(Path.GetTempPath() + "application.ccn");
                    }
                    else if (Path.GetExtension(entry.Name) == ".mp3" || 
                        Path.GetExtension(entry.Name) == ".ogg" || 
                        Path.GetExtension(entry.Name) == ".wav")
                    {
                        Stream soundBytes = entry.Open();
                        SoundItem Sound = new SoundItem();
                        Sound.AndroidRead(new ByteReader(soundBytes), entry.Name);
                        androidSoundBank.Items.Add(Sound);
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
