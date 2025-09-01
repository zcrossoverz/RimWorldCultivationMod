using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using TuTien.Systems.Registry;

namespace TuTien.Systems.Artifacts
{
    /// <summary>
    /// ✅ Task 1.3: Core artifact data structure with ELO-based stats
    /// Stores runtime artifact state including generated stats, buffs, and Qi pool
    /// </summary>
    public class CultivationArtifactData : IExposable
    {
        #region Generated Stats (ELO-based)
        /// <summary>Hidden ELO rating determining overall power level</summary>
        public float eloRating = 100f;
        
        /// <summary>Power multiplier derived from ELO (0.8x - 1.5x)</summary>
        public float powerMultiplier = 1f;
        
        /// <summary>Generated base damage</summary>
        public float baseDamage = 20f;
        
        /// <summary>Generated attack range</summary>
        public float range = 12f;
        
        /// <summary>Generated accuracy (0-1)</summary>
        public float accuracy = 0.8f;
        
        /// <summary>Generated cooldown in ticks</summary>
        public int cooldownTicks = 180;
        
        /// <summary>Generated critical hit chance</summary>
        public float criticalChance = 0.1f;
        #endregion

        #region Qi Management
        /// <summary>Maximum Qi this artifact can store</summary>
        public float maxArtifactQi = 100f;
        
        /// <summary>Current Qi stored in artifact</summary>
        public float currentArtifactQi = 100f;
        
        /// <summary>Rate of Qi absorption from wearer per tick</summary>
        public float qiAbsorptionRate = 1f;
        
        /// <summary>Qi efficiency multiplier (lower = more efficient)</summary>
        public float qiEfficiency = 1f;
        
        /// <summary>Qi cost per basic attack</summary>
        public float qiCostPerAttack = 5f;
        #endregion

        #region Artifact State
        /// <summary>Pawn currently wearing this artifact</summary>
        public Pawn equippedBy;
        
        /// <summary>Whether artifact is currently active and functional</summary>
        public bool isActive = true;
        
        /// <summary>Last tick when Qi absorption occurred</summary>
        public int lastAbsorptionTick = -1;
        
        /// <summary>Last tick when artifact was updated</summary>
        public int lastUpdateTick = -1;
        
        /// <summary>Reference to the artifact definition</summary>
        public CultivationArtifactDef artifactDef;
        #endregion

        #region Buff System
        /// <summary>Active buffs provided by this artifact</summary>
        public List<ArtifactBuff> activeBuffs = new List<ArtifactBuff>();
        
        /// <summary>Skill cooldowns for auto-cast abilities</summary>
        public Dictionary<string, int> skillCooldowns = new Dictionary<string, int>();
        #endregion

        #region Properties
        /// <summary>Get Qi percentage (0-1)</summary>
        public float QiPercentage => maxArtifactQi > 0 ? currentArtifactQi / maxArtifactQi : 0f;
        
        /// <summary>Check if artifact has enough Qi for an ability</summary>
        public bool HasEnoughQi(float cost) => currentArtifactQi >= cost;
        
        /// <summary>Check if artifact is effectively active</summary>
        public bool IsEffectivelyActive => isActive && equippedBy != null && QiPercentage > 0.1f;
        
        /// <summary>Get effective damage including all modifiers</summary>
        public float EffectiveDamage 
        { 
            get 
            { 
                float damage = baseDamage * powerMultiplier;
                // Apply buff modifiers
                foreach (var buff in activeBuffs)
                {
                    if (buff.buffType == ArtifactBuffType.DamageMultiplier)
                        damage *= buff.magnitude;
                }
                return damage;
            } 
        }
        #endregion

        #region Constructor
        public CultivationArtifactData()
        {
            // Default constructor for save/load
        }
        
        public CultivationArtifactData(CultivationArtifactDef def)
        {
            artifactDef = def;
            GenerateStats(def);
        }
        #endregion

        #region Stat Generation
        /// <summary>
        /// ✅ Task 1.3: Generate artifact stats based on definition and ELO
        /// </summary>
        public void GenerateStats(CultivationArtifactDef def)
        {
            // Generate ELO within rarity range
            eloRating = GenerateELO(def.rarity);
            powerMultiplier = ELOToPowerMultiplier(eloRating);
            
            // Generate core stats scaled by ELO
            baseDamage = ScaleByELO(def.damageRange, powerMultiplier);
            range = ScaleByELO(def.rangeMultiplier, powerMultiplier) * 12f; // Base 12 tile range
            accuracy = ScaleByELO(def.accuracyRange, powerMultiplier);
            cooldownTicks = (int)ScaleByELO(new FloatRange(def.cooldownRange.max, def.cooldownRange.min), powerMultiplier);
            criticalChance = Mathf.Clamp01(powerMultiplier * 0.1f);
            
            // Generate Qi stats
            maxArtifactQi = ScaleByELO(def.qiPoolRange, powerMultiplier);
            currentArtifactQi = maxArtifactQi;
            qiAbsorptionRate = def.qiAbsorptionRate * powerMultiplier;
            qiEfficiency = def.qiEfficiency / powerMultiplier; // Better ELO = more efficient
            qiCostPerAttack = baseDamage * 0.2f; // Scale with damage
            
            // Generate buffs
            activeBuffs = GenerateBuffs(def, eloRating);
            
            Log.Message($"[TuTien] Generated artifact: ELO {eloRating:F0}, Power {powerMultiplier:F2}x, " +
                       $"Damage {baseDamage:F1}, Qi Pool {maxArtifactQi:F0}, Buffs {activeBuffs.Count}");
        }
        
