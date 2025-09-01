# Tu Tien Mod - Scalability & Maintainability Analysis

## 🔍 Current Architecture Analysis

### ✅ **Strong Points**
```
1. Component-based Architecture
   ✓ CultivationComp separates cultivation logic from pawn
   ✓ Modular skill system with worker classes
   ✓ XML-driven configuration for easy modification

2. Harmony Integration
   ✓ Non-intrusive patching
   ✓ Clean separation from vanilla code
   ✓ Easy to disable/enable features

3. Save/Load System
   ✓ Proper ExposeData implementation
   ✓ Data integrity maintained
```

### ⚠️ **Scalability Issues**

```
Current Issues:
┌─────────────────────────────────────────────────────┐
│ 1. SKILL SYSTEM                                     │
│    ❌ Hard-coded skill types in workers             │
│    ❌ No skill categories/grouping                  │
│    ❌ Manual skill registration                     │
│                                                     │
│ 2. STATS SYSTEM                                     │
│    ❌ Stats scattered across multiple files         │
│    ❌ No centralized stat calculation               │
│    ❌ Hard to add new stat types                    │
│                                                     │
│ 3. UI SYSTEM                                        │
│    ❌ Monolithic drawing methods                    │
│    ❌ Hard-coded UI layouts                         │
│    ❌ No UI modularity for new features             │
│                                                     │
│ 4. TECHNIQUE SYSTEM                                 │
│    ❌ Basic implementation, not extensible          │
│    ❌ No technique trees or prerequisites           │
│    ❌ Limited technique effects                     │
└─────────────────────────────────────────────────────┘
```

## 🏗️ Proposed Scalable Architecture

### 1. **Skill System Redesign**

```csharp
// Current: Hard-coded workers
public class QiPunchWorker : CultivationSkillWorker { }
public class QiShieldWorker : CultivationSkillWorker { }

// Proposed: Plugin-based system
public abstract class CultivationSkillWorker
{
    public abstract string SkillCategory { get; }
    public abstract SkillTier SkillTier { get; }
    public abstract List<string> Prerequisites { get; }
    public abstract List<SkillEffect> Effects { get; }
    
    public virtual bool CanExecute(Pawn pawn, CultivationSkillDef skill)
    {
        // Base validation + custom validation
        return BaseValidation(pawn, skill) && CustomValidation(pawn, skill);
    }
    
    protected virtual bool CustomValidation(Pawn pawn, CultivationSkillDef skill) => true;
    public abstract void Execute(Pawn pawn, CultivationSkillDef skill);
}

// Auto-discovery system
public static class SkillRegistry
{
    private static Dictionary<string, Type> skillWorkers;
    
    static SkillRegistry()
    {
        // Auto-discover all skill worker types
        skillWorkers = typeof(CultivationSkillWorker).AllSubclassesNonAbstract()
            .ToDictionary(t => t.Name.Replace("Worker", ""), t => t);
    }
    
    public static CultivationSkillWorker GetWorker(string skillType)
    {
        if (skillWorkers.TryGetValue(skillType, out Type workerType))
        {
            return (CultivationSkillWorker)Activator.CreateInstance(workerType);
        }
        return null;
    }
}
```

### 2. **Modular Stats System**

```csharp
// Stats calculation pipeline
public class CultivationStatsManager
{
    private List<IStatCalculator> calculators = new List<IStatCalculator>();
    
    public void RegisterCalculator(IStatCalculator calculator)
    {
        calculators.Add(calculator);
    }
    
    public CultivationStats Calculate(CultivationData data)
    {
        var stats = new CultivationStats();
        
        foreach (var calculator in calculators)
        {
            if (calculator.Applies(data))
            {
                calculator.Apply(data, stats);
            }
        }
        
        return stats;
    }
}

public interface IStatCalculator
{
    int Priority { get; }
    bool Applies(CultivationData data);
    void Apply(CultivationData data, CultivationStats stats);
}

// Example implementations
public class RealmStatCalculator : IStatCalculator
{
    public int Priority => 100;
    public bool Applies(CultivationData data) => true;
    
    public void Apply(CultivationData data, CultivationStats stats)
    {
        var realmMultiplier = GetRealmMultiplier(data.currentRealm);
        stats.maxQi *= realmMultiplier;
        stats.qiRegenRate *= realmMultiplier;
    }
}

public class TechniqueStatCalculator : IStatCalculator
{
    public int Priority => 200;
    public bool Applies(CultivationData data) => data.currentTechnique != null;
    
    public void Apply(CultivationData data, CultivationStats stats)
    {
        var technique = data.currentTechnique;
        technique.ApplyStatModifiers(stats);
    }
}
```

