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
        
        [Flags]
        public enum GameType
        {
            NORMAL=1,
            MMF2=2,
            MMF15=4,
            CNC=8,
            ANDROID=16,
            TWOFIVEPLUS=32,
            UNKNOWN=64
        }
        public static object DumpPath { get; internal set; }
    }
}
