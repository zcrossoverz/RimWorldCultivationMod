using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using TuTien.SkillWorkers;
using TuTien.Core;

namespace TuTien
{
    public class CultivationStageStats
    {
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
        
        // Deprecated - keeping for compatibility
        public float tuViRequired = 50f;
    }

    public class CultivationData : IExposable
    {
        // Core cultivation properties with event triggers
        private CultivationRealm _currentRealm = CultivationRealm.Mortal;
        private int _currentStage = 1;
        private float _currentQi = 0f;
        private float _cultivationPoints = 0f;
        
        public CultivationRealm currentRealm 
        { 
            get => _currentRealm;
            set
            {
                if (_currentRealm != value)
                {
                    var oldRealm = _currentRealm;
                    _currentRealm = value;
                    if (pawn != null)
                        CultivationEvents.TriggerRealmChanged(pawn, oldRealm, _currentRealm);
                }
            }
        }
        
        public int currentStage 
        { 
            get => _currentStage;
            set
            {
                if (_currentStage != value)
                {
                    var oldStage = _currentStage;
                    _currentStage = value;
                    if (pawn != null)
                        CultivationEvents.TriggerStageChanged(pawn, oldStage, _currentStage);
                }
            }
        }
        
        public float currentQi 
        { 
            get => _currentQi;
            set
            {
                if (Math.Abs(_currentQi - value) > 0.01f)
                {
                    var oldQi = _currentQi;
                    _currentQi = Mathf.Clamp(value, 0, maxQi);
                    if (pawn != null)
                        CultivationEvents.TriggerQiChanged(pawn, oldQi, _currentQi);
                }
            }
        }
        
        public float cultivationPoints 
        { 
            get => _cultivationPoints;
            set
            {
                if (Math.Abs(_cultivationPoints - value) > 0.01f)
                {
                    var oldTuVi = _cultivationPoints;
                    _cultivationPoints = Mathf.Max(0, value);
                    if (pawn != null)
                        CultivationEvents.TriggerTuViChanged(pawn, oldTuVi, _cultivationPoints);
                }
            }
        }
        
        // Other properties remain as fields for now
        public TalentLevel talent = TalentLevel.Common;
        public float maxQi = 50f;
        public float qiRegenRate = 0.5f;
        public int lastBreakthroughTick = -999999;
        public List<CultivationSkillDef> unlockedSkills = new List<CultivationSkillDef>();
        public List<CultivationTechniqueDef> knownTechniques = new List<CultivationTechniqueDef>();
        public Dictionary<string, int> skillCooldowns = new Dictionary<string, int>();
        
        private Pawn pawn;

        public CultivationData() { }

        public CultivationData(Pawn pawn)
        {
            this.pawn = pawn;
            InitializeTalent();
            UpdateStatsForRealm();
        }

        public float GetBreakthroughChance()
        {
            float baseChance = GetBaseBreakthroughChance();
            float realmMultiplier = GetRealmMultiplier();
            float talentMultiplier = GetTalentMultiplier();
            
            float finalChance = baseChance * realmMultiplier * talentMultiplier;
            
            // Cap at 100% (1.0)
            return UnityEngine.Mathf.Min(finalChance, 1.0f);
        }

        private float GetBaseBreakthroughChance()
        {
            switch (currentRealm)
            {
                case CultivationRealm.Mortal:
                    switch (currentStage)
                    {
                        case 1: return 0.85f;
                        case 2: return 0.80f;
                        case 3: return 0.75f;
                        default: return 0.70f;
                    }
                    
                case CultivationRealm.QiCondensation:
                    switch (currentStage)
                    {
                        case 1: return 0.90f;
                        case 2: return 0.85f;
                        case 3: return 0.80f;
                        case 4: return 0.75f;
                        case 5: return 0.70f;
                        case 6: return 0.60f;
                        case 7: return 0.50f;
                        case 8: return 0.40f;
                        case 9: return 0.30f;
                        default: return 0.25f;
                    }
                    
                case CultivationRealm.FoundationEstablishment:
                    switch (currentStage)
                    {
                        case 1: return 0.60f;
                        case 2: return 0.50f;
                        case 3: return 0.40f;
                        case 4: return 0.30f;
                        case 5: return 0.20f;
                        default: return 0.15f;
                    }
                    
                case CultivationRealm.GoldenCore:
                    switch (currentStage)
                    {
                        case 1: return 0.25f;
                        case 2: return 0.15f;
                        case 3: return 0.10f;
                        default: return 0.05f;
                    }
                    
                default:
                    return 0.50f;
            }
        }

