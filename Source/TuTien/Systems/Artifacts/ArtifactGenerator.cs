using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using TuTien.Systems.Registry;

namespace TuTien.Systems.Artifacts
{
    /// <summary>
    /// ✅ Task 2.2: Artifact generation system
    /// Creates artifacts with ELO-based stats and appropriate buffs
    /// Central factory for all artifact creation
    /// </summary>
    public static class ArtifactGenerator
    {
        #region Core Generation
        /// <summary>
        /// Generate complete artifact data from definition
        /// </summary>
        public static CultivationArtifactData GenerateArtifact(CultivationArtifactDef def)
        {
            if (def == null)
            {
                Log.Error("[TuTien] Cannot generate artifact from null definition");
                return new CultivationArtifactData();
            }
            
            var data = new CultivationArtifactData(def);
            
            Log.Message($"[TuTien] Generated {def.rarity} {def.artifactType}: " +
                       $"ELO {data.eloRating:F0}, Damage {data.baseDamage:F1}, " +
                       $"Qi Pool {data.maxArtifactQi:F0}, Buffs {data.activeBuffs.Count}");
            
            return data;
        }
        
        /// <summary>
        /// Generate artifact with specific ELO (for testing/admin tools)
        /// </summary>
        public static CultivationArtifactData GenerateArtifactWithELO(CultivationArtifactDef def, float targetELO)
        {
            var data = new CultivationArtifactData(def);
            
            // Override ELO
            data.eloRating = targetELO;
            data.powerMultiplier = ELOToPowerMultiplier(targetELO);
            
            // Regenerate stats with new ELO
            RegenerateStatsWithELO(data, def, targetELO);
            
            Log.Message($"[TuTien] Generated custom ELO {targetELO:F0} {def.rarity} {def.artifactType}");
            
            return data;
        }
        
        /// <summary>
        /// Generate random artifact of specific rarity
        /// </summary>
        public static CultivationArtifactData GenerateRandomArtifact(ArtifactRarity rarity)
        {
            var availableArtifacts = CultivationRegistry.GetArtifactsByRarity(rarity);
            if (availableArtifacts.Count == 0)
            {
                Log.Warning($"[TuTien] No artifact definitions found for rarity {rarity}");
                return new CultivationArtifactData();
            }
            
            var randomDef = availableArtifacts.RandomElement();
            return GenerateArtifact(randomDef);
        }
        #endregion

        #region Stat Generation Helpers
        /// <summary>
        /// Regenerate all stats for an artifact with specific ELO
        /// </summary>
        private static void RegenerateStatsWithELO(CultivationArtifactData data, CultivationArtifactDef def, float elo)
        {
            float powerMultiplier = ELOToPowerMultiplier(elo);
            
            // Regenerate combat stats
            data.baseDamage = ScaleByELO(def.damageRange, powerMultiplier);
            data.range = ScaleByELO(def.rangeMultiplier, powerMultiplier) * 12f;
            data.accuracy = ScaleByELO(def.accuracyRange, powerMultiplier);
            data.cooldownTicks = (int)ScaleByELO(
                new FloatRange(def.cooldownRange.max, def.cooldownRange.min), powerMultiplier);
            data.criticalChance = Mathf.Clamp01(powerMultiplier * 0.1f);
            
            // Regenerate Qi stats
            data.maxArtifactQi = ScaleByELO(def.qiPoolRange, powerMultiplier);
            data.currentArtifactQi = data.maxArtifactQi;
            data.qiAbsorptionRate = def.qiAbsorptionRate * powerMultiplier;
            data.qiEfficiency = def.qiEfficiency / powerMultiplier;
            data.qiCostPerAttack = data.baseDamage * 0.2f;
            
            // Regenerate buffs
            data.activeBuffs = GenerateBuffsForArtifact(def, elo);
        }
        
        /// <summary>Convert ELO rating to power multiplier</summary>
        private static float ELOToPowerMultiplier(float elo)
        {
            return Mathf.Lerp(0.8f, 1.5f, (elo - 100f) / 1100f);
        }
        
