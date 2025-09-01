using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TuTien.Systems.SkillSynergy
{
    /// <summary>
    /// Definition for skill synergies - combinations of mastered skills that provide special bonuses
    /// Skills represent different aspects of cultivation: Alchemy, Medicine, Combat, Scholarship, etc.
    /// </summary>
    public class SkillSynergyDef : Def
    {
        [NoTranslate]
        public string labelKey;
        
        [NoTranslate]
        public string descriptionKey;

        /// <summary>
        /// List of cultivation skills required for this synergy
        /// </summary>
        public List<CultivationSkillDef> requiredSkills = new List<CultivationSkillDef>();

        /// <summary>
        /// Minimum realm required to activate this synergy
        /// </summary>
        public CultivationRealm requiredRealm = CultivationRealm.Mortal;

        /// <summary>
        /// Minimum stage within the realm required
        /// </summary>
        public int requiredStage = 1;

        /// <summary>
        /// Specific skill mastery levels required (optional - for fine-tuned requirements)
        /// Key: CultivationSkillDef, Value: Required mastery level
        /// </summary>
        public Dictionary<CultivationSkillDef, int> requiredSkillLevels;

        /// <summary>
        /// Rarity of this synergy combination
        /// </summary>
        public SkillSynergyRarity rarity = SkillSynergyRarity.Common;

        /// <summary>
        /// Cultivation speed bonus multiplier (e.g., 0.2 = +20% cultivation speed)
        /// </summary>
        public float cultivationSpeedBonus = 0f;

        /// <summary>
        /// Qi generation bonus multiplier
        /// </summary>
        public float qiGenerationBonus = 0f;

        /// <summary>
        /// Breakthrough chance bonus (additive percentage)
        /// </summary>
        public float breakthroughChanceBonus = 0f;

        /// <summary>
        /// Stat bonuses provided by this synergy
        /// Key: StatDef, Value: Bonus amount
        /// </summary>
        public Dictionary<StatDef, float> statBonuses;

        /// <summary>
        /// Special skill bonuses for specific cultivation skills
        /// Key: CultivationSkillDef, Value: Bonus multiplier
        /// </summary>
        public Dictionary<CultivationSkillDef, float> skillEfficiencyBonuses;

        /// <summary>
        /// Learning speed bonuses for regular RimWorld skills
        /// Key: SkillDef, Value: Learning speed multiplier
        /// </summary>
        public Dictionary<SkillDef, float> learningSpeedBonuses;

        /// <summary>
        /// Synergies that conflict with this one (can't be active simultaneously)
        /// </summary>
        public List<SkillSynergyDef> conflictingSynergies;

        /// <summary>
        /// Special periodic effects that trigger randomly
        /// </summary>
        public float specialEffectChance = 0f;

        /// <summary>
        /// Range for area effects (if applicable)
        /// </summary>
        public float effectRange = 0f;

        /// <summary>
        /// Work speed bonuses for specific work types
        /// Key: WorkTypeDef, Value: Speed multiplier
        /// </summary>
        public Dictionary<WorkTypeDef, float> workSpeedBonuses;

        /// <summary>
        /// Research speed bonus (for intellectual work)
        /// </summary>
        public float researchSpeedBonus = 0f;

        /// <summary>
        /// Medical treatment quality bonus
        /// </summary>
        public float medicalQualityBonus = 0f;

        /// <summary>
        /// Crafting quality chance bonus
        /// </summary>
        public float craftingQualityBonus = 0f;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            
            // Validate required skills exist
            if (requiredSkills == null || requiredSkills.Count == 0)
            {
                Log.Error($"[TuTien] SkillSynergyDef {defName} has no required skills defined");
            }

            // Validate skill references
            foreach (var skill in requiredSkills)
            {
                if (skill == null)
                {
                    Log.Error($"[TuTien] SkillSynergyDef {defName} references null skill");
                }
            }

            // Validate realm requirements
            if (requiredRealm < CultivationRealm.Mortal || requiredRealm > CultivationRealm.GoldenCore)
            {
                Log.Warning($"[TuTien] SkillSynergyDef {defName} has unusual realm requirement: {requiredRealm}");
            }

            if (requiredStage < 1 || requiredStage > 9)
            {
                Log.Warning($"[TuTien] SkillSynergyDef {defName} has unusual stage requirement: {requiredStage}");
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }

            if (string.IsNullOrEmpty(labelKey))
            {
                yield return $"labelKey is required for localization";
            }

            if (string.IsNullOrEmpty(descriptionKey))
            {
                yield return $"descriptionKey is required for localization";
            }

            if (requiredSkills == null || requiredSkills.Count < 2)
            {
                yield return $"At least 2 required skills needed for a synergy";
            }

            if (cultivationSpeedBonus < -1f || cultivationSpeedBonus > 5f)
            {
                yield return $"cultivationSpeedBonus should be between -1.0 and 5.0 (got {cultivationSpeedBonus})";
            }

            if (specialEffectChance < 0f || specialEffectChance > 1f)
            {
                yield return $"specialEffectChance should be between 0.0 and 1.0 (got {specialEffectChance})";
            }
        }

        /// <summary>
        /// Get localized label
        /// </summary>
        public new string LabelCap => labelKey?.Translate().CapitalizeFirst() ?? label?.CapitalizeFirst() ?? defName;

        /// <summary>
        /// Get localized description
        /// </summary>
        public string Description => descriptionKey?.Translate() ?? description ?? "No description available.";

        /// <summary>
        /// Get display string showing all requirements
        /// </summary>
        public string GetRequirementsString()
        {
            var requirements = new List<string>();
            
            // Realm requirement
            requirements.Add($"Realm: {requiredRealm} Stage {requiredStage}+");
            
            // Skill requirements
            var skillNames = requiredSkills.Select(s => s.LabelCap).ToList();
            requirements.Add($"Skills: {string.Join(", ", skillNames)}");
            
            // Specific mastery levels
            if (requiredSkillLevels != null && requiredSkillLevels.Any())
            {
                var masteryReqs = requiredSkillLevels.Select(kvp => $"{kvp.Key.LabelCap} Lv.{kvp.Value}").ToList();
                requirements.Add($"Mastery: {string.Join(", ", masteryReqs)}");
            }
            
            return string.Join("\n", requirements);
        }

        /// <summary>
        /// Get display string showing all bonuses
        /// </summary>
        public string GetBonusesString()
        {
            var bonuses = new List<string>();
            
            if (cultivationSpeedBonus != 0f)
                bonuses.Add($"Cultivation Speed: {cultivationSpeedBonus:+0.0%;-0.0%;+0%}");
                
            if (qiGenerationBonus != 0f)
                bonuses.Add($"Qi Generation: {qiGenerationBonus:+0.0%;-0.0%;+0%}");
                
            if (breakthroughChanceBonus != 0f)
                bonuses.Add($"Breakthrough Chance: +{breakthroughChanceBonus:0.0%}");
                
            if (researchSpeedBonus != 0f)
                bonuses.Add($"Research Speed: {researchSpeedBonus:+0.0%;-0.0%;+0%}");
                
            if (medicalQualityBonus != 0f)
                bonuses.Add($"Medical Quality: +{medicalQualityBonus:0.0%}");
                
            if (craftingQualityBonus != 0f)
                bonuses.Add($"Crafting Quality: +{craftingQualityBonus:0.0%}");
            
            return bonuses.Any() ? string.Join("\n", bonuses) : "Special effects only";
        }
    }

    /// <summary>
    /// Rarity levels for skill synergies
    /// </summary>
    public enum SkillSynergyRarity
    {
        Common,      // 2-3 basic skills
        Uncommon,    // 3-4 skills with some mastery requirements  
        Rare,        // 4-5 skills with high mastery requirements
        Epic,        // 5-6 skills with very high requirements
        Legendary    // 6+ skills with maximum mastery requirements
    }
}
