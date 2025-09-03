# ⚔️ Cultivation Artifacts System - Implementation Guide

## 📖 **Table of Contents**

1. [System Overview](#system-overview)
2. [Component Architecture](#component-architecture)
3. [Implementation Workflow](#implementation-workflow)
4. [ELO System Details](#elo-system-details)
5. [Auto-Skills Mechanism](#auto-skills-mechanism)
6. [Integration with Core Systems](#integration-with-core-systems)
7. [Common Implementation Patterns](#common-implementation-patterns)
8. [Troubleshooting Guide](#troubleshooting-guide)

---

## 🎯 **System Overview**

The Cultivation Artifacts system allows items to grant cultivation skills automatically when equipped. It features:

- **Auto-Skill Granting**: Items automatically teach skills when equipped
- **ELO Rating System**: Artifacts gain experience and unlock abilities
- **Dual Skill Support**: Works with both CultivationSkillDef and CultivationAbilityDef
- **Dynamic UI Integration**: Skills appear as gizmos automatically
- **Performance Tracking**: ELO-based progression system

```
Artifact System Components:
┌─────────────────────────────────────────────────────────────┐
│                  ARTIFACT ECOSYSTEM                         │
├─────────────────┬─────────────────┬─────────────────────────┤
│  DEFINITIONS    │   COMPONENTS    │    UI INTEGRATION       │
│                 │                 │                         │
│ ┌─────────────┐ │ ┌─────────────┐ │ ┌─────────────────────┐ │
│ │ Artifact    │ │ │ Artifact    │ │ │ Unified Gizmo       │ │
│ │ Def         │ │ │ Comp        │ │ │ System              │ │
│ │             │ │ │             │ │ │                     │ │
│ │ • autoSkills│◄┼►│ • ELO track │◄┼►│ • Dual lookup       │ │
│ │ • baseELO   │ │ │ • Equipment │ │ │ • Smart targeting   │ │
│ │ • qiType    │ │ │   events    │ │ │ • Cooldown mgmt     │ │
│ └─────────────┘ │ └─────────────┘ │ └─────────────────────┘ │
└─────────────────┴─────────────────┴─────────────────────────┘
```

---

## 🏗️ **Component Architecture**

### **CultivationArtifactDef Structure**

```csharp
/// <summary>
/// Defines the properties and capabilities of a cultivation artifact
/// </summary>
public class CultivationArtifactDef : Def
{
    #region Core Properties
    /// <summary>Skills automatically granted when artifact is equipped</summary>
    public List<string> autoSkills = new List<string>();
    
    /// <summary>Base ELO rating for new artifacts</summary>
    public int baseELO = 1000;
    
    /// <summary>ELO cap for this artifact type</summary>
    public int maxELO = 3000;
    
    /// <summary>Minimum ELO (for degradation)</summary>
    public int minELO = 500;
    
    /// <summary>ELO growth rate multiplier</summary>
    public float eloGrowthRate = 1.0f;
    
    /// <summary>Associated elemental type</summary>
    public QiType associatedQiType = QiType.Neutral;
    #endregion
    
    #region Advanced Features
    /// <summary>ELO thresholds that unlock additional skills</summary>
    public List<ELOThreshold> eloThresholds = new List<ELOThreshold>();
    
    /// <summary>Stat modifiers based on ELO</summary>
    public List<ELOStatModifier> eloStatModifiers = new List<ELOStatModifier>();
    
    /// <summary>Special properties that activate at certain ELO levels</summary>
    public List<ELOSpecialProperty> specialProperties = new List<ELOSpecialProperty>();
    #endregion
}

[System.Serializable]
public class ELOThreshold
{
    public int requiredELO;
    public string skillName;
    public string unlockMessage = "Ancient knowledge awakens!";
}

[System.Serializable]
public class ELOStatModifier
{
    public int minELO;
    public StatDef statDef;
    public float value;
    public StatModifierType modifierType = StatModifierType.Additive;
}
```

### **CultivationArtifactComp Implementation**

```csharp
/// <summary>
/// Component that handles artifact-specific cultivation mechanics
/// </summary>
public class CultivationArtifactComp : ThingComp
{
    #region State Data
    public int currentELO;
    public int wins = 0;
    public int losses = 0;
    public int draws = 0;
    
    public List<string> grantedSkills = new List<string>();
    public List<string> unlockedThresholdSkills = new List<string>();
    
    private bool isEquipped = false;
    private Pawn equippedBy = null;
    #endregion
    
    #region Properties
    public CultivationArtifactDef def => parent.def.GetModExtension<CultivationArtifactDef>();
    
    public float ELOPercentage => 
        (float)(currentELO - def.minELO) / (def.maxELO - def.minELO);
    
    public string ELORank => GetELORank(currentELO);
    #endregion
    
    #region Equipment Events
    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        
        isEquipped = true;
        equippedBy = pawn;
        
        // Grant all auto-skills
        GrantAutoSkills(pawn);
        
        // Apply ELO-based stat modifiers
        ApplyELOModifiers(pawn);
        
        // Check for threshold unlocks
        CheckThresholdUnlocks(pawn);
        
        Messages.Message($"{pawn.LabelShort} equipped {parent.LabelShort} (ELO: {currentELO})", 
                        MessageTypeDefOf.NeutralEvent);
    }
    
    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        
        // Remove granted skills
        RemoveGrantedSkills(pawn);
        
        // Remove stat modifiers
        RemoveELOModifiers(pawn);
        
        isEquipped = false;
        equippedBy = null;
        
        Messages.Message($"{pawn.LabelShort} unequipped {parent.LabelShort}", 
                        MessageTypeDefOf.NeutralEvent);
    }
    #endregion
    
    #region Skill Management
    private void GrantAutoSkills(Pawn pawn)
    {
        if (def?.autoSkills == null) return;
        
        var comp = pawn.GetComp<CultivationComp>();
        if (comp == null) return;
        
        foreach (string skillName in def.autoSkills)
        {
            if (!comp.knownSkills.Contains(skillName))
            {
                comp.knownSkills.Add(skillName);
                grantedSkills.Add(skillName);
                
                Log.Message($"[ARTIFACT] Granted skill: {skillName} to {pawn.LabelShort}");
            }
        }
        
        // Also check threshold skills
        CheckThresholdUnlocks(pawn);
    }
    
    private void CheckThresholdUnlocks(Pawn pawn)
    {
        if (def?.eloThresholds == null) return;
        
        var comp = pawn.GetComp<CultivationComp>();
        if (comp == null) return;
        
        foreach (var threshold in def.eloThresholds)
        {
            if (currentELO >= threshold.requiredELO && 
                !unlockedThresholdSkills.Contains(threshold.skillName))
            {
                comp.knownSkills.Add(threshold.skillName);
                unlockedThresholdSkills.Add(threshold.skillName);
                
                Messages.Message(threshold.unlockMessage, MessageTypeDefOf.PositiveEvent);
                Log.Message($"[ARTIFACT] ELO unlock: {threshold.skillName} at {currentELO} ELO");
            }
        }
    }
    #endregion
    
    #region ELO Management
    public void UpdateELO(bool victory, int opponentELO = 1000)
    {
        int K = CalculateKFactor();
        float expectedScore = CalculateExpectedScore(opponentELO);
        float actualScore = victory ? 1.0f : 0.0f;
        
        int eloChange = Mathf.RoundToInt(K * (actualScore - expectedScore) * def.eloGrowthRate);
        currentELO = Mathf.Clamp(currentELO + eloChange, def.minELO, def.maxELO);
        
        if (victory) wins++; else losses++;
        
        // Check for new unlocks
        if (isEquipped && equippedBy != null)
        {
            CheckThresholdUnlocks(equippedBy);
            ApplyELOModifiers(equippedBy);
        }
        
        Log.Message($"[ARTIFACT ELO] {parent.LabelShort}: {currentELO} " +
                   $"({(eloChange >= 0 ? "+" : "")}{eloChange}) " +
                   $"W:{wins} L:{losses}");
    }
    
    private int CalculateKFactor()
    {
        // Dynamic K-factor based on current ELO and experience
        if (currentELO < 1200) return 40; // High volatility for new artifacts
        if (currentELO < 2000) return 20; // Medium volatility
        return 10; // Low volatility for master artifacts
    }
    
    private float CalculateExpectedScore(int opponentELO)
    {
        return 1.0f / (1.0f + Mathf.Pow(10, (opponentELO - currentELO) / 400.0f));
    }
    
    private string GetELORank(int elo)
    {
        return elo switch
        {
            < 800 => "Broken",
            < 1000 => "Inferior",
            < 1200 => "Common",
            < 1500 => "Uncommon", 
            < 1800 => "Rare",
            < 2200 => "Epic",
            < 2600 => "Legendary",
            _ => "Mythical"
        };
    }
    #endregion
}
```

---

## 🔄 **Implementation Workflow**

### **Creating a New Artifact Type**

#### **Step 1: Define Artifact Properties**

```xml
<!-- File: Defs/CultivationArtifactDefs_Weapons.xml -->
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  
  <TuTien.CultivationArtifactDef>
    <defName>DragonScaleSword</defName>
    
    <!-- Auto-granted skills (mix of both systems) -->
    <autoSkills>
      <li>QiPunch</li>                    <!-- CultivationSkillDef -->
      <li>Ability_SwordStrike</li>        <!-- CultivationAbilityDef -->
      <li>QiShield</li>                   <!-- CultivationSkillDef -->
    </autoSkills>
    
    <!-- ELO System -->
    <baseELO>1200</baseELO>
    <maxELO>3500</maxELO>
    <minELO>600</minELO>
    <eloGrowthRate>1.5</eloGrowthRate>
    
    <!-- Element Association -->
    <associatedQiType>Fire</associatedQiType>
    
    <!-- Progressive Unlocks -->
    <eloThresholds>
      <li>
        <requiredELO>1500</requiredELO>
        <skillName>Ability_DragonBreath</skillName>
        <unlockMessage>The dragon's spirit awakens within the blade!</unlockMessage>
      </li>
      <li>
        <requiredELO>2000</requiredELO>
        <skillName>QiDragonform</skillName>
        <unlockMessage>Ancient draconic techniques emerge!</unlockMessage>
      </li>
    </eloThresholds>
    
    <!-- Stat Modifiers -->
    <eloStatModifiers>
      <li>
        <minELO>1000</minELO>
        <statDef>MeleeHitChance</statDef>
        <value>0.1</value>
        <modifierType>Additive</modifierType>
      </li>
      <li>
        <minELO>2000</minELO>
        <statDef>MeleeDPS</statDef>
        <value>1.5</value>
        <modifierType>Multiplicative</modifierType>
      </li>
    </eloStatModifiers>
    
  </TuTien.CultivationArtifactDef>

</Defs>
```

#### **Step 2: Create Thing Definition**

```xml
<!-- File: Defs/ThingDefs_ArtifactWeapons.xml -->
<?xml version="1.0" encoding="utf-8"?>
<Defs>

  <ThingDef ParentName="BaseMeleeWeapon">
    <defName>DragonScaleSword</defName>
    <label>dragon scale sword</label>
    <description>A legendary blade forged from dragon scales, pulsing with ancient qi.</description>
    
    <!-- Basic weapon stats -->
    <graphicData>
      <texPath>Things/Item/Equipment/WeaponMelee/DragonSword</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    
    <techLevel>Medieval</techLevel>
    <weaponTags><li>Medieval</li></weaponTags>
    <tradeTags><li>Weapon</li></tradeTags>
    
    <!-- Weapon properties -->
    <statBases>
      <WorkToMake>20000</WorkToMake>
      <Mass>2.5</Mass>
      <MarketValue>2000</MarketValue>
    </statBases>
    
    <tools>
      <li>
        <label>handle</label>
        <capacities><li>Poke</li></capacities>
        <power>12</power>
        <cooldownTime>1.5</cooldownTime>
      </li>
      <li>
        <label>blade</label>
        <capacities><li>Cut</li></capacities>
        <power>25</power>
        <cooldownTime>2.0</cooldownTime>
        <armorPenetration>0.3</armorPenetration>
      </li>
    </tools>
    
    <!-- CRITICAL: Artifact Component -->
    <comps>
      <li Class="TuTien.CultivationArtifactCompProperties">
        <artifactDef>DragonScaleSword</artifactDef>
      </li>
    </comps>
    
  </ThingDef>

</Defs>
```

#### **Step 3: Component Properties Class**

```csharp
/// <summary>
/// Properties class for CultivationArtifactComp
/// Links ThingDef to CultivationArtifactDef
/// </summary>
public class CultivationArtifactCompProperties : CompProperties
{
    public string artifactDef;
    
    public CultivationArtifactCompProperties()
    {
        compClass = typeof(CultivationArtifactComp);
    }
    
    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        
        // Validate artifact def exists
        var def = DefDatabase<CultivationArtifactDef>.GetNamedSilentFail(artifactDef);
        if (def == null)
        {
            Log.Error($"CultivationArtifactDef '{artifactDef}' not found for {parentDef.defName}");
        }
    }
}
```

---

## ⚡ **ELO System Details**

### **ELO Calculation Algorithm**

```csharp
public class ELOCalculator
{
    /// <summary>
    /// Standard ELO calculation with cultivation-specific modifications
    /// </summary>
    public static int CalculateELOChange(int currentELO, int opponentELO, 
                                        bool victory, float growthRate = 1.0f)
    {
        // K-factor determination
        int kFactor = GetKFactor(currentELO);
        
        // Expected score calculation (standard ELO formula)
        float expectedScore = 1.0f / (1.0f + Mathf.Pow(10, (opponentELO - currentELO) / 400.0f));
        
        // Actual score
        float actualScore = victory ? 1.0f : 0.0f;
        
        // ELO change with cultivation growth rate
        int change = Mathf.RoundToInt(kFactor * (actualScore - expectedScore) * growthRate);
        
        return change;
    }
    
    /// <summary>
    /// Dynamic K-factor based on current ELO and artifact experience
    /// </summary>
    private static int GetKFactor(int currentELO)
    {
        return currentELO switch
        {
            < 1000 => 50,  // High volatility for new artifacts
            < 1200 => 40,  // Medium-high for learning artifacts
            < 1600 => 30,  // Standard volatility
            < 2000 => 20,  // Lower volatility for experienced artifacts
            < 2400 => 15,  // Low volatility for master artifacts
            _ => 10        // Minimal volatility for legendary artifacts
        };
    }
    
    /// <summary>
    /// Calculate opponent ELO for different enemy types
    /// </summary>
    public static int EstimateOpponentELO(Pawn opponent)
    {
        int baseELO = 1000;
        
        // Factor in opponent cultivation level
        var opponentComp = opponent.GetComp<CultivationComp>();
        if (opponentComp?.cultivationData != null)
        {
            var data = opponentComp.cultivationData;
            baseELO += ((int)data.currentRealm * 200) + (data.currentStage * 50);
        }
        
        // Factor in equipment quality
        var weapon = opponent.equipment?.Primary;
        if (weapon?.GetComp<CultivationArtifactComp>() != null)
        {
            var artifactComp = weapon.GetComp<CultivationArtifactComp>();
            baseELO += (artifactComp.currentELO - 1000) / 2; // Half of opponent artifact ELO
        }
        
        // Factor in pawn skills and traits
        baseELO += opponent.skills.GetSkill(SkillDefOf.Melee).Level * 20;
        
        return Mathf.Clamp(baseELO, 500, 4000);
    }
}
```

### **ELO Event Integration**

```csharp
public class ArtifactELOTracker
{
    /// <summary>
    /// Track combat events for ELO updates
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_ELOUpdate
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo)
        {
            if (dinfo?.Instigator is Pawn killer)
            {
                UpdateArtifactELO(killer, __instance, true);
            }
        }
    }
    
    [HarmonyPatch(typeof(Pawn), "TakeDamage")]
    public static class Pawn_TakeDamage_ELOUpdate  
    {
        public static void Postfix(Pawn __instance, DamageInfo dinfo)
        {
            if (dinfo.Instigator is Pawn attacker && __instance.Dead)
            {
                UpdateArtifactELO(attacker, __instance, true);
            }
            else if (dinfo.Instigator is Pawn defender && defender.Dead)
            {
                UpdateArtifactELO(__instance, defender, false);
            }
        }
    }
    
    private static void UpdateArtifactELO(Pawn winner, Pawn loser, bool isVictory)
    {
        // Update winner's artifacts
        foreach (var equipment in winner.equipment.AllEquipmentListForReading)
        {
            var artifactComp = equipment.GetComp<CultivationArtifactComp>();
            if (artifactComp != null)
            {
                int opponentELO = ELOCalculator.EstimateOpponentELO(loser);
                artifactComp.UpdateELO(isVictory, opponentELO);
            }
        }
    }
}
```

---

## 🎯 **Auto-Skills Mechanism**

### **Dual System Skill Detection**

```csharp
/// <summary>
/// Smart skill detection that works with both skill systems
/// </summary>
public class ArtifactSkillDetector
{
    public static SkillInfo DetectSkillType(string skillName)
    {
        // Try CultivationSkillDef first (direct execution system)
        var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillName);
        if (skillDef != null)
        {
            return new SkillInfo
            {
                Name = skillName,
                Type = SkillSystemType.CultivationSkill,
                Definition = skillDef,
                ExecutionMethod = SkillExecutionMethod.Direct
            };
        }
        
        // Try CultivationAbilityDef second (effect-based system)
        var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(skillName);
        if (abilityDef != null)
        {
            return new SkillInfo
            {
                Name = skillName,
                Type = SkillSystemType.CultivationAbility,
                Definition = abilityDef,
                ExecutionMethod = SkillExecutionMethod.EffectBased
            };
        }
        
        // Skill not found in either system
        Log.Warning($"[ARTIFACT] Skill '{skillName}' not found in either skill system");
        return null;
    }
    
    public static bool CanPawnUseSkill(Pawn pawn, SkillInfo skillInfo)
    {
        if (skillInfo == null) return false;
        
        var cultivationData = pawn.GetCultivationData();
        if (cultivationData == null) return false;
        
        switch (skillInfo.Type)
        {
            case SkillSystemType.CultivationSkill:
                var skillDef = skillInfo.Definition as CultivationSkillDef;
                return cultivationData.currentRealm >= skillDef.requiredRealm &&
                       cultivationData.currentStage >= skillDef.requiredStage &&
                       cultivationData.currentQi >= skillDef.qiCost;
                       
            case SkillSystemType.CultivationAbility:
                var abilityDef = skillInfo.Definition as CultivationAbilityDef;
                return cultivationData.currentRealm >= abilityDef.requiredRealm &&
                       cultivationData.currentStage >= abilityDef.requiredStage &&
                       cultivationData.currentQi >= abilityDef.qiCost;
                       
            default:
                return false;
        }
    }
}

public class SkillInfo
{
    public string Name;
    public SkillSystemType Type;
    public Def Definition;
    public SkillExecutionMethod ExecutionMethod;
}

public enum SkillSystemType
{
    CultivationSkill,
    CultivationAbility
}

public enum SkillExecutionMethod  
{
    Direct,      // SkillWorker.ExecuteSkillEffect()
    EffectBased  // Ability.TryCast() -> ApplyEffects()
}
```

### **Gizmo Creation for Artifacts**

```csharp
/// <summary>
/// Creates appropriate gizmos for artifact skills
/// </summary>
public static class ArtifactGizmoFactory
{
    public static IEnumerable<Gizmo> CreateArtifactGizmos(Pawn pawn, CultivationArtifactComp artifactComp)
    {
        if (artifactComp?.def?.autoSkills == null) yield break;
        
        foreach (string skillName in artifactComp.def.autoSkills)
        {
            var skillInfo = ArtifactSkillDetector.DetectSkillType(skillName);
            if (skillInfo == null) continue;
            
            var gizmo = CreateSkillGizmo(pawn, skillInfo, artifactComp);
            if (gizmo != null)
            {
                yield return gizmo;
            }
        }
    }
    
    private static Gizmo CreateSkillGizmo(Pawn pawn, SkillInfo skillInfo, CultivationArtifactComp artifactComp)
    {
        switch (skillInfo.Type)
        {
            case SkillSystemType.CultivationSkill:
                return CreateCultivationSkillGizmo(pawn, skillInfo.Definition as CultivationSkillDef, artifactComp);
                
            case SkillSystemType.CultivationAbility:
                return CreateCultivationAbilityGizmo(pawn, skillInfo.Definition as CultivationAbilityDef, artifactComp);
                
            default:
                return null;
        }
    }
    
    private static Command_Action CreateCultivationSkillGizmo(Pawn pawn, CultivationSkillDef skillDef, CultivationArtifactComp artifactComp)
    {
        return new Command_Action
        {
            defaultLabel = skillDef.label,
            defaultDesc = $"{skillDef.description}\n\n" +
                         $"Source: {artifactComp.parent.LabelShort} (ELO: {artifactComp.currentELO})\n" +
                         $"Qi Cost: {skillDef.qiCost}",
            icon = skillDef.uiIcon ?? BaseContent.BadTex,
            
            action = () => {
                // Find valid targets for skill
                var targeter = new Targeter_CultivationSkill(skillDef, pawn);
                Find.Targeter.BeginTargeting(targeter);
            },
            
            // Disable if requirements not met
            Disabled = !ArtifactSkillDetector.CanPawnUseSkill(pawn, 
                new SkillInfo { Type = SkillSystemType.CultivationSkill, Definition = skillDef }),
            disabledReason = GetDisabledReason(pawn, skillDef)
        };
    }
    
    private static Command_Action CreateCultivationAbilityGizmo(Pawn pawn, CultivationAbilityDef abilityDef, CultivationArtifactComp artifactComp)
    {
        var ability = new CultivationAbility(abilityDef, pawn.GetComp<CompAbilityUser>());
        
        return new Command_Action
        {
            defaultLabel = abilityDef.abilityLabel ?? abilityDef.label,
            defaultDesc = $"{abilityDef.abilityDescription ?? abilityDef.description}\n\n" +
                         $"Source: {artifactComp.parent.LabelShort} (ELO: {artifactComp.currentELO})\n" +
                         $"Qi Cost: {abilityDef.qiCost}",
            icon = abilityDef.uiIcon ?? BaseContent.BadTex,
            
            action = () => {
                if (abilityDef.targetType == AbilityTargetType.Self)
                {
                    ability.TryCast(pawn);
                }
                else
                {
                    // Smart targeting based on ability type
                    var targeter = new Targeter_CultivationAbility(abilityDef, ability);
                    Find.Targeter.BeginTargeting(targeter);
                }
            },
            
            Disabled = !ability.CanCast(),
            disabledReason = ability.GetDisabledReason()
        };
    }
}
```

---

## 🔧 **Integration with Core Systems**

### **Cultivation Component Integration**

```csharp
/// <summary>
/// Enhanced CultivationComp with artifact support
/// </summary>
public partial class CultivationComp : ThingComp
{
    #region Artifact Integration
    /// <summary>
    /// Get all skills available to this pawn (including from artifacts)
    /// </summary>
    public List<string> GetAllAvailableSkills()
    {
        var allSkills = new List<string>(knownSkills);
        
        // Add skills from equipped artifacts
        if (parent is Pawn pawn)
        {
            foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
            {
                var artifactComp = equipment.GetComp<CultivationArtifactComp>();
                if (artifactComp?.def?.autoSkills != null)
                {
                    allSkills.AddRange(artifactComp.def.autoSkills);
                }
                
                // Add ELO threshold unlocked skills
                if (artifactComp?.unlockedThresholdSkills != null)
                {
                    allSkills.AddRange(artifactComp.unlockedThresholdSkills);
                }
            }
        }
        
        return allSkills.Distinct().ToList();
    }
    
    /// <summary>
    /// Check if pawn has access to skill (direct or via artifact)
    /// </summary>
    public bool HasSkillAccess(string skillName)
    {
        // Direct knowledge
        if (knownSkills.Contains(skillName)) return true;
        
        // Artifact-granted access
        if (parent is Pawn pawn)
        {
            foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
            {
                var artifactComp = equipment.GetComp<CultivationArtifactComp>();
                if (artifactComp?.def?.autoSkills?.Contains(skillName) == true ||
                    artifactComp?.unlockedThresholdSkills?.Contains(skillName) == true)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Get the source of a skill (learned vs artifact)
    /// </summary>
    public SkillSource GetSkillSource(string skillName)
    {
        if (knownSkills.Contains(skillName))
            return SkillSource.Learned;
            
        if (parent is Pawn pawn)
        {
            foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
            {
                var artifactComp = equipment.GetComp<CultivationArtifactComp>();
                if (artifactComp?.def?.autoSkills?.Contains(skillName) == true)
                    return SkillSource.ArtifactGranted;
                if (artifactComp?.unlockedThresholdSkills?.Contains(skillName) == true)
                    return SkillSource.ArtifactUnlocked;
            }
        }
        
        return SkillSource.Unknown;
    }
    #endregion
}

public enum SkillSource
{
    Learned,          // Directly learned by pawn
    ArtifactGranted,  // Granted by equipped artifact (autoSkills)
    ArtifactUnlocked, // Unlocked by artifact ELO thresholds
    Unknown
}
```

### **CompAbilityUser Integration**

```csharp
/// <summary>
/// Enhanced CompAbilityUser with artifact ability support
/// </summary>
public partial class CompAbilityUser : ThingComp
{
    /// <summary>
    /// Get abilities available through artifacts
    /// </summary>
    public List<CultivationAbility> GetArtifactAbilities()
    {
        var artifactAbilities = new List<CultivationAbility>();
        
        if (parent is Pawn pawn)
        {
            foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
            {
                var artifactComp = equipment.GetComp<CultivationArtifactComp>();
                if (artifactComp?.def?.autoSkills == null) continue;
                
                foreach (string skillName in artifactComp.def.autoSkills)
                {
                    var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(skillName);
                    if (abilityDef != null)
                    {
                        var ability = new CultivationAbility(abilityDef, this);
                        ability.sourceArtifact = artifactComp; // Track source
                        artifactAbilities.Add(ability);
                    }
                }
            }
        }
        
        return artifactAbilities;
    }
    
    /// <summary>
    /// Check if ability is available (learned or through artifact)
    /// </summary>
    public bool HasAbilityAccess(CultivationAbilityDef abilityDef)
    {
        // Direct ability ownership
        if (abilities.Any(a => a.def == abilityDef)) return true;
        
        // Artifact-granted access
        return GetArtifactAbilities().Any(a => a.def == abilityDef);
    }
}
```

---

## 🎨 **Visual Integration**

### **Artifact UI Enhancements**

```csharp
/// <summary>
/// Enhanced info display for artifacts
/// </summary>
public static class ArtifactUIExtensions
{
    [HarmonyPatch(typeof(Thing), "GetInspectTabs")]
    public static class Thing_GetInspectTabs_ArtifactPatch
    {
        public static IEnumerable<InspectTabBase> Postfix(IEnumerable<InspectTabBase> __result, Thing __instance)
        {
            foreach (var tab in __result)
                yield return tab;
                
            // Add artifact inspect tab
            var artifactComp = __instance.GetComp<CultivationArtifactComp>();
            if (artifactComp != null)
            {
                yield return new InspectTab_Artifact();
            }
        }
    }
}

public class InspectTab_Artifact : InspectTabBase
{
    protected override string GetLabel(Thing thing) => "Cultivation";
    
    protected override void FillTab()
    {
        var artifactComp = SelThing.GetComp<CultivationArtifactComp>();
        if (artifactComp == null) return;
        
        var rect = new Rect(0f, 0f, size.x, size.y);
        Widgets.BeginGroup(rect);
        
        float curY = 0f;
        
        // ELO Information
        DrawELOSection(ref curY, artifactComp);
        
        // Granted Skills
        DrawSkillsSection(ref curY, artifactComp);
        
        // Combat Statistics
        DrawStatsSection(ref curY, artifactComp);
        
        Widgets.EndGroup();
    }
    
    private void DrawELOSection(ref float curY, CultivationArtifactComp comp)
    {
        var rect = new Rect(0f, curY, 300f, 100f);
        
        // ELO progress bar
        var progressRect = new Rect(rect.x, rect.y, rect.width, 20f);
        Widgets.FillableBar(progressRect, comp.ELOPercentage);
        
        // ELO text
        var textRect = new Rect(rect.x, rect.y + 25f, rect.width, 25f);
        Widgets.Label(textRect, $"ELO Rating: {comp.currentELO} ({comp.ELORank})");
        
        // Win/Loss record
        var recordRect = new Rect(rect.x, rect.y + 50f, rect.width, 25f);
        Widgets.Label(recordRect, $"Record: {comp.wins}W - {comp.losses}L - {comp.draws}D");
        
        curY += 110f;
    }
}
```

---

## 🛠️ **Common Implementation Patterns**

### **Pattern 1: Simple Artifact with Auto-Skills**

```xml
<!-- Minimal artifact definition -->
<TuTien.CultivationArtifactDef>
  <defName>BasicSword</defName>
  <autoSkills>
    <li>Ability_SwordStrike</li>
  </autoSkills>
</TuTien.CultivationArtifactDef>
```

### **Pattern 2: Progressive Artifact with Unlocks**

```xml
<!-- Advanced artifact with ELO progression -->
<TuTien.CultivationArtifactDef>
  <defName>GrowingSword</defName>
  <autoSkills>
    <li>Ability_SwordStrike</li>
  </autoSkills>
  
  <baseELO>1000</baseELO>
  <maxELO>3000</maxELO>
  <eloGrowthRate>1.2</eloGrowthRate>
  
  <eloThresholds>
    <li>
      <requiredELO>1500</requiredELO>
      <skillName>Ability_PowerStrike</skillName>
    </li>
    <li>
      <requiredELO>2000</requiredELO>
      <skillName>QiSwordMastery</skillName>
    </li>
  </eloThresholds>
</TuTien.CultivationArtifactDef>
```

### **Pattern 3: Elemental Artifact with Affinities**

```xml
<!-- Elemental-specific artifact -->
<TuTien.CultivationArtifactDef>
  <defName>LightningSpear</defName>
  <autoSkills>
    <li>Ability_LightningStrike</li>
    <li>QiLightningShield</li>
  </autoSkills>
  
  <associatedQiType>Lightning</associatedQiType>
  
  <eloStatModifiers>
    <li>
      <minELO>1200</minELO>
      <statDef>ElectricalResistance</statDef>
      <value>0.2</value>
    </li>
  </eloStatModifiers>
</TuTien.CultivationArtifactDef>
```

---

## 🚨 **Troubleshooting Guide**

### **Common Issues & Solutions**

#### **Issue 1: Artifact Skills Not Appearing**
**Symptoms**: Artifact equipped but no skills show in gizmos
**Causes**:
- autoSkills references non-existent skill names
- CultivationArtifactComp not attached to ThingDef
- UI patch not triggering

**Debug Steps**:
```csharp
// Check if component exists
var artifactComp = thing.GetComp<CultivationArtifactComp>();
Log.Message($"Artifact comp: {artifactComp != null}");

// Check if def exists  
var artifactDef = artifactComp?.def;
Log.Message($"Artifact def: {artifactDef != null}");

// Check autoSkills
Log.Message($"Auto skills: {string.Join(", ", artifactDef?.autoSkills ?? new List<string>())}");

// Check skill lookups
foreach (string skillName in artifactDef.autoSkills)
{
    var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillName);
    var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(skillName);
    Log.Message($"Skill '{skillName}': SkillDef={skillDef != null}, AbilityDef={abilityDef != null}");
}
```

**Solutions**:
1. Verify skill names in autoSkills match actual def names
2. Ensure CultivationArtifactCompProperties added to ThingDef
3. Check that artifact def name matches comp properties

#### **Issue 2: Skills Execute but No Damage**
**Symptoms**: Gizmo works, animation plays, but no damage dealt
**Causes**:
- AbilityEffectDef not processed correctly
- Missing effect type handler
- Target validation failing

**Debug Steps**:
```csharp
// In ApplyGenericEffect method
Log.Message($"[DEBUG] Effect type: {effect.effectType}");
Log.Message($"[DEBUG] Effect magnitude: {effect.magnitude}");
Log.Message($"[DEBUG] Target valid: {target.IsValid}");
Log.Message($"[DEBUG] Target is pawn: {target.Thing is Pawn}");

// In ApplyDamageEffect method
Log.Message($"[DEBUG] Damage amount: {effect.magnitude}");
Log.Message($"[DEBUG] Damage type: {damageType}");
Log.Message($"[DEBUG] Caster: {caster?.LabelShort}");
```

**Solutions**:
1. Ensure ApplyGenericEffect handles your effect type
2. Verify damage types are correctly mapped
3. Check target validation logic

#### **Issue 3: ELO Not Updating**
**Symptoms**: Combat happens but artifact ELO stays same
**Causes**:
- Combat event patches not triggering
- ELO update conditions not met
- Save/load issues

**Debug Steps**:
```csharp
// In combat event handlers
Log.Message($"[ELO DEBUG] Combat event: {killer?.LabelShort} vs {victim?.LabelShort}");
Log.Message($"[ELO DEBUG] Artifact count: {artifacts.Count}");

// In UpdateELO method
Log.Message($"[ELO DEBUG] ELO change: {oldELO} -> {newELO} (Victory: {victory})");
```

---

## 📋 **Implementation Checklist**

### **New Artifact Implementation**

- [ ] **Define CultivationArtifactDef**
  - [ ] Set autoSkills array with valid skill names
  - [ ] Configure ELO parameters (base, max, growth rate)
  - [ ] Define elemental affinity if applicable
  - [ ] Set up ELO thresholds for progressive unlocks

- [ ] **Create ThingDef**
  - [ ] Inherit from appropriate base (weapon/apparel/etc)
  - [ ] Add CultivationArtifactCompProperties to comps
  - [ ] Set artifactDef property to match CultivationArtifactDef name
  - [ ] Configure basic item properties (stats, graphics, etc)

- [ ] **Test Integration**
  - [ ] Verify artifact spawns with component
  - [ ] Check skills appear when equipped
  - [ ] Test skill execution and damage
  - [ ] Validate ELO tracking in combat
  - [ ] Confirm save/load persistence

- [ ] **UI/UX Polish**
  - [ ] Add appropriate skill icons
  - [ ] Write descriptive skill descriptions
  - [ ] Test gizmo tooltips and disabled states
  - [ ] Verify visual effects and feedback

### **New Skill System Integration**

- [ ] **Choose Skill System Type**
  - [ ] CultivationSkillDef for complex, custom logic
  - [ ] CultivationAbilityDef for standard RPG effects
  - [ ] Consider hybrid approach for maximum flexibility

- [ ] **Implement Skill Logic**
  - [ ] Create SkillWorker (if CultivationSkillDef)
  - [ ] Define effects array (if CultivationAbilityDef)
  - [ ] Add to artifact autoSkills arrays
  - [ ] Test execution through artifacts

- [ ] **Extend Effect System** (if needed)
  - [ ] Add new effect type to ApplyGenericEffect switch
  - [ ] Implement effect processing method
  - [ ] Add appropriate visual feedback
  - [ ] Document new effect type for other developers

---

**Implementation Version**: 2.0  
**Last Updated**: September 2025  
**Tested With**: RimWorld 1.6
