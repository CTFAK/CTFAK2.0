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
                foreach (string TheFile in Directory.GetFiles(Path.GetTempPath() + "CTFAK\\AndroidSounds"))
                    File.Delete(TheFile);
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
                    else if (Path.GetExtension(entry.Name) == ".mp3" || Path.GetExtension(entry.Name) == ".ogg" || Path.GetExtension(entry.Name) == ".wav")
                    {
                        entry.ExtractToFile(Path.GetTempPath() + "CTFAK\\AndroidSounds\\" + entry.Name);
                        var sound = File.Open(Path.GetTempPath() + "CTFAK\\AndroidSounds\\" + entry.Name, FileMode.Open);
                        Stream soundBytes = entry.Open();
                        SoundItem Sound = new SoundItem();
                        Sound.AndroidRead(new ByteReader(soundBytes), entry.Name);
                        androidSoundBank.Items.Add(Sound);
                    }
                }
                try
                {
                    foreach (string TheFile in Directory.GetFiles(Path.GetTempPath() + "CTFAK\\AndroidSounds"))
                        File.Delete(TheFile);
                }
                catch { }
            }
            if (File.Exists(Path.GetTempPath() + "application.ccn"))
                return Path.GetTempPath() + "application.ccn";
            else
                return apkPath;
        }
    }
}
