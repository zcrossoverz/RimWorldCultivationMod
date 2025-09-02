using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.AI;

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
    /// ✅ UNIFIED GIZMO PATCH - Combines both Cultivation Skills and Abilities
    /// Replaces separate patches to avoid Harmony conflicts between two systems
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_UnifiedPatch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            // Return original gizmos first
            foreach (var gizmo in gizmos)
            {
                yield return gizmo;
            }

            // Only add for colonist pawns
            if (!__instance.IsColonistPlayerControlled)
                yield break;

            // ═══ CULTIVATION ABILITIES (CompAbilityUser) - Khôi Lỗi Thuật ═══
            var abilityComp = __instance.GetComp<TuTien.Abilities.CompAbilityUser>();
            if (abilityComp?.Abilities != null)
            {
                foreach (var ability in abilityComp.Abilities)
                {
                    yield return ability.GetCommand();
                }
            }

            // ═══ CULTIVATION SKILLS (CultivationSkillWorker) - Qi Punch, Shield, etc ═══
            var cultivationComp = __instance.GetComp<CultivationComp>();
            if (cultivationComp?.cultivationData != null)
            {
                foreach (var gizmo in GetCultivationGizmos(__instance, cultivationComp.cultivationData))
                {
                    yield return gizmo;
                }
            }

            // ═══ CULTIVATION ARTIFACTS - Equipped Pháp Bảo Skills ═══
            if (__instance.equipment?.AllEquipmentListForReading != null)
            {
                foreach (var equipment in __instance.equipment.AllEquipmentListForReading)
                {
                    var artifactComp = equipment.GetComp<TuTien.Systems.Artifacts.CultivationArtifactComp>();
                    if (artifactComp?.Props?.artifactDef?.autoSkills != null && artifactComp.Props.artifactDef.autoSkills.Count > 0)
                    {
                        foreach (var skillDefName in artifactComp.Props.artifactDef.autoSkills)
                        {
                            // Try CultivationSkillDef first (QiPunch, QiShield, etc.)
                            var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillDefName);
                            if (skillDef != null)
                            {
                                bool canUse = artifactComp.ArtifactData.currentArtifactQi >= skillDef.qiCost;
                                
                                yield return new Command_Action
                                {
                                    defaultLabel = $"⚔ {skillDef.LabelCap}",
                                    defaultDesc = $"Artifact Skill from {equipment.LabelCap}\n{skillDef.description ?? "Cultivation artifact ability"}\n\nArtifact Qi: {artifactComp.ArtifactData.currentArtifactQi:F0}/{artifactComp.ArtifactData.maxArtifactQi:F0}",
                                    icon = TexCommand.DesirePower,
                                    Disabled = !canUse,
                                    disabledReason = !canUse ? "Artifact has insufficient Qi" : null,
                                    action = () =>
                                    {
                                        // Consume artifact Qi
                                        artifactComp.ArtifactData.currentArtifactQi -= skillDef.qiCost;
                                        
                                        // Execute CultivationSkillWorker
                                        var cultivationData = __instance.GetComp<CultivationComp>()?.cultivationData;
                                        if (cultivationData != null)
                                        {
                                            cultivationData.UseSkill(skillDef);
                                            Messages.Message($"{__instance.Name.ToStringShort} uses {skillDef.LabelCap} from {equipment.LabelCap}!", 
                                                MessageTypeDefOf.PositiveEvent);
                                        }
                                    }
                                };
                            }
                            else
                            {
                                // Try CultivationAbilityDef (Ability_SwordStrike, etc.)
                                var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(skillDefName);
                                if (abilityDef != null)
                                {
                                    bool canUse = artifactComp.ArtifactData.currentArtifactQi >= abilityDef.qiCost;
                                    
                                    yield return new Command_Action
                                    {
                                        defaultLabel = $"🗡 {abilityDef.abilityLabel ?? abilityDef.label}",
                                        defaultDesc = $"Artifact Ability from {equipment.LabelCap}\n{abilityDef.abilityDescription ?? abilityDef.description ?? "Cultivation artifact ability"}\n\nArtifact Qi: {artifactComp.ArtifactData.currentArtifactQi:F0}/{artifactComp.ArtifactData.maxArtifactQi:F0}",
                                        icon = TexCommand.DesirePower,
                                        Disabled = !canUse,
                                        disabledReason = !canUse ? "Artifact has insufficient Qi" : null,
                                        action = () =>
                                        {
                                            // Consume artifact Qi
                                            artifactComp.ArtifactData.currentArtifactQi -= abilityDef.qiCost;
                                            
                                            // Execute CultivationAbility
                                            var abilityComp = __instance.GetComp<TuTien.Abilities.CompAbilityUser>();
                                            var ability = abilityComp?.Abilities?.FirstOrDefault(a => a.def.defName == skillDefName);
                                            if (ability != null)
                                            {
                                                // For targeted abilities, use current position as target
                                                LocalTargetInfo target = __instance;
                                                if (abilityDef.targetType != AbilityTargetType.Self)
                                                {
                                                    // Find nearest enemy for combat abilities
                                                    var nearestEnemy = __instance.Map?.mapPawns?.AllPawnsSpawned?
                                                        .Where(p => p.HostileTo(__instance) && p.Position.DistanceTo(__instance.Position) <= abilityDef.range)
                                                        .OrderBy(p => p.Position.DistanceTo(__instance.Position))
                                                        .FirstOrDefault();
                                                    
                                                    target = nearestEnemy != null ? new LocalTargetInfo(nearestEnemy) : new LocalTargetInfo(__instance);
                                                }
                                                
                                                ability.TryCast(target);
                                                Messages.Message($"{__instance.Name.ToStringShort} uses {abilityDef.abilityLabel} from {equipment.LabelCap}!", 
                                                    MessageTypeDefOf.PositiveEvent);
                                            }
                                        }
                                    };
                                }
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<Gizmo> GetCultivationGizmos(Pawn pawn, CultivationData data)
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
            bool canBreakthrough = data.CanBreakthrough();
            if (canBreakthrough)
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

            // Active skill gizmos - Qi Punch, Qi Shield, etc
            foreach (var skill in data.unlockedSkills.Where(s => s.isActive))
            {
                bool canUse = data.CanUseSkill(skill);
                float cooldownPct = 0f;

                // Calculate cooldown percentage for overlay
                if (data.skillCooldowns.TryGetValue(skill.defName, out int cooldownTicks))
                {
                    if (cooldownTicks > 0)
                    {
                        int maxCooldownTicks = Mathf.RoundToInt(skill.cooldownHours * GenDate.TicksPerHour);
                        cooldownPct = (float)cooldownTicks / (maxCooldownTicks > 0 ? maxCooldownTicks : 1);
                        cooldownPct = Mathf.Clamp01(cooldownPct);
                    }
                }

                // Enhanced description with cooldown info
                string description = skill.description ?? "";
                if (cooldownTicks > 0)
                {
                    float hoursLeft = cooldownTicks / (float)GenDate.TicksPerHour;
                    if (hoursLeft >= 1f)
                    {
                        description += $"\nCooldown: {hoursLeft:F1}h remaining";
                    }
                    else
                    {
                        float minutesLeft = cooldownTicks * 60f / GenDate.TicksPerHour; // 3600 ticks = 1 minute
                        description += $"\nCooldown: {minutesLeft:F0}m remaining";
                    }
                }
                else
                {
                    description += $"\nQi Cost: {skill.qiCost}";
                }

                var gizmo = new TuTien.UI.Command_CultivationSkill
                {
                    defaultLabel = skill.LabelCap,
                    defaultDesc = description,
                    icon = TexCommand.DesirePower,
                    Disabled = !canUse,
                    disabledReason = !canUse ? (data.currentQi < skill.qiCost ? "Not enough Qi" : "On cooldown") : null,
                    action = () => data.UseSkill(skill),
                    cooldownPct = cooldownPct,
                    cooldownTicksRemaining = cooldownTicks
                };

                yield return gizmo;
            }

            // Passive skill gizmos (info only - no click)
            foreach (var skill in data.unlockedSkills.Where(s => !s.isActive))
            {
                yield return new Command_Action
                {
                    defaultLabel = $"🛡 {skill.LabelCap}",
                    defaultDesc = $"Passive Skill - Always Active\n{skill.description ?? "Passive enhancement"}",
                    icon = TexCommand.ForbidOff, // Use shield-like icon
                    Disabled = true, // Always disabled since it's passive
                    disabledReason = "Passive skill - always active",
                    action = () => { } // No action for passive skills
                };
            }
        }
    }
    
    /// <summary>
    /// Add cultivation tab to humanlike pawns
    /// </summary>
    [HarmonyPatch(typeof(Thing), "GetInspectTabs")]
    public static class Thing_GetInspectTabs_Patch
    {
        public static void Postfix(Thing __instance, ref IEnumerable<InspectTabBase> __result)
        {
            if (__instance is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                var comp = pawn.GetComp<CultivationComp>();
                if (comp != null)
                {
                    var tabs = __result.ToList();
                    if (!tabs.Any(tab => tab is ITab_Cultivation))
                    {
                        tabs.Add((InspectTabBase)InspectTabManager.GetSharedInstance(typeof(ITab_Cultivation)));
                    }
                    __result = tabs;
                }
            }
        }
    }
}