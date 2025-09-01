# Tu Tien Mod - Refactoring Implementation Plan

## 🎯 Priority 1: Skill System Refactoring

### Current Issues:
```csharp
// ❌ Current: Hard-coded, difficult to extend
public class QiPunchWorker : CultivationSkillWorker 
{
    public override void ExecuteSkill(Pawn pawn, CultivationSkillDef skill)
    {
        // Hard-coded logic
    }
}
```

### 🔄 **Step 1: Create Skill Registry System**

Create new file: `Source/TuTien/Systems/Skills/SkillRegistry.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace TuTien.Skills
{
    public static class SkillRegistry
    {
        private static Dictionary<string, Type> skillWorkerTypes;
        private static Dictionary<string, CultivationSkillWorker> workerInstances;
        
        static SkillRegistry()
        {
            Initialize();
        }
        
        public static void Initialize()
        {
            Log.Message("[TuTien] Initializing Skill Registry...");
            
            skillWorkerTypes = new Dictionary<string, Type>();
            workerInstances = new Dictionary<string, CultivationSkillWorker>();
            
            // Auto-discover all skill worker types
            var workerTypes = GenTypes.AllTypesWithAttribute<SkillWorkerAttribute>()
                .Where(t => typeof(CultivationSkillWorker).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract);
            
            foreach (var type in workerTypes)
            {
                var attribute = type.GetCustomAttribute<SkillWorkerAttribute>();
                var skillName = attribute?.SkillName ?? type.Name.Replace("Worker", "");
                
                skillWorkerTypes[skillName] = type;
                Log.Message($"[TuTien] Registered skill worker: {skillName} -> {type.Name}");
            }
            
            Log.Message($"[TuTien] Registered {skillWorkerTypes.Count} skill workers");
        }
        
        public static CultivationSkillWorker GetWorker(string skillName)
        {
            if (string.IsNullOrEmpty(skillName))
                return null;
            
            // Return cached instance or create new one
            if (workerInstances.TryGetValue(skillName, out var cachedWorker))
                return cachedWorker;
            
            if (skillWorkerTypes.TryGetValue(skillName, out var workerType))
            {
                try
                {
                    var worker = (CultivationSkillWorker)Activator.CreateInstance(workerType);
                    workerInstances[skillName] = worker;
                    return worker;
                }
                catch (Exception ex)
                {
                    Log.Error($"[TuTien] Failed to create skill worker {skillName}: {ex.Message}");
                }
            }
            
            Log.Warning($"[TuTien] No skill worker found for: {skillName}");
            return null;
        }
        
        public static List<string> GetAllSkillNames()
        {
            return skillWorkerTypes.Keys.ToList();
        }
        
        public static List<CultivationSkillWorker> GetWorkersOfCategory(SkillCategory category)
        {
            return workerInstances.Values
                .Where(w => w.Category == category)
                .ToList();
        }
    }
    
    // Attribute for auto-discovery
    [AttributeUsage(AttributeTargets.Class)]
    public class SkillWorkerAttribute : Attribute
    {
        public string SkillName { get; }
        
        public SkillWorkerAttribute(string skillName)
        {
            SkillName = skillName;
        }
    }
}
```

### 🔄 **Step 2: Enhanced Base Skill Worker**

