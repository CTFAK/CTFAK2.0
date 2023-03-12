using System.Collections.Generic;
using System.Drawing;
using CTFAK.Core.MFA;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.MMFParser.CCN;
using CTFAK.Utils;

namespace CTFAK.EXE
{
    public class MFAFileReader : IFileReader
    {
        public string Name => "MFA";
        public int Priority => 5;
        public GameData Game;
        public MFAData MFA;
        public GameData GetGameData()
        {
            return Game;
        }

        public int ReadHeader(ByteReader reader)
        {
            throw new System.NotImplementedException();
        }

        public virtual bool LoadGame(string gamePath)
        {
            var reader = new ByteReader(gamePath, System.IO.FileMode.Open);
            MFA = new MFAData();
            Settings.isMFA = true;
            MFA.Read(reader);
            Settings.isMFA = false;
            Game = MFA2Pame.ConvertMFA2Pame(MFA);
            return true;
        }

        public Dictionary<int, Bitmap> GetIcons()
        {
            return new Dictionary<int, Bitmap>();
        }

        public void PatchMethods()
        {
            Settings.gameType = Settings.GameType.NORMAL;
        }
    }
}