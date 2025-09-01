using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien
{
    /// <summary>
    /// Memory-optimized and enhanced cultivation data structure
    /// Provides better organization, performance, and extensibility
    /// </summary>
    public partial class CultivationDataEnhanced : IExposable
    {
        #region Core Properties with Event Integration
        
        private CultivationRealm _currentRealm = CultivationRealm.Mortal;
        private int _currentStage = 1;
        private float _currentQi = 0f;
        private float _cultivationPoints = 0f;
        private TalentLevel _talent = TalentLevel.Common;
        
        // Cache for frequently accessed values
        private float? _cachedMaxQi;
        private float? _cachedQiRegenRate;
        private int _lastStatUpdateTick = -1;
        
        public CultivationRealm currentRealm 
        { 
            get => _currentRealm;
            set
            {
                if (_currentRealm != value)
                {
                    var oldRealm = _currentRealm;
                    _currentRealm = value;
                    InvalidateCache();
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
                    InvalidateCache();
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
                float clampedValue = Mathf.Clamp(value, 0, MaxQi);
                if (Math.Abs(_currentQi - clampedValue) > 0.01f)
                {
                    var oldQi = _currentQi;
                    _currentQi = clampedValue;
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
                    var oldPoints = _cultivationPoints;
                    _cultivationPoints = Mathf.Max(0, value);
                    if (pawn != null)
                        CultivationEvents.TriggerTuViChanged(pawn, oldPoints, _cultivationPoints);
                }
            }
        }
        
        public TalentLevel talent 
        { 
            get => _talent;
            set
            {
                if (_talent != value)
                {
                    var oldTalent = _talent;
                    _talent = value;
                    InvalidateCache();
                    if (pawn != null)
                        CultivationEvents.TriggerTalentChanged(pawn, oldTalent, _talent);
                }
            }
        }
        
        #endregion
        
        #region Enhanced Data Components
        
        /// <summary>Enhanced progress tracking</summary>
        public CultivationProgress progress;
        
        /// <summary>Elemental affinities and resistances</summary>
        public CultivationAffinities affinities;
        
        /// <summary>Resource management</summary>
        public CultivationResources resources;
        
        /// <summary>Skill manager reference</summary>
        [NonSerialized]
        public CultivationSkillManager skillManager;
        
        #endregion
        
        #region Legacy Compatibility Properties
        
        // Keep original properties for compatibility
        public float maxQi => MaxQi;
        public float qiRegenRate => QiRegenRate;
        public int lastBreakthroughTick 
        { 
            get => _lastBreakthroughTick; 
            set => _lastBreakthroughTick = value; 
        }
        private int _lastBreakthroughTick = -999999;
        
        // Collections with lazy initialization
        private List<CultivationSkillDef> _unlockedSkills;
        private List<CultivationTechniqueDef> _knownTechniques;
        private Dictionary<string, int> _skillCooldowns;
        
        public List<CultivationSkillDef> unlockedSkills 
        { 
            get => _unlockedSkills ??= new List<CultivationSkillDef>();
            set => _unlockedSkills = value;
        }
        
        public List<CultivationTechniqueDef> knownTechniques 
        { 
            get => _knownTechniques ??= new List<CultivationTechniqueDef>();
            set => _knownTechniques = value;
        }
        
        public Dictionary<string, int> skillCooldowns 
        { 
            get => _skillCooldowns ??= new Dictionary<string, int>();
            set => _skillCooldowns = value;
        }
        
        #endregion
        
        #region Optimized Calculated Properties
        
        /// <summary>Maximum Qi with caching</summary>
        public float MaxQi
        {
            get
            {
                if (!_cachedMaxQi.HasValue || ShouldUpdateCache())
                {
                    _cachedMaxQi = CalculateMaxQi();
                }
                return _cachedMaxQi.Value;
            }
        }
        
        /// <summary>Qi regeneration rate with caching</summary>
        public float QiRegenRate
        {
            get
            {
                if (!_cachedQiRegenRate.HasValue || ShouldUpdateCache())
                {
                    _cachedQiRegenRate = CalculateQiRegenRate();
                }
                return _cachedQiRegenRate.Value;
            }
        }
        
        /// <summary>Current cultivation efficiency</summary>
        public float CultivationEfficiency
        {
            get
            {
                float baseEfficiency = GetTalentEfficiencyMultiplier();
                float affinityBonus = affinities?.GetStrongestElementalAffinity().affinity ?? 1f;
                float momentumBonus = progress?.cultivationMomentum ?? 1f;
                
                return baseEfficiency * (1f + (affinityBonus - 1f) * 0.3f) * momentumBonus;
            }
        }
        
        /// <summary>Current breakthrough chance with all modifiers</summary>
        public float BreakthroughChance
        {
            get
            {
                float baseChance = GetBaseBreakthroughChance();
                float talentMultiplier = GetTalentBreakthroughMultiplier();
                float progressMultiplier = progress?.GetBreakthroughSuccessRate() ?? 0f;
                float affinityMultiplier = affinities?.GetStrongestElementalAffinity().affinity ?? 1f;
                
                return Mathf.Clamp01(baseChance * talentMultiplier * (1f + progressMultiplier * 0.1f) * (1f + (affinityMultiplier - 1f) * 0.2f));
            }
        }
        
        #endregion
        
        #region Internal References
        
        private Pawn pawn;
        
        #endregion
        
        #region Constructors
        
        public CultivationDataEnhanced()
        {
            InitializeComponents();
        }

        public CultivationDataEnhanced(Pawn pawn)
        {
            this.pawn = pawn;
            InitializeComponents();
            InitializeTalent();
        }
        
        private void InitializeComponents()
        {
            progress = new CultivationProgress();
            affinities = new CultivationAffinities();
            resources = new CultivationResources();
        }
        
        #endregion
        
        #region Cache Management
        
        private void InvalidateCache()
        {
            _cachedMaxQi = null;
            _cachedQiRegenRate = null;
            _lastStatUpdateTick = -1;
        }
        
        private bool ShouldUpdateCache()
        {
            int currentTick = Find.TickManager?.TicksGame ?? 0;
            return currentTick - _lastStatUpdateTick > 60; // Update every second
        }
        
        private void UpdateCacheTimestamp()
        {
            _lastStatUpdateTick = Find.TickManager?.TicksGame ?? 0;
        }
        
        #endregion
        
        #region Stat Calculations
        
        private float CalculateMaxQi()
        {
            float baseMaxQi = GetRealmBaseMaxQi(currentRealm, currentStage);
            float talentMultiplier = GetTalentStatMultiplier();
            float affinityMultiplier = affinities?.soulCultivationAffinity ?? 1f;
            
            UpdateCacheTimestamp();
            return baseMaxQi * talentMultiplier * affinityMultiplier;
        }
        
        private float CalculateQiRegenRate()
        {
            float baseRegenRate = GetRealmBaseQiRegen(currentRealm, currentStage);
            float talentMultiplier = GetTalentStatMultiplier();
            float momentumMultiplier = progress?.cultivationMomentum ?? 1f;
            
            UpdateCacheTimestamp();
            return baseRegenRate * talentMultiplier * momentumMultiplier;
        }
        
        private float GetRealmBaseMaxQi(CultivationRealm realm, int stage)
        {
            float baseQi = realm switch
            {
                CultivationRealm.Mortal => 50f,
                CultivationRealm.QiCondensation => 150f,
                CultivationRealm.FoundationEstablishment => 400f,
                CultivationRealm.GoldenCore => 1000f,
                _ => 50f
            };
            
            return baseQi + (stage - 1) * (baseQi * 0.2f);
        }
        
        private float GetRealmBaseQiRegen(CultivationRealm realm, int stage)
        {
            float baseRegen = realm switch
            {
                CultivationRealm.Mortal => 0.5f,
                CultivationRealm.QiCondensation => 1.5f,
                CultivationRealm.FoundationEstablishment => 3f,
                CultivationRealm.GoldenCore => 6f,
                _ => 0.5f
            };
            
            return baseRegen + (stage - 1) * (baseRegen * 0.15f);
        }
        
        private float GetTalentStatMultiplier()
        {
            return talent switch
            {
                TalentLevel.Common => 1f,
                TalentLevel.Rare => 1.2f,
                TalentLevel.Genius => 1.5f,
                TalentLevel.HeavenChosen => 2f,
                _ => 1f
            };
        }
        
        private float GetTalentEfficiencyMultiplier()
        {
            return talent switch
            {
                TalentLevel.Common => 1f,
                TalentLevel.Rare => 1.25f,
                TalentLevel.Genius => 1.6f,
                TalentLevel.HeavenChosen => 2.2f,
                _ => 1f
            };
        }
        
        private float GetTalentBreakthroughMultiplier()
        {
            return talent switch
            {
                TalentLevel.Common => 1f,
                TalentLevel.Rare => 1.1f,
                TalentLevel.Genius => 1.3f,
                TalentLevel.HeavenChosen => 1.6f,
                _ => 1f
            };
        }
        
        private float GetBaseBreakthroughChance()
        {
            return (currentRealm, currentStage) switch
            {
                (CultivationRealm.Mortal, 1) => 0.85f,
                (CultivationRealm.Mortal, 2) => 0.80f,
                (CultivationRealm.Mortal, 3) => 0.75f,
                (CultivationRealm.QiCondensation, <= 3) => 0.9f - (currentStage - 1) * 0.05f,
                (CultivationRealm.QiCondensation, <= 6) => 0.8f - (currentStage - 4) * 0.05f,
                (CultivationRealm.QiCondensation, <= 9) => 0.65f - (currentStage - 7) * 0.1f,
                (CultivationRealm.FoundationEstablishment, _) => 0.6f - (currentStage - 1) * 0.08f,
                (CultivationRealm.GoldenCore, _) => 0.25f - (currentStage - 1) * 0.05f,
                _ => 0.5f
            };
        }
        
        #endregion
        
        #region Talent System
        
        private void InitializeTalent()
        {
            if (pawn == null) return;
            
            // Initialize random talent based on pawn backstory and traits
            var random = new System.Random(pawn.thingIDNumber);
            
            float talentRoll = (float)random.NextDouble();
            talent = talentRoll switch
            {
                < 0.6f => TalentLevel.Common,
                < 0.85f => TalentLevel.Rare,
                < 0.97f => TalentLevel.Genius,
                _ => TalentLevel.HeavenChosen
            };
            
            // Initialize affinities based on talent
            affinities.RandomizeAffinities(talent, random);
        }
        
        #endregion
        
        #region Advanced Methods
        
        /// <summary>Add cultivation experience with efficiency calculation</summary>
        public void AddCultivationExperience(float experience, float environmentMultiplier = 1f)
        {
            float totalExperience = experience * CultivationEfficiency * environmentMultiplier;
            cultivationPoints += totalExperience;
            
            // Update progress tracking
            progress?.AddCultivationTime(1, environmentMultiplier);
            
            // Check for automatic breakthroughs if enabled
            CheckAutomaticBreakthrough();
        }
        
        /// <summary>Check if automatic breakthrough should occur</summary>
        private void CheckAutomaticBreakthrough()
        {
            if (cultivationPoints >= GetBreakthroughCost() && UnityEngine.Random.value < BreakthroughChance * 0.1f)
            {
                AttemptBreakthrough();
            }
        }
        
        /// <summary>Get cost for next breakthrough</summary>
        public float GetBreakthroughCost()
        {
            return (currentRealm, currentStage) switch
            {
                (CultivationRealm.Mortal, _) => 100f * currentStage,
                (CultivationRealm.QiCondensation, _) => 300f * currentStage,
                (CultivationRealm.FoundationEstablishment, _) => 800f * currentStage,
                (CultivationRealm.GoldenCore, _) => 2000f * currentStage,
                _ => 1000f
            };
        }
        
        /// <summary>Attempt breakthrough to next stage/realm</summary>
        public bool AttemptBreakthrough()
        {
            bool success = UnityEngine.Random.value < BreakthroughChance;
            
            // Record attempt
            progress?.RecordBreakthroughAttempt(success, GetNextRealm());
            
            if (success)
            {
                AdvanceStageOrRealm();
                
                // Award milestone if first time reaching this level
                string milestone = $"{currentRealm}_{currentStage}";
                if (progress?.ShouldAwardMilestone(milestone) == true)
                {
                    progress.AwardMilestone(milestone);
                }
            }
            
            return success;
        }
        
        /// <summary>Advance to next stage or realm</summary>
        private void AdvanceStageOrRealm()
        {
            int maxStage = GetMaxStageForRealm(currentRealm);
            
            if (currentStage < maxStage)
            {
                currentStage++;
            }
            else if (currentRealm < CultivationRealm.GoldenCore)
            {
                currentRealm = GetNextRealm();
                currentStage = 1;
            }
            
            // Reset progress for new level
            progress.stageProgress = 0f;
            if (currentStage == 1) // New realm
            {
                progress.realmProgress = 0f;
            }
        }
        
        /// <summary>Get next realm in progression</summary>
        private CultivationRealm GetNextRealm()
        {
            return currentRealm switch
            {
                CultivationRealm.Mortal => CultivationRealm.QiCondensation,
                CultivationRealm.QiCondensation => CultivationRealm.FoundationEstablishment,
                CultivationRealm.FoundationEstablishment => CultivationRealm.GoldenCore,
                _ => currentRealm
            };
        }
        
        /// <summary>Get maximum stage for a realm</summary>
        private int GetMaxStageForRealm(CultivationRealm realm)
        {
            return realm switch
            {
                CultivationRealm.Mortal => 3,
                CultivationRealm.QiCondensation => 9,
                CultivationRealm.FoundationEstablishment => 6,
                CultivationRealm.GoldenCore => 3,
                _ => 1
            };
        }
        
        /// <summary>Get detailed cultivation status summary</summary>
        public string GetDetailedStatus()
        {
            var status = new System.Text.StringBuilder();
            status.AppendLine($"Realm: {currentRealm} Stage {currentStage}");
            status.AppendLine($"Qi: {currentQi:F1}/{MaxQi:F1} (Regen: {QiRegenRate:F2}/h)");
            status.AppendLine($"Cultivation Points: {cultivationPoints:F0}");
            status.AppendLine($"Talent: {talent}");
            status.AppendLine($"Efficiency: {CultivationEfficiency:P0}");
            status.AppendLine($"Breakthrough Chance: {BreakthroughChance:P1}");
            
            if (affinities != null)
            {
                var strongestAffinity = affinities.GetStrongestElementalAffinity();
                status.AppendLine($"Strongest Affinity: {strongestAffinity.element} ({strongestAffinity.affinity:F2}x)");
            }
            
            if (progress != null)
            {
                status.AppendLine($"Cultivation Time: {progress.totalCultivationTime / 60000:F1}h");
                status.AppendLine($"Breakthrough Success Rate: {progress.GetBreakthroughSuccessRate():P1}");
            }
            
            return status.ToString();
        }
        
        #endregion
        
        #region Serialization
        
        public void ExposeData()
        {
            // Core properties
            Scribe_Values.Look(ref _currentRealm, "currentRealm", CultivationRealm.Mortal);
            Scribe_Values.Look(ref _currentStage, "currentStage", 1);
            Scribe_Values.Look(ref _currentQi, "currentQi", 0f);
            Scribe_Values.Look(ref _cultivationPoints, "cultivationPoints", 0f);
            Scribe_Values.Look(ref _talent, "talent", TalentLevel.Common);
            Scribe_Values.Look(ref _lastBreakthroughTick, "lastBreakthroughTick", -999999);
            
            // Enhanced components
            Scribe_Deep.Look(ref progress, "progress");
            Scribe_Deep.Look(ref affinities, "affinities");
            Scribe_Deep.Look(ref resources, "resources");
            
            // Legacy collections
            Scribe_Collections.Look(ref _unlockedSkills, "unlockedSkills");
            Scribe_Collections.Look(ref _knownTechniques, "knownTechniques");
            Scribe_Collections.Look(ref _skillCooldowns, "skillCooldowns");
            
            // Initialize components if null after loading
            if (progress == null) progress = new CultivationProgress();
            if (affinities == null) affinities = new CultivationAffinities();
            if (resources == null) resources = new CultivationResources();
            
            // Invalidate cache after loading
            InvalidateCache();
        }
        
        #endregion
        
        #region Migration and Compatibility
        
        /// <summary>Migrate from legacy CultivationData</summary>
        public static CultivationDataEnhanced MigrateFromLegacy(CultivationData legacy, Pawn pawn)
        {
            if (legacy == null) return new CultivationDataEnhanced(pawn);
            
            var enhanced = new CultivationDataEnhanced(pawn)
            {
                currentRealm = legacy.currentRealm,
                currentStage = legacy.currentStage,
                currentQi = legacy.currentQi,
                cultivationPoints = legacy.cultivationPoints,
                talent = legacy.talent,
                lastBreakthroughTick = legacy.lastBreakthroughTick
            };
            
            // Migrate collections
            enhanced.unlockedSkills.AddRange(legacy.unlockedSkills ?? new List<CultivationSkillDef>());
            enhanced.knownTechniques.AddRange(legacy.knownTechniques ?? new List<CultivationTechniqueDef>());
            
            if (legacy.skillCooldowns != null)
            {
                foreach (var cooldown in legacy.skillCooldowns)
                {
                    enhanced.skillCooldowns[cooldown.Key] = cooldown.Value;
                }
            }
            
            return enhanced;
        }
        
        /// <summary>Set pawn reference for event triggers</summary>
        public void SetPawn(Pawn pawn)
        {
            this.pawn = pawn;
        }
        
        #endregion
    }
}
