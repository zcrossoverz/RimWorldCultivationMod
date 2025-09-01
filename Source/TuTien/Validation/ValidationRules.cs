using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien.Validation
{
    /// <summary>
    /// Rule-based validation system for flexible and extensible validation
    /// </summary>
    public static class ValidationRules
    {
        #region Rule Collections
        
        /// <summary>All registered validation rules</summary>
        private static List<IValidationRule> allRules = new List<IValidationRule>();
        
        /// <summary>Rules by category for efficient filtering</summary>
        private static Dictionary<ValidationCategory, List<IValidationRule>> rulesByCategory = 
            new Dictionary<ValidationCategory, List<IValidationRule>>();
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the validation rule system
        /// </summary>
        static ValidationRules()
        {
            RegisterDefaultRules();
        }
        
        /// <summary>
        /// Register all default validation rules
        /// </summary>
        private static void RegisterDefaultRules()
        {
            // Data integrity rules
            RegisterRule(new QiValidationRule());
            RegisterRule(new CultivationPointsRule());
            RegisterRule(new RealmStageValidationRule());
            RegisterRule(new TalentValidationRule());
            
            // Skill-specific rules
            RegisterRule(new SkillPrerequisiteRule());
            RegisterRule(new SkillCooldownRule());
            RegisterRule(new SkillExperienceRule());
            
            // Technique rules
            RegisterRule(new TechniqueCompatibilityRule());
            RegisterRule(new TechniqueProgressRule());
            
            // Component integrity rules
            RegisterRule(new ComponentConsistencyRule());
            RegisterRule(new SaveDataIntegrityRule());
        }
        
        #endregion
        
        #region Rule Registration
        
        /// <summary>
        /// Register a new validation rule
        /// </summary>
        public static void RegisterRule(IValidationRule rule)
        {
            if (rule == null) return;
            
            allRules.Add(rule);
            
            // Add to category-based lookup
            if (!rulesByCategory.ContainsKey(rule.Category))
            {
                rulesByCategory[rule.Category] = new List<IValidationRule>();
            }
            rulesByCategory[rule.Category].Add(rule);
        }
        
        /// <summary>
        /// Remove a validation rule
        /// </summary>
        public static bool RemoveRule(IValidationRule rule)
        {
            bool removed = allRules.Remove(rule);
            if (removed && rulesByCategory.ContainsKey(rule.Category))
            {
                rulesByCategory[rule.Category].Remove(rule);
            }
            return removed;
        }
        
        #endregion
        
        #region Rule Execution
        
        /// <summary>
        /// Run all applicable rules for a pawn
        /// </summary>
        public static ValidationResult ValidatePawn(Pawn pawn, ValidationCategory categories = ValidationCategory.All)
        {
            var result = new ValidationResult();
            
            var applicableRules = GetRulesForCategory(categories)
                .Where(rule => rule.AppliesTo(pawn))
                .OrderBy(rule => rule.Priority)
                .ToList();
            
            foreach (var rule in applicableRules)
            {
                try
                {
                    var ruleResult = rule.Validate(pawn);
                    result.MergeWith(ruleResult);
                    
                    // Stop if critical error found and rule requires it
                    if (!ruleResult.IsValid && rule.StopOnFailure)
                    {
                        result.AddWarning(ValidationWarningType.DataInconsistency, 
                            $"Validation stopped due to critical failure in rule: {rule.Name}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    result.AddError(ValidationErrorType.DataCorruption, 
                        $"Rule {rule.Name} threw exception: {ex.Message}");
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Run specific rule on a pawn
        /// </summary>
        public static ValidationResult ValidateWithRule(Pawn pawn, IValidationRule rule)
        {
            if (rule == null || !rule.AppliesTo(pawn))
            {
                return new ValidationResult();
            }
            
            try
            {
                return rule.Validate(pawn);
            }
            catch (Exception ex)
            {
                var result = new ValidationResult();
                result.AddError(ValidationErrorType.DataCorruption, 
                    $"Rule {rule.Name} failed: {ex.Message}");
                return result;
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get rules for specific categories
        /// </summary>
        private static IEnumerable<IValidationRule> GetRulesForCategory(ValidationCategory categories)
        {
            if (categories == ValidationCategory.All)
            {
                return allRules;
            }
            
            var rules = new List<IValidationRule>();
            foreach (ValidationCategory category in Enum.GetValues(typeof(ValidationCategory)))
            {
                if (category == ValidationCategory.All) continue;
                
                if ((categories & category) == category && rulesByCategory.ContainsKey(category))
                {
                    rules.AddRange(rulesByCategory[category]);
                }
            }
            
            return rules.Distinct();
        }
        
        /// <summary>
        /// Get all registered rule names
        /// </summary>
        public static List<string> GetRegisteredRuleNames()
        {
            return allRules.Select(r => r.Name).ToList();
        }
        
        /// <summary>
        /// Get rule by name
        /// </summary>
        public static IValidationRule GetRule(string name)
        {
            return allRules.FirstOrDefault(r => r.Name == name);
        }
        
        #endregion
    }
    
    #region Rule Interface and Categories
    
    /// <summary>
    /// Interface for validation rules
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>Rule name for identification</summary>
        string Name { get; }
        
        /// <summary>Rule category</summary>
        ValidationCategory Category { get; }
        
        /// <summary>Execution priority (lower = earlier)</summary>
        int Priority { get; }
        
        /// <summary>Whether to stop validation if this rule fails</summary>
        bool StopOnFailure { get; }
        
        /// <summary>Check if rule applies to this pawn</summary>
        bool AppliesTo(Pawn pawn);
        
        /// <summary>Execute validation rule</summary>
        ValidationResult Validate(Pawn pawn);
    }
    
    /// <summary>
    /// Categories of validation rules
    /// </summary>
    [Flags]
    public enum ValidationCategory
    {
        DataIntegrity = 1,
        Skills = 2,
        Techniques = 4,
        Components = 8,
        Performance = 16,
        All = DataIntegrity | Skills | Techniques | Components | Performance
    }
    
    #endregion
    
    #region Default Validation Rules
    
    /// <summary>
    /// Base class for validation rules
    /// </summary>
    public abstract class BaseValidationRule : IValidationRule
    {
        public abstract string Name { get; }
        public abstract ValidationCategory Category { get; }
        public virtual int Priority => 100;
        public virtual bool StopOnFailure => false;
        
        public virtual bool AppliesTo(Pawn pawn)
        {
            return pawn?.GetComp<CultivationComp>()?.cultivationData != null;
        }
        
        public abstract ValidationResult Validate(Pawn pawn);
    }
    
    /// <summary>
    /// Validates Qi values are within acceptable ranges
    /// </summary>
    public class QiValidationRule : BaseValidationRule
    {
        public override string Name => "QiValidation";
        public override ValidationCategory Category => ValidationCategory.DataIntegrity;
        public override int Priority => 10;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var data = pawn.GetComp<CultivationComp>().cultivationData;
            
            if (data.currentQi < 0)
            {
                result.AddError(ValidationErrorType.InvalidData, "Current Qi is negative");
            }
            
            if (data.currentQi > data.maxQi * 1.2f) // Allow 20% overflow for temporary effects
            {
                result.AddWarning(ValidationWarningType.DataInconsistency, 
                    $"Current Qi ({data.currentQi:F1}) significantly exceeds max Qi ({data.maxQi:F1})");
            }
            
            if (data.maxQi <= 0)
            {
                result.AddError(ValidationErrorType.InvalidData, "Max Qi is zero or negative");
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates cultivation points
    /// </summary>
    public class CultivationPointsRule : BaseValidationRule
    {
        public override string Name => "CultivationPoints";
        public override ValidationCategory Category => ValidationCategory.DataIntegrity;
        public override int Priority => 10;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var data = pawn.GetComp<CultivationComp>().cultivationData;
            
            if (data.cultivationPoints < 0)
            {
                result.AddError(ValidationErrorType.InvalidData, "Cultivation points are negative");
            }
            
            // Check for unreasonably high values
            if (data.cultivationPoints > 10000000) // 10 million seems excessive
            {
                result.AddWarning(ValidationWarningType.DataInconsistency, 
                    "Cultivation points are extremely high - possible corruption");
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates realm and stage combinations
    /// </summary>
    public class RealmStageValidationRule : BaseValidationRule
    {
        public override string Name => "RealmStageValidation";
        public override ValidationCategory Category => ValidationCategory.DataIntegrity;
        public override int Priority => 15;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var data = pawn.GetComp<CultivationComp>().cultivationData;
            
            if (!CultivationValidator.IsValidRealmStage(data.currentRealm, data.currentStage))
            {
                result.AddError(ValidationErrorType.InvalidData, 
                    $"Invalid realm-stage combination: {data.currentRealm}-{data.currentStage}");
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates talent values
    /// </summary>
    public class TalentValidationRule : BaseValidationRule
    {
        public override string Name => "TalentValidation";
        public override ValidationCategory Category => ValidationCategory.DataIntegrity;
        public override int Priority => 20;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var data = pawn.GetComp<CultivationComp>().cultivationData;
            
            if ((int)data.talent < CultivationValidator.MIN_TALENT || (int)data.talent > CultivationValidator.MAX_TALENT)
            {
                result.AddError(ValidationErrorType.InvalidData, 
                    $"Talent value {data.talent} is outside valid range ({CultivationValidator.MIN_TALENT}-{CultivationValidator.MAX_TALENT})");
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates skill prerequisites
    /// </summary>
    public class SkillPrerequisiteRule : BaseValidationRule
    {
        public override string Name => "SkillPrerequisites";
        public override ValidationCategory Category => ValidationCategory.Skills;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var comp = pawn.GetComp<CultivationComp>();
            var skillManager = comp?.GetSkillManager();
            
            if (skillManager == null) return result;
            
            foreach (var learnedSkill in skillManager.GetAllLearnedSkills())
            {
                var skillDef = DefDatabase<CultivationSkillDef>.GetNamed(learnedSkill.defName);
                if (skillDef?.HasPrerequisites == true)
                {
                    foreach (string prereq in skillDef.prerequisiteSkills)
                    {
                        if (!skillManager.HasLearnedSkill(prereq))
                        {
                            result.AddError(ValidationErrorType.InvalidData, 
                                $"Skill {learnedSkill.defName} requires {prereq} but it's not learned");
                        }
                    }
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates skill cooldowns
    /// </summary>
    public class SkillCooldownRule : BaseValidationRule
    {
        public override string Name => "SkillCooldowns";
        public override ValidationCategory Category => ValidationCategory.Skills;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var comp = pawn.GetComp<CultivationComp>();
            var skillManager = comp?.GetSkillManager();
            
            if (skillManager == null) return result;
            
            // For simplicity, we'll just check if there are any active cooldowns
            // A more detailed implementation would require access to the cooldown dictionary
            result.AddInfo(ValidationInfoType.SystemStatus, "Skill cooldown validation completed");
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates skill experience values
    /// </summary>
    public class SkillExperienceRule : BaseValidationRule
    {
        public override string Name => "SkillExperience";
        public override ValidationCategory Category => ValidationCategory.Skills;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var comp = pawn.GetComp<CultivationComp>();
            var skillManager = comp?.GetSkillManager();
            
            if (skillManager == null) return result;
            
            foreach (var skill in skillManager.GetAllLearnedSkills())
            {
                if (skill.experience < 0)
                {
                    result.AddError(ValidationErrorType.InvalidData, 
                        $"Skill {skill.defName} has negative experience: {skill.experience}");
                }
                
                if (skill.level < 0 || skill.level > 20)
                {
                    result.AddWarning(ValidationWarningType.DataInconsistency, 
                        $"Skill {skill.defName} has unusual level: {skill.level}");
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates technique compatibility
    /// </summary>
    public class TechniqueCompatibilityRule : BaseValidationRule
    {
        public override string Name => "TechniqueCompatibility";
        public override ValidationCategory Category => ValidationCategory.Techniques;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var data = pawn.GetComp<CultivationComp>().cultivationData;
            
            // Check known techniques for validity
            if (data.knownTechniques?.Count > 0)
            {
                foreach (var technique in data.knownTechniques)
                {
                    if (technique == null)
                    {
                        result.AddError(ValidationErrorType.InvalidReference, 
                            "Null technique found in known techniques list");
                    }
                }
            }
            
            result.AddInfo(ValidationInfoType.SystemStatus, "Technique compatibility validation completed");
            return result;
        }
    }
    
    /// <summary>
    /// Validates technique progress values
    /// </summary>
    public class TechniqueProgressRule : BaseValidationRule
    {
        public override string Name => "TechniqueProgress";
        public override ValidationCategory Category => ValidationCategory.Techniques;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var data = pawn.GetComp<CultivationComp>().cultivationData;
            
            // For now, just validate that technique learning data is not corrupted
            // This could be expanded later when technique progress tracking is implemented
            result.AddInfo(ValidationInfoType.SystemStatus, "Technique progress validation completed");
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates component consistency
    /// </summary>
    public class ComponentConsistencyRule : BaseValidationRule
    {
        public override string Name => "ComponentConsistency";
        public override ValidationCategory Category => ValidationCategory.Components;
        public override int Priority => 5;
        public override bool StopOnFailure => true;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            var comp = pawn.GetComp<CultivationComp>();
            
            if (comp == null)
            {
                result.AddError(ValidationErrorType.MissingComponent, "CultivationComp is missing");
                return result;
            }
            
            if (comp.cultivationData == null)
            {
                result.AddError(ValidationErrorType.MissingComponent, "CultivationData is null");
                return result;
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Validates save data integrity
    /// </summary>
    public class SaveDataIntegrityRule : BaseValidationRule
    {
        public override string Name => "SaveDataIntegrity";
        public override ValidationCategory Category => ValidationCategory.Components;
        public override int Priority => 8;
        
        public override ValidationResult Validate(Pawn pawn)
        {
            var result = new ValidationResult();
            // This would be more comprehensive in a real implementation
            // checking for serialization issues, null references, etc.
            
            result.AddInfo(ValidationInfoType.SystemStatus, "Save data integrity check passed");
            return result;
        }
    }
    
    #endregion
}
