using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.Utils
{
    public class Settings
    {
        public static int Build;
        public static bool Unicode;
        public static bool Old=>gameType==GameType.MMF15;
        public static bool twofiveplus=>gameType == GameType.TWOFIVEPLUS;
        public static bool android=>gameType == GameType.ANDROID;
        public static bool isMFA;
        public static GameType gameType;
        public enum GameType
        {
            NORMAL,
            MMF2,
            MMF15,
            CNC,
            ANDROID,
            TWOFIVEPLUS,
            UNKNOWN
        }
        public static object DumpPath { get; internal set; }
    }
}
