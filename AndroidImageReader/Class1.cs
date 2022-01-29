using CTFAK.CCN;
using CTFAK.FileReaders;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.IO;

namespace AndroidReader
{
    public class AndroidReaderMain : IFileReader
    {
        public string Name => "Android";
        public GameData game;
        string gameName;

        public static Harmony harmonyInstance;
        public GameData getGameData()
        {
            game.targetFilename = $"{gameName}-dump";
            return game;
        }

        public Dictionary<int, System.Drawing.Bitmap> getIcons()
        {
            return new Dictionary<int, System.Drawing.Bitmap>();
        }

        public void LoadGame(string gamePath)
        {
            gameName = Path.GetFileNameWithoutExtension(gamePath);
            var reader = new ByteReader(gamePath, System.IO.FileMode.Open);
            game = new GameData();
            game.Read(reader);
        }

        public void PatchMethods()
        {
            harmonyInstance = new Harmony("AndroidReader");
            harmonyInstance.PatchAll();
        }

        public int ReadHeader(ByteReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
