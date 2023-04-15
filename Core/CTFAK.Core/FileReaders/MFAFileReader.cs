using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using CTFAK.CCN;
using CTFAK.Core.MFA;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.Utils;

namespace CTFAK.EXE
{
    public class MFAFileReader : IFileReader
    {
        public string Name => "MFA";
        public GameData game;
        public MFAData mfa;
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
            mfa = new MFAData();
            Settings.isMFA = true;
            mfa.Read(reader);
            Settings.isMFA = false;
            game = MFA2Pame.ConvertMFA2Pame(mfa);
        }

        public Dictionary<int, Bitmap> getIcons()
        {
            return new Dictionary<int, Bitmap>();
        }

        public void PatchMethods()
        {
            Settings.gameType = Settings.GameType.NORMAL;
        }

        public IFileReader Copy()
        {
            MFAFileReader reader = new MFAFileReader();
            reader.game = game;
            reader.mfa = mfa;
            return reader;
        }
    }
}