        private float GetRealmMultiplier()
        {
            switch (currentRealm)
            {
                case CultivationRealm.Mortal:
                case CultivationRealm.QiCondensation:
                    return 1.0f;
                case CultivationRealm.FoundationEstablishment:
                    return 0.8f;
                case CultivationRealm.GoldenCore:
                    return 0.6f;
                default:
                    return 1.0f;
            }
        }

        private float GetTalentMultiplier()
        {
            switch (talent)
            {
                case TalentLevel.Common:
                    return 1.0f;
                case TalentLevel.Rare:
                    return 1.2f;
                case TalentLevel.Genius:
                    return 1.5f;
                case TalentLevel.HeavenChosen:
                    return 2.0f;
                default:
                    return 1.0f;
            }
        }

        public void InitializeTalent()
        {
            var rand = Rand.Value;
            if (rand < 0.05f) // 5% Heaven Chosen
                talent = TalentLevel.HeavenChosen;
            else if (rand < 0.15f) // 10% Genius
                talent = TalentLevel.Genius;
            else if (rand < 0.35f) // 20% Rare
                talent = TalentLevel.Rare;
            else
                talent = TalentLevel.Common; // 65% Common

            // Heaven Chosen get techniques
            if (talent == TalentLevel.HeavenChosen && Rand.Chance(0.7f))
            {
                GrantRandomTechnique();
                if (Rand.Chance(0.3f)) // 30% chance for second technique
                    GrantRandomTechnique();
            }
            else if (Rand.Chance(0.2f)) // 20% chance for others
            {
                GrantRandomTechnique();
            }
        }

        public void GrantRandomTechnique()
        {
            var availableTechniques = DefDatabase<CultivationTechniqueDef>.AllDefs
                .Where(t => !knownTechniques.Contains(t)).ToList();
            
            if (availableTechniques.Any())
            {
                var technique = availableTechniques.RandomElement();
                knownTechniques.Add(technique);
                
                // Grant skills from technique
                foreach (var skill in technique.grantedSkills)
                {
                    if (!unlockedSkills.Contains(skill))
                        unlockedSkills.Add(skill);
                }
            }
        }

        public void UpdateStatsForRealm()
        {
            // Get stage-specific stats from our calculation methods
            var stageStats = GetCurrentStageStats();
            
            maxQi = stageStats.maxQi;
            qiRegenRate = stageStats.qiRegenRate;
            
            // Apply talent modifiers to regen rate
            var talentDef = DefDatabase<TalentDef>.AllDefs
                .FirstOrDefault(t => t.talentLevel == talent);
            
            if (talentDef != null)
            {
                qiRegenRate *= talentDef.buffMultiplier;
            }

            // Apply stat buffs to pawn
            ApplyStatBuffs(stageStats);

            // Unlock stage-based skills
            UpdateUnlockedSkills();
        }

        public CultivationStageStats GetCurrentStageStats()
        {
            return GetStageStatsForRealm(currentRealm, currentStage);
        }

        private CultivationStageStats GetStageStatsForRealm(CultivationRealm realm, int stage)
        {
            // Try to get stats from XML defs first
            var statsDef = DefDatabase<CultivationStageStatsDef>.AllDefs
                .FirstOrDefault(def => def.realm == realm && def.stage == stage);
            
            if (statsDef != null)
            {
                return statsDef.ToStageStats();
            }
            
            // Fallback to hard-coded stats if XML def not found
            switch (realm)
            {
                case CultivationRealm.Mortal:
                    return GetMortalStageStats(stage);
                case CultivationRealm.QiCondensation:
                    return GetQiCondensationStageStats(stage);
                case CultivationRealm.FoundationEstablishment:
                    return GetFoundationEstablishmentStageStats(stage);
                case CultivationRealm.GoldenCore:
                    return GetGoldenCoreStageStats(stage);
                default:
                    return new CultivationStageStats();
            }
        }

