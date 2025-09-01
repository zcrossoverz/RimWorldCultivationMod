using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien.Validation
{
    /// <summary>
    /// System-wide validation manager for periodic integrity checks and validation coordination
    /// </summary>
    public class ValidationManager : GameComponent
    {
        #region Fields
        
        /// <summary>Interval between automatic validation checks (in ticks)</summary>
        public int validationInterval = 60000; // ~1 minute
        
        /// <summary>Last tick when validation was performed</summary>
        private int lastValidationTick = 0;
        
        /// <summary>Enable automatic validation</summary>
        public bool enableAutoValidation = true;
        
        /// <summary>Validation statistics</summary>
        private ValidationStats stats = new ValidationStats();
        
        /// <summary>Cache of recent validation results</summary>
        private Dictionary<int, ValidationResult> recentResults = new Dictionary<int, ValidationResult>();
        
        /// <summary>Maximum number of cached results</summary>
        private const int MAX_CACHED_RESULTS = 100;
        
        #endregion
        
        #region Constructors
        
        public ValidationManager(Game game) : base()
        {
        }
        
        #endregion
        
        #region Game Component Overrides
        
        public override void GameComponentTick()
        {
            if (!enableAutoValidation) return;
            
            int currentTick = Find.TickManager.TicksGame;
            if (currentTick - lastValidationTick > validationInterval)
            {
                PerformAutomaticValidation();
                lastValidationTick = currentTick;
            }
        }
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref validationInterval, "validationInterval", 60000);
            Scribe_Values.Look(ref enableAutoValidation, "enableAutoValidation", true);
            Scribe_Values.Look(ref lastValidationTick, "lastValidationTick", 0);
            Scribe_Deep.Look(ref stats, "stats");
            
            if (stats == null) stats = new ValidationStats();
        }
        
        #endregion
        
        #region Automatic Validation
        
        /// <summary>
        /// Perform automatic validation on all cultivating pawns
        /// </summary>
        private void PerformAutomaticValidation()
        {
            try
            {
                var cultivatingPawns = Find.Maps
                    .SelectMany(map => map.mapPawns.AllPawns)
                    .Where(pawn => pawn.GetComp<CultivationComp>()?.cultivationData != null)
                    .ToList();
                
                stats.TotalValidationRuns++;
                int issuesFound = 0;
                
                foreach (var pawn in cultivatingPawns)
                {
                    var result = ValidatePawnQuick(pawn);
                    if (!result.IsValid || result.HasWarnings)
                    {
                        issuesFound++;
                        CacheValidationResult(pawn.thingIDNumber, result);
                        
                        // Log serious issues
                        if (!result.IsValid)
                        {
                            Log.Warning($"[TuTien Validation] Issues found with {pawn.Name.ToStringShort}: {result.GetErrorsOnly()}");
                        }
                    }
                }
                
                stats.LastRunFoundIssues = issuesFound;
                stats.LastRunTimestamp = DateTime.Now;
                
                if (issuesFound > 0)
                {
                    Log.Message($"[TuTien Validation] Automatic validation found issues in {issuesFound}/{cultivatingPawns.Count} cultivating pawns");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Validation] Error during automatic validation: {ex.Message}");
                stats.TotalErrors++;
            }
        }
        
        /// <summary>
        /// Quick validation focused on critical issues only
        /// </summary>
        private ValidationResult ValidatePawnQuick(Pawn pawn)
        {
            var result = new ValidationResult();
            
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null)
            {
                result.AddError(ValidationErrorType.MissingComponent, "Missing cultivation component");
                return result;
            }
            
            var data = comp.cultivationData;
            
            // Critical data validation only
            if (data.currentQi < 0 || data.currentQi > data.maxQi * 1.1f) // Allow 10% overflow
            {
                result.AddError(ValidationErrorType.InvalidData, "Qi values are corrupted");
            }
            
            if (data.cultivationPoints < 0)
            {
                result.AddError(ValidationErrorType.InvalidData, "Negative cultivation points");
            }
            
            if (!CultivationValidator.IsValidRealmStage(data.currentRealm, data.currentStage))
            {
                result.AddError(ValidationErrorType.InvalidData, "Invalid realm-stage combination");
            }
            
            return result;
        }
        
        #endregion
        
        #region Manual Validation
        
        /// <summary>
        /// Perform comprehensive validation on a specific pawn
        /// </summary>
        public ValidationResult ValidatePawn(Pawn pawn, bool useCache = true)
        {
            if (useCache && recentResults.ContainsKey(pawn.thingIDNumber))
            {
                var cached = recentResults[pawn.thingIDNumber];
                // Use cache if less than 5 minutes old
                if ((DateTime.Now - cached.GetAllMessages().First().Timestamp).TotalMinutes < 5)
                {
                    return cached;
                }
            }
            
            var result = CultivationValidator.PerformIntegrityCheck(pawn);
            CacheValidationResult(pawn.thingIDNumber, result);
            
            stats.TotalManualValidations++;
            if (!result.IsValid) stats.TotalErrors++;
            
            return result;
        }
        
        /// <summary>
        /// Validate all cultivating pawns comprehensively
        /// </summary>
        public ValidationResult ValidateAllPawns()
        {
            var overallResult = new ValidationResult();
            
            var cultivatingPawns = Find.Maps
                .SelectMany(map => map.mapPawns.AllPawns)
                .Where(pawn => pawn.GetComp<CultivationComp>()?.cultivationData != null)
                .ToList();
            
            foreach (var pawn in cultivatingPawns)
            {
                var pawnResult = CultivationValidator.PerformIntegrityCheck(pawn);
                overallResult.MergeWith(pawnResult);
            }
            
            overallResult.AddInfo(ValidationInfoType.SystemStatus, 
                $"Validated {cultivatingPawns.Count} cultivating pawns");
            
            return overallResult;
        }
        
        /// <summary>
        /// Validate cultivation skill definitions
        /// </summary>
        public ValidationResult ValidateSkillDefinitions()
        {
            var result = new ValidationResult();
            
            var allSkills = DefDatabase<CultivationSkillDef>.AllDefs;
            
            foreach (var skill in allSkills)
            {
                // Check for missing required properties
                if (string.IsNullOrEmpty(skill.defName))
                {
                    result.AddError(ValidationErrorType.InvalidData, "Skill has empty defName");
                    continue;
                }
                
                if (string.IsNullOrEmpty(skill.label))
                {
                    result.AddWarning(ValidationWarningType.DataInconsistency, 
                        $"Skill {skill.defName} has no label");
                }
                
                if (skill.isActive && skill.workerClass == null)
                {
                    result.AddError(ValidationErrorType.MissingComponent, 
                        $"Active skill {skill.defName} has no worker class");
                }
                
                // Check prerequisite references
                if (skill.HasPrerequisites)
                {
                    foreach (string prereq in skill.prerequisiteSkills)
                    {
                        if (DefDatabase<CultivationSkillDef>.GetNamedSilentFail(prereq) == null)
                        {
                            result.AddError(ValidationErrorType.InvalidReference, 
                                $"Skill {skill.defName} references invalid prerequisite: {prereq}");
                        }
                    }
                }
                
                // Check for circular dependencies
                if (HasCircularDependency(skill, allSkills))
                {
                    result.AddError(ValidationErrorType.DataCorruption, 
                        $"Skill {skill.defName} has circular prerequisite dependency");
                }
            }
            
            return result;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Cache a validation result for a pawn
        /// </summary>
        private void CacheValidationResult(int pawnID, ValidationResult result)
        {
            recentResults[pawnID] = result;
            
            // Clean old cache entries if we exceed the limit
            if (recentResults.Count > MAX_CACHED_RESULTS)
            {
                var oldestEntries = recentResults
                    .OrderBy(kvp => kvp.Value.GetAllMessages().FirstOrDefault()?.Timestamp ?? DateTime.MinValue)
                    .Take(recentResults.Count - MAX_CACHED_RESULTS)
                    .Select(kvp => kvp.Key)
                    .ToList();
                
                foreach (int id in oldestEntries)
                {
                    recentResults.Remove(id);
                }
            }
        }
        
        /// <summary>
        /// Check if a skill has circular prerequisite dependencies
        /// </summary>
        private bool HasCircularDependency(CultivationSkillDef skill, IEnumerable<CultivationSkillDef> allSkills)
        {
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            
            return HasCircularDependencyRecursive(skill.defName, allSkills.ToDictionary(s => s.defName), 
                visited, recursionStack);
        }
        
        /// <summary>
        /// Recursive helper for circular dependency check
        /// </summary>
        private bool HasCircularDependencyRecursive(string skillDefName, 
            Dictionary<string, CultivationSkillDef> skillDict,
            HashSet<string> visited, HashSet<string> recursionStack)
        {
            if (recursionStack.Contains(skillDefName))
                return true; // Circular dependency found
            
            if (visited.Contains(skillDefName))
                return false; // Already processed
            
            visited.Add(skillDefName);
            recursionStack.Add(skillDefName);
            
            if (skillDict.TryGetValue(skillDefName, out var skill) && skill.HasPrerequisites)
            {
                foreach (string prereq in skill.prerequisiteSkills)
                {
                    if (HasCircularDependencyRecursive(prereq, skillDict, visited, recursionStack))
                        return true;
                }
            }
            
            recursionStack.Remove(skillDefName);
            return false;
        }
        
        /// <summary>
        /// Get validation statistics
        /// </summary>
        public ValidationStats GetStatistics()
        {
            return stats;
        }
        
        /// <summary>
        /// Reset validation statistics
        /// </summary>
        public void ResetStatistics()
        {
            stats = new ValidationStats();
        }
        
        /// <summary>
        /// Get cached validation result for a pawn
        /// </summary>
        public ValidationResult GetCachedResult(Pawn pawn)
        {
            return recentResults.TryGetValue(pawn.thingIDNumber, out var result) ? result : null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Statistics about validation operations
    /// </summary>
    [System.Serializable]
    public class ValidationStats : IExposable
    {
        public int totalValidationRuns = 0;
        public int totalManualValidations = 0;
        public int totalErrors = 0;
        public int lastRunFoundIssues = 0;
        
        // Properties for easier access
        public int TotalValidationRuns { get => totalValidationRuns; set => totalValidationRuns = value; }
        public int TotalManualValidations { get => totalManualValidations; set => totalManualValidations = value; }
        public int TotalErrors { get => totalErrors; set => totalErrors = value; }
        public int LastRunFoundIssues { get => lastRunFoundIssues; set => lastRunFoundIssues = value; }
        public DateTime LastRunTimestamp { get; set; } = DateTime.MinValue;
        
        public void ExposeData()
        {
            Scribe_Values.Look(ref totalValidationRuns, "totalValidationRuns", 0);
            Scribe_Values.Look(ref totalManualValidations, "totalManualValidations", 0);
            Scribe_Values.Look(ref totalErrors, "totalErrors", 0);
            Scribe_Values.Look(ref lastRunFoundIssues, "lastRunFoundIssues", 0);
            // DateTime serialization would need custom handling
        }
        
        public string GetSummary()
        {
            return $"Validation Stats:\n" +
                   $"• Total automatic runs: {TotalValidationRuns}\n" +
                   $"• Manual validations: {TotalManualValidations}\n" +
                   $"• Total errors found: {TotalErrors}\n" +
                   $"• Last run found {LastRunFoundIssues} issues\n" +
                   $"• Last run: {(LastRunTimestamp == DateTime.MinValue ? "Never" : LastRunTimestamp.ToString("HH:mm:ss"))}";
        }
    }
}
