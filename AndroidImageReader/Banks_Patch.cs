using CTFAK.CCN.Chunks.Banks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidImageReader
{
    [HarmonyPatch(typeof(FontBank), nameof(FontBank.Read), new Type[0])]
    class FontBank_Patch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
    [HarmonyPatch(typeof(SoundBank), nameof(SoundBank.Read), new Type[0])]
    class SoundBank_Patch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
    [HarmonyPatch(typeof(MusicBank), nameof(MusicBank.Read),new Type[0])]
    class MusicBank_Patch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}