        private CultivationStageStats GetMortalStageStats(int stage)
        {
            switch (stage)
            {
                case 1: return new CultivationStageStats { maxQi = 100, qiRegenRate = 0.5f, maxHpMultiplier = 1.05f, moveSpeedOffset = 0.1f, meleeDamageMultiplier = 1.05f };
                case 2: return new CultivationStageStats { maxQi = 150, qiRegenRate = 0.6f, maxHpMultiplier = 1.10f, moveSpeedOffset = 0.2f, meleeDamageMultiplier = 1.10f };
                case 3: return new CultivationStageStats { maxQi = 200, qiRegenRate = 0.7f, maxHpMultiplier = 1.15f, moveSpeedOffset = 0.3f, meleeDamageMultiplier = 1.15f };
                default: return new CultivationStageStats { maxQi = 200, qiRegenRate = 0.7f };
            }
        }

        private CultivationStageStats GetQiCondensationStageStats(int stage)
        {
            switch (stage)
            {
                case 1: return new CultivationStageStats { maxQi = 250, qiRegenRate = 0.8f, maxHpMultiplier = 1.2f, moveSpeedOffset = 0.4f, meleeDamageMultiplier = 1.2f };
                case 2: return new CultivationStageStats { maxQi = 300, qiRegenRate = 0.9f, maxHpMultiplier = 1.3f, moveSpeedOffset = 0.5f, meleeDamageMultiplier = 1.3f };
                case 3: return new CultivationStageStats { maxQi = 350, qiRegenRate = 1.0f, maxHpMultiplier = 1.4f, moveSpeedOffset = 0.6f, meleeDamageMultiplier = 1.4f };
                case 4: return new CultivationStageStats { maxQi = 400, qiRegenRate = 1.1f, maxHpMultiplier = 1.5f, moveSpeedOffset = 0.7f, meleeDamageMultiplier = 1.5f };
                case 5: return new CultivationStageStats { maxQi = 500, qiRegenRate = 1.2f, maxHpMultiplier = 1.6f, moveSpeedOffset = 0.8f, meleeDamageMultiplier = 1.6f };
                case 6: return new CultivationStageStats { maxQi = 600, qiRegenRate = 1.3f, maxHpMultiplier = 1.7f, moveSpeedOffset = 0.9f, meleeDamageMultiplier = 1.7f };
                case 7: return new CultivationStageStats { maxQi = 700, qiRegenRate = 1.4f, maxHpMultiplier = 1.8f, moveSpeedOffset = 1.0f, meleeDamageMultiplier = 1.8f };
                case 8: return new CultivationStageStats { maxQi = 800, qiRegenRate = 1.5f, maxHpMultiplier = 1.9f, moveSpeedOffset = 1.1f, meleeDamageMultiplier = 1.9f };
                case 9: return new CultivationStageStats { maxQi = 1000, qiRegenRate = 1.6f, maxHpMultiplier = 2.0f, moveSpeedOffset = 1.2f, meleeDamageMultiplier = 2.0f };
                default: return new CultivationStageStats { maxQi = 1000, qiRegenRate = 1.6f };
            }
        }

        private CultivationStageStats GetFoundationEstablishmentStageStats(int stage)
        {
            switch (stage)
            {
                case 1: return new CultivationStageStats { maxQi = 1200, qiRegenRate = 1.8f, maxHpMultiplier = 2.2f, moveSpeedOffset = 1.3f, meleeDamageMultiplier = 2.2f, hungerRateMultiplier = 0.9f, restRateMultiplier = 0.9f };
                case 2: return new CultivationStageStats { maxQi = 1400, qiRegenRate = 2.0f, maxHpMultiplier = 2.4f, moveSpeedOffset = 1.4f, meleeDamageMultiplier = 2.4f, hungerRateMultiplier = 0.85f, restRateMultiplier = 0.85f };
                case 3: return new CultivationStageStats { maxQi = 1600, qiRegenRate = 2.2f, maxHpMultiplier = 2.6f, moveSpeedOffset = 1.5f, meleeDamageMultiplier = 2.6f, hungerRateMultiplier = 0.8f, restRateMultiplier = 0.8f };
                case 4: return new CultivationStageStats { maxQi = 1800, qiRegenRate = 2.4f, maxHpMultiplier = 2.8f, moveSpeedOffset = 1.6f, meleeDamageMultiplier = 2.8f, hungerRateMultiplier = 0.8f, restRateMultiplier = 0.8f };
                case 5: return new CultivationStageStats { maxQi = 2000, qiRegenRate = 2.6f, maxHpMultiplier = 3.0f, moveSpeedOffset = 1.7f, meleeDamageMultiplier = 3.0f, hungerRateMultiplier = 0.8f, restRateMultiplier = 0.8f };
                case 6: return new CultivationStageStats { maxQi = 2200, qiRegenRate = 2.8f, maxHpMultiplier = 3.2f, moveSpeedOffset = 1.8f, meleeDamageMultiplier = 3.2f, hungerRateMultiplier = 0.8f, restRateMultiplier = 0.8f };
                default: return new CultivationStageStats { maxQi = 2200, qiRegenRate = 2.8f };
            }
        }

