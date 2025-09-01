using System;
using System.Collections.Generic;
using Verse;
using TuTien.Core;

namespace TuTien.Systems.TechniqueSynergy
{
    /// <summary>
    /// Defines a technique synergy - combinations of techniques that provide special bonuses
    /// </summary>
    public class TechniqueSynergyDef : Def
    {
        #region XML Fields
        
        /// <summary>Translation key for synergy label (optional)</summary>
        public string labelKey;
        
        /// <summary>Translation key for synergy description (optional)</summary>
        public string descriptionKey;
        
        /// <summary>List of technique defNames required for this synergy</summary>
        public List<string> requiredTechniques = new List<string>();
        
        /// <summary>Minimum cultivation realm required to activate this synergy</summary>
        public CultivationRealm minRealm = CultivationRealm.Mortal;
        
        /// <summary>Minimum mastery level required for each technique (optional)</summary>
        public TechniqueMasteryLevel requiredMasteryLevel = TechniqueMasteryLevel.Beginner;
        
        #endregion
        
        #region Synergy Effects
        
        /// <summary>Bonus to cultivation efficiency (0.0 to 1.0+)</summary>
        public float cultivationEfficiencyBonus = 0f;
        
        /// <summary>Multiplier for qi regeneration rate (1.0 = no change)</summary>
        public float qiRegenerationMultiplier = 1f;
        
        /// <summary>Bonus to breakthrough chance (0.0 to 1.0)</summary>
        public float breakthroughChanceBonus = 0f;
        
        /// <summary>Multiplier for skill experience gain (1.0 = no change)</summary>
        public float skillExperienceMultiplier = 1f;
        
        /// <summary>Bonus to maximum qi capacity (0.0 to 1.0+)</summary>
        public float maxQiBonus = 0f;
        
        /// <summary>Whether this synergy triggers special periodic effects</summary>
        public bool triggersSpecialEffects = false;
        
        /// <summary>Rarity of this synergy (affects discovery chance)</summary>
        public SynergyRarity rarity = SynergyRarity.Common;
        
        #endregion
        
        #region Combat Bonuses
        
        /// <summary>Bonus to melee damage (0.0 to 1.0+)</summary>
        public float meleeDamageBonus = 0f;
        
        /// <summary>Bonus to ranged damage (0.0 to 1.0+)</summary>
        public float rangedDamageBonus = 0f;
        
        /// <summary>Bonus to movement speed (0.0 to 1.0+)</summary>
        public float movementSpeedBonus = 0f;
        
        /// <summary>Damage resistance bonus (0.0 to 1.0)</summary>
        public float damageResistanceBonus = 0f;
        
        /// <summary>Special damage types this synergy provides resistance to</summary>
        public List<DamageDef> specialResistances = new List<DamageDef>();
        
        #endregion
        
        #region Advanced Properties
        
        /// <summary>Synergies that conflict with this one (mutually exclusive)</summary>
        public List<string> conflictingSynergies = new List<string>();
        
        /// <summary>Maximum number of pawns in colony that can have this synergy simultaneously</summary>
        public int maxColonyInstances = -1; // -1 = unlimited
        
        /// <summary>Whether this synergy can be lost if techniques are forgotten</summary>
        public bool canBeLost = true;
        
        /// <summary>Special worker class for complex synergy effects</summary>
        public Type synergyWorkerClass;
        
        #endregion
        
        /// <summary>Get localized label for this synergy</summary>
        public new string LabelCap
        {
            get
            {
                if (!labelKey.NullOrEmpty())
                {
                    return labelKey.Translate();
                }
                return label?.CapitalizeFirst() ?? defName;
            }
        }
        
        /// <summary>Get localized description for this synergy</summary>
        public string DescriptionText
        {
            get
            {
                if (!descriptionKey.NullOrEmpty())
                {
                    return descriptionKey.Translate();
                }
                return description;
            }
        }
        
        /// <summary>Get formatted description including effects</summary>
        public string GetDetailedDescription()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(DescriptionText);
            sb.AppendLine();
            sb.AppendLine("Required Techniques:");
            foreach (var technique in requiredTechniques)
            {
                sb.AppendLine($"  • {technique}");
            }
            
            sb.AppendLine();
            sb.AppendLine("Effects:");
            
            if (cultivationEfficiencyBonus > 0)
                sb.AppendLine($"  • +{cultivationEfficiencyBonus:P0} Cultivation Efficiency");
            
            if (qiRegenerationMultiplier > 1f)
                sb.AppendLine($"  • {qiRegenerationMultiplier:F1}x Qi Regeneration");
            
            if (breakthroughChanceBonus > 0)
                sb.AppendLine($"  • +{breakthroughChanceBonus:P0} Breakthrough Chance");
            
            if (maxQiBonus > 0)
                sb.AppendLine($"  • +{maxQiBonus:P0} Maximum Qi");
            
            if (meleeDamageBonus > 0)
                sb.AppendLine($"  • +{meleeDamageBonus:P0} Melee Damage");
            
            if (movementSpeedBonus > 0)
                sb.AppendLine($"  • +{movementSpeedBonus:P0} Movement Speed");
            
            if (damageResistanceBonus > 0)
                sb.AppendLine($"  • +{damageResistanceBonus:P0} Damage Resistance");
            
            if (triggersSpecialEffects)
                sb.AppendLine("  • Special Periodic Effects");
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// Rarity levels for technique synergies
    /// </summary>
    public enum SynergyRarity
    {
        Common,     // Easy to discover, minor bonuses
        Uncommon,   // Moderate difficulty, good bonuses  
        Rare,       // Hard to discover, strong bonuses
        Legendary,  // Very rare, extremely powerful
        Mythic      // Unique synergies with game-changing effects
    }
}
