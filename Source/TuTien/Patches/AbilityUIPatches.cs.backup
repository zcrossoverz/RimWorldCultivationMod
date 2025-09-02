using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace TuTien.Patches
{
    /// <summary>
    /// Adds CompAbilityUser to all humanlike pawns
    /// </summary>
    [HarmonyPatch(typeof(ThingDef), "PostLoad")]
    public static class ThingDef_PostLoad_Patch
    {
        public static void Postfix(ThingDef __instance)
        {
            // Add both CompAbilityUser and CultivationComp to humanlike races
            if (__instance.race?.Humanlike == true)
            {
                if (__instance.comps == null)
                    __instance.comps = new List<CompProperties>();

                // Add CompAbilityUser if not exists
                if (!__instance.comps.Any(c => c.compClass == typeof(TuTien.Abilities.CompAbilityUser)))
                {
                    __instance.comps.Add(new TuTien.Abilities.CompAbilityUserProperties());
                }

                // Add CultivationComp if not exists  
                if (!__instance.comps.Any(c => c.compClass == typeof(TuTien.CultivationComp)))
                {
                    __instance.comps.Add(new TuTien.CultivationCompProperties());
                }
            }
        }
    }

    /// <summary>
    /// Add ability gizmos to pawn UI
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_AbilityPatch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            // Return original gizmos first
            foreach (var gizmo in gizmos)
            {
                yield return gizmo;
            }

            // Add cultivation ability gizmos
            var abilityComp = __instance.GetComp<TuTien.Abilities.CompAbilityUser>();
            if (abilityComp?.Abilities != null)
            {
                foreach (var ability in abilityComp.Abilities)
                {
                    yield return ability.GetCommand();
                }
            }
        }
    }
}
