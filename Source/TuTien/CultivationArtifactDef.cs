using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

/// <summary>
/// ✅ Task 1.1: Core Artifact Definition System
/// Defines cultivation artifacts with ELO-based stat generation and auto-behaviors
/// Moved to global namespace for RimWorld DefOf compatibility
/// </summary>
public class CultivationArtifactDef : Def
{
    #region Basic Properties
    /// <summary>Artifact rarity level determining stat ranges</summary>
    public ArtifactRarity rarity = ArtifactRarity.Common;
    
    /// <summary>Artifact type determining behavior and stat focus</summary>
    public ArtifactType artifactType = ArtifactType.Sword;
    
    /// <summary>Required cultivation realm level to equip</summary>
    public int requiredRealmLevel = 0;
    
    /// <summary>Required cultivation stage to equip</summary>
    public int requiredStage = 1;
    
    /// <summary>Base equipment type (Primary, Apparel, etc.)</summary>
    public string equipmentType = "Primary";
    #endregion

    #region ELO & Stat Ranges
    /// <summary>ELO rating range for this artifact type - determines final power level</summary>
    public FloatRange eloRange = new FloatRange(100f, 200f);
    
    /// <summary>Base damage range (scaled by ELO)</summary>
    public FloatRange damageRange = new FloatRange(15f, 25f);
    
    /// <summary>Artifact Qi pool range (scaled by ELO)</summary>
    public FloatRange qiPoolRange = new FloatRange(50f, 80f);
    
    /// <summary>Attack range multiplier</summary>
    public FloatRange rangeMultiplier = new FloatRange(0.8f, 1.2f);
    
    /// <summary>Accuracy range (0-1)</summary>
    public FloatRange accuracyRange = new FloatRange(0.70f, 0.85f);
    
    /// <summary>Cooldown ticks range (lower = faster attacks)</summary>
    public IntRange cooldownRange = new IntRange(120, 240);
    #endregion

    #region Auto-Behaviors
    /// <summary>Whether this artifact can auto-attack enemies</summary>
    public bool hasAutoAttack = true;
    
    /// <summary>Detection radius for auto-targeting</summary>
    public float detectionRadius = 12f;
    
    /// <summary>Auto-cast skill defNames</summary>
    public List<string> autoSkills = new List<string>();
    
    /// <summary>Qi absorption rate from wearer (per tick)</summary>
    public float qiAbsorptionRate = 1f;
    
    /// <summary>Qi efficiency multiplier (lower = more efficient)</summary>
    public float qiEfficiency = 1f;
    #endregion

    #region Buff Generation
    /// <summary>Number of buffs to generate (range based on rarity)</summary>
    public IntRange buffCountRange = new IntRange(1, 2);
    
    /// <summary>Primary buff categories for this artifact type</summary>
    public List<string> primaryBuffCategories = new List<string> { "Combat" };
    
    /// <summary>Secondary buff categories (lower chance)</summary>
    public List<string> secondaryBuffCategories = new List<string> { "Cultivation" };
    #endregion

    public override void PostLoad()
    {
        base.PostLoad();
        Log.Message($"[TuTien] ✅ Loaded CultivationArtifactDef: {defName} ({label}) - Type: {artifactType}, Rarity: {rarity}");
    }

    public override string ToString()
    {
        return $"CultivationArtifactDef({defName}: {label} [{rarity} {artifactType}])";
    }
}

/// <summary>
/// Artifact rarity levels - higher rarity = better stat ranges and more buffs
/// </summary>
public enum ArtifactRarity
{
    Common,      // White - Basic artifacts, 1-2 buffs
    Uncommon,    // Green - Decent artifacts, 2-3 buffs  
    Rare,        // Blue - Good artifacts, 3-4 buffs
    Epic,        // Purple - Great artifacts, 4-5 buffs
    Legendary,   // Orange - Excellent artifacts, 5-6 buffs
    Immortal     // Gold - Godlike artifacts, 6-8 buffs
}

/// <summary>
/// Artifact types - determines stat focus and behavior patterns
/// </summary>
public enum ArtifactType
{
    // Weapon Types
    Sword,        // High damage, medium range, auto-attack focus
    Spear,        // Medium damage, long reach, piercing attacks
    Bow,          // Long range, projectile attacks, high accuracy
    Staff,        // AoE attacks, high Qi consumption, elemental effects
    Orb,          // Floating weapons, balanced stats, special abilities
    
    // Armor Types  
    Crown,        // Head slot, mental bonuses, Qi regeneration
    Robe,         // Torso slot, core protection, Qi capacity
    Boots,        // Feet slot, movement bonuses, agility
    Gloves,       // Hand slot, work speed, dexterity
    
    // Accessory Types
    Shield,       // Shield slot, defensive abilities, barriers
    Talisman,     // Belt slot, passive bonuses, aura effects
    Ring          // Accessory slot, subtle but powerful effects
}
