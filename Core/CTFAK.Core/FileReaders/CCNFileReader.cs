using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.Memory;
using CTFAK.MMFParser.CCN;

namespace CTFAK.FileReaders;

public class CCNFileReader : IFileReader
{
    public GameData Game;
    public string Name => "CCN";

    public GameData GetGameData()
    {
        return Game;
    }

    public int ReadHeader(ByteReader reader)
    {
        throw new NotImplementedException();
    }

    public void LoadGame(string gamePath)
    {
        var reader = new ByteReader(gamePath, FileMode.Open);
        Game = new GameData();
        Game.Read(reader);
    }

    public Dictionary<int, Bitmap> GetIcons()
    {
        return new Dictionary<int, Bitmap>();
    }

    public void PatchMethods()
    {
        //Settings.gameType = Settings.GameType.ANDROID;
    }
}