        private CultivationStageStats GetGoldenCoreStageStats(int stage)
        {
            switch (stage)
            {
                case 1: return new CultivationStageStats { maxQi = 2500, qiRegenRate = 3.0f, maxHpMultiplier = 3.5f, moveSpeedOffset = 2.0f, meleeDamageMultiplier = 3.5f, hungerRateMultiplier = 0.7f, restRateMultiplier = 0.7f };
                case 2: return new CultivationStageStats { maxQi = 3000, qiRegenRate = 3.5f, maxHpMultiplier = 4.0f, moveSpeedOffset = 2.2f, meleeDamageMultiplier = 4.0f, hungerRateMultiplier = 0.6f, restRateMultiplier = 0.6f };
                case 3: return new CultivationStageStats { maxQi = 3500, qiRegenRate = 4.0f, maxHpMultiplier = 4.5f, moveSpeedOffset = 2.5f, meleeDamageMultiplier = 4.5f, hungerRateMultiplier = 0.5f, restRateMultiplier = 0.5f };
                default: return new CultivationStageStats { maxQi = 3500, qiRegenRate = 4.0f };
            }
        }

        private void ApplyStatBuffs(CultivationStageStats stats)
        {
            if (pawn == null) return;

            // Remove old cultivation buffs
            var oldBuff = pawn.health.hediffSet.GetFirstHediffOfDef(TuTienDefOf.CultivationBuffHediff);
            if (oldBuff != null)
            {
                pawn.health.RemoveHediff(oldBuff);
            }

            // Apply new cultivation buffs
            var buff = HediffMaker.MakeHediff(TuTienDefOf.CultivationBuffHediff, pawn);
            buff.Severity = GetBuffSeverity(stats);
            pawn.health.AddHediff(buff);
        }

        private float GetBuffSeverity(CultivationStageStats stats)
        {
            // Encode multiple stats into severity for hediff stages
            // We'll use different severity ranges for different buff levels
            float totalBuff = (stats.maxHpMultiplier - 1f) + stats.moveSpeedOffset + (stats.meleeDamageMultiplier - 1f);
            return UnityEngine.Mathf.Clamp(totalBuff, 0.1f, 2f);
        }

        public void UpdateUnlockedSkills()
        {
            // Load skills from XML definitions based on current realm and stage
            var availableSkills = DefDatabase<CultivationSkillDef>.AllDefs
                .Where(s => s.requiredRealm == currentRealm && s.requiredStage <= currentStage);
            
            foreach (var skill in availableSkills)
            {
                if (!unlockedSkills.Contains(skill))
                {
                    unlockedSkills.Add(skill);
                    
                    // Log new skill unlock
                    Log.Message($"[TuTien] {pawn.Name}: Unlocked skill '{skill.label}' - " + 
                               (skill.isActive ? "Active" : "Passive"));
                }
            }
            
            // Also load skills from higher realms that this stage qualifies for
            var nextRealmSkills = DefDatabase<CultivationSkillDef>.AllDefs
                .Where(s => (int)s.requiredRealm < (int)currentRealm || 
                           (s.requiredRealm == currentRealm && s.requiredStage <= currentStage));
                           
            foreach (var skill in nextRealmSkills)
            {
                if (!unlockedSkills.Contains(skill))
                {
                    unlockedSkills.Add(skill);
                }
            }
        }

        public bool CanBreakthrough()
        {
            // Debug: Let's check what's happening
            int maxStageForRealm = GetMaxStageForRealm(currentRealm);
            bool canAdvanceToNext = CanAdvanceToNextRealm();
            float requiredPoints = GetRequiredCultivationPoints();
            bool hasEnoughPoints = cultivationPoints >= requiredPoints;
            bool enoughTimePassed = true; // Remove time restriction for testing
            
            // Log for debugging
            Log.Message($"[TuTien] CanBreakthrough Debug: Realm={currentRealm}, Stage={currentStage}, MaxStage={maxStageForRealm}, " +
                       $"CanAdvanceNext={canAdvanceToNext}, Required={requiredPoints}, Current={cultivationPoints}, " +
                       $"HasEnough={hasEnoughPoints}, TimePassed={enoughTimePassed}");
            
            // Check if we're at max stage for current realm
            if (currentStage >= maxStageForRealm && !canAdvanceToNext) return false;

            return hasEnoughPoints && enoughTimePassed;
        }

