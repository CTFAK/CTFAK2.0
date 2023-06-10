using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.Common.Banks;
using CTFAK.Utils;

namespace CTFAK.FileReaders;

public class ApkFileReader : IFileReader
{
    public static SoundBank AndroidSoundBank = new();
    public CCNFileReader Ccn;
    public int Priority => 5;

    public string Name => "APK";

    public GameData GetGameData()
    {
        return Ccn.Game;
    }

    public bool LoadGame(string gamePath)
    {
        Settings.gameType = Settings.GameType.ANDROID;
        try
        {
            File.Delete(Path.GetTempPath() + "application.ccn");
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
                else if (Path.GetExtension(entry.Name) == ".mp3" ||
                         Path.GetExtension(entry.Name) == ".ogg" ||
                         Path.GetExtension(entry.Name) == ".wav")
                {
                    var soundBytes = entry.Open();
                    var soundItem = new SoundItem();
                    soundItem.AndroidRead(new ByteReader(soundBytes), entry.Name);
                    AndroidSoundBank.Items.Add(soundItem);
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

    public void Close()
    {
        Ccn.Close();
    }

    public ByteReader GetFileReader()
    {
        return Ccn.GetFileReader();
    }
}