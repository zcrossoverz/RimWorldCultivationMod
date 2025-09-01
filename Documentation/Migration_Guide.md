# Tu Tien Mod - Migration Guide: From Current to Scalable Architecture

## 🎯 Migration Overview

This guide provides step-by-step instructions to migrate from the current hardcoded system to a scalable, extensible architecture while maintaining backward compatibility.

## 📋 Migration Checklist

### Phase 1: Foundation Setup (Week 1)
- [ ] Create new folder structure
- [ ] Implement SkillRegistry system
- [ ] Create enhanced base classes
- [ ] Maintain backward compatibility

### Phase 2: Skills Migration (Week 2)
- [ ] Migrate existing skills one by one
- [ ] Test each migrated skill
- [ ] Update skill definitions (XML)
- [ ] Remove old skill system

### Phase 3: Stats System (Week 3)
- [ ] Implement stat calculator system
- [ ] Create base stat calculators
- [ ] Migrate existing stat logic
- [ ] Performance testing

### Phase 4: UI Refactoring (Week 4)
- [ ] Break down monolithic UI
- [ ] Create UI component system
- [ ] Migrate UI sections
- [ ] Test UI flexibility

## 🗂️ New Folder Structure

First, create the new organized folder structure:

```
Source/TuTien/
├── Core/                          # Core systems
│   ├── CultivationComp.cs        # Enhanced main component
│   ├── CultivationData.cs        # Data structure
│   ├── CultivationEvents.cs      # Event system
│   └── CultivationDefOf.cs       # Def references
├── Systems/                       # Modular systems
│   ├── Skills/                    # Skill system
│   │   ├── SkillRegistry.cs      # Auto-discovery system
│   │   ├── CultivationSkillWorker.cs  # Enhanced base class
│   │   ├── SkillTargeting.cs     # Targeting system
│   │   └── Workers/              # Individual skill workers
│   │       ├── Combat/           # Combat skills
│   │       ├── Utility/          # Utility skills
│   │       └── Ultimate/         # Ultimate skills
│   ├── Stats/                     # Stats calculation
│   │   ├── CultivationStatsManager.cs
│   │   ├── IStatCalculator.cs
│   │   └── Calculators/          # Individual calculators
│   ├── Techniques/               # Technique system
│   │   ├── CultivationTechnique.cs
│   │   ├── TechniqueEffects.cs
│   │   └── Effects/              # Individual effects
│   └── UI/                       # UI components
│       ├── CultivationUIManager.cs
│       ├── ICultivationUIComponent.cs
│       └── Components/           # UI components
├── Integration/                   # Game integration
│   ├── Patches/                  # Harmony patches
│   └── Compatibility/            # Mod compatibility
├── Utils/                        # Utilities
│   ├── Extensions.cs             # Extension methods
│   ├── Helpers.cs                # Helper functions
│   └── Constants.cs              # Game constants
└── Legacy/                       # Backward compatibility
    ├── LegacySkillSystem.cs      # Old system wrapper
    └── Migration/                # Migration utilities
```

## 🔧 Step-by-Step Migration

### Step 1: Create Event System

