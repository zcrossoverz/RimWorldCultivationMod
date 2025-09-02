using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien
{
    /// <summary>
    /// Enhanced cultivation skill definition with progression system
    /// </summary>
    public class CultivationSkillDef : Def
    {
        #region XML Fields
        
        /// <summary>Translation key for skill label (optional)</summary>
        public string labelKey;
        
        /// <summary>Translation key for skill description (optional)</summary>
        public string descriptionKey;
        
        /// <summary>Required cultivation realm to unlock this skill</summary>
        public CultivationRealm requiredRealm = CultivationRealm.Mortal;
        
        /// <summary>Required stage within the realm</summary>
        public int requiredStage = 1;
        
        /// <summary>Whether this is an active skill (requires activation) or passive</summary>
        public bool isActive = true;
        
        /// <summary>Qi cost to use this skill (for active skills)</summary>
        public int qiCost = 0;
        
        /// <summary>Cooldown in hours after using this skill</summary>
        public float cooldownHours = 0f;
        
        /// <summary>Worker class that handles skill execution</summary>
        public Type workerClass;
        
        /// <summary>Skill category for organization</summary>
        public CultivationSkillCategory category = CultivationSkillCategory.Combat;
        
        /// <summary>Required talent level to learn this skill</summary>
        public TalentLevel? requiredTalent = null;
        
        /// <summary>Prerequisites - other skills that must be learned first</summary>
        public List<string> prerequisiteSkills = new List<string>();
        
        /// <summary>Skill progression levels with different effects</summary>
        public List<SkillProgressionLevel> progressionLevels = new List<SkillProgressionLevel>();
        
        /// <summary>Base experience required to unlock this skill</summary>
        public float baseExperienceRequired = 100f;
        
        /// <summary>Multiplier for experience gain when practicing this skill</summary>
        public float experienceMultiplier = 1f;
        
        /// <summary>Maximum level this skill can reach</summary>
        public int maxLevel = 10;
        
        /// <summary>Tags for skill filtering and organization</summary>
        public List<string> skillTags = new List<string>();
        
        #endregion
        
        #region Computed Properties
        
        /// <summary>Get the worker instance for this skill</summary>
        public ICultivationSkillWorker Worker
        {
            get
            {
                if (_worker == null && workerClass != null)
                {
                    _worker = (ICultivationSkillWorker)Activator.CreateInstance(workerClass);
                    _worker.def = this;
                }
                return _worker;
            }
        }
        private ICultivationSkillWorker _worker;
        
        /// <summary>Check if this skill has progression levels</summary>
        public bool HasProgression => progressionLevels.Count > 0;
        
        /// <summary>Check if this skill has prerequisites</summary>
        public bool HasPrerequisites => prerequisiteSkills.Count > 0;
        
        /// <summary>Get cooldown in ticks</summary>
        public int CooldownTicks => Mathf.RoundToInt(cooldownHours * GenDate.TicksPerHour);
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Check if a pawn can learn this skill
        /// </summary>
        public bool CanLearnSkill(Pawn pawn, out string failReason)
        {
            failReason = null;
            
            var cultivationComp = pawn.GetComp<CultivationComp>();
            if (cultivationComp?.cultivationData == null)
            {
                failReason = "No cultivation data";
                return false;
            }
            
            var data = cultivationComp.cultivationData;
            
            // Check realm and stage requirements
            if (data.currentRealm < requiredRealm || 
                (data.currentRealm == requiredRealm && data.currentStage < requiredStage))
            {
                failReason = $"Requires {requiredRealm} stage {requiredStage}";
                return false;
            }
            
            // Check talent requirements
            if (requiredTalent.HasValue && data.talent < requiredTalent.Value)
            {
                failReason = $"Requires {requiredTalent.Value} talent or higher";
                return false;
            }
            
            // Check prerequisites
            if (HasPrerequisites)
            {
                var skillManager = cultivationComp.GetSkillManager();
                foreach (string prereq in prerequisiteSkills)
                {
                    if (!skillManager.HasLearnedSkill(prereq))
                    {
                        failReason = $"Requires prerequisite skill: {prereq}";
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if a pawn can use this skill right now
        /// </summary>
        public bool CanUseSkill(Pawn pawn, out string failReason)
        {
            failReason = null;
            
            if (!isActive)
            {
                failReason = "Passive skill cannot be activated";
                return false;
            }
            
            var cultivationComp = pawn.GetComp<CultivationComp>();
            if (cultivationComp?.cultivationData == null)
            {
                failReason = "No cultivation data";
                return false;
            }
            
            var data = cultivationComp.cultivationData;
            var skillManager = cultivationComp.GetSkillManager();
            
            // Check if skill is learned
            if (!skillManager.HasLearnedSkill(defName))
            {
                failReason = "Skill not learned";
                return false;
            }
            
            // Check qi cost
            if (data.currentQi < qiCost)
            {
                failReason = $"Insufficient qi (need {qiCost}, have {data.currentQi})";
                return false;
            }
            
            // Check cooldown
            if (skillManager.IsSkillOnCooldown(defName))
            {
                var remainingTicks = skillManager.GetSkillCooldownRemaining(defName);
                failReason = $"On cooldown for {remainingTicks.ToStringSecondsFromTicks()}";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get the experience required for a specific level
        /// </summary>
        public float GetExperienceRequiredForLevel(int level)
        {
            if (level <= 0) return 0f;
            
            // Exponential progression: base * (level^1.5)
            return baseExperienceRequired * Mathf.Pow(level, 1.5f);
        }
        
        /// <summary>
        /// Get the progression level data for a specific level
        /// </summary>
        public SkillProgressionLevel GetProgressionLevel(int level)
        {
            if (level <= 0 || level > progressionLevels.Count)
                return null;
                
            return progressionLevels[level - 1];
        }
        
        #endregion
        
        #region DefOf Integration
        
        public override void PostLoad()
        {
            base.PostLoad();
            
            // Validate worker class
            if (isActive && workerClass == null)
            {
                Log.Warning($"[TuTien] Active skill {defName} has no worker class defined");
            }
            
            // Validate progression levels
            if (progressionLevels.Count > maxLevel)
            {
                Log.Warning($"[TuTien] Skill {defName} has more progression levels ({progressionLevels.Count}) than max level ({maxLevel})");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Skill categories for organization
    /// </summary>
    public enum CultivationSkillCategory
    {
        Combat,
        Movement,
        Utility,
        Enhancement,
        Technique,
        Passive
    }
    
    /// <summary>
    /// Progression level data for skills
    /// </summary>
    [System.Serializable]
    public class SkillProgressionLevel
    {
        /// <summary>Level number (1-based)</summary>
        public int level = 1;
        
        /// <summary>Description of what this level provides</summary>
        public string description = "";
        
        /// <summary>Stat modifiers at this level</summary>
        public List<StatModifier> statModifiers = new List<StatModifier>();
        
        /// <summary>Multiplier for skill effects (damage, duration, etc.)</summary>
        public float effectMultiplier = 1f;
        
        /// <summary>Reduction in qi cost at this level</summary>
        public float qiCostReduction = 0f;
        
        /// <summary>Reduction in cooldown at this level</summary>
        public float cooldownReduction = 0f;
        
        /// <summary>Additional abilities unlocked at this level</summary>
        public List<string> unlockedAbilities = new List<string>();
    }
}
