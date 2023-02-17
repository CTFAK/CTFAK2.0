using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.Shared.Banks;
using CTFAK.Utils;

namespace CTFAK.FileReaders;

public class ApkFileReader: IFileReader
{
    public int Priority => 5;
    public static SoundBank AndroidSoundBank = new();
    



    public string Name => "APK";
    public CCNFileReader Ccn;
    public GameData GetGameData()
    {
        return Ccn.Game;
    }

    public int ReadHeader(ByteReader reader)
    {
        return 0;
    }

    public bool LoadGame(string gamePath)
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
        using (var archive = ZipFile.OpenRead(gamePath))
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

        Ccn = new CCNFileReader();
        Ccn.LoadGame(Path.GetTempPath() + "application.ccn");
        return true;

    }

    public Dictionary<int, Bitmap> GetIcons()
    {
        return new Dictionary<int, Bitmap>();
    }


}