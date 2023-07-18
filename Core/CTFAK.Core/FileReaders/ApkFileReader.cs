using CTFAK.CCN;
using CTFAK.Core.CCN.Chunks.Banks.SoundBank;
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
        public static Dictionary<int, Bitmap> androidIcons = new();

        public static string ExtractCCN(string apkPath)
        {
            Settings.gameType = Settings.GameType.ANDROID;
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
                    else if (entry.FullName == "res/drawable-xhdpi/launcher.png")
                    {
                        Bitmap icon = new Bitmap(entry.Open());
                        androidIcons[16]  = icon.ResizeImage(16);
                        androidIcons[17]  = androidIcons[16];
                        androidIcons[32]  = icon.ResizeImage(32);
                        androidIcons[33]  = androidIcons[32];
                        androidIcons[48]  = icon.ResizeImage(48);
                        androidIcons[49]  = androidIcons[48];
                        androidIcons[128] = icon.ResizeImage(128);
                        androidIcons[256] = icon.ResizeImage(256);
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
