using System;
using System.Collections.Generic;
using Verse;

namespace TuTien.Systems.Artifacts
{
    /// <summary>
    /// ✅ Task 1.3: Artifact buff system for ELO-scaled bonuses
    /// Represents individual buffs provided by cultivation artifacts
    /// </summary>
    public class ArtifactBuff : IExposable
    {
        #region Core Properties
        /// <summary>Type of buff provided</summary>
        public ArtifactBuffType buffType = ArtifactBuffType.DamageMultiplier;
        
        /// <summary>Magnitude of the buff effect</summary>
        public float magnitude = 1f;
        
        /// <summary>Duration in ticks (-1 = permanent)</summary>
        public int duration = -1;
        
        /// <summary>Remaining duration for temporary buffs</summary>
        public int remainingDuration = -1;
        
        /// <summary>Human-readable description of the buff</summary>
        public string description = "";
        
        /// <summary>Whether this buff is currently active</summary>
        public bool isActive = true;
        #endregion

        #region Constructors
        public ArtifactBuff()
        {
            // Default constructor for save/load
        }
        
        public ArtifactBuff(ArtifactBuffType type, float magnitude, string description = "")
        {
            this.buffType = type;
            this.magnitude = magnitude;
            this.description = description;
            this.duration = -1; // Permanent by default
            this.remainingDuration = -1;
        }
        
        public ArtifactBuff(ArtifactBuffType type, float magnitude, int duration, string description = "")
        {
            this.buffType = type;
            this.magnitude = magnitude;
            this.duration = duration;
            this.remainingDuration = duration;
            this.description = description;
        }
        #endregion

        #region Static Factory Methods
        /// <summary>Create a damage multiplier buff scaled by ELO</summary>
        public static ArtifactBuff CreateDamageMultiplier(float elo)
        {
            float multiplier = 1f + (elo / 1000f) * 0.8f; // 1.0x - 1.8x based on ELO
            return new ArtifactBuff(ArtifactBuffType.DamageMultiplier, multiplier, 
                                   $"+{(multiplier - 1f) * 100f:F0}% Damage");
        }
        
        /// <summary>Create a Qi regeneration multiplier buff</summary>
        public static ArtifactBuff CreateQiRegenMultiplier(float elo)
        {
            float multiplier = 1f + (elo / 600f) * 2f; // 1.0x - 3.0x based on ELO
            return new ArtifactBuff(ArtifactBuffType.QiRegenMultiplier, multiplier,
                                   $"+{(multiplier - 1f) * 100f:F0}% Qi Regeneration");
        }
        
        /// <summary>Create a movement speed bonus buff</summary>
        public static ArtifactBuff CreateMovementSpeed(float elo)
        {
            float bonus = (elo / 1500f) * 0.8f; // 0% - 80% bonus based on ELO
            return new ArtifactBuff(ArtifactBuffType.MovementSpeedBonus, bonus,
                                   $"+{bonus * 100f:F0}% Movement Speed");
        }
        
        /// <summary>Create a critical hit chance bonus</summary>
        public static ArtifactBuff CreateCriticalChance(float elo)
        {
            float bonus = (elo / 1200f) * 0.25f; // 0% - 25% bonus based on ELO
            return new ArtifactBuff(ArtifactBuffType.CriticalChanceBonus, bonus,
                                   $"+{bonus * 100f:F1}% Critical Hit Chance");
        }
        
        /// <summary>Create an armor rating bonus</summary>
        public static ArtifactBuff CreateArmorRating(float elo)
        {
            float bonus = (elo / 800f) * 15f; // 0 - 15 armor based on ELO
            return new ArtifactBuff(ArtifactBuffType.ArmorRatingBonus, bonus,
                                   $"+{bonus:F0} Armor Rating");
        }
        #endregion

        #region Update Methods
        /// <summary>Update buff state (handle duration, etc.)</summary>
        public void Update()
        {
            if (duration > 0 && remainingDuration > 0)
            {
                remainingDuration--;
                if (remainingDuration <= 0)
                {
                    isActive = false;
                }
            }
        }
        
        /// <summary>Check if this buff has expired</summary>
        public bool HasExpired => duration > 0 && remainingDuration <= 0;
        
        /// <summary>Get remaining time as a human-readable string</summary>
        public string GetRemainingTimeString()
        {
            if (duration < 0) return "Permanent";
            if (remainingDuration <= 0) return "Expired";
            
            float seconds = remainingDuration / 60f;
            if (seconds < 60f) return $"{seconds:F0}s";
            
            float minutes = seconds / 60f;
            if (minutes < 60f) return $"{minutes:F1}m";
            
            float hours = minutes / 60f;
            return $"{hours:F1}h";
        }
        #endregion

        #region Save/Load
        public void ExposeData()
        {
            Scribe_Values.Look(ref buffType, "buffType");
            Scribe_Values.Look(ref magnitude, "magnitude");
            Scribe_Values.Look(ref duration, "duration");
            Scribe_Values.Look(ref remainingDuration, "remainingDuration");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref isActive, "isActive");
        }
        #endregion

        public override string ToString()
        {
            string durationStr = duration < 0 ? "" : $" ({GetRemainingTimeString()})";
            return $"{description}{durationStr}";
        }
    }

    /// <summary>
    /// ✅ Task 1.3: Types of buffs artifacts can provide
    /// Each type corresponds to different gameplay effects
    /// </summary>
    public enum ArtifactBuffType
    {
        // Combat Buffs
        DamageMultiplier,           // Multiply damage dealt
        CriticalChanceBonus,        // Add critical hit chance
        CriticalDamageMultiplier,   // Multiply critical damage
        AttackSpeedMultiplier,      // Multiply attack speed
        ArmorPenetrationBonus,      // Add armor penetration
        
        // Defensive Buffs
        ArmorRatingBonus,           // Add armor rating
        DamageReduction,            // Reduce incoming damage
        DodgeChanceBonus,           // Add dodge chance
        ImmunityChance,             // Chance to ignore status effects
        
        // Cultivation Buffs
        QiRegenMultiplier,          // Multiply Qi regeneration
        MaxQiMultiplier,            // Multiply maximum Qi
        CultivationSpeedBonus,      // Add cultivation speed
        ExperienceMultiplier,       // Multiply XP gain
        
        // Utility Buffs
        MovementSpeedBonus,         // Add movement speed
        WorkSpeedMultiplier,        // Multiply work speed
        CarryCapacityBonus,         // Add carrying capacity
        SocialImpactBonus,          // Add social impact
        
        // Mystical Buffs
        ElementalResistance,        // Resist elemental damage
        StatusResistance,           // Resist status effects
        LuckyEventBonus,            // Improve random events
        SenseRangeBonus,            // Increase detection range
        
        // Aura Buffs (affect nearby pawns)
        LeadershipAura,             // Buff nearby allies
        HealingAura,                // Heal nearby pawns
        CultivationAura,            // Boost ally cultivation
        FearAura                    // Debuff nearby enemies
    }
}