### 3. **Technique System Architecture**

```csharp
public class CultivationTechnique
{
    public string defName;
    public string labelKey;
    public TechniqueType type;
    public List<TechniqueNode> nodes;
    public List<string> conflictingTechniques;
    
    public class TechniqueNode
    {
        public string nodeId;
        public string labelKey;
        public int requiredStage;
        public CultivationRealm requiredRealm;
        public List<string> prerequisites;
        public List<TechniqueEffect> effects;
        public List<string> unlockedSkills;
    }
}

public abstract class TechniqueEffect
{
    public abstract void Apply(CultivationData data, CultivationStats stats);
    public abstract void OnActivate(Pawn pawn);
    public abstract void OnDeactivate(Pawn pawn);
}

// Example: Sword Path Technique
public class SwordPathTechnique : TechniqueEffect
{
    public float meleeDamageBonus = 0.2f;
    public float meleeAccuracyBonus = 0.15f;
    
    public override void Apply(CultivationData data, CultivationStats stats)
    {
        stats.meleeDamageMultiplier += meleeDamageBonus;
        stats.meleeHitChanceOffset += meleeAccuracyBonus;
    }
    
    public override void OnActivate(Pawn pawn)
    {
        // Unlock sword-related skills
        var swordSkills = DefDatabase<CultivationSkillDef>.AllDefs
            .Where(s => s.defName.Contains("Sword"));
        
        foreach (var skill in swordSkills)
        {
            pawn.GetComp<CultivationComp>().cultivationData.UnlockSkill(skill);
        }
    }
}
```

### 4. **Modular UI System**

```csharp
// UI Component system
public abstract class CultivationUIComponent
{
    public abstract string ComponentName { get; }
    public abstract int Priority { get; }
    public abstract bool ShouldShow(CultivationData data);
    public abstract float GetRequiredHeight(CultivationData data);
    public abstract void Draw(Rect rect, CultivationData data);
}

public class CultivationUIManager
{
    private List<CultivationUIComponent> components = new List<CultivationUIComponent>();
    
    public void RegisterComponent(CultivationUIComponent component)
    {
        components.Add(component);
        components = components.OrderBy(c => c.Priority).ToList();
    }
    
    public void DrawCultivationTab(Rect rect, CultivationData data)
    {
        var listing = new Listing_Standard();
        listing.Begin(rect);
        
        foreach (var component in components.Where(c => c.ShouldShow(data)))
        {
            var componentRect = listing.GetRect(component.GetRequiredHeight(data));
            component.Draw(componentRect, data);
            listing.Gap();
        }
        
        listing.End();
    }
}

// Example components
public class RealmProgressComponent : CultivationUIComponent
{
    public override string ComponentName => "RealmProgress";
    public override int Priority => 1;
    public override bool ShouldShow(CultivationData data) => true;
    
    public override float GetRequiredHeight(CultivationData data) => 50f;
    
    public override void Draw(Rect rect, CultivationData data)
    {
        // Draw realm and progress bars
    }
}

public class SkillsComponent : CultivationUIComponent
{
    public override string ComponentName => "Skills";
    public override int Priority => 3;
    public override bool ShouldShow(CultivationData data) => data.unlockedSkills.Any();
    
    public override float GetRequiredHeight(CultivationData data)
    {
        return 30f + (data.unlockedSkills.Count * 35f);
    }
    
    public override void Draw(Rect rect, CultivationData data)
    {
        // Draw skills list
    }
}
```

### 5. **Event System for Extensibility**

