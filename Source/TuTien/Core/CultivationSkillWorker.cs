using System;
using UnityEngine;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien
{
    /// <summary>
    /// Base class for all cultivation skill workers
    /// Handles execution, validation, and progression logic
    /// </summary>
    public abstract class CultivationSkillWorker
    {
        #region Properties
        
        /// <summary>The skill definition this worker handles</summary>
        public CultivationSkillDef def { get; set; }
        
        #endregion
        
        #region Core Methods
        
        /// <summary>
        /// Execute the skill effect
        /// </summary>
        public virtual void Execute(Pawn pawn, CultivationSkillDef skill)
        {
            if (!ValidateExecution(pawn, skill, out string failReason))
            {
                if (pawn.IsColonist)
                {
                    Messages.Message($"Cannot use {skill.LabelCap}: {failReason}", 
                        pawn, MessageTypeDefOf.RejectInput, false);
                }
                return;
            }
            
            // Get skill level for scaling effects
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            int skillLevel = skillManager?.GetSkillLevel(skill.defName) ?? 1;
            var progressionLevel = skill.GetProgressionLevel(skillLevel);
            
            // Execute the actual skill effect
            ExecuteSkillEffect(pawn, skill, skillLevel, progressionLevel);
            
            // Apply costs and cooldowns
            ApplyCosts(pawn, skill, progressionLevel);
            
            // Trigger events
            CultivationEvents.TriggerSkillUsed(pawn, skill);
            
            // Gain experience
            GainExperience(pawn, skill, skillLevel);
        }
        
        /// <summary>
        /// Validate if the skill can be executed
        /// </summary>
        protected virtual bool ValidateExecution(Pawn pawn, CultivationSkillDef skill, out string failReason)
        {
            // Use the skill def's validation
            return skill.CanUseSkill(pawn, out failReason);
        }
        
        /// <summary>
        /// Execute the actual skill effect - must be implemented by derived classes
        /// </summary>
        protected abstract void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill, int level, SkillProgressionLevel progressionLevel);
        
        /// <summary>
        /// Apply qi costs and cooldowns
        /// </summary>
        protected virtual void ApplyCosts(Pawn pawn, CultivationSkillDef skill, SkillProgressionLevel progressionLevel)
        {
            var cultivationData = pawn.GetComp<CultivationComp>()?.cultivationData;
            if (cultivationData == null) return;
            
            // Calculate actual qi cost with progression reduction
            float actualQiCost = skill.qiCost;
            if (progressionLevel != null)
            {
                actualQiCost *= (1f - progressionLevel.qiCostReduction);
            }
            actualQiCost = Mathf.Max(1f, actualQiCost); // Minimum 1 qi
            
            // Consume qi
            cultivationData.currentQi = Mathf.Max(0, cultivationData.currentQi - Mathf.RoundToInt(actualQiCost));
            
            // Apply cooldown with progression bonuses (both systems integrated)
            if (skill.cooldownHours > 0)
            {
                var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
                if (skillManager != null)
                {
                    float actualCooldown = skill.cooldownHours;
                    if (progressionLevel != null)
                    {
                        actualCooldown *= (1f - progressionLevel.cooldownReduction);
                    }
                    actualCooldown = Mathf.Max(0.1f, actualCooldown); // Minimum 6 minutes
                    
                    int cooldownTicks = Mathf.RoundToInt(actualCooldown * GenDate.TicksPerHour);
                    
                    // Update both cooldown systems for full integration
                    cultivationData.skillCooldowns[skill.defName] = cooldownTicks;
                    skillManager.SetSkillCooldown(skill.defName, cooldownTicks);
                    
                    Log.Message($"[TuTien] Integrated cooldown: {skill.defName} = {actualCooldown:F1}h ({cooldownTicks} ticks)");
                }
            }
        }
        
        /// <summary>
        /// Gain experience for using this skill
        /// </summary>
        protected virtual void GainExperience(Pawn pawn, CultivationSkillDef skill, int currentLevel)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager == null) return;
            
            // Base experience gain
            float baseExp = 5f * skill.experienceMultiplier;
            
            // Apply talent modifiers
            var cultivationData = pawn.GetComp<CultivationComp>()?.cultivationData;
            if (cultivationData != null)
            {
                float talentMultiplier = GetTalentMultiplier(cultivationData.talent);
                baseExp *= talentMultiplier;
            }
            
            // Higher levels gain less experience
            float levelPenalty = 1f / (1f + currentLevel * 0.1f);
            baseExp *= levelPenalty;
            
            skillManager.AddSkillExperience(skill.defName, baseExp);
        }
        
        /// <summary>
        /// Get talent multiplier for experience gain
        /// </summary>
        protected virtual float GetTalentMultiplier(TalentLevel talent)
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
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get skill power scaling based on level and progression
        /// </summary>
        protected virtual float GetSkillPower(int level, SkillProgressionLevel progressionLevel)
        {
            float basePower = 1f + (level - 1) * 0.2f; // 20% increase per level
            
            if (progressionLevel != null)
            {
                basePower *= progressionLevel.effectMultiplier;
            }
            
            return basePower;
        }
        
        /// <summary>
        /// Apply stat bonuses from skill progression
        /// </summary>
        protected virtual void ApplyStatBonuses(Pawn pawn, SkillProgressionLevel progressionLevel)
        {
            if (progressionLevel?.statModifiers == null) return;
            
            // This would integrate with RimWorld's stat system
            // Implementation depends on how permanent/temporary the bonuses should be
            foreach (var modifier in progressionLevel.statModifiers)
            {
                // Apply stat modifier to pawn
                // This might involve hediffs or direct stat modifications
            }
        }
        
        /// <summary>
        /// Get localized description with current level effects
        /// </summary>
        public virtual string GetDetailedDescription(Pawn pawn)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager == null) return def.description;
            
            int currentLevel = skillManager.GetSkillLevel(def.defName);
            var progressionLevel = def.GetProgressionLevel(currentLevel);
            
            string desc = def.description;
            
            if (progressionLevel != null && !progressionLevel.description.NullOrEmpty())
            {
                desc += $"\n\nLevel {currentLevel}: {progressionLevel.description}";
            }
            
            // Add power scaling info
            float power = GetSkillPower(currentLevel, progressionLevel);
            if (power != 1f)
            {
                desc += $"\nCurrent power: {power:P0}";
            }
            
            return desc;
        }
        
        #endregion
        
        #region Effects Framework
        
        /// <summary>
        /// Create a temporary hediff effect
        /// </summary>
        protected virtual void ApplyTemporaryEffect(Pawn pawn, HediffDef hediffDef, float durationHours, float severity = 1f)
        {
            if (hediffDef == null) return;
            
            var hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            hediff.Severity = severity;
            
            // Set duration if the hediff supports it
            if (hediff is HediffWithComps hediffComps)
            {
                var comp = hediffComps.TryGetComp<HediffComp_Disappears>();
                if (comp != null)
                {
                    comp.ticksToDisappear = Mathf.RoundToInt(durationHours * GenDate.TicksPerHour);
                }
            }
            
            pawn.health.AddHediff(hediff);
        }
        
        /// <summary>
        /// Deal qi-enhanced damage to a target
        /// </summary>
        protected virtual void DealQiDamage(Pawn attacker, Thing target, float baseDamage, float qiMultiplier = 1f)
        {
            if (target is Pawn targetPawn)
            {
                float totalDamage = baseDamage * qiMultiplier;
                
                DamageInfo damageInfo = new DamageInfo(
                    DamageDefOf.Blunt, // Could be a custom qi damage type
                    totalDamage,
                    0f, // Armor penetration
                    -1f, // Angle
                    attacker,
                    null, // Body part
                    null, // Weapon
                    DamageInfo.SourceCategory.ThingOrUnknown
                );
                
                targetPawn.TakeDamage(damageInfo);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Base class for passive skill workers (no activation required)
    /// </summary>
    public abstract class PassiveCultivationSkillWorker : CultivationSkillWorker
    {
        /// <summary>
        /// Passive skills don't have execution effects, they provide permanent bonuses
        /// </summary>
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill, int level, SkillProgressionLevel progressionLevel)
        {
            // Passive skills don't execute, they provide continuous effects
            // These effects are typically handled in CultivationComp.CompTick or through hediffs
        }
        
        /// <summary>
        /// Apply the passive effect when skill is learned
        /// </summary>
        public abstract void ApplyPassiveEffect(Pawn pawn, int level, SkillProgressionLevel progressionLevel);
        
        /// <summary>
        /// Remove the passive effect when skill is unlearned
        /// </summary>
        public abstract void RemovePassiveEffect(Pawn pawn);
        
        /// <summary>
        /// Update passive effect when skill level changes
        /// </summary>
        public virtual void UpdatePassiveEffect(Pawn pawn, int newLevel, SkillProgressionLevel progressionLevel)
        {
            RemovePassiveEffect(pawn);
            ApplyPassiveEffect(pawn, newLevel, progressionLevel);
        }
    }
}
