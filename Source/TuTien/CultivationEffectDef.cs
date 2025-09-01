using System;
using Verse;
using RimWorld;

/// <summary>
/// ✅ Cultivation Effect Definition - Moved to global namespace for RimWorld compatibility
/// This defines cultivation effects that can be easily added via XML files
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
    
    #region Advanced Effect Properties
    /// <summary>For QiBurst effects: burst damage amount</summary>
    public float burstDamage = 50f;
    
    /// <summary>For QiBurst effects: explosion radius</summary>
    public float burstRadius = 3f;
    
    /// <summary>For Qi-consuming abilities: Qi cost per use</summary>
    public float qiCost = 10f;
    
    /// <summary>For HealingAura effects: amount healed per interval</summary>
    public float healAmount = 1f;
    
    /// <summary>For area effects: radius of effect</summary>
    public float auraRadius = 5f;
    
    /// <summary>For periodic effects: interval between applications in ticks</summary>
    public int healInterval = 300;
    
    /// <summary>For DamageResistance effects: specific damage type to resist</summary>
    public string targetDamageType = "Any";
    #endregion
    
    public override void PostLoad()
    {
        base.PostLoad();
        // Debug logging to track when definitions are loaded
        Log.Message($"[TuTien] ✅ Loaded CultivationEffectDef: {defName} ({label}) - Class: {effectClass}, Category: {category}");
    }
    
    public override string ToString()
    {
        return $"CultivationEffectDef({defName}: {label})";
    }
}