        private int GetMaxStageForRealm(CultivationRealm realm)
        {
            switch (realm)
            {
                case CultivationRealm.Mortal: return 3;
                case CultivationRealm.QiCondensation: return 9;
                case CultivationRealm.FoundationEstablishment: return 6;
                case CultivationRealm.GoldenCore: return 3;
                default: return 1;
            }
        }

        private bool CanAdvanceToNextRealm()
        {
            var nextRealm = (CultivationRealm)((int)currentRealm + 1);
            return System.Enum.IsDefined(typeof(CultivationRealm), nextRealm);
        }

        public float GetRequiredCultivationPoints()
        {
            return GetRequiredPointsForRealmAndStage(currentRealm, currentStage);
        }

        // Mapping table for cultivation points required by realm and stage
        private float GetRequiredPointsForRealmAndStage(CultivationRealm realm, int stage)
        {
            // Simple cumulative system - easier to debug
            float totalRequired = 0f;
            
            // Add all previous realms first
            if (realm >= CultivationRealm.QiCondensation)
            {
                // Add all Mortal stages: 50 + 135 + 255 = 255 for stage 3
                totalRequired += 50f;  // Stage 1
                totalRequired += 135f; // Stage 2 
                totalRequired += 255f; // Stage 3
            }
            
            if (realm >= CultivationRealm.FoundationEstablishment)
            {
                // Add all QiCondensation stages
                for (int i = 1; i <= 9; i++)
                {
                    totalRequired += 100f + (i - 1) * 50f;
                }
            }
            
            if (realm >= CultivationRealm.GoldenCore)
            {
                // Add all Foundation stages
                for (int i = 1; i <= 6; i++)
                {
                    totalRequired += 200f + (i - 1) * 100f;
                }
            }
            
            // Add current realm stages
            switch (realm)
            {
                case CultivationRealm.Mortal:
                    if (stage >= 1) totalRequired += 50f;   // Stage 1: 50 total
                    if (stage >= 2) totalRequired += 85f;   // Stage 2: 135 total
                    if (stage >= 3) totalRequired += 120f;  // Stage 3: 255 total
                    break;
                    
                case CultivationRealm.QiCondensation:
                    for (int i = 1; i <= stage; i++)
                    {
                        totalRequired += 100f + (i - 1) * 50f;
                    }
                    break;
                    
                case CultivationRealm.FoundationEstablishment:
                    for (int i = 1; i <= stage; i++)
                    {
                        totalRequired += 200f + (i - 1) * 100f;
                    }
                    break;
                    
                case CultivationRealm.GoldenCore:
                    for (int i = 1; i <= stage; i++)
                    {
                        totalRequired += 500f + (i - 1) * 200f;
                    }
                    break;
                    
                default:
                    totalRequired = 100f;
                    break;
            }

            return totalRequired;
        }

        public bool AttemptBreakthrough()
        {
            if (!CanBreakthrough()) return false;

            // Store old values for dialog
            var oldRealm = currentRealm;
            var oldStage = currentStage;
            var oldStats = GetCurrentStageStats();

            // Calculate success chance using new system
            float finalChance = GetBreakthroughChance();
            
            bool success = Rand.Chance(finalChance);
            
            if (success)
            {
                // Get new skills before advancing
                var newSkills = GetNewSkillsAfterAdvancement();
                
                AdvanceStage();
                lastBreakthroughTick = Find.TickManager.TicksGame;
                
                // Get new stats after advancement
                var newStats = GetCurrentStageStats();
                
                // Show success dialog with stats comparison
                Find.WindowStack.Add(new UI.Dialog_BreakthroughResult(
                    pawn, true, oldRealm, oldStage, currentRealm, currentStage, 
                    oldStats, newStats, newSkills));
                
                return true;
            }
            else
            {
                // Failed breakthrough with detailed consequences
                FailedBreakthrough(finalChance);
                lastBreakthroughTick = Find.TickManager.TicksGame;
                
                // Show failure dialog
                Find.WindowStack.Add(new UI.Dialog_BreakthroughResult(
                    pawn, false, oldRealm, oldStage, currentRealm, currentStage, 
                    oldStats, oldStats, null));
                
                return false;
            }
        }

