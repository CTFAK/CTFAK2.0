using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;
using CTFAK.MMFParser.MFA;
using CTFAK.MMFParser.MMFUtils;
using CTFAK.Utils;

namespace CTFAK.FileReaders;

public class MFAFileReader : IFileReader
{
    public GameData Game;
    public MFAData MFA;
    public string Name => "MFA";
    public int Priority => 5;

    private ByteReader _reader;

    public GameData GetGameData()
    {
        return Game;
    }

    public virtual bool LoadGame(string gamePath)
    {
        _reader = new ByteReader(gamePath, FileMode.Open);
        MFA = new MFAData();
        Settings.isMFA = true;
        MFA.Read(_reader);
        Settings.isMFA = false;
        Game = Mfa2Pame.Convert(MFA);
        return true;
    }

    public Dictionary<int, Bitmap> GetIcons()
    {
        return new Dictionary<int, Bitmap>();
    }

    public void Close()
    {
        _reader.Close();
        _reader.Dispose();
    }

    public ByteReader GetFileReader()
    {
        return _reader;
    }

    public void PatchMethods()
    {
        Settings.gameType = Settings.GameType.NORMAL;
    }
}