        /// <summary>Scale a value range by ELO power multiplier</summary>
        private static float ScaleByELO(FloatRange range, float powerMultiplier)
        {
            float scalingFactor = Mathf.Pow(powerMultiplier, 1.2f);
            float normalizedScale = (scalingFactor - 0.8f) / (1.5f - 0.8f);
            normalizedScale = Mathf.Clamp01(normalizedScale);
            return Mathf.Lerp(range.min, range.max, normalizedScale);
        }
        #endregion

        #region Buff Generation
        /// <summary>
        /// Generate appropriate buffs for an artifact based on type and ELO
        /// </summary>
        public static List<ArtifactBuff> GenerateBuffsForArtifact(CultivationArtifactDef def, float elo)
        {
            var buffs = new List<ArtifactBuff>();
            
            // Determine number of buffs based on ELO and rarity
            int baseBuffCount = GetBaseBuffCount(def.rarity);
            int eloBonus = Mathf.FloorToInt((elo - 100f) / 200f); // +1 buff per 200 ELO
            int totalBuffCount = Mathf.Clamp(baseBuffCount + eloBonus, 1, 8);
            
            // Generate buffs by priority
            buffs.AddRange(GeneratePrimaryBuffs(def, elo, totalBuffCount / 2 + 1));
            
            int remainingSlots = totalBuffCount - buffs.Count;
            if (remainingSlots > 0)
            {
                buffs.AddRange(GenerateSecondaryBuffs(def, elo, remainingSlots));
            }
            
            return buffs;
        }
        
        /// <summary>Get base buff count by rarity</summary>
        private static int GetBaseBuffCount(ArtifactRarity rarity)
        {
            return rarity switch
            {
                ArtifactRarity.Common => 1,
                ArtifactRarity.Uncommon => 2,
                ArtifactRarity.Rare => 3,
                ArtifactRarity.Epic => 4,
                ArtifactRarity.Legendary => 5,
                ArtifactRarity.Immortal => 6,
                _ => 1
            };
        }
        
        /// <summary>Generate primary buffs based on artifact type</summary>
        private static List<ArtifactBuff> GeneratePrimaryBuffs(CultivationArtifactDef def, float elo, int count)
        {
            var buffs = new List<ArtifactBuff>();
            var availableBuffs = GetPrimaryBuffTypesForArtifactType(def.artifactType);
            
            for (int i = 0; i < count && i < availableBuffs.Count; i++)
            {
                var buffType = availableBuffs[i];
                var buff = CreateBuffByType(buffType, elo);
                if (buff != null)
                {
                    buffs.Add(buff);
                }
            }
            
            return buffs;
        }
        
        /// <summary>Generate secondary buffs from general pool</summary>
        private static List<ArtifactBuff> GenerateSecondaryBuffs(CultivationArtifactDef def, float elo, int count)
        {
            var buffs = new List<ArtifactBuff>();
            var availableBuffs = GetSecondaryBuffTypes();
            var random = new System.Random();
            
            for (int i = 0; i < count; i++)
            {
                var buffType = availableBuffs[random.Next(availableBuffs.Count)];
                var buff = CreateBuffByType(buffType, elo);
                if (buff != null && !buffs.Any(b => b.buffType == buff.buffType))
                {
                    buffs.Add(buff);
                }
            }
            
            return buffs;
        }
        