        private System.Collections.Generic.List<CultivationSkillDef> GetNewSkillsAfterAdvancement()
        {
            var currentSkills = unlockedSkills.ToList();
            
            // Simulate advancement to see what new skills would be unlocked
            var tempRealm = currentRealm;
            var tempStage = currentStage;
            
            int maxStageForRealm = GetMaxStageForRealm(tempRealm);
            
            if (tempStage < maxStageForRealm)
            {
                tempStage++;
            }
            else
            {
                var nextRealm = (CultivationRealm)((int)tempRealm + 1);
                if (System.Enum.IsDefined(typeof(CultivationRealm), nextRealm))
                {
                    tempRealm = nextRealm;
                    tempStage = 1;
                }
            }

            // Get skills that would be unlocked
            var newSkills = DefDatabase<CultivationSkillDef>.AllDefs
                .Where(s => s.requiredRealm == tempRealm && s.requiredStage <= tempStage && !currentSkills.Contains(s))
                .ToList();

            return newSkills;
        }

        private void FailedBreakthrough(float attemptedChance)
        {
            // Lose cultivation progress based on stage difficulty
            float lossPercent = 0.3f + (currentStage * 0.1f); // 30-80% loss
            cultivationPoints *= (1f - lossPercent);
            
            // Apply Qi deviation injury - more severe for higher stages
            var injuryChance = 0.5f + (currentStage * 0.1f);
            if (Rand.Chance(injuryChance))
            {
                // Random severe injury
                var bodyParts = pawn.health.hediffSet.GetNotMissingParts().Where(p => p.depth == BodyPartDepth.Inside);
                if (bodyParts.Any())
                {
                    var randomPart = bodyParts.RandomElement();
                    var injury = HediffMaker.MakeHediff(HediffDefOf.Cut, pawn, randomPart);
                    injury.Severity = Rand.Range(0.2f, 0.6f);
                    pawn.health.AddHediff(injury);
                }
            }
            
            // Apply temporary cultivation weakness
            var weakness = HediffMaker.MakeHediff(TuTienDefOf.QiDeviationHediff, pawn);
            weakness.Severity = 0.8f;
            pawn.health.AddHediff(weakness);
            
            // Show failure message with attempted chance
            Messages.Message($"{pawn.Name.ToStringShort} failed breakthrough (chance: {attemptedChance:P1}) and suffered Qi deviation!", 
                MessageTypeDefOf.NegativeEvent);
        }

        private void AdvanceStage()
        {
            int maxStageForRealm = GetMaxStageForRealm(currentRealm);

            if (currentStage < maxStageForRealm)
            {
                currentStage++;
            }
            else
            {
                // Advance to next realm
                var nextRealm = (CultivationRealm)((int)currentRealm + 1);
                if (Enum.IsDefined(typeof(CultivationRealm), nextRealm))
                {
                    currentRealm = nextRealm;
                    currentStage = 1;
                }
            }

            // Don't reset cultivation points - let them accumulate
            // cultivationPoints = 0f; // REMOVED - tu vi now accumulates
            UpdateStatsForRealm();
        }

        public void Tick()
        {
            if (pawn == null || pawn.Dead) return;

            // Regenerate Qi over time
            if (pawn.IsHashIntervalTick(60)) // Every second
            {
                RegenerateQi();
                CheckTuViConversion();
            }

            // Update skill cooldowns (faster for testing)
            if (pawn.IsHashIntervalTick(60)) // Every second
            {
                var keysToUpdate = skillCooldowns.Keys.ToList();
                foreach (var key in keysToUpdate)
                {
                    if (skillCooldowns[key] > 0)
                    {
                        int oldValue = skillCooldowns[key];
                        // Speed up cooldown for testing - 60x faster (1 hour = 1 minute)
                        skillCooldowns[key] -= 60 * 60; // Reduce by 1 minute (3600 ticks) each update
                        Log.Warning($"[TuTien] Skill {key} cooldown: {oldValue} -> {skillCooldowns[key]}");
                        
                        if (skillCooldowns[key] <= 0)
                        {
                            skillCooldowns.Remove(key);
                            Log.Warning($"[TuTien] Skill {key} cooldown finished!");
                        }
                    }
                    else
                    {
                        skillCooldowns.Remove(key);
                    }
                }
            }
        }

