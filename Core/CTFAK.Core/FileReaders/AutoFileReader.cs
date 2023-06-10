using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.FileReaders;

public class AutoFileReader : IFileReader
{
    public IFileReader RealReader;
    public int Priority => 1;
    public string Name => "Auto";

    public GameData GetGameData()
    {
        return RealReader.GetGameData();
    }

    public bool LoadGame(string gamePath)
    {
        switch (Path.GetExtension(gamePath))
        {
            case ".exe":
                RealReader = new ExeFileReader();
                break;
            case ".dat":
            case ".ccn":
                RealReader = new CCNFileReader();
                break;
            case ".apk":
                RealReader = new ApkFileReader();
                break;
            default:
                return false;
        }

        if (RealReader != null)
        {
            RealReader.LoadGame(gamePath);
            return true;
        }

        return false;
    }

    public Dictionary<int, Bitmap> GetIcons()
    {
        return RealReader.GetIcons();
    }

    public void Close()
    {
        RealReader.Close();
    }

    public ByteReader GetFileReader()
    {
        return RealReader.GetFileReader();
    }
}