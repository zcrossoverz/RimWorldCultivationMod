# ⚙️ Core System Framework

## 📖 **Table of Contents**

1. [Core Architecture](#core-architecture)
2. [Component System](#component-system)
3. [Data Management](#data-management)
4. [Event System](#event-system)
5. [Performance Framework](#performance-framework)
6. [Extension Points](#extension-points)

---

## 🏗️ **Core Architecture**

The Tu Tiên core system provides the foundational framework for cultivation mechanics, built on RimWorld's component architecture with extensive caching and performance optimizations.

```
Core System Architecture:
┌─────────────────────────────────────────────────────────────────────────────┐
│                            CORE SYSTEM LAYERS                               │
├──────────────────┬──────────────────┬──────────────────┬─────────────────────┤
│  DEFINITION      │   COMPONENT      │    CACHING       │    INTEGRATION      │
│     LAYER        │     LAYER        │     LAYER        │       LAYER         │
│                  │                  │                  │                     │
│ ┌──────────────┐ │ ┌──────────────┐ │ ┌──────────────┐ │ ┌─────────────────┐ │
│ │ XML Defs     │ │ │ Cultivation  │ │ │ DefDatabase  │ │ │ Harmony         │ │
│ │              │ │ │ Comp         │ │ │ Cache        │ │ │ Patches         │ │
│ │ • SkillDef   │ │ │              │ │ │              │ │ │                 │ │
│ │ • AbilityDef │◄┼►│ • Data       │◄┼►│ • Fast Lookup│◄┼►│ • UI Hook       │ │
│ │ • ArtifactDef│ │ │ • State      │ │ │ • Type Cache │ │ │ • Save/Load     │ │
│ │ • RealmDef   │ │ │ • Methods    │ │ │ • Index Maps │ │ │ • Events        │ │
│ └──────────────┘ │ └──────────────┘ │ └──────────────┘ │ └─────────────────┘ │
└──────────────────┴──────────────────┴──────────────────┴─────────────────────┘
                                       │
                                       ▼
              ┌─────────────────────────────────────────────┐
              │            EVENT DISTRIBUTION               │
              │                                             │
              │ ┌─────────────┐ ┌─────────────┐ ┌────────┐ │
              │ │ Skill       │ │ Artifact    │ │ UI     │ │
              │ │ Events      │ │ Events      │ │ Events │ │
              │ │             │ │             │ │        │ │
              │ │ • Learning  │ │ • Equip     │ │ • Show │ │
              │ │ • Usage     │ │ • Unequip   │ │ • Hide │ │
              │ │ • Mastery   │ │ • Activate  │ │ • Tick │ │
              │ └─────────────┘ └─────────────┘ └────────┘ │
              └─────────────────────────────────────────────┘
```

---

## 🧩 **Component System**

### **CultivationComp - The Core Component**

```csharp
/// <summary>
/// Main cultivation component attached to pawns
/// Manages all cultivation-related data and functionality
/// </summary>
public class CultivationComp : ThingComp
{
    #region Core Data
    public CultivationData cultivationData;
    public HashSet<string> knownSkills = new HashSet<string>();
    public Dictionary<string, int> skillCooldowns = new Dictionary<string, int>();
    public Dictionary<string, SkillMasteryData> skillMasteryData = new Dictionary<string, SkillMasteryData>();
    
    // Performance optimization
    private List<string> cachedAvailableSkills;
    private int lastCacheUpdate = -1;
    private bool isDirty = true;
    #endregion
    
    #region Initialization
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        
        if (cultivationData == null)
        {
            InitializeCultivationData();
        }
        
        // Register for events
        CultivationEventManager.RegisterComponent(this);
    }
    
    private void InitializeCultivationData()
    {
        cultivationData = new CultivationData
        {
            currentRealm = CultivationRealm.QiCondensation,
            currentStage = 1,
            currentQi = 100f,
            maxQi = 100f,
            cultivationPoints = 0f,
            lastCultivationTime = Find.TickManager.TicksGame
        };
        
        // Grant basic skills based on race/background
        GrantBasicSkills();
        
        Log.Message($"Initialized cultivation for {parent.LabelShort}");
    }
    
    private void GrantBasicSkills()
    {
        // Every cultivator starts with basic qi manipulation
        knownSkills.Add("QiSense");
        knownSkills.Add("BasicCultivation");
        
        // Check for special backgrounds
        var pawn = parent as Pawn;
        if (pawn?.story?.traits?.HasTrait(TraitDefOf.Psychopath) == true)
        {
            knownSkills.Add("QiPunch"); // Violent types get offensive skills
        }
    }
    #endregion
    
    #region Skill Management
    public void LearnSkill(string skillDefName, bool silent = false)
    {
        if (knownSkills.Contains(skillDefName)) return;
        
        // Validate prerequisites
        if (!CanLearnSkill(skillDefName, out string reason))
        {
            if (!silent)
            {
                Messages.Message($"{parent.LabelShort} cannot learn {skillDefName}: {reason}", 
                               MessageTypeDefOf.RejectInput);
            }
            return;
        }
        
        // Add skill
        knownSkills.Add(skillDefName);
        
        // Initialize mastery data
        skillMasteryData[skillDefName] = new SkillMasteryData();
        
        // Invalidate cache
        InvalidateSkillCache();
        
        // Notify systems
        CultivationEventManager.OnSkillLearned(parent as Pawn, skillDefName);
        
        if (!silent)
        {
            Messages.Message($"{parent.LabelShort} learned {skillDefName}!", 
                           MessageTypeDefOf.PositiveEvent);
        }
    }
    
    public bool CanLearnSkill(string skillDefName, out string reason)
    {
        reason = "";
        
        var skillDef = CultivationCache.GetSkillDef(skillDefName);
        var abilityDef = CultivationCache.GetAbilityDef(skillDefName);
        
        if (skillDef == null && abilityDef == null)
        {
            reason = "Skill definition not found";
            return false;
        }
        
        // Check realm requirements
        var requiredRealm = skillDef?.requiredRealm ?? abilityDef?.requiredRealm ?? CultivationRealm.QiCondensation;
        if (cultivationData.currentRealm < requiredRealm)
        {
            reason = $"Requires {requiredRealm} realm";
            return false;
        }
        
        // Check stage requirements
        var requiredStage = skillDef?.requiredStage ?? abilityDef?.requiredStage ?? 1;
        if (cultivationData.currentRealm == requiredRealm && cultivationData.currentStage < requiredStage)
        {
            reason = $"Requires stage {requiredStage}";
            return false;
        }
        
        // Check prerequisites
        var prerequisites = skillDef?.prerequisites ?? abilityDef?.prerequisites;
        if (prerequisites?.Count > 0)
        {
            foreach (var prereq in prerequisites)
            {
                if (!knownSkills.Contains(prereq))
                {
                    reason = $"Requires {prereq}";
                    return false;
                }
            }
        }
        
        return true;
    }
    
    public List<string> GetAllAvailableSkills()
    {
        // Use cached result if still valid
        var currentTick = Find.TickManager.TicksGame;
        if (!isDirty && cachedAvailableSkills != null && 
            currentTick - lastCacheUpdate < 60) // Cache for 1 second
        {
            return cachedAvailableSkills;
        }
        
        var skills = new List<string>();
        
        // 1. Known skills
        skills.AddRange(knownSkills);
        
        // 2. Artifact-granted skills
        var pawn = parent as Pawn;
        if (pawn?.equipment?.AllEquipmentListForReading != null)
        {
            foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
            {
                var artifactComp = equipment.GetComp<CultivationArtifactComp>();
                if (artifactComp?.def?.autoSkills != null)
                {
                    skills.AddRange(artifactComp.def.autoSkills);
                }
            }
        }
        
        // Remove duplicates and update cache
        cachedAvailableSkills = skills.Distinct().ToList();
        lastCacheUpdate = currentTick;
        isDirty = false;
        
        return cachedAvailableSkills;
    }
    
    public void InvalidateSkillCache()
    {
        isDirty = true;
        cachedAvailableSkills = null;
    }
    #endregion
    
    #region Qi Management
    public bool ConsumeQi(float amount, bool force = false)
    {
        if (!force && cultivationData.currentQi < amount)
            return false;
        
        cultivationData.currentQi = Mathf.Max(0f, cultivationData.currentQi - amount);
        
        // Trigger qi-related events
        CultivationEventManager.OnQiConsumed(parent as Pawn, amount);
        
        return true;
    }
    
    public void RestoreQi(float amount)
    {
        var oldQi = cultivationData.currentQi;
        cultivationData.currentQi = Mathf.Min(cultivationData.maxQi, cultivationData.currentQi + amount);
        
        var actualRestored = cultivationData.currentQi - oldQi;
        if (actualRestored > 0f)
        {
            CultivationEventManager.OnQiRestored(parent as Pawn, actualRestored);
        }
    }
    
    public void RegenerateQi()
    {
        var regenRate = GetQiRegenRate();
        RestoreQi(regenRate);
    }
    
    private float GetQiRegenRate()
    {
        // Base regen rate depends on realm and stage
        float baseRate = (int)cultivationData.currentRealm * 0.5f + cultivationData.currentStage * 0.1f;
        
        // Apply meditation bonus
        var pawn = parent as Pawn;
        if (pawn?.jobs?.curDriver is JobDriver_Meditate)
        {
            baseRate *= 3f; // 3x regen while meditating
        }
        
        // Apply artifact bonuses
        var artifactBonus = GetArtifactQiRegenBonus(pawn);
        baseRate *= artifactBonus;
        
        return baseRate;
    }
    
    private float GetArtifactQiRegenBonus(Pawn pawn)
    {
        if (pawn?.equipment?.AllEquipmentListForReading == null) return 1f;
        
        float bonus = 1f;
        foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
        {
            var artifactComp = equipment.GetComp<CultivationArtifactComp>();
            if (artifactComp?.def?.qiRegenMultiplier != null)
            {
                bonus *= artifactComp.def.qiRegenMultiplier.Value;
            }
        }
        
        return bonus;
    }
    #endregion
    
    #region Ticking and Updates
    public override void CompTick()
    {
        base.CompTick();
        
        // Qi regeneration (every 60 ticks = 1 second)
        if (Find.TickManager.TicksGame % 60 == 0)
        {
            RegenerateQi();
        }
        
        // Cooldown reduction
        ReduceCooldowns();
        
        // Cultivation progress (every 5 seconds)
        if (Find.TickManager.TicksGame % 300 == 0)
        {
            ProcessCultivationProgress();
        }
    }
    
    private void ReduceCooldowns()
    {
        var keysToUpdate = skillCooldowns.Keys.ToList();
        foreach (var skill in keysToUpdate)
        {
            if (skillCooldowns[skill] > 0)
            {
                skillCooldowns[skill]--;
                if (skillCooldowns[skill] <= 0)
                {
                    // Cooldown finished
                    CultivationEventManager.OnSkillCooldownFinished(parent as Pawn, skill);
                }
            }
        }
    }
    
    private void ProcessCultivationProgress()
    {
        var pawn = parent as Pawn;
        if (pawn == null) return;
        
        // Natural cultivation progress
        var progressGain = CalculateNaturalProgress(pawn);
        if (progressGain > 0f)
        {
            AddCultivationProgress(progressGain);
        }
        
        // Check for breakthrough opportunities
        CheckForBreakthrough();
    }
    
    private float CalculateNaturalProgress(Pawn pawn)
    {
        float baseProgress = 0.1f; // Base cultivation rate
        
        // Environmental factors
        var room = pawn.GetRoom();
        if (room?.GetStat(StatDefOf.Beauty) > 10f)
        {
            baseProgress *= 1.2f; // Beautiful environments help cultivation
        }
        
        // Mood factor
        var mood = pawn.needs?.mood?.CurLevel ?? 0.5f;
        baseProgress *= (0.5f + mood); // Better mood = better cultivation
        
        // Time of day factor
        var hour = GenLocalDate.HourOfDay(pawn);
        if (hour >= 3 && hour <= 6) // Dawn hours are best for cultivation
        {
            baseProgress *= 1.5f;
        }
        
        return baseProgress;
    }
    #endregion
    
    #region Breakthrough System
    public void CheckForBreakthrough()
    {
        if (!CanAttemptBreakthrough()) return;
        
        var success = RollBreakthroughAttempt();
        
        if (success)
        {
            ExecuteBreakthrough();
        }
        else
        {
            // Failed breakthrough penalties
            cultivationData.currentQi *= 0.8f; // Lose 20% qi
            Messages.Message($"{parent.LabelShort} failed breakthrough attempt!", 
                           MessageTypeDefOf.NegativeEvent);
        }
    }
    
    private bool CanAttemptBreakthrough()
    {
        var requiredPoints = cultivationData.GetRequiredPoints();
        return cultivationData.cultivationPoints >= requiredPoints;
    }
    
    private bool RollBreakthroughAttempt()
    {
        var pawn = parent as Pawn;
        
        // Base success chance
        float successChance = 0.6f;
        
        // Skill bonuses
        successChance += pawn.skills.GetSkill(SkillDefOf.Intellectual).Level * 0.01f;
        successChance += pawn.skills.GetSkill(SkillDefOf.Medicine).Level * 0.005f;
        
        // Trait bonuses
        if (pawn.story?.traits?.HasTrait(TraitDefOf.Ascetic) == true)
            successChance += 0.15f;
        
        // Artifact bonuses
        var artifactBonus = GetArtifactBreakthroughBonus(pawn);
        successChance += artifactBonus;
        
        // Random roll
        return Rand.Chance(successChance);
    }
    
    private void ExecuteBreakthrough()
    {
        var pawn = parent as Pawn;
        var oldRealm = cultivationData.currentRealm;
        var oldStage = cultivationData.currentStage;
        
        // Advance stage/realm
        if (cultivationData.currentStage < 9)
        {
            cultivationData.currentStage++;
        }
        else if (cultivationData.currentRealm < CultivationRealm.Immortal)
        {
            cultivationData.currentRealm = (CultivationRealm)((int)cultivationData.currentRealm + 1);
            cultivationData.currentStage = 1;
        }
        
        // Reset progress
        cultivationData.cultivationPoints = 0f;
        
        // Increase max qi
        var qiIncrease = GetQiIncreaseForAdvancement();
        cultivationData.maxQi += qiIncrease;
        cultivationData.currentQi = cultivationData.maxQi; // Full qi on breakthrough
        
        // Grant new skills
        GrantSkillsForRealm(cultivationData.currentRealm, cultivationData.currentStage);
        
        // Visual effects
        CultivationVFX.PlayEffect("breakthrough", pawn.DrawPos, pawn.Map, 2f);
        
        // Notification
        Messages.Message($"{pawn.LabelShort} broke through to {cultivationData.currentRealm} Stage {cultivationData.currentStage}!", 
                        MessageTypeDefOf.PositiveEvent);
        
        // Event notification
        CultivationEventManager.OnBreakthrough(pawn, oldRealm, oldStage, 
                                             cultivationData.currentRealm, cultivationData.currentStage);
    }
    
    private float GetQiIncreaseForAdvancement()
    {
        // Qi increase scales with realm
        return (int)cultivationData.currentRealm * 25f + cultivationData.currentStage * 5f;
    }
    
    private void GrantSkillsForRealm(CultivationRealm realm, int stage)
    {
        // Find skills that should be unlocked at this realm/stage
        var availableSkills = DefDatabase<CultivationSkillDef>.AllDefs
            .Where(s => s.requiredRealm == realm && s.requiredStage <= stage)
            .Where(s => s.autoGrant) // Only auto-granted skills
            .Select(s => s.defName);
            
        var availableAbilities = DefDatabase<CultivationAbilityDef>.AllDefs
            .Where(a => a.requiredRealm == realm && a.requiredStage <= stage)
            .Where(a => a.autoGrant) // Only auto-granted abilities
            .Select(a => a.defName);
        
        foreach (var skill in availableSkills.Concat(availableAbilities))
        {
            LearnSkill(skill, true);
        }
    }
    #endregion
    
    #region Save/Load
    public override void PostExposeData()
    {
        base.PostExposeData();
        
        Scribe_Deep.Look(ref cultivationData, "cultivationData");
        Scribe_Collections.Look(ref knownSkills, "knownSkills", LookMode.Value);
        Scribe_Collections.Look(ref skillCooldowns, "skillCooldowns", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref skillMasteryData, "skillMasteryData", LookMode.Value, LookMode.Deep);
        
        // Reinitialize if needed
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (cultivationData == null)
            {
                InitializeCultivationData();
            }
            
            knownSkills ??= new HashSet<string>();
            skillCooldowns ??= new Dictionary<string, int>();
            skillMasteryData ??= new Dictionary<string, SkillMasteryData>();
            
            InvalidateSkillCache();
        }
    }
    #endregion
}
```

### **Component Properties Interface**

```csharp
/// <summary>
/// Properties interface for cultivation component configuration
/// </summary>
public class CultivationCompProperties : CompProperties
{
    public bool enableNaturalCultivation = true;
    public float cultivationRateMultiplier = 1f;
    public List<string> startingSkills = new List<string>();
    public CultivationRealm startingRealm = CultivationRealm.QiCondensation;
    public int startingStage = 1;
    public float startingQi = 100f;
    
    public CultivationCompProperties()
    {
        compClass = typeof(CultivationComp);
    }
    
    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        
        // Validate starting skills exist
        startingSkills.RemoveAll(skill => 
            CultivationCache.GetSkillDef(skill) == null && 
            CultivationCache.GetAbilityDef(skill) == null);
    }
}
```

---

## 💾 **Data Management**

### **Core Data Structures**

```csharp
/// <summary>
/// Primary cultivation data container
/// </summary>
[System.Serializable]
public class CultivationData : IExposable
{
    #region Core Fields
    public CultivationRealm currentRealm = CultivationRealm.QiCondensation;
    public int currentStage = 1;
    public float currentQi = 100f;
    public float maxQi = 100f;
    public float cultivationPoints = 0f;
    public int lastCultivationTime = 0;
    
    // Advanced data
    public QiType primaryElement = QiType.Neutral;
    public QiType secondaryElement = QiType.None;
    public float elementAffinityLevel = 1f;
    public CultivationPath chosenPath = CultivationPath.Balanced;
    
    // Experience tracking
    public Dictionary<string, float> elementalExperience = new Dictionary<string, float>();
    public Dictionary<string, int> skillUsageCount = new Dictionary<string, int>();
    public long totalCultivationTime = 0L;
    #endregion
    
    #region Calculated Properties
    public float GetRequiredPoints()
    {
        // Exponential growth for advancement
        float baseRequirement = 100f;
        float realmMultiplier = Mathf.Pow(2f, (int)currentRealm);
        float stageMultiplier = currentStage * 1.5f;
        
        return baseRequirement * realmMultiplier * stageMultiplier;
    }
    
    public float GetCultivationProgress()
    {
        return Mathf.Clamp01(cultivationPoints / GetRequiredPoints());
    }
    
    public float GetQiPercent()
    {
        return maxQi > 0f ? (currentQi / maxQi) : 0f;
    }
    
    public int GetTotalRank()
    {
        return (int)currentRealm * 10 + currentStage;
    }
    
    public string GetDisplayString()
    {
        return $"{currentRealm} Stage {currentStage}";
    }
    #endregion
    
    #region Elemental System
    public void GainElementalExperience(QiType element, float amount)
    {
        if (!elementalExperience.ContainsKey(element.ToString()))
        {
            elementalExperience[element.ToString()] = 0f;
        }
        
        elementalExperience[element.ToString()] += amount;
        
        // Check for element mastery milestones
        CheckElementMastery(element);
    }
    
    private void CheckElementMastery(QiType element)
    {
        var experience = elementalExperience.GetValueOrDefault(element.ToString(), 0f);
        
        // Milestones at 100, 500, 1000 experience
        if (experience >= 1000f && primaryElement == QiType.Neutral)
        {
            primaryElement = element;
            // Grant element mastery benefits
        }
        else if (experience >= 500f && secondaryElement == QiType.None && element != primaryElement)
        {
            secondaryElement = element;
            // Grant secondary element benefits
        }
    }
    
    public float GetElementMasteryLevel(QiType element)
    {
        var experience = elementalExperience.GetValueOrDefault(element.ToString(), 0f);
        return Mathf.Clamp01(experience / 1000f); // Max mastery at 1000 exp
    }
    #endregion
    
    #region Save/Load
    public void ExposeData()
    {
        Scribe_Values.Look(ref currentRealm, "currentRealm", CultivationRealm.QiCondensation);
        Scribe_Values.Look(ref currentStage, "currentStage", 1);
        Scribe_Values.Look(ref currentQi, "currentQi", 100f);
        Scribe_Values.Look(ref maxQi, "maxQi", 100f);
        Scribe_Values.Look(ref cultivationPoints, "cultivationPoints", 0f);
        Scribe_Values.Look(ref lastCultivationTime, "lastCultivationTime", 0);
        
        Scribe_Values.Look(ref primaryElement, "primaryElement", QiType.Neutral);
        Scribe_Values.Look(ref secondaryElement, "secondaryElement", QiType.None);
        Scribe_Values.Look(ref elementAffinityLevel, "elementAffinityLevel", 1f);
        Scribe_Values.Look(ref chosenPath, "chosenPath", CultivationPath.Balanced);
        
        Scribe_Collections.Look(ref elementalExperience, "elementalExperience", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref skillUsageCount, "skillUsageCount", LookMode.Value, LookMode.Value);
        Scribe_Values.Look(ref totalCultivationTime, "totalCultivationTime", 0L);
        
        // Initialize collections if null after loading
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            elementalExperience ??= new Dictionary<string, float>();
            skillUsageCount ??= new Dictionary<string, int>();
        }
    }
    #endregion
}

/// <summary>
/// Skill mastery tracking data
/// </summary>
[System.Serializable]
public class SkillMasteryData : IExposable
{
    public int usageCount = 0;
    public int successCount = 0;
    public float masteryLevel = 0f;
    public int lastUsedTick = 0;
    public float totalDamageDealt = 0f;
    public float totalHealingDone = 0f;
    
    public float SuccessRate => usageCount > 0 ? (float)successCount / usageCount : 0f;
    public MasteryRank MasteryRank => GetMasteryRank();
    
    private MasteryRank GetMasteryRank()
    {
        return masteryLevel switch
        {
            < 0.2f => MasteryRank.Novice,
            < 0.4f => MasteryRank.Apprentice,
            < 0.6f => MasteryRank.Adept,
            < 0.8f => MasteryRank.Expert,
            < 0.95f => MasteryRank.Master,
            _ => MasteryRank.Grandmaster
        };
    }
    
    public float GetMasteryBonus()
    {
        // Mastery provides power and efficiency bonuses
        return 1f + masteryLevel * 0.5f; // Up to +50% bonus at max mastery
    }
    
    public void RecordUsage(bool success, float damage = 0f, float healing = 0f)
    {
        usageCount++;
        if (success) successCount++;
        
        totalDamageDealt += damage;
        totalHealingDone += healing;
        lastUsedTick = Find.TickManager.TicksGame;
        
        // Increase mastery based on success and usage
        var masteryGain = success ? 0.01f : 0.005f; // Less gain for failures
        
        // Diminishing returns
        var currentMasteryPenalty = masteryLevel * 0.5f;
        masteryGain *= (1f - currentMasteryPenalty);
        
        masteryLevel = Mathf.Clamp01(masteryLevel + masteryGain);
    }
    
    public void ExposeData()
    {
        Scribe_Values.Look(ref usageCount, "usageCount", 0);
        Scribe_Values.Look(ref successCount, "successCount", 0);
        Scribe_Values.Look(ref masteryLevel, "masteryLevel", 0f);
        Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", 0);
        Scribe_Values.Look(ref totalDamageDealt, "totalDamageDealt", 0f);
        Scribe_Values.Look(ref totalHealingDone, "totalHealingDone", 0f);
    }
}

public enum MasteryRank
{
    Novice,
    Apprentice,
    Adept,
    Expert,
    Master,
    Grandmaster
}

public enum CultivationPath
{
    Balanced,    // Equal focus on all aspects
    Combat,      // Focus on fighting abilities
    Support,     // Focus on healing/utility
    Elemental,   // Focus on elemental mastery
    Body,        // Focus on physical enhancement
    Soul         // Focus on spiritual development
}
```

---

## ⚡ **Event System**

### **Centralized Event Management**

```csharp
/// <summary>
/// Centralized event management for cultivation system
/// </summary>
public static class CultivationEventManager
{
    #region Event Delegates
    public static event Action<Pawn, string> OnSkillLearned;
    public static event Action<Pawn, string, bool> OnSkillUsed;
    public static event Action<Pawn, string> OnSkillCooldownFinished;
    public static event Action<Pawn, float> OnQiConsumed;
    public static event Action<Pawn, float> OnQiRestored;
    public static event Action<Pawn, CultivationRealm, int, CultivationRealm, int> OnBreakthrough;
    public static event Action<Pawn, ThingWithComps> OnArtifactEquipped;
    public static event Action<Pawn, ThingWithComps> OnArtifactUnequipped;
    #endregion
    
    #region Component Registry
    private static readonly HashSet<CultivationComp> registeredComponents = 
        new HashSet<CultivationComp>();
    
    public static void RegisterComponent(CultivationComp comp)
    {
        registeredComponents.Add(comp);
    }
    
    public static void UnregisterComponent(CultivationComp comp)
    {
        registeredComponents.Remove(comp);
    }
    #endregion
    
    #region Event Triggers
    public static void TriggerSkillLearned(Pawn pawn, string skillName)
    {
        OnSkillLearned?.Invoke(pawn, skillName);
        
        // Additional system notifications
        UpdateAchievements(pawn, "skill_learned", skillName);
        LogCultivationEvent(pawn, $"Learned skill: {skillName}");
    }
    
    public static void TriggerSkillUsed(Pawn pawn, string skillName, bool success)
    {
        OnSkillUsed?.Invoke(pawn, skillName, success);
        
        // Update mastery
        var comp = pawn.GetComp<CultivationComp>();
        if (comp?.skillMasteryData?.TryGetValue(skillName, out var mastery) == true)
        {
            mastery.RecordUsage(success);
        }
        
        UpdateAchievements(pawn, "skill_used", skillName);
        LogCultivationEvent(pawn, $"Used skill: {skillName} (Success: {success})");
    }
    
    public static void TriggerBreakthrough(Pawn pawn, CultivationRealm oldRealm, int oldStage, 
                                         CultivationRealm newRealm, int newStage)
    {
        OnBreakthrough?.Invoke(pawn, oldRealm, oldStage, newRealm, newStage);
        
        // Major milestone
        UpdateAchievements(pawn, "breakthrough", $"{newRealm}_{newStage}");
        LogCultivationEvent(pawn, $"Breakthrough: {oldRealm} S{oldStage} → {newRealm} S{newStage}");
        
        // Notify other systems
        NotifyBreakthroughEffects(pawn, newRealm, newStage);
    }
    
    private static void NotifyBreakthroughEffects(Pawn pawn, CultivationRealm realm, int stage)
    {
        // Physical changes based on realm
        switch (realm)
        {
            case CultivationRealm.FoundationBuilding:
                // Enhanced physical capabilities
                ApplyRealmBonus(pawn, "foundation_building");
                break;
                
            case CultivationRealm.CoreFormation:
                // Spiritual awareness increases
                ApplyRealmBonus(pawn, "core_formation");
                break;
                
            case CultivationRealm.NascentSoul:
                // Soul-based abilities unlock
                ApplyRealmBonus(pawn, "nascent_soul");
                break;
        }
    }
    
    private static void ApplyRealmBonus(Pawn pawn, string bonusType)
    {
        // Apply realm-specific bonuses
        // This could modify stats, grant traits, or unlock content
        switch (bonusType)
        {
            case "foundation_building":
                // +10% manipulation, +5% movement
                break;
                
            case "core_formation":
                // +15% mental capacity
                break;
                
            case "nascent_soul":
                // Soul-based resistances
                break;
        }
    }
    #endregion
    
    #region Achievement System
    private static readonly Dictionary<Pawn, Dictionary<string, AchievementProgress>> 
        achievementProgress = new Dictionary<Pawn, Dictionary<string, AchievementProgress>>();
    
    private static void UpdateAchievements(Pawn pawn, string category, string detail)
    {
        if (!achievementProgress.ContainsKey(pawn))
        {
            achievementProgress[pawn] = new Dictionary<string, AchievementProgress>();
        }
        
        var pawnAchievements = achievementProgress[pawn];
        
        // Check different achievement types
        CheckSkillAchievements(pawn, pawnAchievements, category, detail);
        CheckProgressAchievements(pawn, pawnAchievements, category, detail);
        CheckMasteryAchievements(pawn, pawnAchievements, category, detail);
    }
    
    private static void CheckSkillAchievements(Pawn pawn, Dictionary<string, AchievementProgress> achievements, 
                                            string category, string detail)
    {
        if (category != "skill_learned") return;
        
        var comp = pawn.GetComp<CultivationComp>();
        var skillCount = comp.knownSkills.Count;
        
        // Skill collection achievements
        if (skillCount >= 5 && !achievements.ContainsKey("skill_collector"))
        {
            GrantAchievement(pawn, "skill_collector", "Learned 5 skills");
        }
        
        if (skillCount >= 10 && !achievements.ContainsKey("skill_master"))
        {
            GrantAchievement(pawn, "skill_master", "Learned 10 skills");
        }
    }
    
    private static void GrantAchievement(Pawn pawn, string achievementId, string description)
    {
        if (!achievementProgress.ContainsKey(pawn))
        {
            achievementProgress[pawn] = new Dictionary<string, AchievementProgress>();
        }
        
        achievementProgress[pawn][achievementId] = new AchievementProgress
        {
            unlocked = true,
            unlockedAt = Find.TickManager.TicksGame,
            description = description
        };
        
        // Notification
        Messages.Message($"{pawn.LabelShort} unlocked achievement: {description}!", 
                        MessageTypeDefOf.PositiveEvent);
    }
    
    [System.Serializable]
    public class AchievementProgress
    {
        public bool unlocked = false;
        public int unlockedAt = 0;
        public string description = "";
        public Dictionary<string, object> metadata = new Dictionary<string, object>();
    }
    #endregion
    
    #region Logging System
    private static readonly List<CultivationLogEntry> eventLog = new List<CultivationLogEntry>();
    private const int MaxLogEntries = 1000;
    
    private static void LogCultivationEvent(Pawn pawn, string message)
    {
        var entry = new CultivationLogEntry
        {
            pawn = pawn,
            message = message,
            timestamp = Find.TickManager.TicksGame,
            gameTime = Find.TickManager.TicksToSeconds(Find.TickManager.TicksGame)
        };
        
        eventLog.Add(entry);
        
        // Trim log if too large
        while (eventLog.Count > MaxLogEntries)
        {
            eventLog.RemoveAt(0);
        }
        
        // Debug output
        if (Prefs.DevMode)
        {
            Log.Message($"[Cultivation] {pawn.LabelShort}: {message}");
        }
    }
    
    public static IEnumerable<CultivationLogEntry> GetEventLog(Pawn pawn = null)
    {
        if (pawn == null) return eventLog;
        return eventLog.Where(e => e.pawn == pawn);
    }
    
    [System.Serializable]
    public class CultivationLogEntry
    {
        public Pawn pawn;
        public string message;
        public int timestamp;
        public float gameTime;
        
        public string GetFormattedTime()
        {
            var quadrum = GenDate.Quadrum(timestamp, 0f);
            var day = GenDate.DayOfQuadrum(timestamp, 0f);
            var year = GenDate.Year(timestamp, 0f);
            
            return $"{day} {quadrum} {year}";
        }
    }
    #endregion
}
```

---

## 🔌 **Extension Points**

### **Plugin Architecture**

```csharp
/// <summary>
/// Plugin system for extending cultivation functionality
/// </summary>
public static class CultivationPluginManager
{
    private static readonly List<ICultivationPlugin> loadedPlugins = new List<ICultivationPlugin>();
    
    static CultivationPluginManager()
    {
        LoadAllPlugins();
    }
    
    private static void LoadAllPlugins()
    {
        // Find all types implementing ICultivationPlugin
        var pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ICultivationPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        
        foreach (var pluginType in pluginTypes)
        {
            try
            {
                var plugin = (ICultivationPlugin)Activator.CreateInstance(pluginType);
                loadedPlugins.Add(plugin);
                
                plugin.Initialize();
                Log.Message($"[Cultivation] Loaded plugin: {plugin.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"[Cultivation] Failed to load plugin {pluginType.Name}: {ex.Message}");
            }
        }
    }
    
    #region Plugin Event Distribution
    public static void NotifySkillExecution(Pawn pawn, string skillName, bool success)
    {
        foreach (var plugin in loadedPlugins)
        {
            try
            {
                plugin.OnSkillExecuted(pawn, skillName, success);
            }
            catch (Exception ex)
            {
                Log.Error($"[Cultivation] Plugin {plugin.Name} error in OnSkillExecuted: {ex.Message}");
            }
        }
    }
    
    public static void NotifyArtifactEquip(Pawn pawn, ThingWithComps artifact)
    {
        foreach (var plugin in loadedPlugins)
        {
            try
            {
                plugin.OnArtifactEquipped(pawn, artifact);
            }
            catch (Exception ex)
            {
                Log.Error($"[Cultivation] Plugin {plugin.Name} error in OnArtifactEquipped: {ex.Message}");
            }
        }
    }
    
    public static float ModifySkillPower(Pawn pawn, string skillName, float basePower)
    {
        float modifiedPower = basePower;
        
        foreach (var plugin in loadedPlugins)
        {
            try
            {
                modifiedPower = plugin.ModifySkillPower(pawn, skillName, modifiedPower);
            }
            catch (Exception ex)
            {
                Log.Error($"[Cultivation] Plugin {plugin.Name} error in ModifySkillPower: {ex.Message}");
            }
        }
        
        return modifiedPower;
    }
    #endregion
}

/// <summary>
/// Interface for cultivation system plugins
/// </summary>
public interface ICultivationPlugin
{
    string Name { get; }
    string Version { get; }
    string Author { get; }
    
    void Initialize();
    void OnSkillExecuted(Pawn pawn, string skillName, bool success);
    void OnArtifactEquipped(Pawn pawn, ThingWithComps artifact);
    void OnBreakthrough(Pawn pawn, CultivationRealm newRealm, int newStage);
    
    float ModifySkillPower(Pawn pawn, string skillName, float basePower);
    float ModifyQiConsumption(Pawn pawn, string skillName, float baseCost);
    bool ShouldBlockSkillExecution(Pawn pawn, string skillName);
}

/// <summary>
/// Example plugin implementation
/// </summary>
public class ElementalAffinityPlugin : ICultivationPlugin
{
    public string Name => "Elemental Affinity Enhancer";
    public string Version => "1.0.0";
    public string Author => "Tu Tiên Development Team";
    
    public void Initialize()
    {
        Log.Message("[Cultivation] Elemental Affinity Plugin initialized");
    }
    
    public void OnSkillExecuted(Pawn pawn, string skillName, bool success)
    {
        // Track elemental skill usage for affinity bonuses
        var skillDef = CultivationCache.GetSkillDef(skillName);
        if (skillDef?.associatedElement != null && skillDef.associatedElement != QiType.Neutral)
        {
            var comp = pawn.GetComp<CultivationComp>();
            comp?.cultivationData?.GainElementalExperience(skillDef.associatedElement, success ? 2f : 1f);
        }
    }
    
    public void OnArtifactEquipped(Pawn pawn, ThingWithComps artifact)
    {
        // Could provide artifact-specific bonuses
    }
    
    public void OnBreakthrough(Pawn pawn, CultivationRealm newRealm, int newStage)
    {
        // Could grant elemental insights on breakthrough
    }
    
    public float ModifySkillPower(Pawn pawn, string skillName, float basePower)
    {
        var skillDef = CultivationCache.GetSkillDef(skillName);
        if (skillDef?.associatedElement == null) return basePower;
        
        var comp = pawn.GetComp<CultivationComp>();
        var masteryLevel = comp?.cultivationData?.GetElementMasteryLevel(skillDef.associatedElement) ?? 0f;
        
        // Bonus based on elemental mastery
        return basePower * (1f + masteryLevel * 0.3f);
    }
    
    public float ModifyQiConsumption(Pawn pawn, string skillName, float baseCost)
    {
        var skillDef = CultivationCache.GetSkillDef(skillName);
        if (skillDef?.associatedElement == null) return baseCost;
        
        var comp = pawn.GetComp<CultivationComp>();
        var masteryLevel = comp?.cultivationData?.GetElementMasteryLevel(skillDef.associatedElement) ?? 0f;
        
        // Reduce cost based on elemental mastery
        return baseCost * (1f - masteryLevel * 0.2f);
    }
    
    public bool ShouldBlockSkillExecution(Pawn pawn, string skillName)
    {
        // Could implement complex blocking logic
        return false;
    }
}
```

---

## 🎯 **Integration Best Practices**

### **Mod Compatibility Framework**

```csharp
/// <summary>
/// Framework for ensuring compatibility with other mods
/// </summary>
public static class CultivationCompatibility
{
    private static readonly Dictionary<string, IModCompatibilityHandler> compatibilityHandlers = 
        new Dictionary<string, IModCompatibilityHandler>();
    
    static CultivationCompatibility()
    {
        RegisterCompatibilityHandlers();
    }
    
    private static void RegisterCompatibilityHandlers()
    {
        // Psychology mod compatibility
        if (ModsConfig.IsActive("psychology"))
        {
            compatibilityHandlers["psychology"] = new PsychologyCompatibilityHandler();
        }
        
        // VE Framework compatibility
        if (ModsConfig.IsActive("VanillaExpandedFramework"))
        {
            compatibilityHandlers["ve_framework"] = new VEFrameworkCompatibilityHandler();
        }
        
        // Combat Extended compatibility
        if (ModsConfig.IsActive("CombatExtended"))
        {
            compatibilityHandlers["combat_extended"] = new CombatExtendedCompatibilityHandler();
        }
    }
    
    public static void NotifyModInteraction(string modId, string eventType, object data)
    {
        if (compatibilityHandlers.TryGetValue(modId, out var handler))
        {
            handler.HandleEvent(eventType, data);
        }
    }
    
    public interface IModCompatibilityHandler
    {
        void HandleEvent(string eventType, object data);
        bool IsCompatible();
        void ApplyCompatibilityPatches();
    }
}
```

### **Performance Monitoring**

```csharp
/// <summary>
/// Performance monitoring for cultivation system
/// </summary>
public static class CultivationPerformanceMonitor
{
    private static readonly Dictionary<string, PerformanceMetric> metrics = 
        new Dictionary<string, PerformanceMetric>();
    
    public static void StartTiming(string operation)
    {
        if (!metrics.ContainsKey(operation))
        {
            metrics[operation] = new PerformanceMetric();
        }
        
        metrics[operation].StartTiming();
    }
    
    public static void EndTiming(string operation)
    {
        if (metrics.TryGetValue(operation, out var metric))
        {
            metric.EndTiming();
        }
    }
    
    public static void LogPerformanceReport()
    {
        if (!Prefs.DevMode) return;
        
        var sb = new StringBuilder();
        sb.AppendLine("[Cultivation Performance Report]");
        
        foreach (var kvp in metrics.OrderByDescending(m => m.Value.AverageTime))
        {
            var metric = kvp.Value;
            sb.AppendLine($"{kvp.Key}: {metric.AverageTime:F2}ms (calls: {metric.CallCount})");
        }
        
        Log.Message(sb.ToString());
    }
    
    public class PerformanceMetric
    {
        private long startTime;
        public long TotalTime { get; private set; }
        public int CallCount { get; private set; }
        public float AverageTime => CallCount > 0 ? TotalTime / (float)CallCount / 10000f : 0f; // Convert to ms
        
        public void StartTiming()
        {
            startTime = DateTime.Now.Ticks;
        }
        
        public void EndTiming()
        {
            var elapsed = DateTime.Now.Ticks - startTime;
            TotalTime += elapsed;
            CallCount++;
        }
    }
}
```

---

**Core Framework Version**: 3.0  
**Performance Rating**: Optimized  
**Extension Support**: Full  
**Compatibility**: RimWorld 1.6+
