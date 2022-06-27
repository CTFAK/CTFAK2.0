using CTFAK.CCN;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace CTFAK.FileReaders
{
    public class ApkFileReader : IFileReader
    {
        public string Name => "Android";
        public GameData game;
        public GameData getGameData()
        {
            return game;
        }

        public int ReadHeader(ByteReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void LoadGame(string apkPath)
        {
            File.Delete(Path.GetTempPath() + "application.ccn");
            using (ZipArchive archive = ZipFile.OpenRead(apkPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name == "application.ccn")
                    {
                        entry.ExtractToFile(Path.GetTempPath() + "application.ccn");
                        break;
                    }
                }
            }
            if (File.Exists(Path.GetTempPath() + "application.ccn"))
                ReadGame(Path.GetTempPath() + "application.ccn");
        }

        public Dictionary<int, Bitmap> getIcons()
        {
            return new Dictionary<int, Bitmap>();
        }

        public void PatchMethods()
        {

        }

        void ReadGame(string gamePath)
        {
            Settings.gameType = Settings.GameType.ANDROID;
            var reader = new ByteReader(gamePath, FileMode.Open);
            game = new GameData();
            game.Read(reader);
        }
    }
}
