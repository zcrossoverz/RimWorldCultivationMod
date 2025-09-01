using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;

namespace TuTien.Patches
{
    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public static class StatWorker_GetValueUnfinalized_Patch
    {
        public static void Postfix(StatWorker __instance, StatRequest req, ref float __result)
        {
            if (req.Thing is Pawn pawn && req.Def != null)
            {
                var cultivationComp = pawn.GetComp<CultivationComp>();
                if (cultivationComp?.cultivationData == null) return;

                var stageStats = cultivationComp.cultivationData.GetCurrentStageStats();
                
                // Apply cultivation stat modifiers
                if (req.Def == StatDefOf.MoveSpeed)
                {
                    __result += stageStats.moveSpeedOffset;
                }
                else if (req.Def == StatDefOf.MeleeDamageFactor)
                {
                    __result *= stageStats.meleeDamageMultiplier;
                }
                else if (req.Def == StatDefOf.MaxHitPoints)
                {
                    __result *= stageStats.maxHpMultiplier;
                }
                else if (req.Def.defName == "HungerRateMultiplier")
                {
                    __result *= stageStats.hungerRateMultiplier;
                }
                else if (req.Def.defName == "RestRateMultiplier")
                {
                    __result *= stageStats.restRateMultiplier;
                }
            }
        }
    }
}
