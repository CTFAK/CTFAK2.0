using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.FileReaders;

public class CCNFileReader : IFileReader
{
    public GameData Game;
    public int Priority => 5;
    public string Name => "CCN";

    public GameData GetGameData()
    {
        return Game;
    }


    public virtual bool LoadGame(string gamePath)
    {
        var reader = new ByteReader(gamePath, FileMode.Open);
        Game = new GameData();
        Game.Read(reader);
        reader.Close();
        return true;
    }

    public Dictionary<int, Bitmap> GetIcons()
    {
        return new Dictionary<int, Bitmap>();
    }
}