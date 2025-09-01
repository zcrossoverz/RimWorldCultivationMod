using System;
using System.Collections.Generic;
using Verse;

namespace TuTien.Core
{
    /// <summary>
    /// Represents different types of cultivation techniques
    /// </summary>
    public enum CultivationTechniqueType
    {
        Body,       // Physical cultivation (body strengthening)
        Soul,       // Spiritual cultivation (soul refinement) 
        Dual,       // Both body and soul cultivation
        Special     // Unique or legendary techniques
    }

    /// <summary>
    /// Mastery levels for cultivation techniques
    /// </summary>
    public enum TechniqueMasteryLevel
    {
        None = 0,           // Not learned
        Beginner = 1,       // Basic understanding
        Intermediate = 2,   // Decent proficiency
        Advanced = 3,       // High skill level
        Expert = 4,         // Near mastery
        Master = 5,         // Complete mastery
        Transcendent = 6    // Beyond normal mastery
    }

    /// <summary>
    /// Base class for all cultivation techniques (Công Pháp)
    /// Techniques are methods of cultivation that pawns can learn and practice
    /// </summary>
    public abstract class CultivationTechnique : IExposable
    {
        #region Core Properties
        
        /// <summary>
        /// Unique identifier for this technique
        /// </summary>
        public string defName;
        
        /// <summary>
        /// Display name of the technique
        /// </summary>
        public string label;
        
        /// <summary>
        /// Detailed description of the technique
        /// </summary>
        public string description;
        
        /// <summary>
        /// Type of cultivation technique
        /// </summary>
        public CultivationTechniqueType techniqueType;
        
        /// <summary>
        /// Current mastery level of this technique
        /// </summary>
        public TechniqueMasteryLevel masteryLevel = TechniqueMasteryLevel.None;
        
        /// <summary>
        /// Experience points in current mastery level
        /// </summary>
        public float masteryExperience = 0f;
        
        /// <summary>
        /// Experience required to advance to next mastery level
        /// </summary>
        public float masteryExperienceRequired = 100f;
        
        #endregion

        #region Requirements and Prerequisites
        
        /// <summary>
        /// Minimum cultivation realm required to learn this technique
        /// </summary>
        public CultivationRealm minimumRealm = CultivationRealm.Mortal;
        
        /// <summary>
        /// Minimum stage within realm required
        /// </summary>
        public int minimumStage = 1;
        
        /// <summary>
        /// Other techniques that must be learned first
        /// </summary>
        public List<string> prerequisiteTechniques = new List<string>();
        
        /// <summary>
        /// Minimum talent requirements (talent type -> minimum level)
        /// </summary>
        public Dictionary<string, int> talentRequirements = new Dictionary<string, int>();
        
        #endregion

        #region Practice and Usage
        
        /// <summary>
        /// Base time (in ticks) required to practice this technique once
        /// </summary>
        public int basePracticeTime = 2500; // ~1 minute at 1x speed
        
        /// <summary>
        /// Qi cost per practice session
        /// </summary>
        public float qiCostPerPractice = 10f;
        
        /// <summary>
        /// Cooldown between practice sessions (in ticks)
        /// </summary>
        public int practiceCooldown = 0;
        
        /// <summary>
        /// Last time this technique was practiced
        /// </summary>
        public int lastPracticeTime = -1;
        
        #endregion

        #region Constructors
        
        protected CultivationTechnique()
        {
            // Default constructor for serialization
        }
        
        protected CultivationTechnique(string defName, string label, string description, CultivationTechniqueType type)
        {
            this.defName = defName;
            this.label = label;
            this.description = description;
            this.techniqueType = type;
        }
        
        #endregion

        #region Mastery System
        
        /// <summary>
        /// Add experience to this technique and check for level up
        /// </summary>
        public virtual bool AddMasteryExperience(float experience, Pawn practitioner)
        {
            if (masteryLevel >= TechniqueMasteryLevel.Transcendent)
                return false;
                
            masteryExperience += experience;
            
            // Check for mastery level advancement
            if (masteryExperience >= masteryExperienceRequired)
            {
                return AdvanceMasteryLevel(practitioner);
            }
            
            return false;
        }
        
