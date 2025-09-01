using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien.Systems.TechniqueSynergy
{
    /// <summary>
    /// Manages technique combinations and synergy effects
    /// Techniques can work together to create powerful combo effects
    /// </summary>
    public static class TechniqueSynergyManager
    {
        private static Dictionary<string, TechniqueSynergyDef> _synergyCache;
        private static Dictionary<string, List<TechniqueSynergyDef>> _techniqueSynergies;

        static TechniqueSynergyManager()
        {
            _synergyCache = new Dictionary<string, TechniqueSynergyDef>();
            _techniqueSynergies = new Dictionary<string, List<TechniqueSynergyDef>>();
            InitializeSynergies();
        }

        #region Initialization

        /// <summary>Initialize synergy database from definitions</summary>
        public static void InitializeSynergies()
        {
            _synergyCache.Clear();
            _techniqueSynergies.Clear();

            foreach (var synergyDef in DefDatabase<TechniqueSynergyDef>.AllDefs)
            {
                _synergyCache[synergyDef.defName] = synergyDef;

                // Index by technique combinations
                var key = CreateSynergyKey(synergyDef.requiredTechniques);
                if (!_techniqueSynergies.ContainsKey(key))
                {
                    _techniqueSynergies[key] = new List<TechniqueSynergyDef>();
                }
                _techniqueSynergies[key].Add(synergyDef);

                Log.Message($"[TuTien Synergy] Registered synergy: {synergyDef.defName} for techniques: {string.Join(", ", synergyDef.requiredTechniques)}");
            }
        }

        private static string CreateSynergyKey(List<string> techniques)
        {
            return string.Join("|", techniques.OrderBy(t => t));
        }

        #endregion

        #region Synergy Detection

        /// <summary>
        /// Check if pawn has active synergy effects
        /// </summary>
        public static List<TechniqueSynergyDef> GetActiveSynergies(Pawn pawn)
        {
            var cultivationComp = pawn.GetComp<CultivationCompEnhanced>();
            if (cultivationComp == null) return new List<TechniqueSynergyDef>();

            var knownTechniques = GetKnownTechniqueNames(cultivationComp);
            var activeSynergies = new List<TechniqueSynergyDef>();

            foreach (var synergy in _synergyCache.Values)
            {
                if (HasRequiredTechniques(knownTechniques, synergy.requiredTechniques) &&
                    MeetsRealmRequirement(cultivationComp, synergy.minRealm) &&
                    MeetsMasteryRequirements(cultivationComp, synergy))
                {
                    activeSynergies.Add(synergy);
                }
            }

            return activeSynergies;
        }

        /// <summary>
        /// Check if pawn can potentially unlock a synergy
        /// </summary>
        public static List<TechniqueSynergyDef> GetPotentialSynergies(Pawn pawn)
        {
            var cultivationComp = pawn.GetComp<CultivationCompEnhanced>();
            if (cultivationComp == null) return new List<TechniqueSynergyDef>();

            var knownTechniques = GetKnownTechniqueNames(cultivationComp);
            var potentialSynergies = new List<TechniqueSynergyDef>();

            foreach (var synergy in _synergyCache.Values)
            {
                if (!HasRequiredTechniques(knownTechniques, synergy.requiredTechniques))
                {
                    // Check how many techniques they're missing
                    var missingCount = synergy.requiredTechniques.Count(req => !knownTechniques.Contains(req));
                    if (missingCount <= 2) // Show synergies they're close to unlocking
                    {
                        potentialSynergies.Add(synergy);
                    }
                }
            }

            return potentialSynergies;
        }

        private static List<string> GetKnownTechniqueNames(CultivationCompEnhanced comp)
        {
            if (comp.IsUsingEnhancedData)
            {
                return comp.EnhancedData.knownTechniques.Select(t => t.defName).ToList();
            }
            else
            {
                return comp.LegacyData.knownTechniques.Select(t => t.defName).ToList();
            }
        }

        private static bool HasRequiredTechniques(List<string> knownTechniques, List<string> requiredTechniques)
        {
            return requiredTechniques.All(req => knownTechniques.Contains(req));
        }

        private static bool MeetsRealmRequirement(CultivationCompEnhanced comp, CultivationRealm minRealm)
        {
            var currentRealm = comp.IsUsingEnhancedData ? comp.EnhancedData.currentRealm : comp.LegacyData.currentRealm;
            return currentRealm >= minRealm;
        }

        private static bool MeetsMasteryRequirements(CultivationCompEnhanced comp, TechniqueSynergyDef synergy)
        {
            // TODO: Implement mastery level checking when technique mastery system is complete
            return true;
        }

        #endregion

        #region Synergy Effects

        /// <summary>
        /// Apply synergy stat modifiers to cultivation data
        /// </summary>
        public static void ApplySynergyEffects(Pawn pawn, CultivationDataEnhanced data)
        {
            var activeSynergies = GetActiveSynergies(pawn);
            
            foreach (var synergy in activeSynergies)
            {
                ApplySynergyStatModifiers(data, synergy);
                
                if (synergy.triggersSpecialEffects)
                {
                    TriggerSpecialSynergyEffects(pawn, synergy);
                }
            }
        }

        private static void ApplySynergyStatModifiers(CultivationDataEnhanced data, TechniqueSynergyDef synergy)
        {
            // Apply cultivation efficiency bonus
            if (synergy.cultivationEfficiencyBonus > 0)
            {
                // This would integrate with the enhanced data calculation system
                Log.Message($"[TuTien Synergy] Applying {synergy.cultivationEfficiencyBonus:P0} cultivation bonus from {synergy.LabelCap}");
            }

            // Apply qi regeneration bonus
            if (synergy.qiRegenerationMultiplier > 1f)
            {
                Log.Message($"[TuTien Synergy] Applying {synergy.qiRegenerationMultiplier:F1}x qi regen from {synergy.LabelCap}");
            }

            // Apply breakthrough chance bonus
            if (synergy.breakthroughChanceBonus > 0)
            {
                Log.Message($"[TuTien Synergy] Applying {synergy.breakthroughChanceBonus:P0} breakthrough bonus from {synergy.LabelCap}");
            }
        }

        private static void TriggerSpecialSynergyEffects(Pawn pawn, TechniqueSynergyDef synergy)
        {
            // Special synergy effects (auras, periodic healing, etc.)
            switch (synergy.defName)
            {
                case "ElementalMastery":
                    ApplyElementalMasteryEffect(pawn);
                    break;
                case "BodySoulHarmony":
                    ApplyBodySoulHarmonyEffect(pawn);
                    break;
                case "VoidWalker":
                    ApplyVoidWalkerEffect(pawn);
                    break;
            }
        }

        #endregion

        #region Special Synergy Effects

        private static void ApplyElementalMasteryEffect(Pawn pawn)
        {
            // Elemental techniques work together - damage resistance and elemental attacks
            if (Rand.Chance(0.1f)) // 10% chance per tick
            {
                if (pawn.Map != null)
                {
                    FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.DustPuffThick, 1f);
                }
            }
        }

        private static void ApplyBodySoulHarmonyEffect(Pawn pawn)
        {
            // Perfect balance between body and soul cultivation - enhanced regeneration
            if (Rand.Chance(0.05f)) // 5% chance per tick
            {
                // Small healing over time
                var injuries = new List<Hediff_Injury>();
                pawn.health.hediffSet.GetHediffs(ref injuries, h => h is Hediff_Injury injury && !injury.IsPermanent());
                
                if (injuries.Any())
                {
                    var injury = injuries.RandomElement();
                    injury.Heal(0.1f);
                    if (pawn.Map != null)
                    {
                        FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.Heart, 0.8f);
                    }
                }
            }
        }

        private static void ApplyVoidWalkerEffect(Pawn pawn)
        {
            // Master of space and time techniques - occasional teleportation ability
            if (Rand.Chance(0.01f)) // 1% chance per tick
            {
                // Just visual effect for now - can add hediff later when we have proper defs
                if (pawn.Map != null)
                {
                    FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastAreaEffect, 1f);
                }
            }
        }

        #endregion

        #region Synergy Discovery

        /// <summary>
        /// Check if pawn just unlocked a new synergy
        /// </summary>
        public static void CheckForNewSynergies(Pawn pawn, string newlyLearnedTechnique)
        {
            var previousSynergies = GetActiveSynergies(pawn);
            
            // Simulate learning the new technique (this would be called after actually learning it)
            var newSynergies = GetActiveSynergies(pawn);
            
            var unlockedSynergies = newSynergies.Except(previousSynergies).ToList();
            
            foreach (var synergy in unlockedSynergies)
            {
                NotifySynergyUnlocked(pawn, synergy);
                CultivationEvents.TriggerTechniqueMasteryAdvanced(pawn, null, TechniqueMasteryLevel.None, TechniqueMasteryLevel.Beginner);
            }
        }

        private static void NotifySynergyUnlocked(Pawn pawn, TechniqueSynergyDef synergy)
        {
            Messages.Message(
                $"{pawn.Name.ToStringShort} has unlocked the synergy: {synergy.LabelCap}!",
                pawn,
                MessageTypeDefOf.PositiveEvent,
                true
            );

            Log.Message($"[TuTien Synergy] {pawn.Name.ToStringShort} unlocked synergy: {synergy.LabelCap}");
        }

        #endregion

        #region Debugging

        /// <summary>Get synergy debug information for a pawn</summary>
        public static string GetSynergyDebugInfo(Pawn pawn)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Technique Synergy Debug for {pawn.Name.ToStringShort} ===");
            
            var activeSynergies = GetActiveSynergies(pawn);
            sb.AppendLine($"Active Synergies: {activeSynergies.Count}");
            foreach (var synergy in activeSynergies)
            {
                sb.AppendLine($"  • {synergy.LabelCap}: {string.Join(", ", synergy.requiredTechniques)}");
            }
            
            var potentialSynergies = GetPotentialSynergies(pawn);
            sb.AppendLine($"Potential Synergies: {potentialSynergies.Count}");
            foreach (var synergy in potentialSynergies)
            {
                sb.AppendLine($"  ○ {synergy.LabelCap}: {string.Join(", ", synergy.requiredTechniques)}");
            }
            
            return sb.ToString();
        }

        #endregion
    }
}
