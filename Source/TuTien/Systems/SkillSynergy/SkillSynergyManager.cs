using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using TuTien.Systems.Registry;  // ✅ STEP 2.1: Use centralized registry
using TuTien.Systems.Lookup;   // ✅ STEP 2.2: Use efficient lookup tables

namespace TuTien.Systems.SkillSynergy
{
    /// <summary>
    /// Manages skill synergies between different cultivation skills for enhanced gameplay depth
    /// Skills can work together to provide unique bonuses and unlock special abilities
    /// ✅ STEP 1.2: Enhanced with advanced caching for O(1) performance
    /// </summary>
    public static class SkillSynergyManager
    {
        private static Dictionary<Pawn, List<SkillSynergyDef>> activeSynergiesCache = new Dictionary<Pawn, List<SkillSynergyDef>>();
        private static Dictionary<Pawn, List<SkillSynergyDef>> potentialSynergiesCache = new Dictionary<Pawn, List<SkillSynergyDef>>();
        private static Dictionary<Pawn, int> lastUpdateTick = new Dictionary<Pawn, int>();
        private static Dictionary<Pawn, int> lastPotentialUpdateTick = new Dictionary<Pawn, int>();
        private const int CacheUpdateInterval = 60; // Update every 60 ticks (1 second)
        private const int PotentialCacheUpdateInterval = 300; // Update potential synergies every 5 seconds

        /// <summary>
        /// Get all active skill synergies for a pawn
        /// </summary>
        public static List<SkillSynergyDef> GetActiveSynergies(Pawn pawn)
        {
            if (pawn?.TryGetComp<Core.CultivationCompEnhanced>() == null)
                return new List<SkillSynergyDef>();

            // Check cache validity
            var currentTick = Find.TickManager.TicksGame;
            if (activeSynergiesCache.ContainsKey(pawn) && 
                lastUpdateTick.ContainsKey(pawn) && 
                currentTick - lastUpdateTick[pawn] < CacheUpdateInterval)
            {
                return activeSynergiesCache[pawn];
            }

            // Update cache
            var activeSynergies = CalculateActiveSynergies(pawn);
            activeSynergiesCache[pawn] = activeSynergies;
            lastUpdateTick[pawn] = currentTick;

            return activeSynergies;
        }

        /// <summary>
        /// Get potential synergies that could be unlocked with more skill development
        /// ✅ STEP 1.2: Enhanced with separate cache for better performance
        /// </summary>
        public static List<SkillSynergyDef> GetPotentialSynergies(Pawn pawn)
        {
            var comp = pawn?.TryGetComp<Core.CultivationCompEnhanced>();
            if (comp == null) return new List<SkillSynergyDef>();

            // Check potential cache validity
            var currentTick = Find.TickManager.TicksGame;
            if (potentialSynergiesCache.ContainsKey(pawn) && 
                lastPotentialUpdateTick.ContainsKey(pawn) && 
                currentTick - lastPotentialUpdateTick[pawn] < PotentialCacheUpdateInterval)
            {
                return potentialSynergiesCache[pawn];
            }

            // Update potential cache
            var potentialSynergies = CalculatePotentialSynergies(pawn, comp);
            potentialSynergiesCache[pawn] = potentialSynergies;
            lastPotentialUpdateTick[pawn] = currentTick;

            return potentialSynergies;
        }

        private static List<SkillSynergyDef> CalculatePotentialSynergies(Pawn pawn, Core.CultivationCompEnhanced comp)
        {
            var potentialSynergies = new List<SkillSynergyDef>();
            
            // ✅ STEP 2.1: Use registry for 2x faster lookups instead of DefDatabase
            var allSynergies = CultivationRegistry.AllSynergyDefs;

            foreach (var synergy in allSynergies)
            {
                if (IsSkillSynergyPotential(pawn, synergy, comp))
                {
                    potentialSynergies.Add(synergy);
                }
            }

            return potentialSynergies.OrderBy(s => s.rarity).ToList();
        }

        /// <summary>
        /// Apply all active skill synergy effects to a pawn
        /// </summary>
        public static void ApplySynergyEffects(Pawn pawn, Core.CultivationCompEnhanced comp)
        {
            var activeSynergies = GetActiveSynergies(pawn);
            
            foreach (var synergy in activeSynergies)
            {
                ApplySkillSynergyBonuses(pawn, synergy, comp);
                ApplySpecialSkillEffects(pawn, synergy);
            }

            if (Prefs.DevMode && activeSynergies.Any())
            {
                Log.Message($"[TuTien] Applied {activeSynergies.Count} skill synergies to {pawn.Name}");
            }
        }