Create `Source/TuTien/Core/CultivationEvents.cs`:
```csharp
using System;
using Verse;

namespace TuTien.Core
{
    public static class CultivationEvents
    {
        // Realm events
        public static event Action<Pawn, CultivationRealm, CultivationRealm> OnRealmChanged;
        public static event Action<Pawn, int, int> OnStageChanged;
        public static event Action<Pawn, bool> OnBreakthroughAttempt;
        public static event Action<Pawn, bool> OnBreakthroughResult;
        
        // Skill events
        public static event Action<Pawn, CultivationSkillDef> OnSkillUsed;
        public static event Action<Pawn, CultivationSkillDef> OnSkillUnlocked;
        public static event Action<Pawn, CultivationSkillDef> OnSkillCooldownExpired;
        
        // Resource events
        public static event Action<Pawn, float, float> OnQiChanged;
        public static event Action<Pawn, float, float> OnTuViChanged;
        
        // Technique events
        public static event Action<Pawn, CultivationTechnique, CultivationTechnique> OnTechniqueChanged;
        
        // Stats events
        public static event Action<Pawn, CultivationStats> OnStatsRecalculated;
        
        #region Event Triggers
        
        public static void TriggerRealmChanged(Pawn pawn, CultivationRealm oldRealm, CultivationRealm newRealm)
        {
            Log.Message($"[TuTien Events] {pawn.Name} realm changed: {oldRealm} -> {newRealm}");
            OnRealmChanged?.Invoke(pawn, oldRealm, newRealm);
        }
        
        public static void TriggerStageChanged(Pawn pawn, int oldStage, int newStage)
        {
            Log.Message($"[TuTien Events] {pawn.Name} stage changed: {oldStage} -> {newStage}");
            OnStageChanged?.Invoke(pawn, oldStage, newStage);
        }
        
        public static void TriggerSkillUsed(Pawn pawn, CultivationSkillDef skill)
        {
            OnSkillUsed?.Invoke(pawn, skill);
        }
        
        public static void TriggerSkillUnlocked(Pawn pawn, CultivationSkillDef skill)
        {
            Log.Message($"[TuTien Events] {pawn.Name} unlocked skill: {skill.LabelCap}");
            OnSkillUnlocked?.Invoke(pawn, skill);
        }
        
        public static void TriggerQiChanged(Pawn pawn, float oldQi, float newQi)
        {
            OnQiChanged?.Invoke(pawn, oldQi, newQi);
        }
        
        public static void TriggerTuViChanged(Pawn pawn, float oldTuVi, float newTuVi)
        {
            OnTuViChanged?.Invoke(pawn, oldTuVi, newTuVi);
        }
        
        public static void TriggerStatsRecalculated(Pawn pawn, CultivationStats stats)
        {
            OnStatsRecalculated?.Invoke(pawn, stats);
        }
        
        #endregion
    }
}
```

### Step 2: Enhanced CultivationData

