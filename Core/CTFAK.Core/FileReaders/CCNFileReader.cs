using System.Collections.Generic;
using System.Drawing;
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

        public int ReadHeader(ByteReader reader)
        {
            throw new System.NotImplementedException();
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
            return new Dictionary<int, Bitmap>();
        }

        public void PatchMethods()
        {
            Settings.gameType = Settings.GameType.ANDROID;
        }
    }
}