Update `Source/TuTien/Skills/CultivationSkillWorker.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.Skills
{
    public enum SkillCategory
    {
        Combat,     // Qi Punch, Qi Shield, etc.
        Movement,   // Teleportation, Speed boost
        Healing,    // Regeneration, Cure
        Mental,     // Meditation, Focus
        Utility,    // Item enhancement, Detection
        Ultimate    // Powerful realm-specific skills
    }
    
    public enum SkillTier
    {
        Basic,      // Available from early realms
        Advanced,   // Mid-tier realms
        Master,     // High realms only
        Legendary   // Peak cultivation
    }
    
    public abstract class CultivationSkillWorker
    {
        // Core properties
        public abstract SkillCategory Category { get; }
        public virtual SkillTier Tier => SkillTier.Basic;
        public virtual List<string> Prerequisites => new List<string>();
        public virtual List<CultivationRealm> RequiredRealms => new List<CultivationRealm>();
        
        // Skill properties
        public virtual float BaseQiCost => 10f;
        public virtual float BaseCooldown => 60f;
        public virtual float Range => 0f; // 0 = self-target
        public virtual bool RequiresTarget => false;
        public virtual bool CanTargetSelf => true;
        public virtual bool CanTargetAllies => false;
        public virtual bool CanTargetEnemies => false;
        
        // Visual/UI properties
        public virtual Color SkillColor => Color.white;
        public virtual string IconPath => null;
        public virtual string SoundDefName => null;
        
        // Validation
        public virtual bool CanExecute(Pawn pawn, CultivationSkillDef skill)
        {
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null)
                return false;
            
            // Base validation
            if (!HasEnoughQi(pawn, skill))
                return false;
            
            if (!MeetsRealmRequirement(pawn))
                return false;
            
            if (!MeetsPrerequisites(pawn))
                return false;
            
            if (IsOnCooldown(pawn, skill))
                return false;
            
            // Custom validation
            return CustomValidation(pawn, skill);
        }
        
        protected virtual bool CustomValidation(Pawn pawn, CultivationSkillDef skill)
        {
            return true;
        }
        
        private bool HasEnoughQi(Pawn pawn, CultivationSkillDef skill)
        {
            var comp = pawn.GetComp<CultivationComp>();
            var qiCost = GetQiCost(pawn, skill);
            return comp.cultivationData.currentQi >= qiCost;
        }
        
        private bool MeetsRealmRequirement(Pawn pawn)
        {
            if (!RequiredRealms.Any())
                return true;
            
            var comp = pawn.GetComp<CultivationComp>();
            return RequiredRealms.Contains(comp.cultivationData.currentRealm);
        }
        
        private bool MeetsPrerequisites(Pawn pawn)
        {
            if (!Prerequisites.Any())
                return true;
            
            var comp = pawn.GetComp<CultivationComp>();
            return Prerequisites.All(prereq => comp.cultivationData.HasLearnedSkill(prereq));
        }
        
        private bool IsOnCooldown(Pawn pawn, CultivationSkillDef skill)
        {
            var comp = pawn.GetComp<CultivationComp>();
            return comp.cultivationData.GetSkillCooldown(skill.defName) > 0;
        }
        
        // Cost calculation with modifiers
        public virtual float GetQiCost(Pawn pawn, CultivationSkillDef skill)
        {
            var baseCost = skill.qiCost > 0 ? skill.qiCost : BaseQiCost;
            
            // Apply modifiers based on realm, techniques, etc.
            var comp = pawn.GetComp<CultivationComp>();
            var modifier = GetSkillCostModifier(pawn, comp.cultivationData);
            
            return baseCost * modifier;
        }
        
        protected virtual float GetSkillCostModifier(Pawn pawn, CultivationData data)
        {
            float modifier = 1f;
            
            // Realm-based cost reduction
            modifier *= GetRealmCostModifier(data.currentRealm);
            
            // Technique-based modifiers
            if (data.currentTechnique != null)
            {
                modifier *= GetTechniqueCostModifier(data.currentTechnique);
            }
            
            return Mathf.Max(0.1f, modifier); // Minimum 10% cost
        }
        
        protected virtual float GetRealmCostModifier(CultivationRealm realm)
        {
            return realm switch
            {
                CultivationRealm.QiGathering => 1.0f,
                CultivationRealm.FoundationEstablishment => 0.9f,
                CultivationRealm.CoreFormation => 0.8f,
                CultivationRealm.NascentSoul => 0.7f,
                _ => 1.0f
            };
        }
        
        protected virtual float GetTechniqueCostModifier(CultivationTechnique technique)
        {
            // Override in specific workers
            return 1.0f;
        }
        
        // Main execution method
        public void Execute(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target = default)
        {
            if (!CanExecute(pawn, skill))
                return;
            
            var comp = pawn.GetComp<CultivationComp>();
            
            // Consume Qi
            var qiCost = GetQiCost(pawn, skill);
            comp.cultivationData.currentQi -= qiCost;
            
            // Set cooldown
            var cooldown = GetCooldown(pawn, skill);
            comp.cultivationData.SetSkillCooldown(skill.defName, cooldown);
            
            // Play effects
            PlayEffects(pawn, skill, target);
            
            // Execute skill logic
            ExecuteSkillEffect(pawn, skill, target);
            
            // Post-execution
            OnSkillExecuted(pawn, skill, target);
        }
        
        // Abstract methods for implementation
        protected abstract void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target);
        
        // Virtual methods for customization
        protected virtual float GetCooldown(Pawn pawn, CultivationSkillDef skill)
        {
            return skill.cooldown > 0 ? skill.cooldown : BaseCooldown;
        }
        
        protected virtual void PlayEffects(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target)
        {
            // Play sound
            if (!string.IsNullOrEmpty(SoundDefName))
            {
                var soundDef = DefDatabase<SoundDef>.GetNamedSilentFail(SoundDefName);
                soundDef?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }
            
            // Spawn visual effects
            SpawnVisualEffects(pawn, skill, target);
        }
        
        protected virtual void SpawnVisualEffects(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target)
        {
            // Override in specific skills
        }
        
        protected virtual void OnSkillExecuted(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target)
        {
            // Trigger events
            CultivationEvents.TriggerSkillUsed(pawn, skill);
        }
        
        // Helper methods
        protected void DamageTarget(Pawn caster, LocalTargetInfo target, float damage, DamageDef damageType = null)
        {
            if (target.Thing is Pawn targetPawn)
            {
                damageType = damageType ?? DamageDefOf.Blunt;
                var dinfo = new DamageInfo(damageType, damage, 0f, -1f, caster);
                targetPawn.TakeDamage(dinfo);
            }
        }
        
        protected void HealTarget(Pawn target, float amount)
        {
            if (target.health?.hediffSet?.hediffs == null)
                return;
            
            var injuries = target.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(h => h.CanHealNaturally())
                .OrderByDescending(h => h.Severity);
            
            foreach (var injury in injuries)
            {
                if (amount <= 0)
                    break;
                
                var healAmount = Mathf.Min(amount, injury.Severity);
                injury.Heal(healAmount);
                amount -= healAmount;
            }
        }
        
        protected List<Pawn> GetTargetsInRange(Pawn caster, float range)
        {
            if (range <= 0)
                return new List<Pawn> { caster };
            
            return caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p.Position.DistanceTo(caster.Position) <= range)
                .ToList();
        }
    }
}
```

