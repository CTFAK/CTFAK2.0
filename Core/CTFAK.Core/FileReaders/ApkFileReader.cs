using System.IO;
using System.IO.Compression;
using CTFAK.Memory;
using CTFAK.MMFParser.Shared.Banks;
using CTFAK.Utils;

namespace CTFAK.FileReaders;

public class ApkFileReader
{
    public static SoundBank AndroidSoundBank = new();

    public static string ExtractCcn(string apkPath)
    {
        Settings.gameType = Settings.GameType.ANDROID;
        try
        {
            File.Delete(Path.GetTempPath() + "application.ccn");
            foreach (var theFile in Directory.GetFiles(Path.GetTempPath() + "CTFAK\\AndroidSounds"))
                File.Delete(theFile);
        }
        catch
        {
            Logger.LogWarning("Error while unpacking the APK file");
        }

        Directory.CreateDirectory(Path.GetTempPath() + "CTFAK\\AndroidSounds");
        using (var archive = ZipFile.OpenRead(apkPath))
        {
            foreach (var entry in archive.Entries)
                if (entry.Name == "application.ccn")
                {
                    entry.ExtractToFile(Path.GetTempPath() + "application.ccn");
                }
                else if (Path.GetExtension(entry.Name) == ".mp3" || Path.GetExtension(entry.Name) == ".ogg" ||
                         Path.GetExtension(entry.Name) == ".wav")
                {
                    entry.ExtractToFile(Path.GetTempPath() + "CTFAK\\AndroidSounds\\" + entry.Name);
                    var sound = File.Open(Path.GetTempPath() + "CTFAK\\AndroidSounds\\" + entry.Name, FileMode.Open); // I don't know why this is not used. Yuni, you answer this
                    var soundBytes = entry.Open();
                    var soundItem = new SoundItem();
                    soundItem.AndroidRead(new ByteReader(soundBytes), entry.Name);
                    AndroidSoundBank.Items.Add(soundItem);
                }

            try
            {
                foreach (var theFile in Directory.GetFiles(Path.GetTempPath() + "CTFAK\\AndroidSounds"))
                    File.Delete(theFile);
            }
            catch
            {
                Logger.LogWarning("Error while doing cleanup after APK reading");
            }
        }

        if (File.Exists(Path.GetTempPath() + "application.ccn"))
            return Path.GetTempPath() + "application.ccn";
        return apkPath;
    }
}