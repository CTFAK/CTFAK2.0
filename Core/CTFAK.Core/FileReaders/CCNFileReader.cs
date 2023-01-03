using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CTFAK.CCN;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.EXE;

public class CCNFileReader : IFileReader
{
    public GameData game;
    public string Name => "CCN";

    public GameData getGameData()
    {
        return game;
    }

    public int ReadHeader(ByteReader reader)
    {
        throw new NotImplementedException();
    }

    public void LoadGame(string gamePath)
    {
        var reader = new ByteReader(gamePath, FileMode.Open);
        game = new GameData();
        game.Read(reader);
    }

    public Dictionary<int, Bitmap> getIcons()
    {
        return new Dictionary<int, Bitmap>();
    }

    public void PatchMethods()
    {
        //Settings.gameType = Settings.GameType.ANDROID;
    }
}