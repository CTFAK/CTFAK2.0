using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.Utils
{
    class Settings
    {
        public static int Build;
        public static bool Unicode;
        public static bool Old;
        public enum GameType
        {
            NORMAL,
            MMF2,
            MMF15,
            CNC,
            ANDROID,
            UNKNOWN
        }
        public static object DumpPath { get; internal set; }
    }
}