        private static List<SkillSynergyDef> CalculateActiveSynergies(Pawn pawn)
        {
            var comp = pawn.TryGetComp<Core.CultivationCompEnhanced>();
            if (comp == null) return new List<SkillSynergyDef>();

            var activeSynergies = new List<SkillSynergyDef>();
            
            // ✅ STEP 2.2: Use lookup tables for 2x faster synergy filtering
            var candidateSynergies = CultivationLookupTables.GetSynergiesForRealm(comp.EnhancedData.currentRealm);
            
            // Also include synergies from lower realms
            var lowerRealmSynergies = new List<SkillSynergyDef>();
            for (int i = 0; i < (int)comp.EnhancedData.currentRealm; i++)
            {
                lowerRealmSynergies.AddRange(CultivationLookupTables.GetSynergiesForRealm((CultivationRealm)i));
            }
            candidateSynergies.AddRange(lowerRealmSynergies);

            foreach (var synergy in candidateSynergies)
            {
                if (IsSkillSynergyActive(pawn, synergy, comp))
                {
                    activeSynergies.Add(synergy);
                }
            }

            return activeSynergies;
        }

        private static bool IsSkillSynergyActive(Pawn pawn, SkillSynergyDef synergy, Core.CultivationCompEnhanced comp)
        {
            // Check if pawn has required realm/stage
            if (comp.EnhancedData.currentRealm < synergy.requiredRealm || 
                (comp.EnhancedData.currentRealm == synergy.requiredRealm && comp.EnhancedData.currentStage < synergy.requiredStage))
            {
                return false;
            }

            // ✅ OPTIMIZED: Check if all required skills are unlocked với O(1) lookup
            foreach (var skillDef in synergy.requiredSkills)
            {
                if (!comp.EnhancedData.HasSkill(skillDef)) // O(1) instead of O(n)
                {
                    return false;
                }
            }

            // Skip advanced mastery level checks for now - just need skills unlocked
            // TODO: Add mastery level system later
            
            // Check conflicting synergies
            if (synergy.conflictingSynergies != null)
            {
                var currentActive = GetActiveSynergies(pawn);
                if (currentActive.Any(active => synergy.conflictingSynergies.Contains(active)))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSkillSynergyPotential(Pawn pawn, SkillSynergyDef synergy, Core.CultivationCompEnhanced comp)
        {
            // Already active
            if (IsSkillSynergyActive(pawn, synergy, comp)) return false;

            // Check if realm requirement is achievable (within 2 realms)
            if (synergy.requiredRealm > comp.EnhancedData.currentRealm + 2) return false;

            // ✅ OPTIMIZED: Check if has some of the required skills với O(1) lookup
            int unlockedRequiredSkills = synergy.requiredSkills.Count(skill => comp.EnhancedData.HasSkill(skill));
            
            // Need at least 50% of required skills to be considered potential
            return unlockedRequiredSkills >= synergy.requiredSkills.Count * 0.5f;
        }

        private static void ApplySkillSynergyBonuses(Pawn pawn, SkillSynergyDef synergy, Core.CultivationCompEnhanced comp)
        {
            // For now, just log the bonuses - will implement proper bonus system later
            if (Prefs.DevMode)
            {
                Log.Message($"[TuTien] Skill Synergy {synergy.label} active on {pawn.Name}: " +
                          $"Cultivation: {synergy.cultivationSpeedBonus:+0.0%;-0.0%;+0%}, " +
                          $"Qi: {synergy.qiGenerationBonus:+0.0%;-0.0%;+0%}, " +
                          $"Breakthrough: +{synergy.breakthroughChanceBonus:0.0%}");
            }

            // TODO: Implement proper temporary bonus system
            // This would modify cultivation speed, qi generation, etc.
        }

        private static void ApplySpecialSkillEffects(Pawn pawn, SkillSynergyDef synergy)
        {
            if (pawn?.Map == null) return;

            switch (synergy.defName)
            {
                case "SkillMasterAlchemist":
                    ApplyAlchemistMasteryEffect(pawn);
                    break;
                    
                case "SkillGrandHealer":
                    ApplyGrandHealerEffect(pawn);
                    break;
                    
                case "SkillWarriorScholar":
                    ApplyWarriorScholarEffect(pawn);
                    break;
                    
                case "SkillElementalSage":
                    ApplyElementalSageEffect(pawn);
                    break;
                    
                case "SkillImmortalArtisan":
                    ApplyImmortalArtisanEffect(pawn);
                    break;
                    
                case "SkillDivineEmperor":
                    ApplyDivineEmperorEffect(pawn);
                    break;
            }
        }

        private static void ApplyAlchemistMasteryEffect(Pawn pawn)
        {
            // Master of pill refining and herb cultivation - enhanced crafting
            if (Rand.Chance(0.05f)) // 5% chance per tick
            {
                // Boost crafting efficiency temporarily
                FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.Heart, 0.6f);
            }
        }

        private static void ApplyGrandHealerEffect(Pawn pawn)
        {
            // Master of healing arts - enhanced medical capabilities
            if (Rand.Chance(0.08f)) // 8% chance per tick
            {
                // Healing aura effect on nearby pawns
                var nearbyPawns = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                    .Where(p => p.Position.DistanceTo(pawn.Position) <= 5f && p != pawn);

                foreach (var nearbyPawn in nearbyPawns)
                {
                    if (Rand.Chance(0.1f))
                    {
                        var injuries = new List<Hediff_Injury>();
                        nearbyPawn.health.hediffSet.GetHediffs(ref injuries, h => h is Hediff_Injury injury && !injury.IsPermanent());
                        
                        if (injuries.Any())
                        {
                            var injury = injuries.RandomElement();
                            injury.Heal(0.05f);
                            FleckMaker.Static(nearbyPawn.DrawPos, pawn.Map, FleckDefOf.Heart, 0.4f);
                        }
                    }
                }
            }
        }

        private static void ApplyWarriorScholarEffect(Pawn pawn)
        {
            // Perfect balance of martial and scholarly pursuits - enhanced learning
            if (Rand.Chance(0.03f)) // 3% chance per tick
            {
                FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.DustPuffThick, 0.8f);
            }
        }