```csharp
public static class CultivationEvents
{
    public static event Action<Pawn, CultivationRealm, CultivationRealm> OnRealmChanged;
    public static event Action<Pawn, CultivationSkillDef> OnSkillUsed;
    public static event Action<Pawn, CultivationTechnique> OnTechniqueChanged;
    public static event Action<Pawn, bool> OnBreakthroughAttempt;
    
    public static void TriggerRealmChanged(Pawn pawn, CultivationRealm oldRealm, CultivationRealm newRealm)
    {
        OnRealmChanged?.Invoke(pawn, oldRealm, newRealm);
    }
    
    // Other event triggers...
}

// Usage: Other systems can subscribe to events
public class CultivationAchievements
{
    static CultivationAchievements()
    {
        CultivationEvents.OnRealmChanged += CheckRealmAchievements;
        CultivationEvents.OnSkillUsed += CheckSkillAchievements;
    }
    
    private static void CheckRealmAchievements(Pawn pawn, CultivationRealm oldRealm, CultivationRealm newRealm)
    {
        // Check and award achievements
    }
}
```

## 📋 Implementation Roadmap

### Phase 1: Foundation Refactoring
```
Week 1-2: Core Systems
├── Refactor SkillRegistry system
├── Implement IStatCalculator interface
├── Create CultivationStatsManager
└── Add event system

Week 3-4: UI Modularity  
├── Break down monolithic UI methods
├── Implement UI component system
├── Create base UI components
└── Test UI flexibility
```

### Phase 2: Advanced Features
```
Week 5-6: Technique System
├── Design technique tree structure
├── Implement technique effects
├── Create technique UI
└── Add technique conflicts/synergies

Week 7-8: Extensibility
├── Plugin system for skills
├── Mod compatibility framework
├── Configuration system
└── Documentation for modders
```

### Phase 3: Optimization
```
Week 9-10: Performance
├── Implement object pooling
├── Optimize tick systems
├── Cache expensive calculations
└── Memory usage optimization
```

## 🛠️ Recommended File Structure

```
Source/TuTien/
├── Core/
│   ├── CultivationComp.cs
│   ├── CultivationData.cs
│   └── CultivationEvents.cs
├── Systems/
│   ├── Skills/
│   │   ├── SkillRegistry.cs
│   │   ├── ISkillWorker.cs
│   │   └── Workers/
│   ├── Stats/
│   │   ├── IStatCalculator.cs
│   │   ├── CultivationStatsManager.cs
│   │   └── Calculators/
│   ├── Techniques/
│   │   ├── CultivationTechnique.cs
│   │   ├── TechniqueEffect.cs
│   │   └── Effects/
│   └── UI/
│       ├── CultivationUIManager.cs
│       ├── ICultivationUIComponent.cs
│       └── Components/
├── Integration/
│   ├── Patches/
│   └── Compatibility/
└── Utils/
    ├── Extensions.cs
    └── Helpers.cs
```

## 🎯 Benefits of Proposed Architecture

### ✅ **Scalability**
- ✓ Easy to add new skills (just create worker class)
- ✓ Modular stat system (new calculators plug in)
- ✓ Flexible UI (new components auto-integrate)
- ✓ Technique system supports complex trees

### ✅ **Maintainability**
- ✓ Single responsibility principle
- ✓ Clear separation of concerns
- ✓ Event-driven architecture reduces coupling
- ✓ Auto-discovery reduces manual registration

### ✅ **Extensibility**
- ✓ Plugin system for third-party skills
- ✓ Event system for other mods to hook into
- ✓ Configuration system for customization
- ✓ Clean APIs for extension

### ✅ **Testability**
- ✓ Interface-based design
- ✓ Dependency injection ready
- ✓ Modular components easy to unit test
- ✓ Mock-friendly architecture

## 🚀 Migration Strategy

### 1. **Backward Compatibility**
```csharp
// Keep old methods as wrappers
[Obsolete("Use SkillRegistry.GetWorker() instead")]
public static void UseSkillOld(CultivationSkillDef skill)
{
    var worker = SkillRegistry.GetWorker(skill.workerClass?.Name ?? skill.defName);
    worker?.Execute(pawn, skill);
}
```

### 2. **Gradual Migration**
```
Step 1: Implement new systems alongside old ones
Step 2: Migrate one system at a time
Step 3: Update save compatibility
Step 4: Remove deprecated code
```

### 3. **Configuration Migration**
```csharp
public static class SaveMigration
{
    public static void MigrateFromV1(CultivationData data)
    {
        // Convert old format to new format
        if (data.version < 2)
        {
            // Migrate skills format
            // Migrate stats format
            // Update version
        }
    }
}
```

Với architecture này, bạn có thể dễ dàng thêm hàng trăm skills, techniques, và stats mà không cần modify core code! 🚀✨