Update `Source/TuTien/CultivationData.cs`:
```csharp
using System.Collections.Generic;
using System.Linq;
using Verse;
using TuTien.Core;
using TuTien.Stats;

namespace TuTien
{
    public class CultivationData : IExposable
    {
        // ... existing fields ...
        
        // New fields for scalability
        public CultivationStats calculatedStats;
        public CultivationTechnique currentTechnique;
        public List<string> unlockedSkills = new List<string>();
        public Dictionary<string, float> skillCooldowns = new Dictionary<string, float>();
        public Dictionary<string, object> customData = new Dictionary<string, object>();
        
        // Enhanced properties with event triggers
        private float _currentQi;
        public float currentQi 
        { 
            get => _currentQi;
            set
            {
                if (Math.Abs(_currentQi - value) > 0.01f)
                {
                    var oldQi = _currentQi;
                    _currentQi = Mathf.Clamp(value, 0, calculatedStats?.maxQi ?? maxQi);
                    CultivationEvents.TriggerQiChanged(pawn, oldQi, _currentQi);
                }
            }
        }
        
        private float _tuViPoints;
        public float tuViPoints
        {
            get => _tuViPoints;
            set
            {
                if (Math.Abs(_tuViPoints - value) > 0.01f)
                {
                    var oldTuVi = _tuViPoints;
                    _tuViPoints = Mathf.Max(0, value);
                    CultivationEvents.TriggerTuViChanged(pawn, oldTuVi, _tuViPoints);
                }
            }
        }
        
        private CultivationRealm _currentRealm;
        public CultivationRealm currentRealm
        {
            get => _currentRealm;
            set
            {
                if (_currentRealm != value)
                {
                    var oldRealm = _currentRealm;
                    _currentRealm = value;
                    CultivationEvents.TriggerRealmChanged(pawn, oldRealm, _currentRealm);
                    RecalculateStats();
                }
            }
        }
        
        private int _currentStage;
        public int currentStage
        {
            get => _currentStage;
            set
            {
                if (_currentStage != value)
                {
                    var oldStage = _currentStage;
                    _currentStage = Mathf.Clamp(value, 1, GetMaxStageForRealm(_currentRealm));
                    CultivationEvents.TriggerStageChanged(pawn, oldStage, _currentStage);
                    RecalculateStats();
                }
            }
        }
        
        // Reference to owner pawn
        private Pawn pawn;
        
        public CultivationData()
        {
            calculatedStats = new CultivationStats();
        }
        
        public CultivationData(Pawn pawn) : this()
        {
            this.pawn = pawn;
        }
        
        // New methods for skill system
        public bool HasLearnedSkill(string skillDefName)
        {
            return unlockedSkills.Contains(skillDefName);
        }
        
        public void UnlockSkill(string skillDefName)
        {
            if (!HasLearnedSkill(skillDefName))
            {
                unlockedSkills.Add(skillDefName);
                var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillDefName);
                if (skillDef != null && pawn != null)
                {
                    CultivationEvents.TriggerSkillUnlocked(pawn, skillDef);
                }
            }
        }
        
        public void UnlockSkill(CultivationSkillDef skill)
        {
            UnlockSkill(skill.defName);
        }
        
        public float GetSkillCooldown(string skillDefName)
        {
            return skillCooldowns.TryGetValue(skillDefName, out float cooldown) ? cooldown : 0f;
        }
        
        public void SetSkillCooldown(string skillDefName, float cooldown)
        {
            if (cooldown <= 0)
            {
                skillCooldowns.Remove(skillDefName);
            }
            else
            {
                skillCooldowns[skillDefName] = cooldown;
            }
        }
        
        public void TickCooldowns()
        {
            var keys = skillCooldowns.Keys.ToList();
            foreach (var skill in keys)
            {
                var newCooldown = skillCooldowns[skill] - 1f;
                if (newCooldown <= 0)
                {
                    skillCooldowns.Remove(skill);
                    
                    // Trigger cooldown expired event
                    var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skill);
                    if (skillDef != null && pawn != null)
                    {
                        CultivationEvents.OnSkillCooldownExpired?.Invoke(pawn, skillDef);
                    }
                }
                else
                {
                    skillCooldowns[skill] = newCooldown;
                }
            }
        }
        
        // Stats management
        public void RecalculateStats()
        {
            if (pawn != null)
            {
                calculatedStats = CultivationStatsManager.Calculate(this);
                CultivationEvents.TriggerStatsRecalculated(pawn, calculatedStats);
            }
        }
        
        // Custom data for extensibility
        public T GetCustomData<T>(string key, T defaultValue = default(T))
        {
            if (customData.TryGetValue(key, out object value) && value is T)
            {
                return (T)value;
            }
            return defaultValue;
        }
        
        public void SetCustomData<T>(string key, T value)
        {
            customData[key] = value;
        }
        
        // ... rest of existing code ...
        
        public override void ExposeData()
        {
            // ... existing expose data ...
            
            // New fields
            Scribe_Collections.Look(ref unlockedSkills, "unlockedSkills", LookMode.Value);
            Scribe_Collections.Look(ref skillCooldowns, "skillCooldowns", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref customData, "customData", LookMode.Value, LookMode.Value);
            Scribe_Deep.Look(ref currentTechnique, "currentTechnique");
            
            // Recalculate stats after loading
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (calculatedStats == null)
                    calculatedStats = new CultivationStats();
                
                if (pawn != null)
                    RecalculateStats();
            }
        }
    }
}
```

### Step 3: Migration Wrapper for Backward Compatibility

Create `Source/TuTien/Legacy/LegacySkillSystem.cs`:
```csharp
using System;
using Verse;
using TuTien.Skills;

namespace TuTien.Legacy
{
    /// <summary>
    /// Backward compatibility wrapper for old skill system
    /// Remove this class after full migration
    /// </summary>
    [Obsolete("Use SkillRegistry.GetWorker() instead", false)]
    public static class LegacySkillSystem
    {
        [Obsolete("Use SkillRegistry.GetWorker() instead")]
        public static void UseSkillOld(Pawn pawn, CultivationSkillDef skill)
        {
            Log.Warning("[TuTien] Using legacy skill system. Please update to new system.");
            
            var worker = SkillRegistry.GetWorker(skill.defName);
            if (worker != null)
            {
                worker.Execute(pawn, skill);
            }
            else
            {
                Log.Error($"[TuTien] No worker found for skill: {skill.defName}");
            }
        }
        
        [Obsolete("Use new skill worker system")]
        public static bool CanUseSkillOld(Pawn pawn, CultivationSkillDef skill)
        {
            var worker = SkillRegistry.GetWorker(skill.defName);
            return worker?.CanExecute(pawn, skill) ?? false;
        }
    }
}
```