        /// <summary>Get primary buff types for artifact type</summary>
        private static List<ArtifactBuffType> GetPrimaryBuffTypesForArtifactType(ArtifactType type)
        {
            return type switch
            {
                ArtifactType.Sword => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.DamageMultiplier,
                    ArtifactBuffType.CriticalChanceBonus,
                    ArtifactBuffType.AttackSpeedMultiplier
                },
                ArtifactType.Spear => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.DamageMultiplier,
                    ArtifactBuffType.ArmorPenetrationBonus,
                    ArtifactBuffType.CriticalDamageMultiplier
                },
                ArtifactType.Bow => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.DamageMultiplier,
                    ArtifactBuffType.CriticalChanceBonus,
                    ArtifactBuffType.SenseRangeBonus
                },
                ArtifactType.Staff => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.QiRegenMultiplier,
                    ArtifactBuffType.MaxQiMultiplier,
                    ArtifactBuffType.ElementalResistance
                },
                ArtifactType.Crown => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.QiRegenMultiplier,
                    ArtifactBuffType.CultivationSpeedBonus,
                    ArtifactBuffType.ExperienceMultiplier
                },
                ArtifactType.Robe => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.ArmorRatingBonus,
                    ArtifactBuffType.MaxQiMultiplier,
                    ArtifactBuffType.DamageReduction
                },
                ArtifactType.Boots => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.MovementSpeedBonus,
                    ArtifactBuffType.DodgeChanceBonus,
                    ArtifactBuffType.WorkSpeedMultiplier
                },
                ArtifactType.Shield => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.ArmorRatingBonus,
                    ArtifactBuffType.DamageReduction,
                    ArtifactBuffType.DodgeChanceBonus
                },
                ArtifactType.Talisman => new List<ArtifactBuffType>
                {
                    ArtifactBuffType.LuckyEventBonus,
                    ArtifactBuffType.LeadershipAura,
                    ArtifactBuffType.CultivationAura
                },
                _ => new List<ArtifactBuffType> { ArtifactBuffType.DamageMultiplier }
            };
        }
        
        /// <summary>Get secondary buff types (general pool)</summary>
        private static List<ArtifactBuffType> GetSecondaryBuffTypes()
        {
            return new List<ArtifactBuffType>
            {
                ArtifactBuffType.MovementSpeedBonus,
                ArtifactBuffType.WorkSpeedMultiplier,
                ArtifactBuffType.CarryCapacityBonus,
                ArtifactBuffType.SocialImpactBonus,
                ArtifactBuffType.StatusResistance,
                ArtifactBuffType.ImmunityChance,
                ArtifactBuffType.HealingAura
            };
        }
        
        /// <summary>Create a buff of specific type scaled by ELO</summary>
        private static ArtifactBuff CreateBuffByType(ArtifactBuffType type, float elo)
        {
            return type switch
            {
                ArtifactBuffType.DamageMultiplier => ArtifactBuff.CreateDamageMultiplier(elo),
                ArtifactBuffType.QiRegenMultiplier => ArtifactBuff.CreateQiRegenMultiplier(elo),
                ArtifactBuffType.MovementSpeedBonus => ArtifactBuff.CreateMovementSpeed(elo),
                ArtifactBuffType.CriticalChanceBonus => ArtifactBuff.CreateCriticalChance(elo),
                ArtifactBuffType.ArmorRatingBonus => ArtifactBuff.CreateArmorRating(elo),
                // TODO: Add more buff creation methods as needed
                _ => CreateGenericBuff(type, elo)
            };
        }
        
        /// <summary>Create a generic buff for types without specific factory methods</summary>
        private static ArtifactBuff CreateGenericBuff(ArtifactBuffType type, float elo)
        {
            float magnitude = 1f + (elo / 1000f) * 0.5f; // Generic scaling
            string description = $"{type} +{(magnitude - 1f) * 100f:F0}%";
            return new ArtifactBuff(type, magnitude, description);
        }
        #endregion

        #region Quality & Testing
        /// <summary>
        /// Generate test artifact for debugging
        /// </summary>
        public static CultivationArtifactData GenerateTestArtifact(ArtifactType type, ArtifactRarity rarity)
        {
            var artifacts = CultivationRegistry.GetArtifactsByType(type)
                                              .Where(a => a.rarity == rarity)
                                              .ToList();
            
            if (artifacts.Count == 0)
            {
                Log.Warning($"[TuTien] No {rarity} {type} artifacts found for testing");
                return new CultivationArtifactData();
            }
            
            return GenerateArtifact(artifacts.First());
        }
        
        /// <summary>
        /// Validate generated artifact data
        /// </summary>
        public static bool ValidateArtifactData(CultivationArtifactData data)
        {
            if (data.eloRating < 50f || data.eloRating > 1500f)
            {
                Log.Warning($"[TuTien] Invalid ELO rating: {data.eloRating}");
                return false;
            }
            
            if (data.baseDamage <= 0f || data.maxArtifactQi <= 0f)
            {
                Log.Warning("[TuTien] Invalid artifact stats: damage or Qi pool is zero or negative");
                return false;
            }
            
            return true;
        }
        #endregion
    }
}