### 🔄 **Step 3: Refactor Existing Skills**

Update existing skills to use new system:

**QiPunchWorker.cs:**
```csharp
using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.Skills
{
    [SkillWorker("QiPunch")]
    public class QiPunchWorker : CultivationSkillWorker
    {
        public override SkillCategory Category => SkillCategory.Combat;
        public override SkillTier Tier => SkillTier.Basic;
        
        public override float BaseQiCost => 15f;
        public override float BaseCooldown => 30f;
        public override float Range => 3f;
        public override bool RequiresTarget => true;
        public override bool CanTargetEnemies => true;
        
        public override Color SkillColor => Color.red;
        public override string SoundDefName => "TuTien_QiPunch";
        
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target)
        {
            if (target.Thing is Pawn targetPawn)
            {
                var comp = pawn.GetComp<CultivationComp>();
                var damage = GetDamageAmount(comp.cultivationData);
                
                DamageTarget(pawn, target, damage, DamageDefOf.Blunt);
                
                // Knockback effect
                if (targetPawn.Map != null)
                {
                    var direction = (targetPawn.Position - pawn.Position).ToVector3();
                    var knockbackDistance = Mathf.RoundToInt(damage / 10f);
                    
                    for (int i = 0; i < knockbackDistance; i++)
                    {
                        var newPos = targetPawn.Position + direction.ToIntVec3();
                        if (newPos.InBounds(targetPawn.Map) && newPos.Standable(targetPawn.Map))
                        {
                            targetPawn.Position = newPos;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        
        private float GetDamageAmount(CultivationData data)
        {
            float baseDamage = 20f;
            
            // Scale with realm
            float realmMultiplier = data.currentRealm switch
            {
                CultivationRealm.QiGathering => 1.0f,
                CultivationRealm.FoundationEstablishment => 1.5f,
                CultivationRealm.CoreFormation => 2.0f,
                CultivationRealm.NascentSoul => 3.0f,
                _ => 1.0f
            };
            
            // Scale with cultivation stage
            float stageMultiplier = 1f + (data.currentStage * 0.1f);
            
            return baseDamage * realmMultiplier * stageMultiplier;
        }
        
        protected override void SpawnVisualEffects(Pawn pawn, CultivationSkillDef skill, LocalTargetInfo target)
        {
            // Qi energy effect from caster to target
            var line = new FleckCreationData
            {
                def = FleckDefOf.PsycastPsychicEffect,
                spawnPosition = pawn.DrawPos,
                targetPosition = target.Cell.ToVector3(),
                scale = 1.5f,
                rotationRate = 0f,
                velocityAngle = (target.Cell - pawn.Position).AngleFlat,
                velocitySpeed = 10f
            };
            
            pawn.Map.flecks.CreateFleck(line);
            
            // Impact effect at target
            var impact = new FleckCreationData
            {
                def = FleckDefOf.ExplosionFlash,
                spawnPosition = target.Cell.ToVector3(),
                scale = 2f
            };
            
            pawn.Map.flecks.CreateFleck(impact);
        }
    }
}
```