### Step 4: Update CultivationComp

Update the main component to use new systems:
```csharp
// In CultivationComp.cs
public override void CompTick()
{
    if (pawn.IsHashIntervalTick(60)) // Every second
    {
        // Tick cooldowns
        cultivationData.TickCooldowns();
        
        // Regenerate Qi
        RegenerateQi();
        
        // Auto-cultivation
        if (cultivationData.isAutoCultivating)
        {
            AutoCultivate();
        }
    }
}

private void RegenerateQi()
{
    if (cultivationData.currentQi < cultivationData.calculatedStats.maxQi)
    {
        var regenAmount = cultivationData.calculatedStats.qiRegenRate;
        cultivationData.currentQi += regenAmount;
    }
}

// Updated skill usage with new system
public void UseSkill(CultivationSkillDef skill, LocalTargetInfo target = default)
{
    var worker = SkillRegistry.GetWorker(skill.defName);
    if (worker == null)
    {
        Log.Error($"[TuTien] No worker found for skill: {skill.defName}");
        return;
    }
    
    if (!worker.CanExecute(pawn, skill))
    {
        // Show appropriate failure message
        ShowSkillFailureMessage(pawn, skill, worker);
        return;
    }
    
    worker.Execute(pawn, skill, target);
}

private void ShowSkillFailureMessage(Pawn pawn, CultivationSkillDef skill, CultivationSkillWorker worker)
{
    string reason = "cannot use this skill";
    
    // Determine specific reason
    if (cultivationData.currentQi < worker.GetQiCost(pawn, skill))
        reason = "not enough Qi";
    else if (cultivationData.GetSkillCooldown(skill.defName) > 0)
        reason = "skill is on cooldown";
    else if (!worker.MeetsRealmRequirement(pawn))
        reason = "cultivation realm too low";
    
    Messages.Message(
        $"{pawn.Name.ToStringShort} {reason} ({skill.LabelCap})",
        pawn,
        MessageTypeDefOf.RejectInput
    );
}
```

### Step 5: Skill Migration Template

Template for migrating existing skills:
```csharp
// Before: Old skill worker
public class QiShieldWorker : CultivationSkillWorker
{
    public override void ExecuteSkill(Pawn pawn, CultivationSkillDef skill)
    {
        // Old hardcoded logic
    }
}

// After: New skill worker
[SkillWorker("QiShield")]
public class QiShieldWorker : CultivationSkillWorker
{
    public override SkillCategory Category => SkillCategory.Combat;
    public override SkillTier Tier => SkillTier.Basic;
    
    public override float BaseQiCost => 20f;
    public override float BaseCooldown => 45f;
    public override bool CanTargetSelf => true;
    
    public override Color SkillColor => Color.blue;
    
    protected override bool CustomValidation(Pawn pawn, CultivationSkillDef skill)
    {
        // Don't shield if already shielded
        return !pawn.health.hediffSet.HasHediff(CultivationDefOf.QiShield);
    }
    
    protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target)
    {
        var comp = pawn.GetComp<CultivationComp>();
        var shieldStrength = GetShieldStrength(comp.cultivationData);
        var duration = GetShieldDuration(comp.cultivationData);
        
        // Add shield hediff
        var shield = HediffMaker.MakeHediff(CultivationDefOf.QiShield, pawn) as Hediff_QiShield;
        if (shield != null)
        {
            shield.shieldStrength = shieldStrength;
            shield.maxDuration = duration;
            pawn.health.AddHediff(shield);
        }
    }
    
    private float GetShieldStrength(CultivationData data)
    {
        return 50f + (data.currentStage * 10f) + ((int)data.currentRealm * 25f);
    }
    
    private float GetShieldDuration(CultivationData data)
    {
        return 300f + (data.currentStage * 30f); // 5-15 minutes
    }
    
    protected override void SpawnVisualEffects(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target)
    {
        // Blue shield effect
        var effect = new FleckCreationData
        {
            def = FleckDefOf.PsycastAreaEffect,
            spawnPosition = pawn.DrawPos,
            scale = 2f,
            solidTimeOverride = 1f
        };
        
        pawn.Map.flecks.CreateFleck(effect);
    }
}
```

