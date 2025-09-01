using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien.Validation
{
    /// <summary>
    /// Comprehensive validation system for all cultivation operations
    /// Provides data integrity, input validation, and error handling
    /// </summary>
    public static class CultivationValidator
    {
        #region Constants
        
        /// <summary>Minimum talent value</summary>
        public const int MIN_TALENT = 1;
        
        /// <summary>Maximum talent value</summary>
        public const int MAX_TALENT = 10;
        
        #endregion
        
        #region Core Validation Methods
        
        /// <summary>
        /// Validate if a pawn can perform cultivation operations
        /// </summary>
        public static ValidationResult ValidatePawnCultivation(Pawn pawn)
        {
            var result = new ValidationResult();
            
            if (pawn == null)
            {
                result.AddError(ValidationErrorType.NullReference, "Pawn cannot be null");
                return result;
            }
            
            // Check if pawn is alive
            if (pawn.Dead)
            {
                result.AddError(ValidationErrorType.InvalidState, "Dead pawns cannot cultivate");
                return result;
            }
            
            // Check if pawn has cultivation comp
            var cultivationComp = pawn.GetComp<CultivationComp>();
            if (cultivationComp?.cultivationData == null)
            {
                result.AddError(ValidationErrorType.MissingComponent, "Pawn has no cultivation component or data");
                return result;
            }
            
            // Check basic cultivation data integrity
            var dataValidation = ValidateCultivationData(cultivationComp.cultivationData);
            result.MergeWith(dataValidation);
            
            return result;
        }
        
        /// <summary>
        /// Perform comprehensive integrity check on a pawn's cultivation data
        /// </summary>
        public static ValidationResult PerformIntegrityCheck(Pawn pawn)
        {
            var result = new ValidationResult();
            
            if (pawn?.GetComp<CultivationComp>()?.cultivationData == null)
            {
                result.AddError(ValidationErrorType.MissingComponent, "No cultivation data found");
                return result;
            }
            
            var data = pawn.GetComp<CultivationComp>().cultivationData;
            
            // Validate core data
            result.MergeWith(ValidateCultivationData(data));
            
            // Validate realm-stage combination
            if (!IsValidRealmStage(data.currentRealm, data.currentStage))
            {
                result.AddError(ValidationErrorType.InvalidData, "Invalid realm-stage combination");
            }
            
            // Validate Qi values
            if (data.currentQi < 0 || data.currentQi > data.maxQi * 1.2f)
            {
                result.AddError(ValidationErrorType.InvalidData, "Qi values are corrupted");
            }
            
            // Environmental checks
            if (pawn.Map == null)
            {
                result.AddWarning(ValidationWarningType.SuboptimalConditions, "Pawn not on map");
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate cultivation data integrity
        /// </summary>
        public static ValidationResult ValidateCultivationData(CultivationData data)
        {
            var result = new ValidationResult();
            
            if (data == null)
            {
                result.AddError(ValidationErrorType.NullReference, "CultivationData is null");
                return result;
            }
            
            // Validate Qi values
            if (data.currentQi < 0)
            {
                result.AddError(ValidationErrorType.InvalidData, "Current Qi cannot be negative");
            }
            
            if (data.maxQi <= 0)
            {
                result.AddError(ValidationErrorType.InvalidData, "Max Qi must be positive");
            }
            
            // Validate cultivation points
            if (data.cultivationPoints < 0)
            {
                result.AddError(ValidationErrorType.InvalidData, "Cultivation points cannot be negative");
            }
            
            // Validate talent
            if ((int)data.talent < MIN_TALENT || (int)data.talent > MAX_TALENT)
            {
                result.AddWarning(ValidationWarningType.DataInconsistency, "Talent value is unusual");
            }
            
            return result;
        }
        
        /// <summary>
        /// Check if a realm-stage combination is valid
        /// </summary>
        public static bool IsValidRealmStage(CultivationRealm realm, int stage)
        {
            int maxStage = GetMaxStageForRealm(realm);
            return stage >= 1 && stage <= maxStage;
        }
        
        /// <summary>
        /// Get maximum stage for a cultivation realm
        /// </summary>
        private static int GetMaxStageForRealm(CultivationRealm realm)
        {
            switch (realm)
            {
                case CultivationRealm.Mortal:
                    return 3;
                case CultivationRealm.QiCondensation:
                    return 9;
                case CultivationRealm.FoundationEstablishment:
                    return 6;
                case CultivationRealm.GoldenCore:
                    return 3;
                default:
                    return 1;
            }
        }
        
        /// <summary>
        /// Validate environmental conditions for cultivation
        /// </summary>
        public static ValidationResult ValidateEnvironmentalConditions(Pawn pawn)
        {
            var result = new ValidationResult();
            
            if (pawn?.Map == null)
            {
                result.AddWarning(ValidationWarningType.SuboptimalConditions, "Pawn not on map");
                return result;
            }
            
            // Check for injuries that might affect cultivation
            if (pawn.health?.hediffSet?.hediffs?.Any(h => h is Hediff_Injury) == true)
            {
                result.AddWarning(ValidationWarningType.SuboptimalConditions, "Pawn has injuries affecting cultivation");
            }
            
            // Check mood
            if (pawn.needs?.mood?.CurLevel < 0.3f)
            {
                result.AddWarning(ValidationWarningType.SuboptimalConditions, "Low mood affects cultivation efficiency");
            }
            
            return result;
        }
        
        #endregion
        
        #region Skill Validation
        
        /// <summary>
        /// Validate if a pawn can learn a specific skill
        /// </summary>
        public static ValidationResult ValidateSkillLearning(Pawn pawn, CultivationSkillDef skillDef)
        {
            var result = new ValidationResult();
            
            if (pawn == null || skillDef == null)
            {
                result.AddError(ValidationErrorType.NullReference, "Pawn or skill definition is null");
                return result;
            }
            
            var comp = pawn.GetComp<CultivationComp>();
            var skillManager = comp?.GetSkillManager();
            
            if (skillManager == null)
            {
                result.AddError(ValidationErrorType.MissingComponent, "No skill manager found");
                return result;
            }
            
            // Check if already learned
            if (skillManager.HasLearnedSkill(skillDef.defName))
            {
                result.AddWarning(ValidationWarningType.MaximumReached, "Skill already learned");
            }
            
            // Check prerequisites
            if (skillDef.HasPrerequisites)
            {
                foreach (string prereq in skillDef.prerequisiteSkills)
                {
                    if (!skillManager.HasLearnedSkill(prereq))
                    {
                        result.AddError(ValidationErrorType.PrerequisiteNotMet, $"Missing prerequisite: {prereq}");
                    }
                }
            }
            
            // Check realm requirements
            if (skillDef.requiredRealm > comp.cultivationData.currentRealm)
            {
                result.AddError(ValidationErrorType.RequirementNotMet, 
                    $"Requires {skillDef.requiredRealm}, current: {comp.cultivationData.currentRealm}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate if a pawn can use a specific skill
        /// </summary>
        public static ValidationResult ValidateSkillUsage(Pawn pawn, CultivationSkillDef skillDef)
        {
            var result = new ValidationResult();
            
            if (pawn == null || skillDef == null)
            {
                result.AddError(ValidationErrorType.NullReference, "Pawn or skill definition is null");
                return result;
            }
            
            var comp = pawn.GetComp<CultivationComp>();
            var skillManager = comp?.GetSkillManager();
            
            if (skillManager == null)
            {
                result.AddError(ValidationErrorType.MissingComponent, "No skill manager found");
                return result;
            }
            
            // Check if learned
            if (!skillManager.HasLearnedSkill(skillDef.defName))
            {
                result.AddError(ValidationErrorType.RequirementNotMet, "Skill not learned");
                return result;
            }
            
            // Check cooldown
            if (skillManager.IsSkillOnCooldown(skillDef.defName))
            {
                result.AddError(ValidationErrorType.OnCooldown, "Skill is on cooldown");
            }
            
            // Check Qi cost
            if (comp.cultivationData.currentQi < skillDef.qiCost)
            {
                result.AddError(ValidationErrorType.InsufficientResources, "Not enough Qi");
            }
            
            return result;
        }
        
        #endregion
        
        #region Technique Validation
        
        /// <summary>
        /// Validate if a pawn can practice a technique
        /// </summary>
        public static ValidationResult ValidateTechniquePractice(Pawn pawn, CultivationTechniqueDef techniqueDef)
        {
            var result = new ValidationResult();
            
            if (pawn == null || techniqueDef == null)
            {
                result.AddError(ValidationErrorType.NullReference, "Pawn or technique definition is null");
                return result;
            }
            
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null)
            {
                result.AddError(ValidationErrorType.MissingComponent, "No cultivation data");
                return result;
            }
            
            var data = comp.cultivationData;
            
            // Check if technique is known (simplified validation)
            if (!data.knownTechniques.Contains(techniqueDef))
            {
                result.AddError(ValidationErrorType.RequirementNotMet, "Technique not known");
            }
            
            return result;
        }
        
        #endregion
        
        #region Auto-Fix Methods
        
        /// <summary>
        /// Attempt to automatically fix corrupted cultivation data
        /// </summary>
        public static ValidationResult AutoFixData(CultivationData data)
        {
            var result = new ValidationResult();
            
            if (data == null)
            {
                result.AddError(ValidationErrorType.NullReference, "Cannot fix null data");
                return result;
            }
            
            bool wasFixed = false;
            
            // Fix negative Qi
            if (data.currentQi < 0)
            {
                data.currentQi = 0;
                result.AddInfo(ValidationInfoType.SystemStatus, "Fixed negative current Qi");
                wasFixed = true;
            }
            
            // Fix negative cultivation points
            if (data.cultivationPoints < 0)
            {
                data.cultivationPoints = 0;
                result.AddInfo(ValidationInfoType.SystemStatus, "Fixed negative cultivation points");
                wasFixed = true;
            }
            
            // Fix Qi overflow
            if (data.currentQi > data.maxQi * 1.5f)
            {
                data.currentQi = data.maxQi;
                result.AddInfo(ValidationInfoType.SystemStatus, "Fixed Qi overflow");
                wasFixed = true;
            }
            
            if (!wasFixed)
            {
                result.AddInfo(ValidationInfoType.SystemStatus, "No fixes needed");
            }
            
            return result;
        }
        
        /// <summary>
        /// Attempt to automatically fix corrupted enhanced cultivation data
        /// </summary>
        public static ValidationResult AutoFixData(CultivationDataEnhanced data)
        {
            var result = new ValidationResult();
            
            if (data == null)
            {
                result.AddError(ValidationErrorType.NullReference, "Cannot fix null enhanced data");
                return result;
            }
            
            bool wasFixed = false;
            
            // Fix negative Qi
            if (data.currentQi < 0)
            {
                data.currentQi = 0;
                result.AddInfo(ValidationInfoType.SystemStatus, "Fixed negative current Qi in enhanced data");
                wasFixed = true;
            }
            
            // Fix negative cultivation points
            if (data.cultivationPoints < 0)
            {
                data.cultivationPoints = 0;
                result.AddInfo(ValidationInfoType.SystemStatus, "Fixed negative cultivation points in enhanced data");
                wasFixed = true;
            }
            
            // Fix Qi overflow
            if (data.currentQi > data.MaxQi * 1.5f)
            {
                data.currentQi = data.MaxQi;
                result.AddInfo(ValidationInfoType.SystemStatus, "Fixed Qi overflow in enhanced data");
                wasFixed = true;
            }
            
            // Fix invalid progress values
            if (data.progress != null)
            {
                if (data.progress.cultivationMomentum < 0)
                {
                    data.progress.cultivationMomentum = 0;
                    result.AddInfo(ValidationInfoType.SystemStatus, "Fixed negative cultivation momentum");
                    wasFixed = true;
                }
                
                if (data.progress.totalCultivationTime < 0)
                {
                    data.progress.totalCultivationTime = 0;
                    result.AddInfo(ValidationInfoType.SystemStatus, "Fixed negative cultivation time");
                    wasFixed = true;
                }
            }
            
            if (!wasFixed)
            {
                result.AddInfo(ValidationInfoType.SystemStatus, "No fixes needed for enhanced data");
            }
            
            return result;
        }
        
        #endregion
    }
}
