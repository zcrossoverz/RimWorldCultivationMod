using HarmonyLib;
using RimWorld.Planet;

namespace TuTien.Patches
{
    /// <summary>
    /// Add QiRegenerationWorldComponent to world
    /// </summary>
    [HarmonyPatch(typeof(World), "ConstructComponents")]
    public static class World_ConstructComponents_Patch
    {
        public static void Postfix(World __instance)
        {
            __instance.components.Add(new TuTien.Abilities.QiRegenerationWorldComponent(__instance));
        }
    }
}