## 🧪 Testing Strategy

### Unit Testing Framework
Create simple test framework for skills:
```csharp
// Source/TuTien/Testing/SkillTestFramework.cs
public static class SkillTestFramework
{
    public static bool TestSkill(string skillName, CultivationRealm realm = CultivationRealm.QiGathering)
    {
        try
        {
            // Create test pawn
            var pawn = CreateTestPawn(realm);
            var skill = DefDatabase<CultivationSkillDef>.GetNamed(skillName);
            var worker = SkillRegistry.GetWorker(skillName);
            
            if (worker == null)
            {
                Log.Error($"[TuTien Test] No worker for skill: {skillName}");
                return false;
            }
            
            // Test validation
            var canExecute = worker.CanExecute(pawn, skill);
            Log.Message($"[TuTien Test] {skillName} can execute: {canExecute}");
            
            if (canExecute)
            {
                // Test execution
                worker.Execute(pawn, skill);
                Log.Message($"[TuTien Test] {skillName} executed successfully");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[TuTien Test] Error testing {skillName}: {ex.Message}");
            return false;
        }
    }
    
    private static Pawn CreateTestPawn(CultivationRealm realm)
    {
        // Create minimal test pawn with cultivation comp
        var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
        var comp = pawn.GetComp<CultivationComp>();
        if (comp != null)
        {
            comp.cultivationData.currentRealm = realm;
            comp.cultivationData.currentQi = 1000f; // Max Qi for testing
            comp.cultivationData.RecalculateStats();
        }
        return pawn;
    }
    
    public static void RunAllSkillTests()
    {
        Log.Message("[TuTien Test] Running all skill tests...");
        
        var skillNames = SkillRegistry.GetAllSkillNames();
        int passed = 0;
        int total = skillNames.Count;
        
        foreach (var skillName in skillNames)
        {
            if (TestSkill(skillName))
                passed++;
        }
        
        Log.Message($"[TuTien Test] Results: {passed}/{total} skills passed tests");
    }
}
```

## 📊 Performance Considerations

### Caching Strategy
```csharp
public static class CultivationCache
{
    private static Dictionary<Pawn, CultivationStats> statsCache = new Dictionary<Pawn, CultivationStats>();
    private static Dictionary<string, CultivationSkillWorker> workerCache = new Dictionary<string, CultivationSkillWorker>();
    
    public static CultivationStats GetCachedStats(Pawn pawn)
    {
        if (!statsCache.TryGetValue(pawn, out var stats))
        {
            stats = CultivationStatsManager.Calculate(pawn.GetComp<CultivationComp>().cultivationData);
            statsCache[pawn] = stats;
        }
        return stats;
    }
    
    public static void InvalidateStatsCache(Pawn pawn)
    {
        statsCache.Remove(pawn);
    }
    
    public static void ClearCache()
    {
        statsCache.Clear();
    }
}
```

## 🎯 Migration Timeline

### Week 1: Foundation
1. Create folder structure
2. Implement event system
3. Create SkillRegistry
4. Test auto-discovery

### Week 2: Skills Migration
1. Migrate QiPunch → Test
2. Migrate QiShield → Test
3. Migrate remaining skills
4. Remove old skill methods

### Week 3: Stats System
1. Implement IStatCalculator
2. Create base calculators
3. Test performance
4. Migrate existing stat logic

### Week 4: UI & Testing
1. Create UI components
2. Test modular UI
3. Run full test suite
4. Performance optimization

## ✅ Success Metrics

- [ ] All existing skills work with new system
- [ ] Performance same or better than before
- [ ] Save/load compatibility maintained
- [ ] New skills can be added in <10 lines of code
- [ ] UI is modular and extensible
- [ ] Full test coverage

Với migration plan này, bạn sẽ có một hệ thống hoàn toàn mới mà vẫn giữ được tính tương thích và ổn định! 🚀

Bạn có muốn tôi bắt đầu implement một phần cụ thể nào không?