        private void RegenerateQi()
        {
            Log.Warning($"[TuTien] RegenerateQi called - Current Qi: {currentQi}/{maxQi}");
            
            if (currentQi < maxQi)
            {
                float regenAmount = qiRegenRate * 5f; // 5x faster regen for testing
                Log.Warning($"[TuTien] Base regen amount: {regenAmount}");
                
                // Check if pawn is meditating for faster regen
                if (pawn.CurJob?.def?.defName == "MeditateCultivation")
                {
                    regenAmount *= 3f; // 3x faster when meditating
                    Log.Warning($"[TuTien] Pawn is meditating, regen amount: {regenAmount}");
                    
                    // Check cultivation spot bonus
                    var target = pawn.CurJob.targetA.Thing as Building;
                    if (target?.def?.defName == "CultivationMeditationSpot")
                    {
                        var spotComp = target.GetComp<CultivationSpotComp>();
                        if (spotComp != null)
                        {
                            regenAmount *= spotComp.CultivationBonus; // Additional spot bonus
                            Log.Warning($"[TuTien] Cultivation spot bonus applied, final regen: {regenAmount}");
                        }
                    }
                }
                else
                {
                    Log.Warning($"[TuTien] Pawn not meditating, current job: {pawn.CurJob?.def?.defName}");
                }
                
                currentQi += regenAmount;
                if (currentQi > maxQi)
                    currentQi = maxQi;
                    
                Log.Warning($"[TuTien] After regen - Current Qi: {currentQi}/{maxQi}");
            }
        }

        private void CheckTuViConversion()
        {
            // When Qi is at max, allow Tu Vi to continue growing
            if (currentQi >= maxQi)
            {
                // Keep Qi at max level
                currentQi = maxQi;
                
                // Tu Vi continues to grow when Qi is full - NO LIMITS
                float tuViGain = CalculateTuViGain(1f);
                
                // Always allow tu vi to grow - player decides when to breakthrough
                cultivationPoints += tuViGain;
                
                // Show cultivation effect when Tu Vi grows
                if (Rand.Chance(0.1f)) // 10% chance to show effect
                {
                    FleckMaker.ThrowLightningGlow(pawn.Position.ToVector3(), pawn.Map, 0.5f);
                }
                
                // Don't auto breakthrough - let player decide
                // CheckAutoBreakthrough();
            }
        }

        private float CalculateTuViGain(float cycles)
        {
            float baseTuVi = 1f; // Base Tu Vi per cycle
            
            // Realm multiplier
            float realmMultiplier = GetRealmMultiplierForTuVi();
            
            // Talent multiplier
            float talentMultiplier = GetTalentMultiplierForTuVi();
            
            return cycles * baseTuVi * realmMultiplier * talentMultiplier;
        }

        private float GetRealmMultiplierForTuVi()
        {
            switch (currentRealm)
            {
                case CultivationRealm.Mortal: return 1f;
                case CultivationRealm.QiCondensation: return 1f;
                case CultivationRealm.FoundationEstablishment: return 1.5f;
                case CultivationRealm.GoldenCore: return 2f;
                default: return 1f;
            }
        }

        private float GetTalentMultiplierForTuVi()
        {
            switch (talent)
            {
                case TalentLevel.Common: return 1f;        // Base speed
                case TalentLevel.Rare: return 1.5f;       // 50% faster
                case TalentLevel.Genius: return 2.5f;     // 150% faster
                case TalentLevel.HeavenChosen: return 4f;  // 300% faster
                default: return 1f;
            }
        }

        public bool CanUseSkill(CultivationSkillDef skill)
        {
            Log.Warning($"[TuTien] CanUseSkill check for {skill.defName}:");
            Log.Warning($"[TuTien] - Unlocked? {unlockedSkills.Contains(skill)}");
            Log.Warning($"[TuTien] - Is Active? {skill.isActive}");
            Log.Warning($"[TuTien] - Current Qi: {currentQi}, Required: {skill.qiCost}");
            
            bool onCooldown = skillCooldowns.ContainsKey(skill.defName);
            Log.Warning($"[TuTien] - On Cooldown? {onCooldown}");
            
            if (onCooldown)
            {
                int ticksLeft = skillCooldowns[skill.defName];
                int hoursLeft = Mathf.CeilToInt(ticksLeft / (float)GenDate.TicksPerHour);
                Log.Warning($"[TuTien] - Cooldown ticks left: {ticksLeft}, hours: {hoursLeft}");
            }
            
            if (!unlockedSkills.Contains(skill)) return false;
            if (!skill.isActive) return true; // Passive skills are always usable
            if (currentQi < skill.qiCost) return false;
            
            return !onCooldown;
        }

