using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.Utils
{
    public class Settings
    {
        [Flags]
        public enum GameType:byte
        {
            NORMAL = 0b00000001,
            MMF2 = 0b00000010,
            MMF15 = 0b00000100,
            ANDROID = 0b00001000,
            TWOFIVEPLUS = 0b00010000,
            F3 = 0b00100000,
            IOS = 0b01000000,
            UNKNOWN = 0b00000000
        }

        public static int Build;
        public static byte BuildType;
        public static bool Unicode;
        public static bool isMFA;
        public static bool Fusion3Seed;
        public static GameType gameType;
        public static bool Old => gameType.HasFlag(GameType.MMF15);
        public static bool TwoFivePlus => gameType.HasFlag(GameType.TWOFIVEPLUS);
        public static bool Android => gameType.HasFlag(GameType.ANDROID);
        public static bool F3 => gameType.HasFlag(GameType.F3);
        public static bool iOS => gameType.HasFlag(GameType.IOS);
        public static bool Normal => gameType == GameType.NORMAL;
    }
}
