# 🎯 Skills & Abilities Framework - Complete Guide

## 📖 **Table of Contents**

1. [Framework Overview](#framework-overview)
2. [CultivationSkillDef System](#cultivationskilldef-system)
3. [CultivationAbilityDef System](#cultivationabilitydef-system)
4. [Effect System Architecture](#effect-system-architecture)
5. [Execution Flow Diagrams](#execution-flow-diagrams)
6. [Implementation Examples](#implementation-examples)
7. [Best Practices](#best-practices)
8. [Performance Guidelines](#performance-guidelines)

---

## 🎯 **Framework Overview**

Tu Tiên implements a dual skill system to provide maximum flexibility for different types of cultivation abilities:

```
Dual Skill Framework Architecture:
┌─────────────────────────────────────────────────────────────┐
│                 CULTIVATION SKILLS FRAMEWORK                │
├─────────────────────────────┬───────────────────────────────┤
│    CULTIVATION SKILL DEF    │   CULTIVATION ABILITY DEF     │
│         (Direct)            │        (Effect-Based)         │
├─────────────────────────────┼───────────────────────────────┤
│                             │                               │
│ ┌─────────────────────────┐ │ ┌─────────────────────────────┐ │
│ │ Design Philosophy       │ │ │ Design Philosophy           │ │
│ │                         │ │ │                             │ │
│ │ • Full C# control       │ │ │ • XML-driven configuration  │ │
│ │ • Complex mechanics     │ │ │ • Modular effect system     │ │
│ │ • Performance optimized │ │ │ • Easy to extend            │ │
│ │ • Direct execution      │ │ │ • Standardized patterns     │ │
│ └─────────────────────────┘ │ └─────────────────────────────┘ │
│                             │                               │
│ ┌─────────────────────────┐ │ ┌─────────────────────────────┐ │
│ │ Implementation          │ │ │ Implementation              │ │
│ │                         │ │ │                             │ │
│ │ • SkillWorker class     │ │ │ • Effects array in XML      │ │
│ │ • ExecuteSkillEffect()  │ │ │ • Generic effect processing │ │
│ │ • Custom validation     │ │ │ • Standardized targeting    │ │
│ │ • Direct damage calc    │ │ │ • Flexible effect types     │ │
│ └─────────────────────────┘ │ └─────────────────────────────┘ │
│                             │                               │
│ ┌─────────────────────────┐ │ ┌─────────────────────────────┐ │
│ │ Use Cases               │ │ │ Use Cases                   │ │
│ │                         │ │ │                             │ │
│ │ • Advanced combat       │ │ │ • Standard RPG abilities    │ │
│ │ • Passive abilities     │ │ │ • Simple damage/heal        │ │
│ │ • Complex interactions  │ │ │ • Buff/debuff application   │ │
│ │ • Custom algorithms     │ │ │ • Multi-target effects      │ │
│ └─────────────────────────┘ │ └─────────────────────────────┘ │
└─────────────────────────────┴───────────────────────────────┘
```

### **System Selection Guide**

```
When to use CultivationSkillDef:
✅ Complex damage calculations
✅ Multi-step skill effects  
✅ Advanced targeting logic
✅ Performance-critical skills
✅ Integration with other systems
✅ Custom visual effects

When to use CultivationAbilityDef:
✅ Standard damage/heal/buff patterns
✅ XML-only implementations
✅ Modular effect combinations
✅ Rapid prototyping
✅ Community mod compatibility
✅ Simple effect stacking
```

---

## ⚔️ **CultivationSkillDef System**

### **Core Architecture**

```csharp
/// <summary>
/// Direct execution skill system with maximum flexibility
/// </summary>
public abstract class CultivationSkillWorker
{
    public CultivationSkillDef def;
    
    /// <summary>
    /// Main execution method - implement your skill logic here
    /// </summary>
    public abstract void ExecuteSkillEffect(Pawn pawn, LocalTargetInfo target);
    
    /// <summary>
    /// Validation method - check if skill can be used
    /// </summary>
    public virtual bool CanExecute(Pawn pawn, LocalTargetInfo target)
    {
        var cultivationData = pawn.GetCultivationData();
        if (cultivationData == null) return false;
        
        // Basic requirements
        if (cultivationData.currentRealm < def.requiredRealm) return false;
        if (cultivationData.currentStage < def.requiredStage) return false;
        if (cultivationData.currentQi < def.qiCost) return false;
        
        // Range check
        if (def.range > 0 && pawn.Position.DistanceTo(target.Cell) > def.range) return false;
        
        return true;
    }
    
    /// <summary>
    /// Get reason why skill cannot be executed
    /// </summary>
    public virtual string GetDisabledReason(Pawn pawn, LocalTargetInfo target)
    {
        var cultivationData = pawn.GetCultivationData();
        if (cultivationData == null) return "No cultivation data";
        
        if (cultivationData.currentRealm < def.requiredRealm)
            return $"Requires {def.requiredRealm} realm";
        if (cultivationData.currentStage < def.requiredStage)
            return $"Requires stage {def.requiredStage}";
        if (cultivationData.currentQi < def.qiCost)
            return $"Need {def.qiCost} Qi ({cultivationData.currentQi} available)";
        if (def.range > 0 && pawn.Position.DistanceTo(target.Cell) > def.range)
            return "Target out of range";
            
        return "Cannot use";
    }
    
    #region Helper Methods
    /// <summary>
    /// Calculate skill power based on cultivation level
    /// </summary>
    protected float GetSkillPowerMultiplier(Pawn pawn)
    {
        var data = pawn.GetCultivationData();
        if (data == null) return 1f;
        
        // Base multiplier from realm and stage
        float realmMultiplier = (int)data.currentRealm * 0.5f;
        float stageMultiplier = data.currentStage * 0.1f;
        
        // Talent bonus
        float talentMultiplier = GetTalentMultiplier(data.talent);
        
        return 1f + realmMultiplier + stageMultiplier + talentMultiplier;
    }
    
    /// <summary>
    /// Apply qi-enhanced damage to target
    /// </summary>
    protected void ApplyQiDamage(Pawn attacker, Thing target, float baseDamage, DamageDef damageType)
    {
        float finalDamage = baseDamage * GetSkillPowerMultiplier(attacker);
        
        var dinfo = new DamageInfo(damageType, finalDamage, 0f, -1f, attacker);
        target.TakeDamage(dinfo);
        
        Log.Message($"[SKILL] {attacker.LabelShort} dealt {finalDamage} {damageType.label} damage");
    }
    
    /// <summary>
    /// Create visual effects for skill use
    /// </summary>
    protected void CreateSkillVisualEffects(Pawn pawn, LocalTargetInfo target, string effectType = "default")
    {
        switch (effectType.ToLower())
        {
            case "qi":
                FleckMaker.ThrowAirPuffUp(pawn.DrawPos, pawn.Map);
                break;
            case "lightning":
                FleckMaker.ThrowLightningGlow(target.Cell.ToVector3(), pawn.Map, 1f);
                break;
            case "fire":
                FleckMaker.ThrowFireGlow(target.Cell, pawn.Map, 1f);
                break;
            case "impact":
                FleckMaker.ThrowMicroSparks(target.Thing.DrawPos, pawn.Map);
                break;
            default:
                FleckMaker.ThrowDustPuffThick(target.Cell.ToVector3(), pawn.Map, 2f, Color.cyan);
                break;
        }
    }
    #endregion
}
```

### **Advanced Skill Worker Examples**

#### **Example 1: QiSwordSkillWorker (AOE Attack)**

```csharp
public class QiSwordSkillWorker : CultivationSkillWorker
{
    public override void ExecuteSkillEffect(Pawn pawn, LocalTargetInfo target)
    {
        var cultivationData = pawn.GetCultivationData();
        if (cultivationData == null) return;
        
        // Consume Qi
        cultivationData.currentQi -= def.qiCost;
        
        // Calculate damage
        float baseDamage = 15f;
        float powerMultiplier = GetSkillPowerMultiplier(pawn);
        float finalDamage = baseDamage * powerMultiplier;
        
        // Get primary target
        if (target.Thing is Pawn primaryTarget)
        {
            ApplyQiDamage(pawn, primaryTarget, finalDamage, DamageDefOf.Cut);
        }
        
        // AOE effect - damage nearby enemies
        var cellsInRange = GenAdj.CellsAdjacent8Way(target.Cell).ToList();
        foreach (var cell in cellsInRange)
        {
            var thingsInCell = cell.GetThingList(pawn.Map);
            foreach (var thing in thingsInCell)
            {
                if (thing is Pawn nearbyPawn && nearbyPawn.HostileTo(pawn))
                {
                    // Reduced AOE damage
                    ApplyQiDamage(pawn, nearbyPawn, finalDamage * 0.5f, DamageDefOf.Cut);
                }
            }
        }
        
        // Visual effects
        CreateSkillVisualEffects(pawn, target, "qi");
        
        // Create qi wave visual
        for (int i = 0; i < 8; i++)
        {
            var direction = GenAdj.AdjacentCells[i];
            var effectCell = target.Cell + direction;
            if (effectCell.InBounds(pawn.Map))
            {
                FleckMaker.ThrowDustPuffThick(effectCell.ToVector3(), pawn.Map, 1f, Color.cyan);
            }
        }
        
        // Sound effect
        def.soundDefOnHit?.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
        
        Log.Message($"[QI SWORD] {pawn.LabelShort} executed qi sword strike for {finalDamage} damage");
    }
    
    public override bool CanExecute(Pawn pawn, LocalTargetInfo target)
    {
        if (!base.CanExecute(pawn, target)) return false;
        
        // Additional validation: need melee weapon
        var weapon = pawn.equipment?.Primary;
        if (weapon == null || !weapon.def.IsMeleeWeapon) return false;
        
        return true;
    }
    
    public override string GetDisabledReason(Pawn pawn, LocalTargetInfo target)
    {
        var baseReason = base.GetDisabledReason(pawn, target);
        if (baseReason != "Cannot use") return baseReason;
        
        var weapon = pawn.equipment?.Primary;
        if (weapon == null || !weapon.def.IsMeleeWeapon)
            return "Requires melee weapon";
            
        return "Cannot use";
    }
}
```

#### **Example 2: QiShieldSkillWorker (Passive Defense)**

```csharp
public class QiShieldSkillWorker : CultivationSkillWorker
{
    public override void ExecuteSkillEffect(Pawn pawn, LocalTargetInfo target)
    {
        var cultivationData = pawn.GetCultivationData();
        if (cultivationData == null) return;
        
        // Check if shield already active
        var existingShield = pawn.health.hediffSet.GetFirstHediffOfDef(TuTienDefOf.QiShieldHediff);
        if (existingShield != null)
        {
            // Refresh/strengthen existing shield
            existingShield.Severity = Mathf.Min(existingShield.Severity + 1f, 5f);
            Messages.Message($"{pawn.LabelShort}'s qi shield strengthened!", MessageTypeDefOf.NeutralEvent);
        }
        else
        {
            // Create new shield
            var shield = HediffMaker.MakeHediff(TuTienDefOf.QiShieldHediff, pawn);
            shield.Severity = GetShieldStrength(pawn);
            pawn.health.AddHediff(shield);
            
            Messages.Message($"{pawn.LabelShort} activates qi shield!", MessageTypeDefOf.PositiveEvent);
        }
        
        // Consume Qi
        cultivationData.currentQi -= def.qiCost;
        
        // Visual effects
        CreateShieldVisualEffects(pawn);
        
        Log.Message($"[QI SHIELD] {pawn.LabelShort} activated qi shield");
    }
    
    private float GetShieldStrength(Pawn pawn)
    {
        var data = pawn.GetCultivationData();
        if (data == null) return 1f;
        
        // Shield strength based on cultivation level
        float baseStrength = 2f;
        float cultivationBonus = ((int)data.currentRealm * 0.5f) + (data.currentStage * 0.1f);
        
        return baseStrength + cultivationBonus;
    }
    
    private void CreateShieldVisualEffects(Pawn pawn)
    {
        // Protective aura effect
        FleckMaker.ThrowAirPuffUp(pawn.DrawPos, pawn.Map);
        
        // Shield shimmer effects around pawn
        for (int i = 0; i < 6; i++)
        {
            var angle = (360f / 6f) * i;
            var offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 1.5f,
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 1.5f
            );
            
            FleckMaker.ThrowGlow(pawn.DrawPos + offset, pawn.Map, 0.5f, ColorLibrary.Cyan);
        }
    }
    
    public override bool CanExecute(Pawn pawn, LocalTargetInfo target)
    {
        // Override to allow self-targeting
        var cultivationData = pawn.GetCultivationData();
        if (cultivationData == null) return false;
        
        if (cultivationData.currentRealm < def.requiredRealm) return false;
        if (cultivationData.currentStage < def.requiredStage) return false;
        if (cultivationData.currentQi < def.qiCost) return false;
        
        return true; // No range restrictions for self-buff
    }
}
```

---

## 🎭 **CultivationAbilityDef System**

### **Effect-Based Architecture**

```csharp
/// <summary>
/// Cultivation ability definition with flexible effect system
/// </summary>
public class CultivationAbilityDef : Def
{
    #region Basic Properties
    public string abilityLabel;
    public string abilityDescription;
    public string category = "General";
    
    // Requirements
    public CultivationRealm requiredRealm = CultivationRealm.FoundationBuilding;
    public int requiredStage = 1;
    public float qiCost = 10f;
    
    // Targeting
    public AbilityTargetType targetType = AbilityTargetType.Touch;
    public float range = 1f;
    public bool requiresLineOfSight = true;
    public bool canTargetSelf = false;
    
    // Timing
    public int cooldownTicks = 300;
    public int castingTime = 0; // Instant by default
    
    // UI
    public Texture2D uiIcon;
    public Color uiIconColor = Color.white;
    #endregion
    
    #region Effect System
    /// <summary>
    /// List of effects applied when ability is used
    /// Supports both concrete effect classes and generic AbilityEffectDef
    /// </summary>
    public List<object> effects = new List<object>();
    
    /// <summary>
    /// Sound played when ability is cast
    /// </summary>
    public SoundDef castSound;
    
    /// <summary>
    /// Sound played when ability hits target
    /// </summary>
    public SoundDef impactSound;
    #endregion
    
    #region Advanced Features
    /// <summary>
    /// Chance for ability to not consume Qi (mastery system)
    /// </summary>
    public float qiRefundChance = 0f;
    
    /// <summary>
    /// Scaling factor for effect magnitudes based on cultivation level
    /// </summary>
    public float cultivationScaling = 0.1f;
    
    /// <summary>
    /// Elements this ability is associated with
    /// </summary>
    public List<QiType> elementalTypes = new List<QiType>();
    
    /// <summary>
    /// Combo system - abilities that can chain after this one
    /// </summary>
    public List<string> comboFollowUps = new List<string>();
    #endregion
}
```

### **Effect Processing System**

```csharp
/// <summary>
/// Advanced effect processing with extensible architecture
/// </summary>
public class AbilityEffectProcessor
{
    private static readonly Dictionary<string, Func<AbilityEffectDef, Pawn, LocalTargetInfo, bool>> 
        effectHandlers = new Dictionary<string, Func<AbilityEffectDef, Pawn, LocalTargetInfo, bool>>();
    
    static AbilityEffectProcessor()
    {
        RegisterDefaultEffectHandlers();
    }
    
    private static void RegisterDefaultEffectHandlers()
    {
        effectHandlers["damage"] = ProcessDamageEffect;
        effectHandlers["heal"] = ProcessHealEffect;
        effectHandlers["buff"] = ProcessBuffEffect;
        effectHandlers["debuff"] = ProcessDebuffEffect;
        effectHandlers["teleport"] = ProcessTeleportEffect;
        effectHandlers["summon"] = ProcessSummonEffect;
        effectHandlers["projectile"] = ProcessProjectileEffect;
        effectHandlers["aoe"] = ProcessAOEEffect;
    }
    
    public static void RegisterCustomEffectHandler(string effectType, 
        Func<AbilityEffectDef, Pawn, LocalTargetInfo, bool> handler)
    {
        effectHandlers[effectType.ToLower()] = handler;
        Log.Message($"[EFFECT SYSTEM] Registered custom effect handler: {effectType}");
    }
    
    public static bool ProcessEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target)
    {
        var effectType = effect.effectType.ToLower();
        
        if (effectHandlers.TryGetValue(effectType, out var handler))
        {
            return handler(effect, caster, target);
        }
        
        Log.Warning($"[EFFECT SYSTEM] No handler for effect type: {effect.effectType}");
        return false;
    }
    
    #region Default Effect Handlers
    private static bool ProcessDamageEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target)
    {
        if (!(target.Thing is Pawn targetPawn)) return false;
        
        // Get damage type
        var damageType = GetDamageDefFromString(effect.damageType);
        
        // Calculate final damage with cultivation scaling
        float finalDamage = effect.magnitude;
        if (effect.cultivationScaling > 0)
        {
            var cultivationData = caster.GetCultivationData();
            if (cultivationData != null)
            {
                float cultivationLevel = (int)cultivationData.currentRealm * 10 + cultivationData.currentStage;
                finalDamage += cultivationLevel * effect.cultivationScaling;
            }
        }
        
        // Apply damage
        var dinfo = new DamageInfo(damageType, finalDamage, 0f, -1f, caster);
        targetPawn.TakeDamage(dinfo);
        
        // Visual feedback
        FleckMaker.ThrowMicroSparks(targetPawn.DrawPos, targetPawn.Map);
        
        // Message feedback
        Messages.Message($"{caster.LabelShort} dealt {finalDamage:F1} damage to {targetPawn.LabelShort}!", 
                        MessageTypeDefOf.NeutralEvent);
        
        return true;
    }
    
    private static bool ProcessHealEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target)
    {
        if (!(target.Thing is Pawn targetPawn)) return false;
        
        // Calculate healing amount
        float healAmount = effect.magnitude;
        if (effect.cultivationScaling > 0)
        {
            var cultivationData = caster.GetCultivationData();
            if (cultivationData != null)
            {
                float cultivationLevel = (int)cultivationData.currentRealm * 10 + cultivationData.currentStage;
                healAmount += cultivationLevel * effect.cultivationScaling;
            }
        }
        
        // Apply healing to injuries
        var hediffSet = targetPawn.health.hediffSet;
        var injuries = new List<Hediff_Injury>();
        hediffSet.GetHediffs<Hediff_Injury>(ref injuries);
        
        float healingLeft = healAmount;
        foreach (var injury in injuries)
        {
            if (healingLeft <= 0) break;
            
            var healThisInjury = Mathf.Min(healingLeft, injury.Severity);
            injury.Heal(healThisInjury);
            healingLeft -= healThisInjury;
        }
        
        // Visual effects
        FleckMaker.ThrowMetaIcon(targetPawn.Position, targetPawn.Map, FleckDefOf.Heart);
        
        Messages.Message($"{caster.LabelShort} healed {targetPawn.LabelShort} for {healAmount:F1} HP!", 
                        MessageTypeDefOf.PositiveEvent);
        
        return true;
    }
    
    private static bool ProcessBuffEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target)
    {
        if (!(target.Thing is Pawn targetPawn) || string.IsNullOrEmpty(effect.hediffDef)) 
            return false;
        
        var hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(effect.hediffDef);
        if (hediffDef == null)
        {
            Log.Warning($"HediffDef '{effect.hediffDef}' not found");
            return false;
        }
        
        // Create and apply hediff
        var hediff = HediffMaker.MakeHediff(hediffDef, targetPawn);
        hediff.Severity = effect.magnitude;
        
        // Set duration if specified
        if (effect.duration > 0 && hediff is HediffWithDuration timedHediff)
        {
            timedHediff.Duration = effect.duration;
        }
        
        targetPawn.health.AddHediff(hediff);
        
        // Visual effects
        FleckMaker.ThrowGlow(targetPawn.DrawPos, targetPawn.Map, 1f, ColorLibrary.Green);
        
        Messages.Message($"{caster.LabelShort} applied {hediffDef.label} to {targetPawn.LabelShort}!", 
                        MessageTypeDefOf.PositiveEvent);
        
        return true;
    }
    #endregion
    
    private static DamageDef GetDamageDefFromString(string damageType)
    {
        return damageType.ToLower() switch
        {
            "cut" => DamageDefOf.Cut,
            "blunt" => DamageDefOf.Blunt,
            "stab" => DamageDefOf.Stab,
            "burn" => DamageDefOf.Burn,
            "frostbite" => DamageDefOf.Frostbite,
            "electrical" => DamageDefOf.EMP, // Closest equivalent
            _ => DamageDefOf.Cut
        };
    }
}
```

---

## 🔄 **Execution Flow Diagrams**

### **CultivationSkillDef Execution Flow**

```
Skill Execution (Direct System):
┌─────────────┐
│ User Clicks │
│ Skill Gizmo │
└──────┬──────┘
       │
       ▼
┌─────────────┐    ┌─────────────┐
│ Validation  │───►│ CanExecute()│
│   Check     │    │  Method     │
└──────┬──────┘    └─────────────┘
       │ Valid ✓
       ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│ Target      │───►│ Execute     │───►│ Apply       │
│ Selection   │    │ SkillEffect │    │ Direct      │
│             │    │ ()          │    │ Effects     │
└─────────────┘    └─────────────┘    └─────────────┘
       │                   │                   │
       ▼                   ▼                   ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│ Range/LOS   │    │ Custom      │    │ Damage,     │
│ Validation  │    │ Logic       │    │ Buffs,      │
│             │    │ Execution   │    │ Effects     │
└─────────────┘    └─────────────┘    └─────────────┘
       │                   │                   │
       └─────────────────┬─│─┬─────────────────┘
                         ▼ │ ▼
                ┌─────────────────┐
                │ Visual Effects  │
                │ & Feedback      │
                └─────────────────┘
```

### **CultivationAbilityDef Execution Flow**

```
Ability Execution (Effect-Based System):
┌─────────────┐
│ User Clicks │
│Ability Gizmo│
└──────┬──────┘
       │
       ▼
┌─────────────┐    ┌─────────────┐
│ Ability     │───►│ Targeting   │
│ Validation  │    │ System      │
└──────┬──────┘    └─────────────┘
       │ Valid ✓
       ▼
┌─────────────┐    ┌─────────────┐
│ TryCast()   │───►│ ApplyEffects│
│ Method      │    │ ()          │
└──────┬──────┘    └─────────────┘
       │                   │
       ▼                   ▼
┌─────────────┐    ┌─────────────────────────────────┐
│ Qi Cost &   │    │        EFFECT PIPELINE          │
│ Cooldown    │    ├─────────────┬─────────────┬─────┤
│ Management  │    │ Concrete    │ Generic     │ Ext │
└─────────────┘    │ Effects     │ Effects     │ end │
                   │             │             │     │
                   │ • Heal      │ • Damage    │ • C │
                   │ • Projectile│ • Buff      │ u s │
                   │ • Revival   │ • Debuff    │ s t │
                   │             │ • Teleport  │ t o │
                   │             │ • Summon    │ o m │
                   └─────────────┴─────────────┴─────┘
                              │
                              ▼
                   ┌─────────────────┐
                   │ Target Effects  │
                   │ Application     │
                   └─────────────────┘
```

### **Enhanced CultivationAbility Implementation**

```csharp
/// <summary>
/// Enhanced ability instance with artifact support
/// </summary>
public class CultivationAbility
{
    public CultivationAbilityDef def;
    public CompAbilityUser comp;
    public CultivationArtifactComp sourceArtifact; // Track if from artifact
    
    #region Casting Logic
    public bool TryCast(LocalTargetInfo target)
    {
        // Enhanced validation
        if (!CanCast(target)) return false;
        
        // Pre-cast effects
        OnPreCast(target);
        
        // Resource consumption
        ConsumeResources();
        
        // Main effect application
        ApplyEffects(target);
        
        // Post-cast effects
        OnPostCast(target);
        
        return true;
    }
    
    public bool CanCast(LocalTargetInfo target = default)
    {
        var pawn = comp.parent as Pawn;
        var cultivationData = pawn.GetCultivationData();
        
        // Basic cultivation requirements
        if (cultivationData == null) return false;
        if (cultivationData.currentRealm < def.requiredRealm) return false;
        if (cultivationData.currentStage < def.requiredStage) return false;
        if (cultivationData.currentQi < def.qiCost) return false;
        
        // Cooldown check
        if (IsOnCooldown()) return false;
        
        // Target validation
        if (target.IsValid && !IsValidTarget(target)) return false;
        
        // Artifact-specific checks
        if (sourceArtifact != null)
        {
            // Check if artifact is still equipped
            if (!pawn.equipment.AllEquipmentListForReading.Contains(sourceArtifact.parent))
                return false;
                
            // ELO-based restrictions
            if (sourceArtifact.currentELO < GetRequiredELO())
                return false;
        }
        
        return true;
    }
    
    private bool IsValidTarget(LocalTargetInfo target)
    {
        switch (def.targetType)
        {
            case AbilityTargetType.Self:
                return target.Thing == comp.parent;
                
            case AbilityTargetType.Touch:
                var distance = comp.parent.Position.DistanceTo(target.Cell);
                return distance <= def.range;
                
            case AbilityTargetType.Ranged:
                return comp.parent.Position.DistanceTo(target.Cell) <= def.range &&
                       (!def.requiresLineOfSight || GenSight.LineOfSight(comp.parent.Position, target.Cell, comp.parent.Map));
                       
            case AbilityTargetType.Enemy:
                return target.Thing is Pawn targetPawn && 
                       targetPawn.HostileTo(comp.parent as Pawn);
                       
            case AbilityTargetType.Ally:
                return target.Thing is Pawn targetPawn && 
                       !targetPawn.HostileTo(comp.parent as Pawn);
                       
            default:
                return true;
        }
    }
    #endregion
    
    #region Enhanced Effect Processing
    private void ApplyEffects(LocalTargetInfo target)
    {
        if (def.effects == null) return;
        
        foreach (var effect in def.effects)
        {
            bool effectApplied = false;
            
            // Handle concrete effect classes
            if (effect is AbilityEffect_LaunchProjectile projectileEffect)
            {
                projectileEffect.Apply(def, comp.parent as Pawn, target);
                effectApplied = true;
            }
            else if (effect is AbilityEffect_Heal healEffect)
            {
                healEffect.Apply(def, comp.parent as Pawn, target);
                effectApplied = true;
            }
            else if (effect is AbilityEffect_CorpseRevival revivalEffect)
            {
                revivalEffect.Apply(def, comp.parent as Pawn, target);
                effectApplied = true;
            }
            // Handle generic effects
            else if (effect is AbilityEffectDef genericEffect)
            {
                effectApplied = AbilityEffectProcessor.ProcessEffect(genericEffect, comp.parent as Pawn, target);
            }
            else
            {
                Log.Warning($"Unknown ability effect type: {effect.GetType()}");
            }
            
            // Track effect application for combo system
            if (effectApplied)
            {
                OnEffectApplied(effect, target);
            }
        }
    }
    
    private void OnEffectApplied(object effect, LocalTargetInfo target)
    {
        // Combo system hooks
        CheckForCombos(effect, target);
        
        // Achievement/progression tracking
        TrackEffectUsage(effect);
        
        // Artifact ELO updates
        if (sourceArtifact != null)
        {
            UpdateArtifactELO(effect, target);
        }
    }
    #endregion
    
    #region Combo System
    private void CheckForCombos(object effect, LocalTargetInfo target)
    {
        if (def.comboFollowUps == null || def.comboFollowUps.Count == 0) return;
        
        var pawn = comp.parent as Pawn;
        
        // Check if any follow-up abilities are available
        foreach (string comboAbilityName in def.comboFollowUps)
        {
            var comboAbilityDef = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(comboAbilityName);
            if (comboAbilityDef == null) continue;
            
            var comboAbility = new CultivationAbility(comboAbilityDef, comp);
            if (comboAbility.CanCast(target))
            {
                // Show combo prompt
                ShowComboPrompt(comboAbility, target);
            }
        }
    }
    
    private void ShowComboPrompt(CultivationAbility comboAbility, LocalTargetInfo target)
    {
        Find.WindowStack.Add(new Dialog_ComboPrompt(comboAbility, target));
    }
    #endregion
}
```

---

## 🎨 **Advanced Implementation Examples**

### **Example 1: Multi-Element Ability**

```xml
<!-- Fire-Lightning Combination Ability -->
<TuTien.CultivationAbilityDef>
  <defName>Ability_ThunderFlame</defName>
  <label>Thunder Flame Strike</label>
  <description>Combines fire and lightning qi for devastating damage.</description>
  
  <qiCost>25</qiCost>
  <cooldownTicks>900</cooldownTicks>
  <requiredRealm>2</requiredRealm>
  <requiredStage>5</requiredStage>
  
  <targetType>Touch</targetType>
  <range>2</range>
  
  <elementalTypes>
    <li>Fire</li>
    <li>Lightning</li>
  </elementalTypes>
  
  <effects>
    <!-- Primary fire damage -->
    <li Class="TuTien.AbilityEffectDef">
      <effectType>Damage</effectType>
      <magnitude>30</magnitude>
      <damageType>Burn</damageType>
      <cultivationScaling>0.5</cultivationScaling>
    </li>
    
    <!-- Secondary lightning damage -->
    <li Class="TuTien.AbilityEffectDef">
      <effectType>Damage</effectType>
      <magnitude>25</magnitude>
      <damageType>Electrical</damageType>
      <cultivationScaling>0.3</cultivationScaling>
    </li>
    
    <!-- Stunning debuff -->
    <li Class="TuTien.AbilityEffectDef">
      <effectType>Debuff</effectType>
      <hediffDef>Stunned</hediffDef>
      <magnitude>1</magnitude>
      <duration>180</duration>
    </li>
  </effects>
  
  <!-- Combo potential -->
  <comboFollowUps>
    <li>Ability_ElementalBurst</li>
    <li>Ability_ChainLightning</li>
  </comboFollowUps>
</TuTien.CultivationAbilityDef>
```

### **Example 2: Summoning Ability**

```xml
<!-- Advanced Summoning Ability -->
<TuTien.CultivationAbilityDef>
  <defName>Ability_SummonQiSpirit</defName>
  <label>Summon Qi Spirit</label>
  <description>Manifest a qi spirit to fight alongside you.</description>
  
  <qiCost>100</qiCost>
  <cooldownTicks>3600</cooldownTicks> <!-- 1 minute -->
  <requiredRealm>3</requiredRealm>
  <requiredStage>1</requiredStage>
  
  <targetType>GroundTarget</targetType>
  <range>5</range>
  
  <effects>
    <li Class="TuTien.Abilities.AbilityEffect_SummonSpirit">
      <spiritType>QiElemental</spiritType>
      <duration>1800</duration> <!-- 30 seconds -->
      <spiritLevel>3</spiritLevel>
      <maxSpirits>2</maxSpirits>
    </li>
  </effects>
</TuTien.CultivationAbilityDef>
```

### **Example 3: Area Effect Ability**

```xml
<!-- AOE Cultivation Ability -->
<TuTien.CultivationAbilityDef>
  <defName>Ability_QiExplosion</defName>
  <label>Qi Explosion</label>
  <description>Release qi in a devastating explosion around the caster.</description>
  
  <qiCost>50</qiCost>
  <cooldownTicks>1200</cooldownTicks>
  <requiredRealm>2</requiredRealm>
  <requiredStage>3</requiredStage>
  
  <targetType>AreaAroundSelf</targetType>
  <range>4</range>
  
  <effects>
    <li Class="TuTien.AbilityEffectDef">
      <effectType>AOE</effectType>
      <magnitude>40</magnitude>
      <damageType>Blunt</damageType>
      <radius>4</radius>
      <cultivationScaling>0.8</cultivationScaling>
    </li>
    
    <!-- Knockback effect -->
    <li Class="TuTien.Abilities.AbilityEffect_Knockback">
      <force>3.5</force>
      <radius>4</radius>
    </li>
  </effects>
</TuTien.CultivationAbilityDef>
```

---

## 📊 **Performance Guidelines**

### **Optimization Strategies**

#### **1. Caching for Frequent Lookups**

```csharp
public static class CultivationCache
{
    private static readonly Dictionary<string, CultivationSkillDef> skillDefCache = 
        new Dictionary<string, CultivationSkillDef>();
    
    private static readonly Dictionary<string, CultivationAbilityDef> abilityDefCache = 
        new Dictionary<string, CultivationAbilityDef>();
    
    private static readonly Dictionary<int, WeakReference<CultivationComp>> componentCache = 
        new Dictionary<int, WeakReference<CultivationComp>>();
    
    public static CultivationSkillDef GetSkillDef(string defName)
    {
        if (!skillDefCache.TryGetValue(defName, out var cached))
        {
            cached = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(defName);
            skillDefCache[defName] = cached; // Cache null results too
        }
        return cached;
    }
    
    public static CultivationAbilityDef GetAbilityDef(string defName)
    {
        if (!abilityDefCache.TryGetValue(defName, out var cached))
        {
            cached = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(defName);
            abilityDefCache[defName] = cached;
        }
        return cached;
    }
    
    public static void ClearCache()
    {
        skillDefCache.Clear();
        abilityDefCache.Clear();
        componentCache.Clear();
    }
}
```

#### **2. Efficient Skill Validation**

```csharp
public static class SkillValidationOptimizer
{
    private static readonly Dictionary<(Pawn, string), (bool canUse, string reason, int cacheFrame)> 
        validationCache = new Dictionary<(Pawn, string), (bool, string, int)>();
    
    public static (bool canUse, string reason) GetValidationResult(Pawn pawn, string skillName)
    {
        var key = (pawn, skillName);
        var currentFrame = Time.frameCount;
        
        // Check cache (valid for 1 frame to handle multiple gizmo updates)
        if (validationCache.TryGetValue(key, out var cached) && 
            cached.cacheFrame == currentFrame)
        {
            return (cached.canUse, cached.reason);
        }
        
        // Perform validation
        var result = PerformValidation(pawn, skillName);
        
        // Cache result
        validationCache[key] = (result.canUse, result.reason, currentFrame);
        
        // Cleanup old cache entries periodically
        if (currentFrame % 60 == 0) // Every 60 frames
        {
            CleanupValidationCache(currentFrame);
        }
        
        return result;
    }
    
    private static void CleanupValidationCache(int currentFrame)
    {
        var keysToRemove = validationCache.Where(kvp => currentFrame - kvp.Value.cacheFrame > 60)
                                         .Select(kvp => kvp.Key)
                                         .ToList();
        
        foreach (var key in keysToRemove)
        {
            validationCache.Remove(key);
        }
    }
}
```

#### **3. Batch Effect Processing**

```csharp
public static class BatchEffectProcessor
{
    private static readonly Queue<EffectApplicationJob> effectQueue = new Queue<EffectApplicationJob>();
    private static readonly List<EffectApplicationJob> currentBatch = new List<EffectApplicationJob>();
    
    public struct EffectApplicationJob
    {
        public AbilityEffectDef effect;
        public Pawn caster;
        public LocalTargetInfo target;
        public int priority;
    }
    
    public static void QueueEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target, int priority = 0)
    {
        effectQueue.Enqueue(new EffectApplicationJob
        {
            effect = effect,
            caster = caster,
            target = target,
            priority = priority
        });
    }
    
    public static void ProcessQueuedEffects()
    {
        // Process up to 10 effects per tick to maintain performance
        int processed = 0;
        while (effectQueue.Count > 0 && processed < 10)
        {
            var job = effectQueue.Dequeue();
            AbilityEffectProcessor.ProcessEffect(job.effect, job.caster, job.target);
            processed++;
        }
    }
    
    // Call from GameComponent or similar tick system
    public static void Tick()
    {
        ProcessQueuedEffects();
    }
}
```

---

## 🎯 **Best Practices**

### **Design Guidelines**

#### **1. Skill System Selection**

```csharp
// Choose CultivationSkillDef when:
public class ComplexSkillExample : CultivationSkillWorker
{
    public override void ExecuteSkillEffect(Pawn pawn, LocalTargetInfo target)
    {
        // Complex multi-step logic
        var phase1Results = ExecutePhase1(pawn, target);
        var phase2Targets = CalculatePhase2Targets(phase1Results);
        ExecutePhase2(pawn, phase2Targets);
        
        // Custom progression tracking
        UpdateSkillMastery(pawn);
        
        // Integration with other mod systems
        TriggerExternalModEvents(pawn, target);
    }
}

// Choose CultivationAbilityDef when:
/*
<TuTien.CultivationAbilityDef>
  <defName>StandardAttack</defName>
  <effects>
    <li Class="TuTien.AbilityEffectDef">
      <effectType>Damage</effectType>
      <magnitude>25</magnitude>
    </li>
  </effects>
</TuTien.CultivationAbilityDef>
*/
```

#### **2. Effect Design Patterns**

```csharp
// Pattern A: Single-target direct effect
public static AbilityEffectDef CreateSingleTargetDamage(float damage, string damageType)
{
    return new AbilityEffectDef
    {
        effectType = "Damage",
        magnitude = damage,
        damageType = damageType
    };
}

// Pattern B: Multi-effect combination
public static List<AbilityEffectDef> CreateHealAndBuff(float healAmount, string buffHediff, int duration)
{
    return new List<AbilityEffectDef>
    {
        new AbilityEffectDef
        {
            effectType = "Heal",
            magnitude = healAmount
        },
        new AbilityEffectDef
        {
            effectType = "Buff",
            hediffDef = buffHediff,
            magnitude = 1f,
            duration = duration
        }
    };
}

// Pattern C: Conditional effect application
public class ConditionalEffect : AbilityEffectDef
{
    public string condition;
    public AbilityEffectDef successEffect;
    public AbilityEffectDef failureEffect;
    
    // Processed by custom effect handler
}
```

#### **3. Error Handling & Validation**

```csharp
public static class SkillValidation
{
    public static ValidationResult ValidateSkillDefinition(CultivationSkillDef skillDef)
    {
        var result = new ValidationResult();
        
        // Check required fields
        if (string.IsNullOrEmpty(skillDef.defName))
            result.AddError("defName is required");
            
        if (skillDef.skillWorkerClass == null)
            result.AddError("skillWorkerClass is required");
            
        if (skillDef.qiCost < 0)
            result.AddWarning("Negative qi cost may cause issues");
            
        // Check skill worker instantiation
        try
        {
            var worker = Activator.CreateInstance(skillDef.skillWorkerClass) as CultivationSkillWorker;
            if (worker == null)
                result.AddError($"Cannot instantiate skill worker: {skillDef.skillWorkerClass}");
        }
        catch (Exception ex)
        {
            result.AddError($"Skill worker instantiation failed: {ex.Message}");
        }
        
        return result;
    }
    
    public static ValidationResult ValidateAbilityDefinition(CultivationAbilityDef abilityDef)
    {
        var result = new ValidationResult();
        
        // Check basic requirements
        if (string.IsNullOrEmpty(abilityDef.defName))
            result.AddError("defName is required");
            
        if (abilityDef.effects == null || abilityDef.effects.Count == 0)
            result.AddWarning("No effects defined - ability will do nothing");
            
        // Validate each effect
        foreach (var effect in abilityDef.effects)
        {
            if (effect is AbilityEffectDef genericEffect)
            {
                if (string.IsNullOrEmpty(genericEffect.effectType))
                    result.AddError("Effect type is required");
                    
                if (genericEffect.magnitude <= 0)
                    result.AddWarning("Zero or negative magnitude may not work as expected");
            }
        }
        
        return result;
    }
}

public class ValidationResult
{
    public List<string> Errors = new List<string>();
    public List<string> Warnings = new List<string>();
    
    public bool IsValid => Errors.Count == 0;
    
    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Errors.Count > 0)
        {
            sb.AppendLine("Errors:");
            foreach (var error in Errors)
                sb.AppendLine($"  - {error}");
        }
        
        if (Warnings.Count > 0)
        {
            sb.AppendLine("Warnings:");
            foreach (var warning in Warnings)
                sb.AppendLine($"  - {warning}");
        }
        
        return sb.ToString();
    }
}
```

---

## 🔧 **Integration Testing**

### **Test Framework**

```csharp
[System.Diagnostics.Conditional("DEBUG")]
public static class CultivationTestFramework
{
    public static void RunAllTests()
    {
        TestSkillSystem();
        TestAbilitySystem();
        TestArtifactIntegration();
        TestUIIntegration();
    }
    
    private static void TestSkillSystem()
    {
        Log.Message("[TEST] Testing CultivationSkillDef system...");
        
        // Test skill definition loading
        var testSkill = DefDatabase<CultivationSkillDef>.GetNamedSilentFail("QiPunch");
        Assert(testSkill != null, "QiPunch skill definition should exist");
        
        // Test skill worker instantiation
        var worker = testSkill.GetSkillWorker();
        Assert(worker != null, "Skill worker should instantiate");
        
        // Test validation logic
        var testPawn = CreateTestPawn();
        var canExecute = worker.CanExecute(testPawn, testPawn);
        Log.Message($"[TEST] QiPunch can execute: {canExecute}");
        
        Log.Message("[TEST] CultivationSkillDef tests completed");
    }
    
    private static void TestAbilitySystem()
    {
        Log.Message("[TEST] Testing CultivationAbilityDef system...");
        
        // Test ability definition loading
        var testAbility = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail("Ability_SwordStrike");
        Assert(testAbility != null, "Ability_SwordStrike should exist");
        
        // Test effect processing
        var testPawn = CreateTestPawn();
        var ability = new CultivationAbility(testAbility, testPawn.GetComp<CompAbilityUser>());
        
        var canCast = ability.CanCast();
        Log.Message($"[TEST] SwordStrike can cast: {canCast}");
        
        Log.Message("[TEST] CultivationAbilityDef tests completed");
    }
    
    private static void TestArtifactIntegration()
    {
        Log.Message("[TEST] Testing artifact integration...");
        
        var testPawn = CreateTestPawn();
        var testArtifact = CreateTestArtifact();
        
        // Test equipment
        testPawn.equipment.AddEquipment(testArtifact);
        
        // Test skill granting
        var artifactComp = testArtifact.GetComp<CultivationArtifactComp>();
        Assert(artifactComp != null, "Artifact should have CultivationArtifactComp");
        
        // Test gizmo generation
        var gizmos = testPawn.GetGizmos().ToList();
        var skillGizmos = gizmos.Where(g => g is Command_Action).Count();
        Log.Message($"[TEST] Gizmos after artifact equip: {skillGizmos}");
        
        Log.Message("[TEST] Artifact integration tests completed");
    }
    
    private static Pawn CreateTestPawn()
    {
        // Create test pawn with cultivation component
        var pawnKind = PawnKindDefOf.Colonist;
        var faction = Faction.OfPlayer;
        var pawn = PawnGenerator.GeneratePawn(pawnKind, faction);
        
        // Ensure cultivation component exists
        var comp = pawn.GetComp<CultivationComp>();
        if (comp?.cultivationData == null)
        {
            comp.cultivationData = new CultivationData();
            comp.cultivationData.currentQi = 100f;
            comp.cultivationData.maxQi = 100f;
        }
        
        return pawn;
    }
    
    private static Thing CreateTestArtifact()
    {
        var artifactDef = ThingDefOf.MeleeWeapon_Sword; // Use existing weapon as base
        var artifact = ThingMaker.MakeThing(artifactDef);
        
        // Would need actual artifact ThingDef in real implementation
        return artifact;
    }
    
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Log.Error($"[TEST FAILED] {message}");
        }
        else
        {
            Log.Message($"[TEST PASSED] {message}");
        }
    }
}
```

---

## 📈 **Advanced Features**

### **Skill Mastery System**

```csharp
public class CultivationSkillMastery
{
    [System.Serializable]
    public class SkillMasteryData
    {
        public string skillName;
        public int usageCount = 0;
        public float masteryLevel = 0f; // 0.0 to 1.0
        public int successfulHits = 0;
        public int totalAttempts = 0;
        
        public float SuccessRate => totalAttempts > 0 ? (float)successfulHits / totalAttempts : 0f;
        public string MasteryRank => GetMasteryRank(masteryLevel);
        
        private string GetMasteryRank(float level)
        {
            return level switch
            {
                < 0.1f => "Novice",
                < 0.3f => "Apprentice", 
                < 0.6f => "Adept",
                < 0.9f => "Expert",
                _ => "Master"
            };
        }
        
        public void RecordUsage(bool successful)
        {
            usageCount++;
            totalAttempts++;
            if (successful) successfulHits++;
            
            // Increase mastery based on usage and success
            float masteryGain = successful ? 0.01f : 0.005f;
            masteryLevel = Mathf.Min(1f, masteryLevel + masteryGain);
        }
        
        public float GetMasteryBonus()
        {
            // Mastery provides bonus to effectiveness
            return 1f + (masteryLevel * 0.5f); // Up to 50% bonus at master level
        }
    }
    
    /// <summary>
    /// Enhanced skill worker with mastery tracking
    /// </summary>
    public abstract class MasteryTrackingSkillWorker : CultivationSkillWorker
    {
        public override void ExecuteSkillEffect(Pawn pawn, LocalTargetInfo target)
        {
            // Record attempt
            var mastery = GetOrCreateMasteryData(pawn, def.defName);
            
            try
            {
                // Execute with mastery bonus
                ExecuteSkillWithMastery(pawn, target, mastery.GetMasteryBonus());
                
                // Record success
                mastery.RecordUsage(true);
            }
            catch (Exception ex)
            {
                // Record failure
                mastery.RecordUsage(false);
                Log.Error($"Skill execution failed: {ex}");
            }
        }
        
        protected abstract void ExecuteSkillWithMastery(Pawn pawn, LocalTargetInfo target, float masteryBonus);
        
        private SkillMasteryData GetOrCreateMasteryData(Pawn pawn, string skillName)
        {
            var comp = pawn.GetComp<CultivationComp>();
            if (comp.skillMasteryData == null)
                comp.skillMasteryData = new Dictionary<string, SkillMasteryData>();
                
            if (!comp.skillMasteryData.TryGetValue(skillName, out var mastery))
            {
                mastery = new SkillMasteryData { skillName = skillName };
                comp.skillMasteryData[skillName] = mastery;
            }
            
            return mastery;
        }
    }
}
```

### **Combo System Implementation**

```csharp
public class CultivationComboSystem
{
    [System.Serializable]
    public class ComboData
    {
        public List<string> sequence = new List<string>();
        public int timeWindow = 300; // Ticks
        public List<AbilityEffectDef> bonusEffects = new List<AbilityEffectDef>();
        public string comboName;
    }
    
    private static readonly Dictionary<Pawn, ComboTracker> activeComboTrackers = 
        new Dictionary<Pawn, ComboTracker>();
    
    public class ComboTracker
    {
        public List<string> currentSequence = new List<string>();
        public int lastUseTick = 0;
        public int timeWindow = 300;
        
        public void RecordSkillUse(string skillName, int currentTick)
        {
            // Reset if too much time passed
            if (currentTick - lastUseTick > timeWindow)
            {
                currentSequence.Clear();
            }
            
            currentSequence.Add(skillName);
            lastUseTick = currentTick;
            
            // Check for combo completion
            CheckForCompletedCombos();
        }
        
        private void CheckForCompletedCombos()
        {
            foreach (var comboDef in DefDatabase<CultivationComboDef>.AllDefs)
            {
                if (IsComboSequenceMatch(comboDef.sequence))
                {
                    ExecuteCombo(comboDef);
                    currentSequence.Clear();
                    break;
                }
            }
        }
        
        private bool IsComboSequenceMatch(List<string> comboSequence)
        {
            if (currentSequence.Count < comboSequence.Count) return false;
            
            var recentSequence = currentSequence.TakeLast(comboSequence.Count);
            return recentSequence.SequenceEqual(comboSequence);
        }
    }
    
    public static void RecordSkillUsage(Pawn pawn, string skillName)
    {
        if (!activeComboTrackers.TryGetValue(pawn, out var tracker))
        {
            tracker = new ComboTracker();
            activeComboTrackers[pawn] = tracker;
        }
        
        tracker.RecordSkillUse(skillName, Find.TickManager.TicksGame);
    }
}
```

---

**Framework Version**: 2.0  
**Last Updated**: September 2025  
**Compatibility**: RimWorld 1.6+