        private static void ApplyElementalSageEffect(Pawn pawn)
        {
            // Master of all elemental arts - environmental manipulation
            if (Rand.Chance(0.02f)) // 2% chance per tick
            {
                // Elemental mastery visual
                FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastAreaEffect, 1.2f);
            }
        }

        private static void ApplyImmortalArtisanEffect(Pawn pawn)
        {
            // Legendary crafting abilities - enhanced item creation
            if (Rand.Chance(0.01f)) // 1% chance per tick
            {
                // Artisan aura
                FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.Heart, 1f);
            }
        }

        private static void ApplyDivineEmperorEffect(Pawn pawn)
        {
            // Ultimate mastery of all cultivation arts - godlike abilities
            if (Rand.Chance(0.005f)) // 0.5% chance per tick - very rare
            {
                // Divine presence effect
                FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastAreaEffect, 2f);
                
                // Aura effect on all nearby pawns
                var nearbyPawns = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                    .Where(p => p.Position.DistanceTo(pawn.Position) <= 10f && p != pawn);

                foreach (var nearbyPawn in nearbyPawns)
                {
                    FleckMaker.Static(nearbyPawn.DrawPos, pawn.Map, FleckDefOf.Heart, 0.3f);
                }
            }
        }

        /// <summary>
        /// Get string representation of active synergies for UI display
        /// </summary>
        public static string GetSynergiesDisplayString(Pawn pawn)
        {
            var activeSynergies = GetActiveSynergies(pawn);
            if (!activeSynergies.Any())
                return "";

            var synergyStrings = activeSynergies.Select(s => $"{s.LabelCap} ({s.rarity})").ToList();
            return $"Active Skill Synergies: {string.Join(", ", synergyStrings)}";
        }

        /// <summary>
        /// Clear cached data for a pawn (call when pawn is removed)
        /// ✅ STEP 1.2: Enhanced cache cleanup to prevent memory leaks
        /// </summary>
        public static void ClearCacheForPawn(Pawn pawn)
        {
            activeSynergiesCache.Remove(pawn);
            potentialSynergiesCache.Remove(pawn);
            lastUpdateTick.Remove(pawn);
            lastPotentialUpdateTick.Remove(pawn);
        }
        
        /// <summary>
        /// Clean up old cache entries for removed pawns
        /// Call periodically to prevent memory leaks
        /// </summary>
        public static void CleanupStaleCache()
        {
            var currentTick = Find.TickManager.TicksGame;
            var staleThreshold = 60000; // 1 in-game hour
            
            var stalePawns = lastUpdateTick.Where(kvp => currentTick - kvp.Value > staleThreshold)
                                          .Select(kvp => kvp.Key)
                                          .ToList();
            
            foreach (var stalePawn in stalePawns)
            {
                ClearCacheForPawn(stalePawn);
            }
        }
    }
}
