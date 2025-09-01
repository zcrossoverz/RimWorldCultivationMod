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
    /// Manages cultivation skills for a single pawn
    /// Handles learning, progression, cooldowns, and activation
    /// </summary>
    public class CultivationSkillManager : IExposable
    {
        #region Fields
        
        /// <summary>Owner pawn of this skill manager</summary>
        public Pawn pawn;
        
        /// <summary>Learned skills with their current levels and experience</summary>
        private Dictionary<string, CultivationSkillData> learnedSkills = new Dictionary<string, CultivationSkillData>();
        
        /// <summary>Skill cooldowns (defName -> tick when cooldown expires)</summary>
        private Dictionary<string, int> skillCooldowns = new Dictionary<string, int>();
        
        /// <summary>Available skill points for learning new skills</summary>
        public int skillPoints = 0;
        
        /// <summary>Total experience gained across all skills</summary>
        public float totalExperience = 0f;
        
        #endregion
        
        #region Constructors
        
        public CultivationSkillManager()
        {
        }
        
        public CultivationSkillManager(Pawn pawn)
        {
            this.pawn = pawn;
        }
        
        #endregion
        
        #region Core Skill Management
        
        /// <summary>
        /// Learn a new skill
        /// </summary>
        public bool LearnSkill(string skillDefName, bool paySkillPoints = true)
        {
            var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillDefName);
            if (skillDef == null)
            {
                Log.Warning($"[TuTien] Skill def not found: {skillDefName}");
                return false;
            }
            
            // Check if already learned
            if (HasLearnedSkill(skillDefName))
            {
                return false;
            }
            
            // Validate learning requirements
            if (!skillDef.CanLearnSkill(pawn, out string failReason))
            {
                if (pawn.IsColonist)
                {
                    Messages.Message($"Cannot learn {skillDef.LabelCap}: {failReason}", 
                        pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            
            // Check skill points cost (if enabled)
            int skillPointCost = CalculateSkillPointCost(skillDef);
            if (paySkillPoints && skillPoints < skillPointCost)
            {
                if (pawn.IsColonist)
                {
                    Messages.Message($"Insufficient skill points to learn {skillDef.LabelCap} (need {skillPointCost}, have {skillPoints})", 
                        pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            
            // Learn the skill
            var skillData = new CultivationSkillData
            {
                defName = skillDefName,
                level = 1,
                experience = 0f,
                learnedTick = Find.TickManager.TicksGame
            };
            
            learnedSkills[skillDefName] = skillData;
            
            // Deduct skill points
            if (paySkillPoints)
            {
                skillPoints -= skillPointCost;
            }
            
            // Apply passive effects if applicable
            if (!skillDef.isActive && skillDef.Worker is PassiveCultivationSkillWorker passiveWorker)
            {
                var progressionLevel = skillDef.GetProgressionLevel(1);
                passiveWorker.ApplyPassiveEffect(pawn, 1, progressionLevel);
            }
            
            // Trigger events
            CultivationEvents.TriggerSkillUnlocked(pawn, skillDef);
            
            if (pawn.IsColonist)
            {
                Messages.Message($"{pawn.Name.ToStringShort} learned {skillDef.LabelCap}!", 
                    pawn, MessageTypeDefOf.PositiveEvent, false);
            }
            
            return true;
        }
        
        /// <summary>
        /// Use an active skill
        /// </summary>
        public bool UseSkill(string skillDefName)
        {
            var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillDefName);
            if (skillDef?.Worker == null)
            {
                return false;
            }
            
            skillDef.Worker.Execute(pawn, skillDef);
            return true;
        }
        
        /// <summary>
        /// Add experience to a skill
        /// </summary>
        public void AddSkillExperience(string skillDefName, float experience)
        {
            if (!learnedSkills.ContainsKey(skillDefName)) return;
            
            var skillData = learnedSkills[skillDefName];
            var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillDefName);
            if (skillDef == null) return;
            
            skillData.experience += experience;
            totalExperience += experience;
            
            // Check for level up
            CheckForLevelUp(skillDefName, skillData, skillDef);
        }
        
        /// <summary>
        /// Check if skill should level up
        /// </summary>
        private void CheckForLevelUp(string skillDefName, CultivationSkillData skillData, CultivationSkillDef skillDef)
        {
            int newLevel = CalculateSkillLevelFromExperience(skillDef, skillData.experience);
            
            if (newLevel > skillData.level && newLevel <= skillDef.maxLevel)
            {
                int oldLevel = skillData.level;
                skillData.level = newLevel;
                
                // Update passive effects if applicable
                if (!skillDef.isActive && skillDef.Worker is PassiveCultivationSkillWorker passiveWorker)
                {
                    var progressionLevel = skillDef.GetProgressionLevel(newLevel);
                    passiveWorker.UpdatePassiveEffect(pawn, newLevel, progressionLevel);
                }
                
                if (pawn.IsColonist)
                {
                    Messages.Message($"{pawn.Name.ToStringShort}'s {skillDef.LabelCap} reached level {newLevel}!", 
                        pawn, MessageTypeDefOf.PositiveEvent, false);
                }
                
                // Award skill points for level ups
                AwardSkillPoints(newLevel - oldLevel);
            }
        }
        
        /// <summary>
        /// Calculate skill level from total experience
        /// </summary>
        private int CalculateSkillLevelFromExperience(CultivationSkillDef skillDef, float experience)
        {
            for (int level = 1; level <= skillDef.maxLevel; level++)
            {
                float required = skillDef.GetExperienceRequiredForLevel(level);
                if (experience < required)
                {
                    return level - 1;
                }
            }
            return skillDef.maxLevel;
        }
        
        #endregion
        
        #region Skill Points System
        
        /// <summary>
        /// Award skill points (from level ups, breakthroughs, etc.)
        /// </summary>
        public void AwardSkillPoints(int amount)
        {
            skillPoints += amount;
            
            if (pawn.IsColonist && amount > 0)
            {
                Messages.Message($"{pawn.Name.ToStringShort} gained {amount} skill points!", 
                    pawn, MessageTypeDefOf.PositiveEvent, false);
            }
        }
        
        /// <summary>
        /// Calculate skill point cost to learn a skill
        /// </summary>
        private int CalculateSkillPointCost(CultivationSkillDef skillDef)
        {
            int baseCost = 1;
            
            // Higher realm requirements cost more
            baseCost += (int)skillDef.requiredRealm;
            
            // Prerequisites increase cost
            if (skillDef.HasPrerequisites)
            {
                baseCost += skillDef.prerequisiteSkills.Count;
            }
            
            // Talent requirements increase cost
            if (skillDef.requiredTalent.HasValue)
            {
                baseCost += (int)skillDef.requiredTalent.Value;
            }
            
            return Mathf.Max(1, baseCost);
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// Check if pawn has learned a specific skill
        /// </summary>
        public bool HasLearnedSkill(string skillDefName)
        {
            return learnedSkills.ContainsKey(skillDefName);
        }
        
        /// <summary>
        /// Get current level of a skill
        /// </summary>
        public int GetSkillLevel(string skillDefName)
        {
            return learnedSkills.TryGetValue(skillDefName, out var skillData) ? skillData.level : 0;
        }
        
        /// <summary>
        /// Get current experience for a skill
        /// </summary>
        public float GetSkillExperience(string skillDefName)
        {
            return learnedSkills.TryGetValue(skillDefName, out var skillData) ? skillData.experience : 0f;
        }
        
        /// <summary>
        /// Get all learned skills
        /// </summary>
        public IEnumerable<CultivationSkillData> GetAllLearnedSkills()
        {
            return learnedSkills.Values;
        }
        
        /// <summary>
        /// Get skills by category
        /// </summary>
        public IEnumerable<CultivationSkillData> GetSkillsByCategory(CultivationSkillCategory category)
        {
            return learnedSkills.Values.Where(skillData =>
            {
                var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillData.defName);
                return skillDef?.category == category;
            });
        }
        
        /// <summary>
        /// Get available skills that can be learned
        /// </summary>
        public IEnumerable<CultivationSkillDef> GetAvailableSkills()
        {
            return DefDatabase<CultivationSkillDef>.AllDefs.Where(skillDef =>
            {
                return !HasLearnedSkill(skillDef.defName) && 
                       skillDef.CanLearnSkill(pawn, out _);
            });
        }
        
        #endregion
        
        #region Cooldown Management
        
        /// <summary>
        /// Check if a skill is on cooldown
        /// </summary>
        public bool IsSkillOnCooldown(string skillDefName)
        {
            if (!skillCooldowns.TryGetValue(skillDefName, out int cooldownExpires))
                return false;
                
            return Find.TickManager.TicksGame < cooldownExpires;
        }
        
        /// <summary>
        /// Get remaining cooldown ticks for a skill
        /// </summary>
        public int GetSkillCooldownRemaining(string skillDefName)
        {
            if (!skillCooldowns.TryGetValue(skillDefName, out int cooldownExpires))
                return 0;
                
            return Mathf.Max(0, cooldownExpires - Find.TickManager.TicksGame);
        }
        
        /// <summary>
        /// Set cooldown for a skill
        /// </summary>
        public void SetSkillCooldown(string skillDefName, int durationTicks)
        {
            skillCooldowns[skillDefName] = Find.TickManager.TicksGame + durationTicks;
        }
        
        /// <summary>
        /// Process cooldowns each tick (called from CultivationComp)
        /// </summary>
        public void ProcessCooldowns()
        {
            var currentTick = Find.TickManager.TicksGame;
            var expiredSkills = new List<string>();
            
            foreach (var kvp in skillCooldowns)
            {
                if (currentTick >= kvp.Value)
                {
                    expiredSkills.Add(kvp.Key);
                }
            }
            
            // Remove expired cooldowns and trigger events
            foreach (string skillDefName in expiredSkills)
            {
                skillCooldowns.Remove(skillDefName);
                
                var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillDefName);
                if (skillDef != null)
                {
                    CultivationEvents.TriggerSkillCooldownExpired(pawn, skillDef);
                }
            }
        }
        
        #endregion
        
        #region Progression System
        
        /// <summary>
        /// Get skills that provide bonuses at current cultivation level
        /// </summary>
        public IEnumerable<CultivationSkillBonus> GetActiveSkillBonuses()
        {
            var cultivationData = pawn.GetComp<CultivationComp>()?.cultivationData;
            if (cultivationData == null) yield break;
            
            foreach (var skillData in learnedSkills.Values)
            {
                var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillData.defName);
                if (skillDef == null) continue;
                
                var progressionLevel = skillDef.GetProgressionLevel(skillData.level);
                if (progressionLevel?.statModifiers != null)
                {
                    yield return new CultivationSkillBonus
                    {
                        skillDef = skillDef,
                        level = skillData.level,
                        statModifiers = progressionLevel.statModifiers,
                        effectMultiplier = progressionLevel.effectMultiplier
                    };
                }
            }
        }
        
        /// <summary>
        /// Calculate total stat bonus from all skills
        /// </summary>
        public float GetTotalStatBonus(StatDef statDef)
        {
            float totalBonus = 0f;
            
            foreach (var bonus in GetActiveSkillBonuses())
            {
                foreach (var modifier in bonus.statModifiers)
                {
                    if (modifier.stat == statDef)
                    {
                        totalBonus += modifier.value * bonus.effectMultiplier;
                    }
                }
            }
            
            return totalBonus;
        }
        
        #endregion
        
        #region Auto-Learning System
        
        /// <summary>
        /// Check for skills that should be auto-learned based on cultivation progress
        /// </summary>
        public void CheckForAutoLearnedSkills()
        {
            var cultivationData = pawn.GetComp<CultivationComp>()?.cultivationData;
            if (cultivationData == null) return;
            
            foreach (var skillDef in DefDatabase<CultivationSkillDef>.AllDefs)
            {
                if (HasLearnedSkill(skillDef.defName)) continue;
                
                // Auto-learn basic skills when reaching new realms
                if (ShouldAutoLearnSkill(skillDef, cultivationData))
                {
                    LearnSkill(skillDef.defName, paySkillPoints: false);
                }
            }
        }
        
        /// <summary>
        /// Determine if a skill should be auto-learned
        /// </summary>
        private bool ShouldAutoLearnSkill(CultivationSkillDef skillDef, CultivationData cultivationData)
        {
            // Auto-learn basic skills for new realms
            if (skillDef.skillTags.Contains("AutoLearn") && 
                cultivationData.currentRealm >= skillDef.requiredRealm &&
                (cultivationData.currentRealm > skillDef.requiredRealm || cultivationData.currentStage >= skillDef.requiredStage))
            {
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region Skill Discovery System
        
        /// <summary>
        /// Discover new skills based on experience and experimentation
        /// </summary>
        public void ProcessSkillDiscovery()
        {
            // Random chance to discover skills through experimentation
            if (Rand.ChanceSeeded(0.001f, pawn.thingIDNumber + Find.TickManager.TicksGame))
            {
                TryDiscoverRandomSkill();
            }
        }
        
        /// <summary>
        /// Try to discover a random skill
        /// </summary>
        private void TryDiscoverRandomSkill()
        {
            var availableSkills = GetAvailableSkills().Where(s => s.skillTags.Contains("Discoverable")).ToList();
            if (availableSkills.Count == 0) return;
            
            var skillToDiscover = availableSkills.RandomElement();
            
            if (pawn.IsColonist)
            {
                Messages.Message($"{pawn.Name.ToStringShort} discovered new cultivation technique: {skillToDiscover.LabelCap}!", 
                    pawn, MessageTypeDefOf.PositiveEvent, false);
            }
            
            // Add to available skills (but don't auto-learn)
            // This could be implemented as a separate "discovered but not learned" system
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get skill data for a specific skill
        /// </summary>
        public CultivationSkillData GetSkillData(string skillDefName)
        {
            return learnedSkills.TryGetValue(skillDefName, out var data) ? data : null;
        }
        
        /// <summary>
        /// Get total number of learned skills
        /// </summary>
        public int GetTotalLearnedSkills()
        {
            return learnedSkills.Count;
        }
        
        /// <summary>
        /// Get highest skill level across all skills
        /// </summary>
        public int GetHighestSkillLevel()
        {
            return learnedSkills.Values.Count > 0 ? learnedSkills.Values.Max(s => s.level) : 0;
        }
        
        /// <summary>
        /// Reset all skill cooldowns (for debug/admin use)
        /// </summary>
        public void ResetAllCooldowns()
        {
            skillCooldowns.Clear();
        }
        
        #endregion
        
        #region Serialization
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref learnedSkills, "learnedSkills", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref skillCooldowns, "skillCooldowns", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref skillPoints, "skillPoints", 0);
            Scribe_Values.Look(ref totalExperience, "totalExperience", 0f);
            
            // Initialize collections if null after loading
            if (learnedSkills == null)
                learnedSkills = new Dictionary<string, CultivationSkillData>();
            if (skillCooldowns == null)
                skillCooldowns = new Dictionary<string, int>();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Data for a single learned skill
    /// </summary>
    [System.Serializable]
    public class CultivationSkillData : IExposable
    {
        /// <summary>Skill definition name</summary>
        public string defName;
        
        /// <summary>Current skill level</summary>
        public int level = 1;
        
        /// <summary>Total experience in this skill</summary>
        public float experience = 0f;
        
        /// <summary>Game tick when this skill was learned</summary>
        public int learnedTick = 0;
        
        /// <summary>Number of times this skill has been used</summary>
        public int usageCount = 0;
        
        /// <summary>Custom data for skill-specific information</summary>
        public Dictionary<string, object> customData = new Dictionary<string, object>();
        
        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref level, "level", 1);
            Scribe_Values.Look(ref experience, "experience", 0f);
            Scribe_Values.Look(ref learnedTick, "learnedTick", 0);
            Scribe_Values.Look(ref usageCount, "usageCount", 0);
            // Note: customData serialization would need special handling if needed
        }
    }
    
    /// <summary>
    /// Skill bonus information for stat calculations
    /// </summary>
    public class CultivationSkillBonus
    {
        public CultivationSkillDef skillDef;
        public int level;
        public List<StatModifier> statModifiers;
        public float effectMultiplier;
    }
}
