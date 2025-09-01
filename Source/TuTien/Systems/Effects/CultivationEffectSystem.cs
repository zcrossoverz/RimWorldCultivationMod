using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;

namespace TuTien
{
    /// <summary>
    /// ✅ Simple Effect Definition for XML - defines cultivation effects that can be easily added via XML
    /// This makes it super easy to add new skills, buffs, debuffs without touching C# code
    /// Moved to TuTien namespace so RimWorld can find it easier
    /// </summary>
    public class CultivationEffectDef : Def
    {
        #region Basic Properties
        /// <summary>Effect class name to instantiate</summary>
        public string effectClass = "StatModifier";
        
        /// <summary>Effect category for grouping and filtering</summary>
        public string category = "General";
        
        /// <summary>Default magnitude/strength of the effect</summary>
        public float magnitude = 1f;
        
        /// <summary>Duration in ticks (-1 = permanent)</summary>
        public int duration = -1;
        
        /// <summary>How this effect stacks with others of the same type</summary>
        public string stackMode = "Stack";
        
        /// <summary>Effect rarity level</summary>
        public string rarity = "Common";
        
        /// <summary>Required cultivation realm level</summary>
        public int minRealmLevel = 0;
        
        /// <summary>Required cultivation stage</summary>
        public int minStage = 1;
        #endregion
        
        #region StatModifier Properties
        /// <summary>For StatModifierEffect: which stat to modify</summary>
        public StatDef statDef;
        
        /// <summary>For StatModifierEffect: value to add/multiply</summary>
        public float value = 0f;
        #endregion
        
        #region QiModifier Properties
        /// <summary>For QiModifierEffect: max Qi modifier</summary>
        public float maxQiModifier = 0f;
        
        /// <summary>For QiModifierEffect: Qi regeneration modifier</summary>
        public float qiRegenModifier = 0f;
        #endregion
        
        #region Combat Effect Properties
        /// <summary>For DamageResistanceEffect: percentage of damage to resist</summary>
        public float resistancePercent = 0f;
        
        /// <summary>For CriticalHitEffect: bonus critical hit chance</summary>
        public float critChanceBonus = 0f;
        
        /// <summary>For CriticalHitEffect: damage multiplier on crit</summary>
        public float critDamageMultiplier = 1.5f;
        
        /// <summary>For special abilities: cooldown in ticks</summary>
        public int cooldownTicks = 3000;
        
        /// <summary>For special abilities: trigger chance (0-1)</summary>
        public float triggerChance = 1f;
        
        /// <summary>For movement effects: speed multiplier</summary>
        public float speedMultiplier = 1.2f;
        
        /// <summary>For movement effects: dodge bonus</summary>
        public float dodgeBonus = 0.1f;
        #endregion
        
        public override void PostLoad()
        {
            base.PostLoad();
            // Any initialization logic can go here
        }
        
        public override string ToString()
        {
            return $"CultivationEffectDef({defName}: {label})";
        }
    }
}
