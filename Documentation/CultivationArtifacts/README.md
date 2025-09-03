# 🔮 Tu Tiên Cultivation Mod - Comprehensive Documentation

## 📖 **Table of Contents**

1. [🏗️ System Architecture Overview](#system-architecture-overview)
2. [⚔️ Cultivation Artifacts System](#cultivation-artifacts-system)
3. [🎯 Skills & Abilities Framework](#skills--abilities-framework)
4. [🔧 Core Components](#core-components)
5. [🎨 UI Integration](#ui-integration)
6. [📊 Visual System Diagrams](#visual-system-diagrams)
7. [🛠️ Implementation Guides](#implementation-guides)
8. [🔌 Extension & Integration](#extension--integration)
9. [⚠️ Important Notes & Gotchas](#important-notes--gotchas)

---

## 🏗️ **System Architecture Overview**

Tu Tiên mod implements a comprehensive cultivation system for RimWorld with multiple interconnected subsystems:

```
┌─────────────────────────────────────────────────────────────┐
│                    TU TIÊN MOD ARCHITECTURE                 │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │   CORE SYSTEM   │    │  ARTIFACTS SYS  │                │
│  │                 │    │                 │                │
│  │ • CultivationComp│    │ • Artifact Comp │                │
│  │ • CultivationData│◄──►│ • Artifact Defs │                │
│  │ • Qi Management │    │ • Auto Skills   │                │
│  │ • Realm/Stages  │    │ • ELO System    │                │
│  └─────────────────┘    └─────────────────┘                │
│           │                       │                        │
│           ▼                       ▼                        │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │   SKILLS SYS    │    │   ABILITIES SYS │                │
│  │                 │    │                 │                │
│  │ • SkillWorkers  │    │ • AbilityDefs   │                │
│  │ • Skill Defs    │◄──►│ • Effect System │                │
│  │ • Execution     │    │ • CompAbilityUser│               │
│  │ • Passive/Active│    │ • Targeting     │                │
│  └─────────────────┘    └─────────────────┘                │
│           │                       │                        │
│           └───────────┬───────────┘                        │
│                       ▼                                    │
│           ┌─────────────────────────┐                      │
│           │      UI SYSTEM          │                      │
│           │                         │                      │
│           │ • Unified Gizmo Patches │                      │
│           │ • AbilityUIPatches      │                      │
│           │ • Cooldown Management   │                      │
│           │ • Visual Effects        │                      │
│           └─────────────────────────┘                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### **Key Design Principles:**

1. **Dual Skill System**: Supports both `CultivationSkillDef` and `CultivationAbilityDef`
2. **Unified UI**: Single gizmo system handles all skill types
3. **Artifact Integration**: Artifacts can grant skills from either system
4. **Modular Effects**: Effect system supports extensible damage/heal/buff patterns
5. **RimWorld Native**: Leverages existing RimWorld systems (Hediffs, DamageInfo, etc.)

---

## ⚔️ **Cultivation Artifacts System**

### **System Overview**

Artifacts are special items that grant cultivation skills when equipped. They use an ELO-based generation system and auto-skill granting mechanism.

```
Artifact Equipment Flow:
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Player    │    │  Artifact   │    │   Skills    │
│  Equips     │───►│ Component   │───►│  Auto-Grant │
│  Artifact   │    │ Activates   │    │   System    │
└─────────────┘    └─────────────┘    └─────────────┘
       │                   │                   │
       ▼                   ▼                   ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│    UI       │    │   ELO       │    │  Gizmo      │
│  Updates    │◄───│  Tracking   │◄───│  Display    │
│  Gizmos     │    │   System    │    │  System     │
└─────────────┘    └─────────────┘    └─────────────┘
```

### **Core Components**

#### **1. CultivationArtifactDef**
```csharp
public class CultivationArtifactDef : Def
{
    public List<string> autoSkills = new List<string>();  // Skills granted when equipped
    public int baseELO = 1000;                           // ELO rating system
    public float eloGrowthRate = 1.0f;                   // Growth multiplier
    public int maxELO = 3000;                            // ELO cap
    public QiType associatedQiType = QiType.Neutral;     // Element affinity
}
```

#### **2. CultivationArtifactComp**
```csharp
public class CultivationArtifactComp : ThingComp
{
    public int currentELO;                    // Current rating
    public int wins, losses;                  // Combat tracking
    public List<string> grantedSkills;        // Currently active skills
    
    // Auto-grant skills when equipped
    public override void Notify_Equipped(Pawn pawn);
    public override void Notify_Unequipped(Pawn pawn);
}
```

### **Artifact Skill Integration**

The artifact system integrates with the dual skill framework:

```
Artifact autoSkills Array:
┌─────────────────────────────────────────────────────────────┐
│  autoSkills: ["QiPunch", "Ability_SwordStrike", "QiShield"] │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                 SKILL LOOKUP SYSTEM                         │
├─────────────────┬───────────────────────────────────────────┤
│ CultivationSkill│ DefDatabase<CultivationSkillDef>         │
│ System Lookup   │ .GetNamedSilentFail("QiPunch")           │
│                 │ → Returns: QiPunchSkillWorker            │
├─────────────────┼───────────────────────────────────────────┤
│ Cultivation     │ DefDatabase<CultivationAbilityDef>       │
│ Ability Lookup  │ .GetNamedSilentFail("Ability_SwordStrike")│
│                 │ → Returns: Ability with Damage Effects   │
└─────────────────┴───────────────────────────────────────────┘
```

---

## 🎯 **Skills & Abilities Framework**

### **Dual System Architecture**

Tu Tiên supports two parallel skill systems for maximum flexibility:

```
┌─────────────────────────────────────────────────────────────┐
│                   DUAL SKILL SYSTEM                        │
├─────────────────────────────┬───────────────────────────────┤
│     CULTIVATION SKILLS      │      CULTIVATION ABILITIES    │
│                             │                               │
│ ┌─────────────────────────┐ │ ┌─────────────────────────────┐ │
│ │   CultivationSkillDef   │ │ │   CultivationAbilityDef     │ │
│ │                         │ │ │                             │ │
│ │ • Direct execution      │ │ │ • Effect-based system       │ │
│ │ • SkillWorker classes   │ │ │ • Modular effects           │ │
│ │ • Immediate effects     │ │ │ • Targeting system          │ │
│ │ • Simple implementation │ │ │ • Complex interactions      │ │
│ └─────────────────────────┘ │ └─────────────────────────────┘ │
│                             │                               │
│ Examples:                   │ Examples:                     │
│ • QiPunch                   │ • Ability_SwordStrike         │
│ • QiShield                  │ • Ability_QiHealing           │
│ • QiBarrier                 │ • Ability_CorpseRevival       │
└─────────────────────────────┴───────────────────────────────┘
```

### **CultivationSkillDef System**

**Direct Execution Pattern:**
```csharp
public class QiPunchSkillWorker : CultivationSkillWorker
{
    public override void ExecuteSkillEffect(Pawn pawn, LocalTargetInfo target)
    {
        // Direct damage calculation and application
        var damage = CalculateDamage(pawn);
        var dinfo = new DamageInfo(DamageDefOf.Blunt, damage, 0f, -1f, pawn);
        target.Thing.TakeDamage(dinfo);
    }
}
```

### **CultivationAbilityDef System**

**Effect-Based Pattern:**
```xml
<TuTien.CultivationAbilityDef>
  <defName>Ability_SwordStrike</defName>
  <effects>
    <li Class="TuTien.AbilityEffectDef">
      <effectType>Damage</effectType>
      <magnitude>20</magnitude>
      <damageType>Cut</damageType>
    </li>
  </effects>
</TuTien.CultivationAbilityDef>
```

**Effect Application Flow:**
```
AbilityEffectDef → ApplyGenericEffect() → ApplyDamageEffect() → TakeDamage()
```

---

## 🔧 **Core Components**

### **1. CultivationComp & CultivationData**

The heart of the cultivation system:

```csharp
public class CultivationComp : ThingComp
{
    public CultivationData cultivationData;     // Core progression data
    public List<string> knownSkills;            // Learned skills
    public Dictionary<string, int> cooldowns;   // Skill cooldowns
}

public class CultivationData : IExposable
{
    public CultivationRealm currentRealm;       // Major cultivation level
    public int currentStage;                    // Minor stage within realm
    public float currentQi;                     // Current Qi energy
    public float maxQi;                         // Maximum Qi capacity
    public float cultivationPoints;             // Progression points
}
```

### **2. Qi Management System**

```
Qi Flow Diagram:
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│ Qi Sources  │───►│ Current Qi  │───►│ Qi Spending │
├─────────────┤    ├─────────────┤    ├─────────────┤
│ • Natural   │    │ • Storage   │    │ • Skills    │
│   Regen     │    │ • Capacity  │    │ • Abilities │
│ • Meditation│    │ • Overflow  │    │ • Artifacts │
│ • Pills     │    │   Handling  │    │ • Special   │
│ • Combat    │    │             │    │   Effects   │
└─────────────┘    └─────────────┘    └─────────────┘
```

### **3. Cultivation Progression**

```
Realm Progression System:
┌─────────────────────────────────────────────────────────────┐
│ Foundation Building → Core Formation → Nascent Soul → ...   │
├─────────────────────────────────────────────────────────────┤
│ Stage 1 → Stage 2 → Stage 3 → ... → Stage 9 → Breakthrough │
├─────────────────────────────────────────────────────────────┤
│ Cultivation Points → Stage Advancement → Realm Breakthrough │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎨 **UI Integration**

### **Unified Gizmo System**

The `AbilityUIPatches.cs` provides a single entry point for all cultivation-related UI:

```csharp
[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public static class Pawn_GetGizmos_UnifiedPatch
{
    // Handles:
    // 1. Cultivation Skills (CultivationSkillDef)
    // 2. Cultivation Abilities (CultivationAbilityDef)  
    // 3. Artifact Skills (from autoSkills array)
}
```

**Gizmo Creation Flow:**
```
Pawn.GetGizmos() → Unified Patch → Check All Sources → Create Gizmos
                                       │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                 ▼
            Known Skills     Known Abilities    Artifact Skills
                 │                 │                 │
                 ▼                 ▼                 ▼
           Create Skill      Create Ability    Dual Lookup
              Gizmo           Gizmo             System
```

### **Artifact Gizmo Integration**

**Smart Skill Detection:**
```csharp
// In artifact gizmo section
foreach (string skillName in artifactDef.autoSkills)
{
    // Try CultivationSkillDef first
    var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillName);
    if (skillDef != null)
    {
        yield return CreateSkillGizmo(skillDef, pawn);
        continue;
    }
    
    // Try CultivationAbilityDef second
    var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(skillName);
    if (abilityDef != null)
    {
        yield return CreateAbilityGizmo(abilityDef, pawn);
    }
}
```

---

## 📊 **Visual System Diagrams**

### **Complete System Flow Diagram**

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           TU TIÊN COMPLETE SYSTEM FLOW                         │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────┐
│   PLAYER    │
│   ACTION    │
└──────┬──────┘
       │
       ▼
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│  Equipment  │ OR   │ Direct Skill│ OR   │ Direct Ability
│  Artifact   │      │ Usage       │      │ Usage       │
└──────┬──────┘      └──────┬──────┘      └──────┬──────┘
       │                    │                    │
       ▼                    ▼                    ▼
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│ Artifact    │      │ Skill       │      │ Ability     │
│ Component   │      │ Gizmo       │      │ Gizmo       │
│ Activation  │      │ Click       │      │ Click       │
└──────┬──────┘      └──────┬──────┘      └──────┬──────┘
       │                    │                    │
       ▼                    ▼                    ▼
┌─────────────────────────────┬─────────────────────────────┐
│     AUTO-SKILL LOOKUP       │      DIRECT EXECUTION       │
│                             │                             │
│ ┌─────────────────────────┐ │ ┌─────────────────────────┐ │
│ │  Try CultivationSkill   │ │ │    SkillWorker.Execute  │ │
│ │  DefDatabase Lookup     │ │ │         System          │ │
│ └───────────┬─────────────┘ │ └─────────────────────────┘ │
│             │ Found ✓       │                             │
│             ▼               │                             │
│ ┌─────────────────────────┐ │                             │
│ │   Execute SkillWorker   │ │                             │
│ │   (Direct Damage)       │ │                             │
│ └─────────────────────────┘ │                             │
│                             │                             │
│ ┌─────────────────────────┐ │                             │
│ │ Try CultivationAbility  │ │                             │
│ │  DefDatabase Lookup     │ │                             │
│ └───────────┬─────────────┘ │                             │
│             │ Found ✓       │                             │
│             ▼               │                             │
│ ┌─────────────────────────┐ │ ┌─────────────────────────┐ │
│ │  Execute Ability.Cast   │ │ │   CompAbilityUser.Cast  │ │
│ │   (Effect System)       │ │ │      System             │ │
│ └─────────────────────────┘ │ └─────────────────────────┘ │
└─────────────────────────────┴─────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    EFFECT APPLICATION                       │
├─────────────────┬─────────────────┬─────────────────────────┤
│  Skill Effects  │ Ability Effects │   Artifact Effects      │
│                 │                 │                         │
│ • Direct damage │ • AbilityEffect │ • ELO tracking          │
│ • Immediate     │   _Damage       │ • Stat bonuses          │
│ • Visual FX     │ • AbilityEffect │ • Passive effects       │
│ • Simple logic  │   _Heal         │ • Auto-skill granting   │
│                 │ • Generic       │                         │
│                 │   AbilityEffect │                         │
│                 │   Def support   │                         │
└─────────────────┴─────────────────┴─────────────────────────┘
```

### **Artifact Generation System**

```
ELO-Based Artifact Generation:
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│ Base Stats  │───►│ ELO Rating  │───►│ Skill Pool  │
├─────────────┤    ├─────────────┤    ├─────────────┤
│ • Quality   │    │ • Current   │    │ • Available │
│ • Material  │    │ • Growth    │    │   Skills    │
│ • Rarity    │    │ • Cap       │    │ • Weights   │
└─────────────┘    └─────────────┘    └─────────────┘
       │                   │                   │
       └─────────────────┬─│─┬─────────────────┘
                         ▼ │ ▼
                ┌─────────────────┐
                │ autoSkills List │
                │ Generation      │
                └─────────────────┘
```

---

## 🛠️ **Implementation Guides**

### **Adding New Artifacts**

#### **Step 1: Create Artifact Definition**
```xml
<!-- Defs/CultivationArtifactDefs_YourCategory.xml -->
<TuTien.CultivationArtifactDef>
  <defName>LightningBlade</defName>
  <autoSkills>
    <li>Ability_LightningStrike</li>
    <li>QiShield</li>
  </autoSkills>
  <baseELO>1200</baseELO>
  <eloGrowthRate>1.2</eloGrowthRate>
  <associatedQiType>Lightning</associatedQiType>
</TuTien.CultivationArtifactDef>
```

#### **Step 2: Create Thing Definition**
```xml
<!-- Defs/ThingDefs_Artifacts.xml -->
<ThingDef ParentName="BaseMeleeWeapon">
  <defName>LightningBlade</defName>
  <label>lightning blade</label>
  <comps>
    <li Class="TuTien.CultivationArtifactCompProperties">
      <artifactDef>LightningBlade</artifactDef>
    </li>
  </comps>
</ThingDef>
```

### **Adding New Skills**

#### **Method A: CultivationSkillDef (Direct)**
```xml
<!-- 1. Define skill -->
<TuTien.CultivationSkillDef>
  <defName>QiLightning</defName>
  <label>Qi Lightning</label>
  <skillWorkerClass>TuTien.QiLightningSkillWorker</skillWorkerClass>
</TuTien.CultivationSkillDef>
```

```csharp
// 2. Implement worker
public class QiLightningSkillWorker : CultivationSkillWorker
{
    public override void ExecuteSkillEffect(Pawn pawn, LocalTargetInfo target)
    {
        // Your damage logic here
    }
}
```

#### **Method B: CultivationAbilityDef (Effect-Based)**
```xml
<TuTien.CultivationAbilityDef>
  <defName>Ability_LightningStrike</defName>
  <effects>
    <li Class="TuTien.AbilityEffectDef">
      <effectType>Damage</effectType>
      <magnitude>30</magnitude>
      <damageType>Burn</damageType>
    </li>
  </effects>
</TuTien.CultivationAbilityDef>
```

### **Adding Custom Effect Types**

#### **Method A: Concrete Effect Class**
```csharp
// Create AbilityEffect_YourEffect.cs
public class AbilityEffect_Lightning
{
    public float damage = 50f;
    public float radius = 3f;
    
    public void Apply(CultivationAbilityDef abilityDef, Pawn caster, LocalTargetInfo target)
    {
        // Your effect implementation
    }
}
```

#### **Method B: Extend Generic AbilityEffectDef**
Modify `ApplyGenericEffect()` in `CultivationAbility.cs`:
```csharp
case "lightning":
    ApplyLightningEffect(effect, caster, target);
    break;
```

---

## 🔌 **Extension & Integration**

### **Integration Points**

#### **1. Harmony Patches**
```csharp
// AbilityUIPatches.cs - Main integration point
[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public static class Pawn_GetGizmos_UnifiedPatch
{
    // Extend this method to add new skill sources
}
```

#### **2. Effect System Extensions**
```csharp
// CultivationAbility.cs - Effect handler
private void ApplyGenericEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target)
{
    // Add new effect types here
}
```

#### **3. DefOf Integration**
```csharp
// TuTienDefOf.cs - Central def references
[DefOf]
public static class TuTienDefOf
{
    // Add new def references here
}
```

### **Mod Compatibility**

**Combat Extended Integration:**
```csharp
// Check for Combat Extended
if (ModsConfig.IsActive("CETeam.CombatExtended"))
{
    // Use CE damage system
}
else
{
    // Use vanilla damage system
}
```

**Other Cultivation Mods:**
```csharp
// Namespace isolation prevents conflicts
namespace TuTien
{
    // All mod classes here
}
```

---

## ⚠️ **Important Notes & Gotchas**

### **Critical Implementation Details**

#### **1. Dual Skill System Handling**
```csharp
// ALWAYS check both databases for artifacts
var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillName);
var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(skillName);
```

#### **2. Effect Type Matching**
```csharp
// AbilityEffectDef must match concrete classes
// XML: Class="TuTien.AbilityEffectDef" 
// Code: effect is AbilityEffectDef genericEffect
```

#### **3. Targeting Validation**
```csharp
// Touch abilities need valid targets
if (abilityDef.targetType == AbilityTargetType.Touch && target.IsValid)
{
    ability.TryCast(target);
}
```

#### **4. Qi Cost Management**
```csharp
// Always check Qi before casting
if (cultivationData.currentQi >= abilityDef.qiCost)
{
    // Execute ability
    cultivationData.currentQi -= abilityDef.qiCost;
}
```

### **Common Pitfalls**

1. **Missing DefDatabase Lookups**: Always use `GetNamedSilentFail()` to avoid exceptions
2. **Wrong Effect Classes**: Ensure XML effect classes match C# implementations
3. **Targeting Issues**: Validate targets before ability execution
4. **Cooldown Overlaps**: Manage cooldowns between skill and ability systems
5. **Qi Overflow**: Check Qi costs to prevent negative values

### **Performance Considerations**

1. **Cache DefDatabase Lookups**: Store frequently accessed defs
2. **Limit Effect Calculations**: Use efficient damage calculations
3. **Batch Visual Effects**: Avoid spam of particle effects
4. **Smart Gizmo Updates**: Only refresh when needed

### **Debugging Tips**

1. **Enable Debug Logging**:
```csharp
Log.Message($"[DEBUG] Artifact skill: {skillName}");
```

2. **Check Effect Types**:
```csharp
Log.Message($"[DEBUG] Effect type: {effect.GetType()}");
```

3. **Validate Targets**:
```csharp
Log.Message($"[DEBUG] Target valid: {target.IsValid}");
```

---

## 🎯 **Best Practices**

### **Code Organization**

1. **Namespace Consistency**: All classes in `TuTien` namespace
2. **Clear Naming**: `CultivationSkillDef` vs `CultivationAbilityDef`
3. **Separation of Concerns**: Skills for simple effects, Abilities for complex
4. **Documentation**: Comment all public APIs

### **XML Definition Patterns**

1. **Consistent Naming**: Use clear, descriptive defNames
2. **Proper Classes**: Match XML Class attributes to C# types
3. **Validation**: Include required fields and sensible defaults
4. **Categories**: Group related definitions together

### **Effect Design**

1. **Modularity**: Design effects to be reusable
2. **Flexibility**: Support multiple damage types and targets
3. **Feedback**: Provide visual and message feedback
4. **Balance**: Consider game balance in effect magnitudes

---

## 🔄 **System Workflow Summary**

```
COMPLETE CULTIVATION WORKFLOW:

1. INITIALIZATION
   ├─ Pawn gets CultivationComp
   ├─ CompAbilityUser added automatically
   └─ UI patches register

2. SKILL ACQUISITION
   ├─ Direct learning (CultivationSkillDef)
   ├─ Ability learning (CultivationAbilityDef)
   └─ Artifact equipment (autoSkills)

3. UI DISPLAY
   ├─ Unified gizmo patch scans all sources
   ├─ Creates appropriate gizmos per skill type
   └─ Manages cooldowns and availability

4. EXECUTION
   ├─ Skills: Direct SkillWorker.ExecuteSkillEffect()
   ├─ Abilities: Ability.TryCast() → ApplyEffects()
   └─ Artifacts: Dual lookup → appropriate execution

5. EFFECT APPLICATION
   ├─ Damage: DamageInfo → TakeDamage()
   ├─ Healing: Hediff modification
   ├─ Buffs/Debuffs: Hediff application
   └─ Visual feedback and messages
```

---

## 📚 **Additional Resources**

- `ARCHITECTURE_DOCUMENTATION.md` - Deep technical details
- `IMPLEMENTATION_GUIDE.md` - Step-by-step examples  
- `EXTENSIBILITY_ANALYSIS.md` - Expansion possibilities
- `REFACTOR_ROADMAP_DETAILED.md` - Future improvements

---

**Version**: 2.0 (Post-Artifact Integration)  
**Last Updated**: September 2025  
**Compatibility**: RimWorld 1.6+
