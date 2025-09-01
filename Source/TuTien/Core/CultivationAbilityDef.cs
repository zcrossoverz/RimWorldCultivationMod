using System.Collections.Generic;
using Verse;
using RimWorld;

/// <summary>
/// Core Ability Definition - defines cultivation skills/techniques
/// Can be used by Pawns, Equipment, or Buildings
/// </summary>
public class CultivationAbilityDef : Def
{
    #region Basic Properties
    /// <summary>Display name of the ability</summary>
    public string abilityLabel;
    
    /// <summary>Description of what the ability does</summary>
    public string abilityDescription;
    
    /// <summary>Icon path for UI</summary>
    public string iconPath = "UI/Commands/Attack";
    
    /// <summary>Ability category for organization</summary>
    public string category = "Combat";
    #endregion

    #region Cost & Cooldown
    /// <summary>Qi cost to cast this ability</summary>
    public float qiCost = 10f;
    
    /// <summary>Cooldown in ticks between casts</summary>
    public int cooldownTicks = 300;
    
    /// <summary>Required cultivation realm to use</summary>
    public int requiredRealm = 0;
    
    /// <summary>Required cultivation stage to use</summary>
    public int requiredStage = 1;
    #endregion

    #region Targeting & Range
    /// <summary>How this ability targets</summary>
    public AbilityTargetType targetType = AbilityTargetType.Self;
    
    /// <summary>Range in tiles</summary>
    public float range = 1f;
    
    /// <summary>Area of effect radius</summary>
    public float aoeRadius = 0f;
    
    /// <summary>Line of sight required</summary>
    public bool requiresLineOfSight = true;
    #endregion

    #region Effects
    /// <summary>List of effects this ability applies</summary>
    public List<AbilityEffectDef> effects = new List<AbilityEffectDef>();
    
    /// <summary>Sound to play when casting</summary>
    public string soundCast;
    
    /// <summary>Sound to play on impact</summary>
    public string soundImpact;
    #endregion

    public override void PostLoad()
    {
        base.PostLoad();
        if (abilityLabel.NullOrEmpty()) abilityLabel = label;
        if (abilityDescription.NullOrEmpty()) abilityDescription = description;
        
        Log.Message($"[TuTien] ✅ Loaded CultivationAbilityDef: {defName} ({abilityLabel}) - Category: {category}");
    }
}

/// <summary>
/// Targeting types for abilities
/// </summary>
public enum AbilityTargetType
{
    Self,           // Target self only
    Touch,          // Touch range target
    Ranged,         // Ranged single target
    GroundTarget,   // Target ground location
    AreaAroundSelf, // AoE around caster
    AreaAroundTarget // AoE around target
}

/// <summary>
/// Individual effect within an ability
/// </summary>
public class AbilityEffectDef
{
    /// <summary>Type of effect</summary>
    public string effectType = "Damage";
    
    /// <summary>Effect magnitude</summary>
    public float magnitude = 1f;
    
    /// <summary>Duration in ticks (for buffs/debuffs)</summary>
    public int duration = 0;
    
    /// <summary>Damage type (if damage effect)</summary>
    public string damageType = "Cut";
    
    /// <summary>Hediff to apply (if buff/debuff)</summary>
    public string hediffDef;
}
