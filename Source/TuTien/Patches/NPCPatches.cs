using HarmonyLib;
using Verse;
using RimWorld;

namespace TuTien.Patches
{
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new System.Type[] { typeof(PawnGenerationRequest) })]
    public static class PawnGenerator_GeneratePawn_Patch
    {
        public static void Postfix(Pawn __result, PawnGenerationRequest request)
        {
            if (__result?.RaceProps?.Humanlike == true)
            {
                var comp = __result.GetComp<CultivationComp>();
                if (comp?.cultivationData != null)
                {
                    // Initialize random cultivation level for NPCs
                    if (!request.ForceGenerateNewPawn && __result.Faction != Faction.OfPlayer)
                    {
                        InitializeNPCCultivation(__result, comp.cultivationData);
                    }
                }
            }
        }

        private static void InitializeNPCCultivation(Pawn pawn, CultivationData data)
        {
            // Random cultivation level based on pawn type
            float chance = 0.1f;
            
            if (pawn.kindDef.combatPower > 50f) chance = 0.3f;
            if (pawn.kindDef.combatPower > 100f) chance = 0.5f;
            if (pawn.kindDef.combatPower > 200f) chance = 0.8f;

            if (Rand.Chance(chance))
            {
                // Advance to random realm/stage
                var targetRealm = (CultivationRealm)Rand.RangeInclusive(1, 2); // Qi Condensation or Foundation
                int targetStage = Rand.RangeInclusive(1, 3);
                
                data.currentRealm = targetRealm;
                data.currentStage = targetStage;
                data.UpdateStatsForRealm();

                // Grant techniques based on talent
                CultivationUtility.SpawnWithRandomTechniques(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        public static void Prefix(Pawn __instance, DamageInfo? dinfo)
        {
            var data = __instance.GetCultivationData();
            if (data?.knownTechniques?.Count > 0 && __instance.Faction?.HostileTo(Faction.OfPlayer) == true)
            {
                // Chance to drop technique manual on death
                if (Rand.Chance(0.3f))
                {
                    var technique = data.knownTechniques.RandomElement();
                    // Could spawn a technique manual item here
                    Messages.Message($"{__instance.Name.ToStringShort} possessed the {technique.labelKey?.Translate() ?? technique.defName}!", 
                        MessageTypeDefOf.NeutralEvent);
                }
            }
        }
    }
}