### 🔄 **Step 4: Update CultivationComp**

Modify `CultivationComp.cs` to use new skill system:
```csharp
public void UseSkill(CultivationSkillDef skill, LocalTargetInfo target = default)
{
    var worker = SkillRegistry.GetWorker(skill.defName);
    if (worker == null)
    {
        Log.Warning($"[TuTien] No worker found for skill: {skill.defName}");
        return;
    }
    
    if (!worker.CanExecute(pawn, skill))
    {
        // Show failure message
        Messages.Message(
            $"{pawn.Name} cannot use {skill.LabelCap} right now.",
            MessageTypeDefOf.RejectInput
        );
        return;
    }
    
    worker.Execute(pawn, skill, target);
}
```

## 🎯 Priority 2: Stats System Refactoring

Create modular stats calculation:

### **Create IStatCalculator System**

`Source/TuTien/Systems/Stats/IStatCalculator.cs`:
```csharp
namespace TuTien.Stats
{
    public interface IStatCalculator
    {
        int Priority { get; }
        string CalculatorName { get; }
        bool Applies(CultivationData data);
        void Apply(CultivationData data, CultivationStats stats);
    }
    
    public class CultivationStats
    {
        // Base stats
        public float maxQi = 100f;
        public float qiRegenRate = 1f;
        public float maxTuVi = 1000f;
        public float tuViGainRate = 1f;
        
        // Combat stats
        public float meleeDamageMultiplier = 1f;
        public float rangedDamageMultiplier = 1f;
        public float meleeHitChanceOffset = 0f;
        public float rangedHitChanceOffset = 0f;
        public float incomingDamageMultiplier = 1f;
        
        // Movement stats
        public float moveSpeedMultiplier = 1f;
        public float carryCapacityMultiplier = 1f;
        
        // Work stats
        public float workSpeedMultiplier = 1f;
        public float learningRateMultiplier = 1f;
        
        // Special stats
        public float breakthroughChance = 0.5f;
        public float cultivationSpeed = 1f;
        public float skillCooldownMultiplier = 1f;
        
        public CultivationStats Clone()
        {
            return (CultivationStats)this.MemberwiseClone();
        }
    }
}
```

### **Stats Manager**

`Source/TuTien/Systems/Stats/CultivationStatsManager.cs`:
```csharp
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TuTien.Stats
{
    public static class CultivationStatsManager
    {
        private static List<IStatCalculator> calculators = new List<IStatCalculator>();
        
        static CultivationStatsManager()
        {
            InitializeCalculators();
        }
        
        private static void InitializeCalculators()
        {
            // Auto-register all stat calculators
            var calculatorTypes = GenTypes.AllTypesWithAttribute<StatCalculatorAttribute>();
            
            foreach (var type in calculatorTypes)
            {
                if (typeof(IStatCalculator).IsAssignableFrom(type))
                {
                    try
                    {
                        var calculator = (IStatCalculator)Activator.CreateInstance(type);
                        RegisterCalculator(calculator);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[TuTien] Failed to create stat calculator {type.Name}: {ex.Message}");
                    }
                }
            }
            
            // Sort by priority
            calculators = calculators.OrderBy(c => c.Priority).ToList();
            
            Log.Message($"[TuTien] Registered {calculators.Count} stat calculators");
        }
        
        public static void RegisterCalculator(IStatCalculator calculator)
        {
            calculators.Add(calculator);
            Log.Message($"[TuTien] Registered stat calculator: {calculator.CalculatorName}");
        }
        
        public static CultivationStats Calculate(CultivationData data)
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
        
        public static void RecalculateStats(CultivationComp comp)
        {
            comp.cultivationData.calculatedStats = Calculate(comp.cultivationData);
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class StatCalculatorAttribute : Attribute
    {
        public StatCalculatorAttribute() { }
    }
}
```

Với refactoring này, bạn sẽ có một hệ thống:
- ✅ **Dễ mở rộng**: Chỉ cần tạo class mới với attribute
- ✅ **Modular**: Mỗi calculator độc lập
- ✅ **Maintainable**: Code rõ ràng, dễ debug
- ✅ **Scalable**: Hỗ trợ hàng trăm skills/stats

Bạn có muốn tôi tiếp tục với UI system refactoring hoặc technique system không?