        private static float GenerateELO(ArtifactRarity rarity)
        {
            var ranges = ArtifactELORanges.ELORanges[rarity];
            var random = new System.Random();
            
            // Bell curve approximation using two random values
            float roll1 = (float)random.NextDouble();
            float roll2 = (float)random.NextDouble();
            float normalizedRoll = (roll1 + roll2) / 2f; // Simple bell curve approximation
            
            return Mathf.Lerp(ranges.min, ranges.max, normalizedRoll);
        }
        
        private static float ELOToPowerMultiplier(float elo)
        {
            // Smooth curve: ELO 100 = 0.8x, ELO 1200 = 1.5x
            return Mathf.Lerp(0.8f, 1.5f, (elo - 100f) / 1100f);
        }
        
        private static float ScaleByELO(FloatRange range, float powerMultiplier)
        {
            // Non-linear scaling for interesting progression
            float scalingFactor = Mathf.Pow(powerMultiplier, 1.2f);
            float normalizedScale = (scalingFactor - 0.8f) / (1.5f - 0.8f); // Normalize to 0-1
            return Mathf.Lerp(range.min, range.max, normalizedScale);
        }
        
        private List<ArtifactBuff> GenerateBuffs(CultivationArtifactDef def, float elo)
        {
            var buffs = new List<ArtifactBuff>();
            int buffCount = Mathf.RoundToInt(elo / 150f) + def.buffCountRange.min;
            buffCount = Mathf.Clamp(buffCount, def.buffCountRange.min, def.buffCountRange.max + 3);
            
            // TODO: Implement buff generation - will be completed in Phase 2
            Log.Message($"[TuTien] Would generate {buffCount} buffs for artifact (placeholder)");
            
            return buffs;
        }
        #endregion

        #region Qi Management Methods
        /// <summary>Try to consume Qi for an ability</summary>
        public bool TryConsumeQi(float amount)
        {
            if (currentArtifactQi >= amount)
            {
                currentArtifactQi = Mathf.Max(0f, currentArtifactQi - amount);
                return true;
            }
            return false;
        }
        
        /// <summary>Add Qi to artifact pool</summary>
        public void AddQi(float amount)
        {
            currentArtifactQi = Mathf.Min(maxArtifactQi, currentArtifactQi + amount);
        }
        
        /// <summary>Try to absorb Qi from equipped pawn</summary>
        public bool TryAbsorbQiFromPawn(float amount)
        {
            if (equippedBy?.GetComp<TuTien.CultivationComp>()?.cultivationData != null)
            {
                var cultivationData = equippedBy.GetComp<TuTien.CultivationComp>().cultivationData;
                if (cultivationData.currentQi >= amount)
                {
                    cultivationData.currentQi -= amount;
                    AddQi(amount * qiEfficiency);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Save/Load
        public void ExposeData()
        {
            Scribe_Values.Look(ref eloRating, "eloRating");
            Scribe_Values.Look(ref powerMultiplier, "powerMultiplier");
            Scribe_Values.Look(ref baseDamage, "baseDamage");
            Scribe_Values.Look(ref range, "range");
            Scribe_Values.Look(ref accuracy, "accuracy");
            Scribe_Values.Look(ref cooldownTicks, "cooldownTicks");
            Scribe_Values.Look(ref criticalChance, "criticalChance");
            
            Scribe_Values.Look(ref maxArtifactQi, "maxArtifactQi");
            Scribe_Values.Look(ref currentArtifactQi, "currentArtifactQi");
            Scribe_Values.Look(ref qiAbsorptionRate, "qiAbsorptionRate");
            Scribe_Values.Look(ref qiEfficiency, "qiEfficiency");
            Scribe_Values.Look(ref qiCostPerAttack, "qiCostPerAttack");
            
            Scribe_References.Look(ref equippedBy, "equippedBy");
            Scribe_Values.Look(ref isActive, "isActive");
            Scribe_Values.Look(ref lastAbsorptionTick, "lastAbsorptionTick");
            Scribe_Values.Look(ref lastUpdateTick, "lastUpdateTick");
            
            Scribe_Defs.Look(ref artifactDef, "artifactDef");
            Scribe_Collections.Look(ref activeBuffs, "activeBuffs", LookMode.Deep);
            Scribe_Collections.Look(ref skillCooldowns, "skillCooldowns", LookMode.Value, LookMode.Value);
        }
        #endregion
    }

    /// <summary>
    /// ✅ Task 1.3: ELO rating ranges by rarity
    /// </summary>
    public static class ArtifactELORanges
    {
        public static Dictionary<ArtifactRarity, FloatRange> ELORanges = new Dictionary<ArtifactRarity, FloatRange>
        {
            { ArtifactRarity.Common,    new FloatRange(100f, 200f) },   // 0.8x - 1.1x power
            { ArtifactRarity.Uncommon,  new FloatRange(180f, 320f) },   // 0.9x - 1.2x power
            { ArtifactRarity.Rare,      new FloatRange(280f, 480f) },   // 1.0x - 1.3x power
            { ArtifactRarity.Epic,      new FloatRange(420f, 680f) },   // 1.1x - 1.4x power
            { ArtifactRarity.Legendary, new FloatRange(600f, 900f) },   // 1.2x - 1.45x power
            { ArtifactRarity.Immortal,  new FloatRange(850f, 1200f) }   // 1.35x - 1.5x power
        };
    }
}
