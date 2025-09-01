using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TuTien
{
    public enum CultivationRealm
    {
        Mortal = 0,
        QiCondensation = 1,
        FoundationEstablishment = 2,
        GoldenCore = 3
    }

    public enum TalentLevel
    {
        Common = 0,
        Rare = 1,
        Genius = 2,
        HeavenChosen = 3
    }

    public class TalentDef : Def
    {
        public TalentLevel talentLevel;
        public float expGainMultiplier = 1.0f;
        public float buffMultiplier = 1.0f;
        public float tuViGainRate = 1.0f;
        public float techniqueChance = 0.0f;
        public bool canHaveMultipleTechniques = false;
        public string labelKey;
        public string descriptionKey;
    }

    public class CultivationTechniqueDef : Def
    {
        public string labelKey;
        public string descriptionKey;
        public List<CultivationSkillDef> grantedSkills = new List<CultivationSkillDef>();
        public float spawnChance = 0.2f;
        public Type workerClass;
    }

    public class CultivationStageStatsDef : Def
    {
        public CultivationRealm realm;
        public int stage;
        
        // Core cultivation stats
        public float maxQi = 50f;
        public float qiRegenRate = 0.5f;
        public float breakthroughChance = 0.5f;
        public float tuViGainMultiplier = 1.0f;
        
        // Combat stats
        public float maxHpMultiplier = 1.0f;
        public float meleeDamageMultiplier = 1.0f;
        public float meleeHitChanceOffset = 0.0f;
        public float rangedDamageMultiplier = 1.0f;
        public float armorRatingSharpMultiplier = 1.0f;
        public float armorRatingBluntMultiplier = 1.0f;
        
        // Movement and utility
        public float moveSpeedOffset = 0.0f;
        public float carryingCapacityMultiplier = 1.0f;
        public float workSpeedGlobalMultiplier = 1.0f;
        
        // Biological needs
        public float hungerRateMultiplier = 1.0f;
        public float restRateMultiplier = 1.0f;
        public float immunityGainSpeedMultiplier = 1.0f;
        public float injuryHealingFactorMultiplier = 1.0f;
        
        // Mental
        public float mentalBreakThresholdMultiplier = 1.0f;
        public float moodOffset = 0.0f;

        public CultivationStageStats ToStageStats()
        {
            return new CultivationStageStats
            {
                maxQi = this.maxQi,
                qiRegenRate = this.qiRegenRate,
                breakthroughChance = this.breakthroughChance,
                tuViGainMultiplier = this.tuViGainMultiplier,
                maxHpMultiplier = this.maxHpMultiplier,
                meleeDamageMultiplier = this.meleeDamageMultiplier,
                meleeHitChanceOffset = this.meleeHitChanceOffset,
                rangedDamageMultiplier = this.rangedDamageMultiplier,
                armorRatingSharpMultiplier = this.armorRatingSharpMultiplier,
                armorRatingBluntMultiplier = this.armorRatingBluntMultiplier,
                moveSpeedOffset = this.moveSpeedOffset,
                carryingCapacityMultiplier = this.carryingCapacityMultiplier,
                workSpeedGlobalMultiplier = this.workSpeedGlobalMultiplier,
                hungerRateMultiplier = this.hungerRateMultiplier,
                restRateMultiplier = this.restRateMultiplier,
                immunityGainSpeedMultiplier = this.immunityGainSpeedMultiplier,
                injuryHealingFactorMultiplier = this.injuryHealingFactorMultiplier,
                mentalBreakThresholdMultiplier = this.mentalBreakThresholdMultiplier,
                moodOffset = this.moodOffset
            };
        }
    }

    [DefOf]
    public static class CultivationStageStatsDefOf
    {
        public static CultivationStageStatsDef MortalStage1;
        public static CultivationStageStatsDef MortalStage2;
        public static CultivationStageStatsDef MortalStage3;
        public static CultivationStageStatsDef QiCondensationStage1;
        public static CultivationStageStatsDef QiCondensationStage5;
        public static CultivationStageStatsDef FoundationEstablishmentStage1;
        
        static CultivationStageStatsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CultivationStageStatsDefOf));
        }
    }
}
