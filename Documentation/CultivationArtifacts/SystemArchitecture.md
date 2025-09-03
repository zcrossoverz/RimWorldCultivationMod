# 🏗️ Tu Tiên System Architecture Deep Dive

## 📋 **Table of Contents**

1. [Core Architecture Components](#core-architecture-components)
2. [System Initialization Flow](#system-initialization-flow)
3. [Artifact System Deep Dive](#artifact-system-deep-dive)
4. [Dual Skill System Implementation](#dual-skill-system-implementation)
5. [UI Integration Architecture](#ui-integration-architecture)
6. [Effect System Design](#effect-system-design)
7. [Data Flow Diagrams](#data-flow-diagrams)
8. [Integration Points](#integration-points)

---

## 🔧 **Core Architecture Components**

### **Component Hierarchy**

```
RimWorld Core Systems
│
├── ThingComp System
│   ├── CultivationComp (Core cultivation logic)
│   ├── CultivationArtifactComp (Artifact-specific logic)
│   └── CompAbilityUser (Ability management)
│
├── Def System
│   ├── CultivationSkillDef (Direct skill definitions)
│   ├── CultivationAbilityDef (Effect-based ability definitions)
│   ├── CultivationArtifactDef (Artifact definitions)
│   └── AbilityEffectDef (Generic effect definitions)
│
├── Worker System
│   ├── CultivationSkillWorker (Abstract base for skill execution)
│   ├── QiPunchSkillWorker (Example concrete implementation)
│   ├── QiShieldSkillWorker (Passive skill example)
│   └── [Custom skill workers...]
│
└── UI System
    ├── AbilityUIPatches (Unified gizmo management)
    ├── Command_CastAbility (Ability gizmo commands)
    └── Command_CastSkill (Skill gizmo commands)
```

### **Core Data Structures**

#### **CultivationData - The Heart of the System**
```csharp
[System.Serializable]
public class CultivationData : IExposable
{
    // Progression System
    public CultivationRealm currentRealm = CultivationRealm.FoundationBuilding;
    public int currentStage = 1;
    public float cultivationPoints = 0f;
    
    // Energy Management
    public float currentQi = 100f;
    public float maxQi = 100f;
    public float qiRegenRate = 1f;
    
    // Talent & Affinity System
    public TalentLevel talent = TalentLevel.Common;
    public QiType primaryAffinity = QiType.Neutral;
    public QiType secondaryAffinity = QiType.None;
    
    // Advanced Systems
    public CultivationAffinities affinities;
    public Dictionary<string, float> elementalResistances;
    public List<string> knownTechniques;
    
    // Progression tracking
    public float GetProgressionMultiplier() => (float)talent * 0.25f + 1f;
    public bool CanAdvanceStage() => cultivationPoints >= GetRequiredPoints();
    public int GetRequiredPoints() => currentStage * 100 * (int)currentRealm;
}
```

#### **CultivationComp - Component Integration**
```csharp
public class CultivationComp : ThingComp
{
    public CultivationData cultivationData;
    public List<string> knownSkills = new List<string>();
    public Dictionary<string, int> skillCooldowns = new Dictionary<string, int>();
    
    // Component lifecycle
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        // Initialize cultivation data if needed
        // Setup default values
        // Register with game systems
    }
    
    public override void CompTick()
    {
        // Handle Qi regeneration
        // Process cooldowns
        // Check for automatic progression
        // Update buffs/debuffs
    }
    
    // Skill management
    public void LearnSkill(string skillDefName) { /* Implementation */ }
    public bool HasSkill(string skillDefName) { /* Implementation */ }
    public bool CanUseSkill(string skillDefName) { /* Implementation */ }
}
```

---

## 🔄 **System Initialization Flow**

### **Game Startup Sequence**

```
Game Launch
│
├─ Mod Loading Phase
│  ├─ DefDatabase population
│  │  ├─ CultivationSkillDef loading
│  │  ├─ CultivationAbilityDef loading
│  │  └─ CultivationArtifactDef loading
│  │
│  └─ Harmony Patches Application
│     ├─ ThingDef_PostLoad_Patch (Add cultivation comps)
│     └─ Pawn_GetGizmos_UnifiedPatch (UI integration)
│
├─ World/Map Generation
│  ├─ Pawn generation with CultivationComp
│  ├─ Artifact generation with CultivationArtifactComp
│  └─ Initial skill/ability assignments
│
└─ Runtime Systems
   ├─ UI gizmo updates
   ├─ Component ticking
   └─ Event handling
```

### **Pawn Initialization Details**

```csharp
// In ThingDef_PostLoad_Patch
public static void Postfix(ThingDef __instance)
{
    if (__instance.category == ThingCategory.Pawn && 
        __instance.race?.Humanlike == true)
    {
        // Add CultivationComp to all humanlike pawns
        if (!__instance.HasComp<CultivationComp>())
        {
            __instance.comps.Add(new CultivationCompProperties());
        }
        
        // Add CompAbilityUser for ability system
        if (!__instance.HasComp<CompAbilityUser>())
        {
            __instance.comps.Add(new CompAbilityUserProperties());
        }
    }
}
```

---

## ⚔️ **Artifact System Deep Dive**

### **Artifact Component Lifecycle**

```
Artifact Creation
│
├─ Thing spawning
│  ├─ CultivationArtifactComp attachment
│  ├─ ELO initialization from def
│  └─ Skill list preparation
│
├─ Equipment Events
│  ├─ Notify_Equipped()
│  │  ├─ Grant autoSkills to pawn
│  │  ├─ Apply stat modifiers
│  │  └─ Update UI gizmos
│  │
│  └─ Notify_Unequipped()
│     ├─ Remove granted skills
│     ├─ Clear stat modifiers
│     └─ Refresh UI
│
└─ Runtime Updates
   ├─ ELO tracking from combat
   ├─ Skill availability updates
   └─ Visual effect management
```

### **ELO System Implementation**

```csharp
public class CultivationArtifactComp : ThingComp
{
    public int currentELO;
    public int wins, losses, draws;
    
    public void UpdateELO(bool victory, int opponentELO)
    {
        int K = 32; // K-factor for ELO calculation
        float expectedScore = 1.0f / (1.0f + Mathf.Pow(10, (opponentELO - currentELO) / 400.0f));
        float actualScore = victory ? 1.0f : 0.0f;
        
        currentELO += Mathf.RoundToInt(K * (actualScore - expectedScore));
        currentELO = Mathf.Clamp(currentELO, def.minELO, def.maxELO);
        
        if (victory) wins++; else losses++;
        
        // Unlock new skills based on ELO thresholds
        CheckSkillUnlocks();
    }
    
    private void CheckSkillUnlocks()
    {
        var artifactDef = def as CultivationArtifactDef;
        foreach (var skillThreshold in artifactDef.eloThresholds)
        {
            if (currentELO >= skillThreshold.requiredELO && 
                !grantedSkills.Contains(skillThreshold.skillName))
            {
                GrantSkill(skillThreshold.skillName);
            }
        }
    }
}
```

### **Auto-Skill Granting System**

```
Equipment Event Flow:
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Equip     │───►│ Comp.Notify │───►│ Grant Skills│
│  Artifact   │    │ _Equipped() │    │   Process   │
└─────────────┘    └─────────────┘    └─────────────┘
                          │                   │
                          ▼                   ▼
                   ┌─────────────┐    ┌─────────────┐
                   │ Scan autoS- │    │ Add to pawn │
                   │ kills Array │    │ knownSkills │
                   └─────────────┘    └─────────────┘
                          │                   │
                          ▼                   ▼
                   ┌─────────────────────────────┐
                   │    Update UI Gizmos         │
                   │  (Trigger GetGizmos patch)  │
                   └─────────────────────────────┘
```

---

## 🎯 **Dual Skill System Implementation**

### **System Architecture Comparison**

```
┌─────────────────────────────────────────────────────────────────────┐
│                    SKILL SYSTEM COMPARISON                          │
├─────────────────────┬───────────────────────┬───────────────────────┤
│     ASPECT          │ CultivationSkillDef   │ CultivationAbilityDef │
├─────────────────────┼───────────────────────┼───────────────────────┤
│ Execution Model     │ Direct method calls   │ Effect-based system   │
│ Implementation      │ C# SkillWorker class  │ XML effect definitions│
│ Flexibility         │ High (full C# control)│ Medium (predefined)   │
│ Extensibility       │ Requires C# knowledge │ XML-only additions    │
│ Performance         │ Optimal               │ Good                  │
│ Complexity          │ High                  │ Low                   │
│ Use Cases           │ Complex mechanics     │ Standard RPG effects  │
│ Examples            │ QiPunch, QiShield     │ SwordStrike, Healing  │
└─────────────────────┴───────────────────────┴───────────────────────┘
```

### **CultivationSkillDef Implementation Pattern**

```csharp
// 1. Define the skill
public class QiPunchSkillDef : CultivationSkillDef
{
    public float baseDamage = 10f;
    public float qiCostMultiplier = 1f;
    public DamageDef damageType = DamageDefOf.Blunt;
}

// 2. Implement the worker
public class QiPunchSkillWorker : CultivationSkillWorker
{
    public override bool CanExecute(Pawn pawn, LocalTargetInfo target)
    {
        // Validation logic
        return base.CanExecute(pawn, target) && 
               pawn.GetCultivationData()?.currentQi >= GetQiCost(pawn);
    }
    
    public override void ExecuteSkillEffect(Pawn pawn, LocalTargetInfo target)
    {
        var cultivationData = pawn.GetCultivationData();
        var skillDef = def as QiPunchSkillDef;
        
        // Calculate damage based on cultivation level
        float damage = skillDef.baseDamage * GetPowerMultiplier(pawn);
        
        // Create and apply damage
        var dinfo = new DamageInfo(skillDef.damageType, damage, 0f, -1f, pawn);
        target.Thing.TakeDamage(dinfo);
        
        // Consume Qi
        cultivationData.currentQi -= GetQiCost(pawn);
        
        // Visual effects
        CreateVisualEffects(pawn, target);
    }
    
    private float GetPowerMultiplier(Pawn pawn)
    {
        var data = pawn.GetCultivationData();
        return 1f + (((int)data.currentRealm * 10 + data.currentStage) * 0.1f);
    }
}
```

### **CultivationAbilityDef Implementation Pattern**

```xml
<!-- XML Definition -->
<TuTien.CultivationAbilityDef>
  <defName>Ability_SwordStrike</defName>
  <label>Sword Strike</label>
  <description>A basic sword technique that deals extra damage.</description>
  
  <!-- Targeting and requirements -->
  <targetType>Touch</targetType>
  <range>1.5</range>
  <qiCost>5</qiCost>
  <cooldownTicks>180</cooldownTicks>
  
  <!-- Effect definitions -->
  <effects>
    <li Class="TuTien.AbilityEffectDef">
      <effectType>Damage</effectType>
      <magnitude>20</magnitude>
      <damageType>Cut</damageType>
    </li>
    <li Class="TuTien.AbilityEffectDef">
      <effectType>Buff</effectType>
      <hediffDef>QiEnhancement</hediffDef>
      <magnitude>1</magnitude>
      <duration>600</duration>
    </li>
  </effects>
</TuTien.CultivationAbilityDef>
```

```csharp
// C# Effect Processing
public class CultivationAbility
{
    private void ApplyGenericEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target)
    {
        switch (effect.effectType.ToLower())
        {
            case "damage":
                ApplyDamageEffect(effect, caster, target);
                break;
                
            case "heal":
                ApplyHealEffect(effect, caster, target);
                break;
                
            case "buff":
            case "debuff":
                ApplyHediffEffect(effect, caster, target);
                break;
                
            case "projectile":
                LaunchProjectile(effect, caster, target);
                break;
                
            default:
                // Extensible for custom effect types
                ApplyCustomEffect(effect, caster, target);
                break;
        }
    }
}
```

---

## 🎨 **UI Integration Architecture**

### **Unified Gizmo System Design**

```
Gizmo Creation Pipeline:
┌─────────────┐
│ Pawn.Get    │
│ Gizmos()    │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ Harmony     │
│ Patch       │ 
│ Intercept   │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────────────┐
│          UNIFIED SCAN PROCESS           │
├─────────────┬─────────────┬─────────────┤
│ Known Skills│ Known Abili-│ Artifact    │
│ (Cultivation│ ties (Culti-│ Skills      │
│ SkillDef)   │ vationAbili-│ (autoSkills)│
│             │ tyDef)      │             │
└──────┬──────┴──────┬──────┴──────┬──────┘
       │             │             │
       ▼             ▼             ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│Create Skill │ │Create Abil- │ │Dual System │
│Gizmo        │ │ity Gizmo    │ │Lookup      │
└─────────────┘ └─────────────┘ └─────────────┘
       │             │             │
       └─────────────┼─────────────┘
                     ▼
            ┌─────────────┐
            │   Yield     │
            │   Return    │
            │   Gizmo     │
            └─────────────┘
```

### **Gizmo Creation Implementation**

```csharp
[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public static class Pawn_GetGizmos_UnifiedPatch
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
    {
        // Return original gizmos first
        foreach (var gizmo in __result)
            yield return gizmo;
        
        // Add cultivation system gizmos
        foreach (var cultivationGizmo in GetCultivationGizmos(__instance))
            yield return cultivationGizmo;
    }
    
    private static IEnumerable<Gizmo> GetCultivationGizmos(Pawn pawn)
    {
        var comp = pawn.GetComp<CultivationComp>();
        if (comp?.cultivationData == null) yield break;
        
        // 1. Known Skills (CultivationSkillDef)
        foreach (var skill in GetKnownSkillGizmos(pawn, comp))
            yield return skill;
            
        // 2. Known Abilities (CultivationAbilityDef)
        foreach (var ability in GetKnownAbilityGizmos(pawn, comp))
            yield return ability;
            
        // 3. Artifact Skills (Dual System)
        foreach (var artifactSkill in GetArtifactSkillGizmos(pawn, comp))
            yield return artifactSkill;
    }
    
    private static IEnumerable<Gizmo> GetArtifactSkillGizmos(Pawn pawn, CultivationComp comp)
    {
        // Scan equipped artifacts for autoSkills
        foreach (var thing in pawn.equipment.AllEquipmentListForReading)
        {
            var artifactComp = thing.GetComp<CultivationArtifactComp>();
            if (artifactComp?.def?.autoSkills == null) continue;
            
            foreach (string skillName in artifactComp.def.autoSkills)
            {
                // Try CultivationSkillDef first
                var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillName);
                if (skillDef != null)
                {
                    yield return CreateSkillGizmo(skillDef, pawn, comp);
                    continue;
                }
                
                // Try CultivationAbilityDef second
                var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamedSilentFail(skillName);
                if (abilityDef != null)
                {
                    yield return CreateAbilityGizmo(abilityDef, pawn, comp);
                }
            }
        }
    }
}
```

---

## 🎭 **Effect System Design**

### **Effect Type Hierarchy**

```
Effect System Architecture:
┌─────────────────────────────────────────────────────────────┐
│                    EFFECT TYPES                             │
├─────────────────┬─────────────────┬─────────────────────────┤
│ Concrete Types  │ Generic Types   │ Custom Extensions       │
├─────────────────┼─────────────────┼─────────────────────────┤
│ AbilityEffect_  │ AbilityEffectDef│ User-defined classes    │
│ Heal            │ (effectType:    │ with Apply() method     │
│                 │  "Damage")      │                         │
├─────────────────┼─────────────────┼─────────────────────────┤
│ AbilityEffect_  │ AbilityEffectDef│ Registerable through    │
│ LaunchProjectile│ (effectType:    │ effect factory pattern  │
│                 │  "Heal")        │                         │
├─────────────────┼─────────────────┼─────────────────────────┤
│ AbilityEffect_  │ AbilityEffectDef│ Extensible switch/case  │
│ CorpseRevival   │ (effectType:    │ in ApplyGenericEffect() │
│                 │  "Buff")        │                         │
└─────────────────┴─────────────────┴─────────────────────────┘
```

### **Generic Effect Processing**

```csharp
private void ApplyGenericEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target)
{
    // Validate effect and target
    if (!ValidateEffect(effect, caster, target)) return;
    
    // Pre-effect hooks
    OnPreEffectApplication(effect, caster, target);
    
    // Main effect processing
    switch (effect.effectType.ToLower())
    {
        case "damage":
            ProcessDamageEffect(effect, caster, target);
            break;
            
        case "heal":
            ProcessHealEffect(effect, caster, target);
            break;
            
        case "buff":
        case "debuff":
            ProcessHediffEffect(effect, caster, target);
            break;
            
        case "teleport":
            ProcessTeleportEffect(effect, caster, target);
            break;
            
        case "summon":
            ProcessSummonEffect(effect, caster, target);
            break;
            
        default:
            // Extensible custom effect processing
            if (!TryApplyCustomEffect(effect, caster, target))
            {
                Log.Warning($"Unknown effect type: {effect.effectType}");
            }
            break;
    }
    
    // Post-effect hooks
    OnPostEffectApplication(effect, caster, target);
}

private bool TryApplyCustomEffect(AbilityEffectDef effect, Pawn caster, LocalTargetInfo target)
{
    // Plugin system for custom effects
    foreach (var customHandler in registeredEffectHandlers)
    {
        if (customHandler.CanHandle(effect.effectType))
        {
            customHandler.Apply(effect, caster, target);
            return true;
        }
    }
    return false;
}
```

---

## 📊 **Data Flow Diagrams**

### **Skill Execution Data Flow**

```
Skill Execution Complete Flow:
┌─────────────┐
│ User Click  │
│   Gizmo     │
└──────┬──────┘
       │
       ▼
┌─────────────┐    ┌─────────────┐
│ Validation  │───►│ Pre-checks  │
│   Check     │    │ (Qi, Range, │
└──────┬──────┘    │ Cooldown)   │
       │           └─────────────┘
       ▼
┌─────────────┐    ┌─────────────┐
│ Execute     │───►│ Apply       │
│ Skill/      │    │ Effects     │
│ Ability     │    │             │
└──────┬──────┘    └─────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│            EFFECT PIPELINE              │
├─────────────┬─────────────┬─────────────┤
│   Target    │   Visual    │  Resource   │
│ Processing  │  Effects    │ Management  │
│             │             │             │
│ • Damage    │ • Particles │ • Qi Cost   │
│ • Healing   │ • Sounds    │ • Cooldown  │
│ • Buffs     │ • Messages  │ • Stats     │
└─────────────┴─────────────┴─────────────┘
```

### **Artifact Integration Data Flow**

```
Artifact Equipment Data Flow:
┌─────────────┐    Equipment Event    ┌─────────────┐
│   Player    │ ──────────────────── │  Artifact   │
│   Action    │                      │ Component   │
└─────────────┘                      └──────┬──────┘
       │                                    │
       ▼                                    ▼
┌─────────────┐                      ┌─────────────┐
│ Equipment   │    Notify Hook       │ Scan autoS- │
│ Manager     │ ──────────────────── │ kills Array │
│ (RimWorld)  │                      └──────┬──────┘
└─────────────┘                             │
       │                                    ▼
       ▼                              ┌─────────────┐
┌─────────────┐                      │ DefDatabase │
│ Update UI   │ ◄─────────────────── │ Dual Lookup │
│ Gizmos      │                      │ Process     │
└─────────────┘                      └─────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│           GIZMO GENERATION              │
├─────────────┬─────────────┬─────────────┤
│ Skill       │ Ability     │ Mixed       │
│ Gizmos      │ Gizmos      │ Display     │
│             │             │             │
│ • Direct    │ • Effect    │ • Cooldowns │
│   Execution │   Based     │ • Qi Costs  │
│ • Simple    │ • Flexible  │ • Range     │
└─────────────┴─────────────┴─────────────┘
```

---

## 🔌 **Integration Points**

### **RimWorld Core System Hooks**

```csharp
// 1. Component System Integration
[HarmonyPatch(typeof(ThingDef), "PostLoad")]
public static class ThingDef_PostLoad_Patch
{
    // Automatically add cultivation components to humanlike pawns
}

// 2. UI System Integration  
[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public static class Pawn_GetGizmos_UnifiedPatch
{
    // Inject cultivation gizmos into standard UI flow
}

// 3. Damage System Integration
[HarmonyPatch(typeof(DamageWorker_AddInjury), "Apply")]
public static class DamageWorker_AddInjury_Apply_Patch
{
    // Cultivation-based damage modification and tracking
}

// 4. Equipment System Integration
public override void Notify_Equipped(Pawn pawn)
{
    // Artifact skill granting integration
}

// 5. Save/Load System Integration
public override void PostExposeData()
{
    // Ensure all cultivation data persists correctly
}
```

### **Mod Compatibility Architecture**

```csharp
public static class ModCompatibility
{
    private static readonly Dictionary<string, IModIntegration> integrations = 
        new Dictionary<string, IModIntegration>();
    
    static ModCompatibility()
    {
        // Combat Extended
        if (ModsConfig.IsActive("CETeam.CombatExtended"))
        {
            integrations["CombatExtended"] = new CombatExtendedIntegration();
        }
        
        // Prepare For War
        if (ModsConfig.IsActive("Lukas.PrepareForWar"))
        {
            integrations["PrepareForWar"] = new PrepareForWarIntegration();
        }
        
        // Custom cultivation mods
        RegisterCultivationModIntegrations();
    }
    
    public static void ApplyCompatibilityPatch(string modId, string context)
    {
        if (integrations.TryGetValue(modId, out var integration))
        {
            integration.Apply(context);
        }
    }
}
```

### **Extension Plugin Architecture**

```csharp
public interface ICultivationExtension
{
    string ExtensionName { get; }
    Version MinimumVersion { get; }
    
    void Initialize();
    void RegisterSkills(ISkillRegistry registry);
    void RegisterEffects(IEffectRegistry registry);
    void RegisterArtifacts(IArtifactRegistry registry);
}

public class CultivationExtensionManager
{
    private readonly List<ICultivationExtension> loadedExtensions = 
        new List<ICultivationExtension>();
    
    public void LoadExtensions()
    {
        // Scan for extension assemblies
        // Load and initialize extensions
        // Register their content with appropriate systems
    }
    
    public void RegisterExtension(ICultivationExtension extension)
    {
        extension.Initialize();
        extension.RegisterSkills(skillRegistry);
        extension.RegisterEffects(effectRegistry);
        extension.RegisterArtifacts(artifactRegistry);
        
        loadedExtensions.Add(extension);
    }
}
```

---

## 📈 **Performance Considerations**

### **Optimization Strategies**

```csharp
public class CultivationPerformanceManager
{
    // 1. Caching Strategy
    private static readonly Dictionary<string, CultivationSkillDef> skillDefCache = 
        new Dictionary<string, CultivationSkillDef>();
    
    private static readonly Dictionary<string, CultivationAbilityDef> abilityDefCache = 
        new Dictionary<string, CultivationAbilityDef>();
    
    // 2. Batch Processing
    private static readonly List<CultivationComp> tickQueue = new List<CultivationComp>();
    private static int currentTickIndex = 0;
    
    public static void OptimizedTick()
    {
        // Process only a subset of components per tick to distribute load
        int batchSize = Math.Max(1, tickQueue.Count / 60); // Spread over 60 ticks
        for (int i = 0; i < batchSize && currentTickIndex < tickQueue.Count; i++)
        {
            tickQueue[currentTickIndex].OptimizedTick();
            currentTickIndex++;
        }
        
        if (currentTickIndex >= tickQueue.Count)
            currentTickIndex = 0;
    }
    
    // 3. Lazy Loading
    public static CultivationSkillDef GetSkillDef(string defName)
    {
        if (!skillDefCache.TryGetValue(defName, out var def))
        {
            def = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(defName);
            skillDefCache[defName] = def; // Cache null results too
        }
        return def;
    }
}
```

### **Memory Management**

```csharp
public class CultivationMemoryManager
{
    // 1. Object Pooling for frequently created objects
    private static readonly Queue<DamageInfo> damageInfoPool = new Queue<DamageInfo>();
    private static readonly Queue<CultivationEffect> effectPool = new Queue<CultivationEffect>();
    
    // 2. Weak References for non-critical caching
    private static readonly Dictionary<int, WeakReference> componentCache = 
        new Dictionary<int, WeakReference>();
    
    // 3. Periodic cleanup
    public static void PerformCleanup()
    {
        // Clean up expired weak references
        var keysToRemove = componentCache.Where(kvp => !kvp.Value.IsAlive).Select(kvp => kvp.Key).ToList();
        foreach (var key in keysToRemove)
        {
            componentCache.Remove(key);
        }
        
        // Trim object pools if they get too large
        while (damageInfoPool.Count > 100)
            damageInfoPool.Dequeue();
    }
}
```

---

## 🔬 **Testing & Debugging Framework**

### **Debug Tools**

```csharp
[System.Diagnostics.Conditional("DEBUG")]
public static class CultivationDebug
{
    public static bool EnableDetailedLogging = false;
    public static bool EnablePerformanceTracking = false;
    public static Dictionary<string, Stopwatch> performanceTimers = new Dictionary<string, Stopwatch>();
    
    public static void LogSkillExecution(string skillName, Pawn pawn, LocalTargetInfo target)
    {
        if (!EnableDetailedLogging) return;
        
        Log.Message($"[CULTIVATION DEBUG] Executing skill: {skillName} " +
                   $"| Pawn: {pawn.LabelShort} " +
                   $"| Target: {target} " +
                   $"| Qi: {pawn.GetCultivationData()?.currentQi}");
    }
    
    public static IDisposable TrackPerformance(string operationName)
    {
        if (!EnablePerformanceTracking) return null;
        
        if (!performanceTimers.TryGetValue(operationName, out var timer))
        {
            timer = new Stopwatch();
            performanceTimers[operationName] = timer;
        }
        
        timer.Start();
        return new PerformanceTracker(operationName, timer);
    }
}

public class PerformanceTracker : IDisposable
{
    private readonly string operationName;
    private readonly Stopwatch timer;
    
    public PerformanceTracker(string name, Stopwatch sw)
    {
        operationName = name;
        timer = sw;
    }
    
    public void Dispose()
    {
        timer.Stop();
        if (timer.ElapsedMilliseconds > 10) // Only log slow operations
        {
            Log.Warning($"[PERFORMANCE] {operationName} took {timer.ElapsedMilliseconds}ms");
        }
    }
}
```

---

## 📝 **Version History & Migration**

### **Architecture Evolution**

```
Version 1.0: Basic Cultivation System
├─ Simple skill definitions
├─ Basic UI integration
└─ Core progression mechanics

Version 1.5: Ability System Addition
├─ CultivationAbilityDef introduction
├─ Effect-based ability system
├─ CompAbilityUser integration
└─ Dual system UI support

Version 2.0: Artifact Integration (Current)
├─ CultivationArtifactDef system
├─ ELO-based artifact progression
├─ Unified gizmo system
├─ Dual skill system support
└─ Generic effect processing

Version 2.5: Planned Extensions
├─ Advanced cultivation realms
├─ Sect system integration
├─ Multi-element techniques
└─ Advanced artifact crafting
```

### **Migration Strategies**

```csharp
public static class CultivationMigration
{
    public static void MigrateToVersion2_0()
    {
        // Migrate old skill definitions to new unified system
        // Convert legacy artifact data to new ELO system
        // Update save game compatibility
        
        Log.Message("[CULTIVATION] Migrating to version 2.0...");
        
        // 1. Update component definitions
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
        {
            if (thingDef.HasComp<LegacyCultivationComp>())
            {
                // Convert to new CultivationComp
                MigrateComponent(thingDef);
            }
        }
        
        // 2. Convert save data
        MigrateSaveData();
        
        Log.Message("[CULTIVATION] Migration complete!");
    }
}
```

---

**Last Updated**: September 2025  
**Architecture Version**: 2.0  
**Compatibility**: RimWorld 1.6+
