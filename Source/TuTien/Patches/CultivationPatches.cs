using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace TuTien.Patches
{
    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public static class Pawn_SpawnSetup_Patch
    {
        public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
        {
            if (__instance.RaceProps.Humanlike && __instance.GetComp<CultivationComp>() == null)
            {
                var comp = new CultivationComp();
                comp.parent = __instance;
                __instance.AllComps.Add(comp);
                comp.PostSpawnSetup(respawningAfterLoad);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard")]
    public static class CharacterCardUtility_DrawCharacterCard_Patch
    {
        public static void Postfix(Rect rect, Pawn pawn)
        {
            // Add cultivation tab - will be implemented in UI extension
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_Patch
    {
        public static void Postfix(Pawn __instance, ref System.Collections.Generic.IEnumerable<Gizmo> __result)
        {
            if (__instance.IsColonistPlayerControlled)
            {
                var comp = __instance.GetComp<CultivationComp>();
                if (comp?.cultivationData != null)
                {
                    var gizmos = __result.ToList();
                    gizmos.AddRange(GetCultivationGizmos(__instance, comp.cultivationData));
                    __result = gizmos;
                }
            }
        }

        private static System.Collections.Generic.IEnumerable<Gizmo> GetCultivationGizmos(Pawn pawn, CultivationData data)
        {
            // Cultivation gizmo
            yield return new Command_Action
            {
                defaultLabel = "Cultivate",
                defaultDesc = "Begin cultivation to gather Qi and advance cultivation.",
                icon = TexCommand.Draft,
                action = () =>
                {
                    // Start cultivation job - find nearest cultivation spot
                    var cultivationSpot = GenClosest.ClosestThingReachable(
                        pawn.Position, 
                        pawn.Map, 
                        ThingRequest.ForDef(TuTienDefOf.CultivationMeditationSpot), 
                        PathEndMode.InteractionCell, 
                        TraverseParms.For(pawn));

                    if (cultivationSpot != null)
                    {
                        var job = JobMaker.MakeJob(TuTienDefOf.MeditateCultivation, cultivationSpot);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }
                    else
                    {
                        Messages.Message("No cultivation spot available!", MessageTypeDefOf.RejectInput);
                    }
                }
            };

            // Breakthrough gizmo
            if (data.CanBreakthrough())
            {
                yield return new Command_Action
                {
                    defaultLabel = "Breakthrough",
                    defaultDesc = $"Attempt to break through to the next stage.\nSuccess depends heavily on talent level and current stage.",
                    icon = TexCommand.Attack,
                    action = () =>
                    {
                        data.AttemptBreakthrough();
                    }
                };
            }

            // Active skill gizmos
            foreach (var skill in data.unlockedSkills.Where(s => s.isActive))
            {
                bool canUse = data.CanUseSkill(skill);
                
                yield return new Command_Action
                {
                    defaultLabel = skill.labelKey?.Translate() ?? skill.defName,
                    defaultDesc = skill.descriptionKey?.Translate() ?? $"Qi Cost: {skill.qiCost}",
                    icon = TexCommand.DesirePower,
                    disabledReason = !canUse ? (data.currentQi < skill.qiCost ? "Not enough Qi" : "On cooldown") : null,
                    action = () => data.UseSkill(skill)
                };
            }
        }
    }

    [HarmonyPatch(typeof(DamageWorker_AddInjury), "Apply")]
    public static class DamageWorker_AddInjury_Apply_Patch
    {
        public static void Prefix(DamageWorker_AddInjury __instance, ref DamageInfo dinfo, Thing thing)
        {
            if (thing is Pawn pawn)
            {
                var comp = pawn.GetComp<CultivationComp>();
                if (comp?.cultivationData != null)
                {
                    // Check for active damage reduction effects
                    var qiShield = pawn.health.hediffSet.GetFirstHediffOfDef(TuTienDefOf.QiShieldHediff);
                    if (qiShield != null)
                    {
                        dinfo.SetAmount(dinfo.Amount * (1f - qiShield.Severity));
                    }

                    var qiBarrier = pawn.health.hediffSet.GetFirstHediffOfDef(TuTienDefOf.QiBarrierHediff);
                    if (qiBarrier != null && qiBarrier.Severity > 0)
                    {
                        qiBarrier.Severity -= 1f;
                        if (qiBarrier.Severity <= 0)
                            pawn.health.RemoveHediff(qiBarrier);
                        
                        dinfo.SetAmount(0f); // Block attack completely
                    }

                    // Gain cultivation points from combat
                    if (dinfo.Amount > 0)
                    {
                        comp.cultivationData.cultivationPoints += dinfo.Amount * 0.1f;
                    }
                }
            }
        }
    }
}
