using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using CTFAK.CCN;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.EXE
{
    public class CCNFileReader:IFileReader
    {
        public string Name => "CCN";
        public GameData game;
        public GameData getGameData()
        {
            return game;
        }

        public void LoadGame(string gamePath)
        {
            var reader = new ByteReader(gamePath, System.IO.FileMode.Open);

            if (reader.PeekInt32() == 2004318071)
                reader.Skip(32);

            game = new GameData();
            game.Read(reader);
        }

        public Dictionary<int, Bitmap> getIcons()
        {
            return ApkFileReader.androidIcons;
        }

        public void PatchMethods()
        {
            //Settings.gameType = Settings.GameType.ANDROID;
        }

        public IFileReader Copy()
        {
            CCNFileReader reader = new CCNFileReader();
            reader.game = game;
            return reader;
        }
    }
}