        /// <summary>
        /// Advance to next mastery level
        /// </summary>
        protected virtual bool AdvanceMasteryLevel(Pawn practitioner)
        {
            if (masteryLevel >= TechniqueMasteryLevel.Transcendent)
                return false;
                
            var oldLevel = masteryLevel;
            masteryLevel = (TechniqueMasteryLevel)((int)masteryLevel + 1);
            
            // Reset experience and increase requirement for next level
            masteryExperience = 0f;
            masteryExperienceRequired = CalculateNextLevelRequirement();
            
            // Trigger mastery advancement event
            CultivationEvents.TriggerTechniqueMasteryAdvanced(practitioner, this, oldLevel, masteryLevel);
            
            if (Prefs.DevMode)
                Log.Message($"[TuTien] {practitioner.Name.ToStringShort} advanced {label} from {oldLevel} to {masteryLevel}");
                
            return true;
        }
        
        /// <summary>
        /// Calculate experience requirement for next mastery level
        /// </summary>
        protected virtual float CalculateNextLevelRequirement()
        {
            // Exponential growth: each level requires 50% more experience
            return masteryExperienceRequired * 1.5f;
        }
        
        #endregion

        #region Practice and Usage Methods
        
        /// <summary>
        /// Check if pawn can practice this technique
        /// </summary>
        public virtual bool CanPractice(Pawn pawn, out string failReason)
        {
            failReason = "";
            
            // Check if technique is learned
            if (masteryLevel == TechniqueMasteryLevel.None)
            {
                failReason = "Technique not learned";
                return false;
            }
            
            // Check cooldown
            if (IsOnCooldown())
            {
                int remainingTicks = (lastPracticeTime + practiceCooldown) - Find.TickManager.TicksGame;
                failReason = $"Cooldown: {remainingTicks.ToStringSecondsFromTicks()}";
                return false;
            }
            
            // Check Qi requirement
            var cultivationData = pawn.GetComp<CultivationComp>()?.cultivationData;
            if (cultivationData == null)
            {
                failReason = "No cultivation data";
                return false;
            }
            
            if (cultivationData.currentQi < qiCostPerPractice)
            {
                failReason = $"Insufficient Qi: {qiCostPerPractice:F1} required, {cultivationData.currentQi:F1} available";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if technique is currently on cooldown
        /// </summary>
        public virtual bool IsOnCooldown()
        {
            if (practiceCooldown <= 0 || lastPracticeTime < 0)
                return false;
                
            return Find.TickManager.TicksGame < (lastPracticeTime + practiceCooldown);
        }
        
        /// <summary>
        /// Execute practice session for this technique
        /// </summary>
        public virtual void Practice(Pawn practitioner)
        {
            if (!CanPractice(practitioner, out string failReason))
            {
                if (Prefs.DevMode)
                    Log.Warning($"[TuTien] {practitioner.Name.ToStringShort} cannot practice {label}: {failReason}");
                return;
            }
            
            // Consume Qi
            var cultivationData = practitioner.GetComp<CultivationComp>()?.cultivationData;
            cultivationData.currentQi -= qiCostPerPractice;
            
            // Calculate experience gain based on mastery and talent
            float experienceGain = CalculateExperienceGain(practitioner);
            
            // Add experience
            bool leveledUp = AddMasteryExperience(experienceGain, practitioner);
            
            // Set cooldown
            lastPracticeTime = Find.TickManager.TicksGame;
            
            // Apply technique effects
            ApplyPracticeEffects(practitioner, leveledUp);
            
            // Trigger practice event
            CultivationEvents.TriggerTechniquePracticed(practitioner, this, experienceGain);
        }
        
        /// <summary>
        /// Calculate experience gain from practice session
        /// </summary>
        protected virtual float CalculateExperienceGain(Pawn practitioner)
        {
            float baseGain = 10f;
            
            // Apply talent modifiers
            var cultivationData = practitioner.GetComp<CultivationComp>()?.cultivationData;
            if (cultivationData != null)
            {
                // Get talent multiplier from cultivation data
                float talentMultiplier = 1f;
                switch (cultivationData.talent)
                {
                    case TalentLevel.Common:
                        talentMultiplier = 1.0f;
                        break;
                    case TalentLevel.Rare:
                        talentMultiplier = 1.5f;
                        break;
                    case TalentLevel.Genius:
                        talentMultiplier = 2.0f;
                        break;
                    case TalentLevel.HeavenChosen:
                        talentMultiplier = 3.0f;
                        break;
                }
                baseGain *= talentMultiplier;
            }
            
            // Apply mastery penalty (higher levels learn slower)
            float masteryPenalty = 1f - ((int)masteryLevel * 0.1f);
            masteryPenalty = Math.Max(masteryPenalty, 0.1f);
            
            return baseGain * masteryPenalty;
        }
        
        /// <summary>
        /// Apply effects of practicing this technique (override in subclasses)
        /// </summary>
        protected virtual void ApplyPracticeEffects(Pawn practitioner, bool leveledUp)
        {
            // Base implementation - override in specific technique types
        }
        
        #endregion

        #region Learning System
        
        /// <summary>
        /// Check if pawn meets requirements to learn this technique
        /// </summary>
        public virtual bool CanLearn(Pawn pawn, out string failReason)
        {
            failReason = "";
            
            var cultivationData = pawn.GetComp<CultivationComp>()?.cultivationData;
            if (cultivationData == null)
            {
                failReason = "No cultivation data";
                return false;
            }
            
            // Check if already learned
            if (masteryLevel != TechniqueMasteryLevel.None)
            {
                failReason = "Already learned";
                return false;
            }
            
            // Check realm requirement
            if (cultivationData.currentRealm < minimumRealm || 
                (cultivationData.currentRealm == minimumRealm && cultivationData.currentStage < minimumStage))
            {
                failReason = $"Requires {minimumRealm} stage {minimumStage}";
                return false;
            }
            
            // Check talent requirements
            foreach (var requirement in talentRequirements)
            {
                // This would need to be implemented based on how talents are structured
                // For now, we'll assume all talent requirements are met
            }
            
            // Check prerequisite techniques
            foreach (string prereq in prerequisiteTechniques)
            {
                // This would need to check if pawn knows the prerequisite technique
                // Implementation depends on how techniques are stored on pawns
            }
            
            return true;
        }
        
        /// <summary>
        /// Learn this technique (set to Beginner level)
        /// </summary>
        public virtual bool Learn(Pawn learner)
        {
            if (!CanLearn(learner, out string failReason))
            {
                if (Prefs.DevMode)
                    Log.Warning($"[TuTien] {learner.Name.ToStringShort} cannot learn {label}: {failReason}");
                return false;
            }
            
            masteryLevel = TechniqueMasteryLevel.Beginner;
            masteryExperience = 0f;
            masteryExperienceRequired = CalculateNextLevelRequirement();
            
            // Trigger learning event
            CultivationEvents.TriggerTechniqueLearned(learner, this);
            
            if (Prefs.DevMode)
                Log.Message($"[TuTien] {learner.Name.ToStringShort} learned {label}");
                
            return true;
        }
        
        #endregion

        #region Serialization
        
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref techniqueType, "techniqueType");
            Scribe_Values.Look(ref masteryLevel, "masteryLevel");
            Scribe_Values.Look(ref masteryExperience, "masteryExperience");
            Scribe_Values.Look(ref masteryExperienceRequired, "masteryExperienceRequired");
            Scribe_Values.Look(ref minimumRealm, "minimumRealm");
            Scribe_Values.Look(ref minimumStage, "minimumStage");
            Scribe_Collections.Look(ref prerequisiteTechniques, "prerequisiteTechniques");
            Scribe_Collections.Look(ref talentRequirements, "talentRequirements");
            Scribe_Values.Look(ref basePracticeTime, "basePracticeTime");
            Scribe_Values.Look(ref qiCostPerPractice, "qiCostPerPractice");
            Scribe_Values.Look(ref practiceCooldown, "practiceCooldown");
            Scribe_Values.Look(ref lastPracticeTime, "lastPracticeTime");
        }
        
        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Get mastery level as percentage (0-100)
        /// </summary>
        public virtual float GetMasteryPercentage()
        {
            if (masteryLevel == TechniqueMasteryLevel.None)
                return 0f;
            if (masteryLevel >= TechniqueMasteryLevel.Transcendent)
                return 100f;
                
            return (masteryExperience / masteryExperienceRequired) * 100f;
        }
        
        /// <summary>
        /// Get display string for mastery level
        /// </summary>
        public virtual string GetMasteryDisplayString()
        {
            if (masteryLevel == TechniqueMasteryLevel.None)
                return "Not Learned";
                
            return $"{masteryLevel} ({GetMasteryPercentage():F1}%)";
        }
        
        /// <summary>
        /// Get technique efficiency based on mastery level
        /// </summary>
        public virtual float GetTechniqueEfficiency()
        {
            return 0.5f + ((int)masteryLevel * 0.1f);
        }
        
        #endregion
    }
}