        public void UseSkill(CultivationSkillDef skill)
        {
            Log.Warning($"[TuTien] UseSkill called for {skill.defName}");
            
            if (!CanUseSkill(skill)) 
            {
                Log.Warning($"[TuTien] CanUseSkill returned false for {skill.defName}");
                return;
            }

            Log.Warning($"[TuTien] Skill {skill.defName} passed CanUseSkill check");

            if (skill.isActive)
            {
                currentQi -= skill.qiCost;
                if (skill.cooldownHours > 0)
                {
                    int cooldownTicks = skill.cooldownHours * GenDate.TicksPerHour;
                    skillCooldowns[skill.defName] = cooldownTicks;
                    Log.Warning($"[TuTien] Set cooldown for {skill.defName}: {skill.cooldownHours}h = {cooldownTicks} ticks");
                    Log.Warning($"[TuTien] Cooldowns dictionary now contains {skillCooldowns.Count} entries");
                }
                Log.Warning($"[TuTien] Applied qi cost and cooldown for {skill.defName}");
            }

            // Execute skill effect (will be implemented in skill workers)
            if (skill.workerClass != null)
            {
                Log.Warning($"[TuTien] Creating worker {skill.workerClass} for {skill.defName}");
                var worker = (CultivationSkillWorker)Activator.CreateInstance(skill.workerClass);
                worker?.Execute(pawn, skill);
                Log.Warning($"[TuTien] Skill {skill.defName} executed successfully");
            }
            else
            {
                Log.Warning($"[TuTien] No workerClass found for {skill.defName}");
            }
        }

        public bool CanCultivate()
        {
            // Can't cultivate if at max realm/stage (truly finished)
            if (currentRealm == CultivationRealm.GoldenCore && currentStage >= 3)
                return false; // At max level
            
            // Always allow cultivation as long as there are higher stages/realms available
            // and qi is not full - player decides when to breakthrough
            return currentQi < maxQi * 0.95f; // Can cultivate if qi not at 95% of max
        }

        public void CheckAutoBreakthrough()
        {
            // Auto breakthrough when cultivation points are enough and enough time has passed
            if (CanBreakthrough() && Find.TickManager.TicksGame - lastBreakthroughTick > GenDate.TicksPerHour)
            {
                bool success = AttemptBreakthrough();
                // Messages are handled in AttemptBreakthrough
            }
        }

        public void ExposeData()
        {
            // Use backing fields for properties with event triggers
            Scribe_Values.Look(ref _currentRealm, "currentRealm", CultivationRealm.Mortal);
            Scribe_Values.Look(ref _currentStage, "currentStage", 1);
            Scribe_Values.Look(ref talent, "talent", TalentLevel.Common);
            Scribe_Values.Look(ref _currentQi, "currentQi", 0f);
            Scribe_Values.Look(ref maxQi, "maxQi", 50f);
            Scribe_Values.Look(ref qiRegenRate, "qiRegenRate", 0.5f);
            Scribe_Values.Look(ref _cultivationPoints, "cultivationPoints", 0f);
            Scribe_Values.Look(ref lastBreakthroughTick, "lastBreakthroughTick", -999999);
            Scribe_Collections.Look(ref unlockedSkills, "unlockedSkills", LookMode.Def);
            Scribe_Collections.Look(ref knownTechniques, "knownTechniques", LookMode.Def);
            Scribe_Collections.Look(ref skillCooldowns, "skillCooldowns", LookMode.Value, LookMode.Value);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (unlockedSkills == null) unlockedSkills = new List<CultivationSkillDef>();
                if (knownTechniques == null) knownTechniques = new List<CultivationTechniqueDef>();
                if (skillCooldowns == null) skillCooldowns = new Dictionary<string, int>();
            }
        }
    }

    public abstract class CultivationTechniqueWorker
    {
        public abstract void Apply(Pawn pawn, CultivationTechniqueDef technique);
    }
}
