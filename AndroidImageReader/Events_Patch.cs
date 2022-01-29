using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidImageReader
{
    [HarmonyPatch(typeof(Events), nameof(Events.Read))]
    class Events_Patch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
    [HarmonyPatch(typeof(ObjectCommon), nameof(ObjectCommon.Read))]
    class OI_Patch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
