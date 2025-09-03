# 🔗 API Reference & Extension Guide

## 📖 **Table of Contents**

1. [Core API Reference](#core-api-reference)
2. [Extension Development](#extension-development)
3. [Event System API](#event-system-api)
4. [Utilities & Helpers](#utilities--helpers)
5. [Integration Examples](#integration-examples)
6. [Advanced Techniques](#advanced-techniques)

---

## 🔧 **Core API Reference**

### **CultivationComp Public API**

```csharp
/// <summary>
/// Main public API for accessing cultivation functionality
/// </summary>
public class CultivationComp : ThingComp
{
    #region Core Data Access
    /// <summary>
    /// Gets the cultivation data for this pawn. Read-only access recommended.
    /// </summary>
    public CultivationData CultivationData => cultivationData;
    
    /// <summary>
    /// Gets all skills known by this pawn (includes artifact-granted skills)
    /// </summary>
    public IReadOnlySet<string> KnownSkills => knownSkills;
    
    /// <summary>
    /// Gets current skill cooldowns. Key = skill name, Value = remaining ticks
    /// </summary>
    public IReadOnlyDictionary<string, int> SkillCooldowns => skillCooldowns;
    
    /// <summary>
    /// Gets skill mastery data. Key = skill name, Value = mastery info
    /// </summary>
    public IReadOnlyDictionary<string, SkillMasteryData> SkillMasteryData => skillMasteryData;
    #endregion
    
    #region Skill Management
    /// <summary>
    /// Teaches a skill to the pawn. Checks prerequisites automatically.
    /// </summary>
    /// <param name="skillDefName">The defName of the skill to learn</param>
    /// <param name="silent">If true, suppresses notification messages</param>
    /// <returns>True if skill was learned, false if already known or can't learn</returns>
    public bool LearnSkill(string skillDefName, bool silent = false);
    
    /// <summary>
    /// Checks if the pawn can learn a specific skill
    /// </summary>
    /// <param name="skillDefName">The skill to check</param>
    /// <param name="reason">Output parameter with failure reason</param>
    /// <returns>True if skill can be learned</returns>
    public bool CanLearnSkill(string skillDefName, out string reason);
    
    /// <summary>
    /// Gets all skills available to this pawn (known + artifact-granted)
    /// Uses caching for performance.
    /// </summary>
    /// <returns>List of skill defNames</returns>
    public List<string> GetAllAvailableSkills();
    
    /// <summary>
    /// Checks if a specific skill is currently usable
    /// </summary>
    /// <param name="skillName">The skill to check</param>
    /// <returns>True if skill can be used right now</returns>
    public bool CanUseSkill(string skillName);
    
    /// <summary>
    /// Gets the reason why a skill cannot be used
    /// </summary>
    /// <param name="skillName">The skill to check</param>
    /// <returns>Human-readable reason, or empty string if skill is usable</returns>
    public string GetSkillDisabledReason(string skillName);
    #endregion
    
    #region Qi Management
    /// <summary>
    /// Attempts to consume qi for skill usage
    /// </summary>
    /// <param name="amount">Amount of qi to consume</param>
    /// <param name="force">If true, allows negative qi</param>
    /// <returns>True if qi was consumed successfully</returns>
    public bool ConsumeQi(float amount, bool force = false);
    
    /// <summary>
    /// Restores qi to the pawn
    /// </summary>
    /// <param name="amount">Amount of qi to restore</param>
    public void RestoreQi(float amount);
    
    /// <summary>
    /// Gets current qi regeneration rate per tick
    /// </summary>
    /// <returns>Qi regen per tick</returns>
    public float GetQiRegenRate();
    
    /// <summary>
    /// Calculates maximum qi for current realm/stage
    /// </summary>
    /// <returns>Maximum qi capacity</returns>
    public float GetMaxQi();
    #endregion
    
    #region Cultivation Progress
    /// <summary>
    /// Adds cultivation progress points
    /// </summary>
    /// <param name="points">Points to add</param>
    /// <param name="source">Source of the progress (for tracking)</param>
    public void AddCultivationProgress(float points, string source = "Unknown");
    
    /// <summary>
    /// Forces a breakthrough attempt
    /// </summary>
    /// <returns>True if breakthrough was successful</returns>
    public bool AttemptBreakthrough();
    
    /// <summary>
    /// Checks if pawn can attempt breakthrough
    /// </summary>
    /// <returns>True if breakthrough is possible</returns>
    public bool CanAttemptBreakthrough();
    
    /// <summary>
    /// Gets breakthrough success chance
    /// </summary>
    /// <returns>Chance from 0.0 to 1.0</returns>
    public float GetBreakthroughChance();
    #endregion
    
    #region Mastery System
    /// <summary>
    /// Records skill usage for mastery tracking
    /// </summary>
    /// <param name="skillName">The skill that was used</param>
    /// <param name="success">Whether the usage was successful</param>
    /// <param name="damage">Damage dealt (if applicable)</param>
    /// <param name="healing">Healing done (if applicable)</param>
    public void RecordSkillUsage(string skillName, bool success, float damage = 0f, float healing = 0f);
    
    /// <summary>
    /// Gets mastery bonus for a skill
    /// </summary>
    /// <param name="skillName">The skill to check</param>
    /// <returns>Multiplier (1.0 = no bonus, 1.5 = 50% bonus)</returns>
    public float GetSkillMasteryBonus(string skillName);
    
    /// <summary>
    /// Gets mastery rank for a skill
    /// </summary>
    /// <param name="skillName">The skill to check</param>
    /// <returns>Mastery rank enum</returns>
    public MasteryRank GetSkillMasteryRank(string skillName);
    #endregion
    
    #region Cache Management
    /// <summary>
    /// Invalidates cached data, forcing recalculation
    /// </summary>
    public void InvalidateCache();
    
    /// <summary>
    /// Forces a complete cache refresh
    /// </summary>
    public void RefreshCache();
    #endregion
}
```

### **CultivationCache Static API**

```csharp
/// <summary>
/// High-performance caching system for cultivation definitions
/// </summary>
public static class CultivationCache
{
    #region Definition Lookups
    /// <summary>
    /// Gets a cultivation skill definition by name
    /// </summary>
    /// <param name="defName">The defName to look up</param>
    /// <returns>Skill definition or null if not found</returns>
    public static CultivationSkillDef GetSkillDef(string defName);
    
    /// <summary>
    /// Gets a cultivation ability definition by name
    /// </summary>
    /// <param name="defName">The defName to look up</param>
    /// <returns>Ability definition or null if not found</returns>
    public static CultivationAbilityDef GetAbilityDef(string defName);
    
    /// <summary>
    /// Gets a cultivation artifact definition by name
    /// </summary>
    /// <param name="defName">The defName to look up</param>
    /// <returns>Artifact definition or null if not found</returns>
    public static CultivationArtifactDef GetArtifactDef(string defName);
    #endregion
    
    #region Collection Access
    /// <summary>
    /// Gets all cultivation skill definitions
    /// </summary>
    /// <returns>Read-only collection of all skill defs</returns>
    public static IReadOnlyCollection<CultivationSkillDef> AllSkillDefs 
        => skillDefCache.Values;
    
    /// <summary>
    /// Gets all cultivation ability definitions
    /// </summary>
    /// <returns>Read-only collection of all ability defs</returns>
    public static IReadOnlyCollection<CultivationAbilityDef> AllAbilityDefs 
        => abilityDefCache.Values;
    
    /// <summary>
    /// Gets all cultivation artifact definitions
    /// </summary>
    /// <returns>Read-only collection of all artifact defs</returns>
    public static IReadOnlyCollection<CultivationArtifactDef> AllArtifactDefs 
        => artifactDefCache.Values;
    #endregion
    
    #region Filtered Queries
    /// <summary>
    /// Gets skills available for a specific realm/stage
    /// </summary>
    /// <param name="realm">Required realm</param>
    /// <param name="stage">Required stage</param>
    /// <returns>Skills available at this level</returns>
    public static IEnumerable<CultivationSkillDef> GetSkillsForRealm(CultivationRealm realm, int stage = 1);
    
    /// <summary>
    /// Gets abilities available for a specific realm/stage
    /// </summary>
    /// <param name="realm">Required realm</param>
    /// <param name="stage">Required stage</param>
    /// <returns>Abilities available at this level</returns>
    public static IEnumerable<CultivationAbilityDef> GetAbilitiesForRealm(CultivationRealm realm, int stage = 1);
    
    /// <summary>
    /// Gets skills of a specific element type
    /// </summary>
    /// <param name="element">Element type to filter by</param>
    /// <returns>Skills with matching element</returns>
    public static IEnumerable<CultivationSkillDef> GetSkillsByElement(QiType element);
    
    /// <summary>
    /// Gets artifacts that grant a specific skill
    /// </summary>
    /// <param name="skillName">Skill to search for</param>
    /// <returns>Artifacts that grant this skill</returns>
    public static IEnumerable<CultivationArtifactDef> GetArtifactsWithSkill(string skillName);
    #endregion
    
    #region Cache Management
    /// <summary>
    /// Forces cache rebuilding (use sparingly)
    /// </summary>
    public static void RebuildCache();
    
    /// <summary>
    /// Gets cache statistics for debugging
    /// </summary>
    /// <returns>Cache performance and size information</returns>
    public static CacheStatistics GetStatistics();
    #endregion
}
```

---

## 🛠️ **Extension Development**

### **Plugin Interface Implementation**

```csharp
/// <summary>
/// Complete example of a cultivation system extension plugin
/// </summary>
public class ExampleCultivationPlugin : ICultivationPlugin
{
    #region Plugin Metadata
    public string Name => "Example Cultivation Enhancement";
    public string Version => "1.0.0";
    public string Author => "Your Name Here";
    public string Description => "Example plugin demonstrating cultivation system extension";
    #endregion
    
    #region Configuration
    private bool enableEnhancedEffects = true;
    private float skillPowerMultiplier = 1.1f;
    private Dictionary<string, float> customSkillBonuses = new Dictionary<string, float>();
    #endregion
    
    #region Plugin Lifecycle
    public void Initialize()
    {
        Log.Message($"[{Name}] Initializing plugin v{Version}");
        
        // Load configuration
        LoadConfiguration();
        
        // Register custom handlers
        RegisterCustomHandlers();
        
        // Apply harmony patches if needed
        ApplyCustomPatches();
        
        Log.Message($"[{Name}] Plugin initialized successfully");
    }
    
    private void LoadConfiguration()
    {
        // Load plugin-specific settings from ModSettings or XML
        customSkillBonuses["QiPunch"] = 1.2f;
        customSkillBonuses["QiShield"] = 1.15f;
    }
    
    private void RegisterCustomHandlers()
    {
        // Register for additional events
        CultivationEventManager.OnQiConsumed += OnQiConsumed;
        CultivationEventManager.OnQiRestored += OnQiRestored;
    }
    
    private void ApplyCustomPatches()
    {
        var harmony = new HarmonyLib.Harmony($"{Name.Replace(" ", "").ToLower()}.plugin");
        harmony.PatchAll(typeof(ExampleCultivationPlugin).Assembly);
    }
    #endregion
    
    #region Event Handlers
    public void OnSkillExecuted(Pawn pawn, string skillName, bool success)
    {
        if (!enableEnhancedEffects) return;
        
        // Add custom visual effects for specific skills
        if (skillName == "QiPunch" && success)
        {
            // Enhanced punch effects
            CreateEnhancedPunchEffect(pawn);
        }
        
        // Track usage statistics
        RecordPluginStatistics(pawn, skillName, success);
    }
    
    public void OnArtifactEquipped(Pawn pawn, ThingWithComps artifact)
    {
        // Apply custom artifact bonuses
        ApplyCustomArtifactBonuses(pawn, artifact);
        
        // Custom notification
        if (enableEnhancedEffects)
        {
            Messages.Message($"{pawn.LabelShort} feels enhanced power from {artifact.Label}!", 
                           MessageTypeDefOf.PositiveEvent);
        }
    }
    
    public void OnArtifactUnequipped(Pawn pawn, ThingWithComps artifact)
    {
        // Remove custom bonuses
        RemoveCustomArtifactBonuses(pawn, artifact);
    }
    
    public void OnBreakthrough(Pawn pawn, CultivationRealm newRealm, int newStage)
    {
        // Grant bonus rewards for breakthrough
        GrantBreakthroughBonuses(pawn, newRealm, newStage);
    }
    
    private void OnQiConsumed(Pawn pawn, float amount)
    {
        // Track qi consumption patterns
        TrackQiUsage(pawn, -amount);
    }
    
    private void OnQiRestored(Pawn pawn, float amount)
    {
        // Track qi restoration patterns
        TrackQiUsage(pawn, amount);
    }
    #endregion
    
    #region Modification Hooks
    public float ModifySkillPower(Pawn pawn, string skillName, float basePower)
    {
        var modifiedPower = basePower;
        
        // Apply global multiplier
        modifiedPower *= skillPowerMultiplier;
        
        // Apply skill-specific bonuses
        if (customSkillBonuses.TryGetValue(skillName, out float bonus))
        {
            modifiedPower *= bonus;
        }
        
        // Apply time-of-day bonuses
        var hour = GenLocalDate.HourOfDay(pawn);
        if (hour >= 6 && hour <= 8) // Morning cultivation bonus
        {
            modifiedPower *= 1.1f;
        }
        
        return modifiedPower;
    }
    
    public float ModifyQiConsumption(Pawn pawn, string skillName, float baseCost)
    {
        var modifiedCost = baseCost;
        
        // Reduce cost for mastered skills
        var comp = pawn.GetComp<CultivationComp>();
        if (comp?.skillMasteryData?.TryGetValue(skillName, out var mastery) == true)
        {
            var efficiency = 1f - (mastery.masteryLevel * 0.2f); // Up to 20% reduction
            modifiedCost *= efficiency;
        }
        
        // Environmental efficiency bonuses
        var room = pawn.GetRoom();
        if (room?.GetStat(StatDefOf.Cleanliness) > 0.5f)
        {
            modifiedCost *= 0.95f; // 5% reduction in clean environments
        }
        
        return modifiedCost;
    }
    
    public bool ShouldBlockSkillExecution(Pawn pawn, string skillName)
    {
        // Block skill usage under certain conditions
        
        // Don't use offensive skills if pacifist
        if (pawn.story?.traits?.HasTrait(TraitDefOf.Kind) == true)
        {
            var skillDef = CultivationCache.GetSkillDef(skillName);
            if (skillDef?.category == "Offensive")
            {
                return true;
            }
        }
        
        // Block if pawn is in bad mental state
        if (pawn.mindState?.mentalBreaker?.BreakMinorIsApproaching == true)
        {
            return true;
        }
        
        return false;
    }
    #endregion
    
    #region Custom Features
    private void CreateEnhancedPunchEffect(Pawn pawn)
    {
        // Custom enhanced visual effect
        FleckMaker.ThrowExplosionCell(pawn.Position, pawn.Map, FleckDefOf.ExplosionFlash, Color.cyan);
        
        // Custom sound
        SoundDefOf.Thunder_OnMap.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
        
        // Screen shake
        Find.CameraDriver.shaker.DoShake(1.5f);
    }
    
    private void ApplyCustomArtifactBonuses(Pawn pawn, ThingWithComps artifact)
    {
        // Could apply hediffs, mood bonuses, skill bonuses, etc.
        var artifactComp = artifact.GetComp<CultivationArtifactComp>();
        if (artifactComp?.def?.defName == "SpecialArtifact")
        {
            // Apply special bonuses for specific artifacts
        }
    }
    
    private void GrantBreakthroughBonuses(Pawn pawn, CultivationRealm realm, int stage)
    {
        // Grant bonus items or abilities on breakthrough
        if (realm == CultivationRealm.FoundationBuilding && stage == 1)
        {
            // First major breakthrough - grant special item
            var comp = pawn.GetComp<CultivationComp>();
            comp?.LearnSkill("SpecialFoundationSkill", false);
        }
    }
    #endregion
    
    #region Statistics Tracking
    private static readonly Dictionary<Pawn, PluginStatistics> pluginStats = 
        new Dictionary<Pawn, PluginStatistics>();
    
    private void RecordPluginStatistics(Pawn pawn, string skillName, bool success)
    {
        if (!pluginStats.ContainsKey(pawn))
        {
            pluginStats[pawn] = new PluginStatistics();
        }
        
        var stats = pluginStats[pawn];
        stats.totalSkillUsage++;
        if (success) stats.successfulUsage++;
        
        if (!stats.skillUsage.ContainsKey(skillName))
        {
            stats.skillUsage[skillName] = 0;
        }
        stats.skillUsage[skillName]++;
    }
    
    private void TrackQiUsage(Pawn pawn, float change)
    {
        if (!pluginStats.ContainsKey(pawn))
        {
            pluginStats[pawn] = new PluginStatistics();
        }
        
        if (change > 0)
            pluginStats[pawn].totalQiRestored += change;
        else
            pluginStats[pawn].totalQiConsumed += -change;
    }
    
    private class PluginStatistics
    {
        public int totalSkillUsage = 0;
        public int successfulUsage = 0;
        public float totalQiConsumed = 0f;
        public float totalQiRestored = 0f;
        public Dictionary<string, int> skillUsage = new Dictionary<string, int>();
    }
    #endregion
}
```

### **Custom Skill Worker Template**

```csharp
/// <summary>
/// Template for creating custom skill workers
/// </summary>
public abstract class CustomSkillWorkerBase : SkillWorkerBase
{
    #region Abstract Requirements
    protected abstract void ApplySkillEffects(Pawn caster, LocalTargetInfo target);
    protected abstract bool ValidateCustomRequirements(Pawn caster, LocalTargetInfo target);
    protected abstract string GetCustomDisabledReason(Pawn caster, LocalTargetInfo target);
    #endregion
    
    #region Template Implementation
    public override bool CanExecute(Pawn caster, LocalTargetInfo target)
    {
        // Base validation
        if (!base.CanExecute(caster, target)) return false;
        
        // Custom validation
        if (!ValidateCustomRequirements(caster, target))
        {
            SetDisabledReason(GetCustomDisabledReason(caster, target));
            return false;
        }
        
        return true;
    }
    
    public override void ExecuteSkillEffect(Pawn caster, LocalTargetInfo target)
    {
        try
        {
            // Handle base requirements (qi, cooldown)
            base.ExecuteSkillEffect(caster, target);
            
            // Apply custom effects
            ApplySkillEffects(caster, target);
            
            // Notify plugins
            CultivationPluginManager.NotifySkillExecution(caster, skillDef.defName, true);
            
            // Record success
            RecordSkillUsage(caster, true);
        }
        catch (Exception ex)
        {
            // Handle failure gracefully
            CultivationErrorHandler.LogError($"Error executing {skillDef.defName}", ex);
            
            // Notify failure
            CultivationPluginManager.NotifySkillExecution(caster, skillDef.defName, false);
            RecordSkillUsage(caster, false);
        }
    }
    
    protected virtual void RecordSkillUsage(Pawn caster, bool success)
    {
        var comp = caster.GetComp<CultivationComp>();
        comp?.RecordSkillUsage(skillDef.defName, success);
    }
    #endregion
    
    #region Helper Methods
    protected float GetSkillPower(Pawn caster)
    {
        var basePower = skillDef.basePower;
        
        // Apply mastery bonus
        var comp = caster.GetComp<CultivationComp>();
        var masteryBonus = comp?.GetSkillMasteryBonus(skillDef.defName) ?? 1f;
        
        // Apply plugin modifications
        var pluginModified = CultivationPluginManager.ModifySkillPower(caster, skillDef.defName, basePower);
        
        return pluginModified * masteryBonus;
    }
    
    protected bool IsValidTarget(Pawn caster, LocalTargetInfo target)
    {
        return skillDef.targetType switch
        {
            SkillTargetType.Self => target.Thing == caster,
            SkillTargetType.Enemy => target.Thing is Pawn targetPawn && targetPawn.HostileTo(caster),
            SkillTargetType.Ally => target.Thing is Pawn targetPawn && !targetPawn.HostileTo(caster),
            SkillTargetType.Any => target.IsValid,
            _ => true
        };
    }
    
    protected void CreateSkillEffects(Pawn caster, LocalTargetInfo target, string effectName)
    {
        // Standardized effect creation
        CultivationVFX.PlayEffect(effectName, target.Thing.DrawPos, caster.Map, 1f);
        
        // Plugin notification for custom effects
        CultivationPluginManager.NotifyCustomEffect(caster, skillDef.defName, effectName, target);
    }
    #endregion
}

/// <summary>
/// Example implementation of custom skill worker
/// </summary>
public class SkillWorker_CustomFireBlast : CustomSkillWorkerBase
{
    protected override void ApplySkillEffects(Pawn caster, LocalTargetInfo target)
    {
        var power = GetSkillPower(caster);
        var damage = power * 1.5f; // Fire skills deal extra damage
        
        // Apply fire damage
        var damageInfo = new DamageInfo(DamageDefOf.Burn, damage, 0f, -1f, caster);
        target.Thing.TakeDamage(damageInfo);
        
        // Area of effect
        var cellsInRadius = GenRadial.RadialCellsAround(target.Cell, 2f, true);
        foreach (var cell in cellsInRadius)
        {
            // Apply lesser damage to nearby targets
            var nearbyTargets = cell.GetThingList(caster.Map).OfType<Pawn>()
                .Where(p => p != target.Thing && p.HostileTo(caster));
            
            foreach (var nearbyTarget in nearbyTargets)
            {
                var aoeDamage = damage * 0.5f;
                var aoeDamageInfo = new DamageInfo(DamageDefOf.Burn, aoeDamage, 0f, -1f, caster);
                nearbyTarget.TakeDamage(aoeDamageInfo);
            }
        }
        
        // Custom effects
        CreateSkillEffects(caster, target, "fire_blast_enhanced");
    }
    
    protected override bool ValidateCustomRequirements(Pawn caster, LocalTargetInfo target)
    {
        // Require line of sight for fire blast
        if (!GenSight.LineOfSight(caster.Position, target.Cell, caster.Map))
        {
            return false;
        }
        
        // Don't target allies with fire
        if (target.Thing is Pawn targetPawn && !targetPawn.HostileTo(caster))
        {
            return false;
        }
        
        return true;
    }
    
    protected override string GetCustomDisabledReason(Pawn caster, LocalTargetInfo target)
    {
        if (!GenSight.LineOfSight(caster.Position, target.Cell, caster.Map))
            return "No line of sight";
        
        if (target.Thing is Pawn targetPawn && !targetPawn.HostileTo(caster))
            return "Cannot target allies with fire";
        
        return "";
    }
}
```

---

## 🎯 **Integration Examples**

### **Psychology Mod Integration**

```csharp
/// <summary>
/// Integration with Psychology mod for enhanced personality effects
/// </summary>
public class PsychologyIntegration : ICultivationPlugin
{
    public string Name => "Psychology Integration";
    public string Version => "1.0.0";
    public string Author => "Tu Tiên Team";
    
    private bool psychologyLoaded = false;
    
    public void Initialize()
    {
        psychologyLoaded = ModsConfig.IsActive("psychology");
        if (!psychologyLoaded) return;
        
        Log.Message("[Tu Tiên] Psychology integration enabled");
    }
    
    public float ModifySkillPower(Pawn pawn, string skillName, float basePower)
    {
        if (!psychologyLoaded) return basePower;
        
        // Access Psychology personality traits (example)
        // This would require actual Psychology API access
        /*
        var personality = pawn.GetPersonality();
        if (personality != null)
        {
            // Aggressive personalities boost combat skills
            if (personality.IsAggressive && IsOffensiveSkill(skillName))
            {
                return basePower * 1.15f;
            }
            
            // Calm personalities boost defensive skills
            if (personality.IsCalm && IsDefensiveSkill(skillName))
            {
                return basePower * 1.1f;
            }
        }
        */
        
        return basePower;
    }
    
    public void OnSkillExecuted(Pawn pawn, string skillName, bool success)
    {
        if (!psychologyLoaded) return;
        
        // Personality could affect skill learning and mastery rates
        // Successful skill usage could affect mood and personality development
    }
    
    // Implement other required interface methods...
}
```

### **Combat Extended Integration**

```csharp
/// <summary>
/// Integration with Combat Extended for enhanced combat mechanics
/// </summary>
public class CombatExtendedIntegration : ICultivationPlugin
{
    public string Name => "Combat Extended Integration";
    public string Version => "1.0.0";
    public string Author => "Tu Tiên Team";
    
    private bool combatExtendedLoaded = false;
    
    public void Initialize()
    {
        combatExtendedLoaded = ModsConfig.IsActive("CombatExtended");
        if (!combatExtendedLoaded) return;
        
        // Apply CE-specific patches
        ApplyCombatExtendedPatches();
        
        Log.Message("[Tu Tiên] Combat Extended integration enabled");
    }
    
    private void ApplyCombatExtendedPatches()
    {
        var harmony = new HarmonyLib.Harmony("tutien.combatextended.integration");
        
        // Patch CE damage calculation to account for cultivation bonuses
        harmony.Patch(
            AccessTools.Method("CombatExtended.DamageWorker_MeleeBase:GetAdjustedDamageAmount"),
            postfix: new HarmonyMethod(typeof(CombatExtendedIntegration), nameof(GetAdjustedDamageAmount_Postfix))
        );
    }
    
    public static void GetAdjustedDamageAmount_Postfix(ref float __result, Pawn pawn)
    {
        // Boost melee damage based on cultivation level
        var comp = pawn?.GetComp<CultivationComp>();
        if (comp?.cultivationData != null)
        {
            var realmBonus = 1f + (int)comp.cultivationData.currentRealm * 0.1f;
            var stageBonus = 1f + comp.cultivationData.currentStage * 0.02f;
            
            __result *= realmBonus * stageBonus;
        }
    }
    
    // Implement other required interface methods...
}
```

---

## 🚀 **Advanced Techniques**

### **Dynamic Skill Generation**

```csharp
/// <summary>
/// System for creating skills dynamically at runtime
/// </summary>
public static class DynamicSkillGenerator
{
    public static CultivationSkillDef CreateDynamicSkill(string baseName, QiType element, 
                                                        CultivationRealm realm, int stage)
    {
        var skillDef = new CultivationSkillDef
        {
            defName = $"Dynamic_{baseName}_{element}_{realm}_{stage}",
            label = $"{element} {baseName}",
            description = $"A {element.ToString().ToLower()} cultivation technique of {realm} realm.",
            
            requiredRealm = realm,
            requiredStage = stage,
            qiCost = CalculateQiCost(realm, stage),
            cooldownTicks = CalculateCooldown(realm, stage),
            range = CalculateRange(realm, stage),
            associatedElement = element,
            
            skillWorkerClass = GetWorkerType(baseName)
        };
        
        // Register in DefDatabase
        DefDatabase<CultivationSkillDef>.Add(skillDef);
        
        // Update cache
        CultivationCache.RebuildCache();
        
        return skillDef;
    }
    
    private static float CalculateQiCost(CultivationRealm realm, int stage)
    {
        return 10f + (int)realm * 5f + stage * 2f;
    }
    
    private static int CalculateCooldown(CultivationRealm realm, int stage)
    {
        return 120 - (int)realm * 10 - stage * 2; // Higher realms = faster cooldown
    }
    
    private static float CalculateRange(CultivationRealm realm, int stage)
    {
        return 3f + (int)realm * 1f + stage * 0.2f;
    }
    
    private static Type GetWorkerType(string baseName)
    {
        return baseName switch
        {
            "Strike" => typeof(SkillWorker_DynamicStrike),
            "Shield" => typeof(SkillWorker_DynamicShield),
            "Heal" => typeof(SkillWorker_DynamicHeal),
            _ => typeof(SkillWorker_DynamicGeneric)
        };
    }
}

/// <summary>
/// Generic dynamic skill worker
/// </summary>
public class SkillWorker_DynamicGeneric : CustomSkillWorkerBase
{
    protected override void ApplySkillEffects(Pawn caster, LocalTargetInfo target)
    {
        var power = GetSkillPower(caster);
        var element = skillDef.associatedElement;
        
        // Apply element-specific effects
        ApplyElementalEffect(caster, target, element, power);
    }
    
    private void ApplyElementalEffect(Pawn caster, LocalTargetInfo target, QiType element, float power)
    {
        switch (element)
        {
            case QiType.Fire:
                ApplyFireEffect(caster, target, power);
                break;
                
            case QiType.Water:
                ApplyWaterEffect(caster, target, power);
                break;
                
            case QiType.Earth:
                ApplyEarthEffect(caster, target, power);
                break;
                
            case QiType.Metal:
                ApplyMetalEffect(caster, target, power);
                break;
                
            case QiType.Wood:
                ApplyWoodEffect(caster, target, power);
                break;
                
            case QiType.Lightning:
                ApplyLightningEffect(caster, target, power);
                break;
                
            default:
                ApplyGenericEffect(caster, target, power);
                break;
        }
    }
    
    private void ApplyFireEffect(Pawn caster, LocalTargetInfo target, float power)
    {
        // Fire damage with burning chance
        var damage = power * 1.2f;
        var damageInfo = new DamageInfo(DamageDefOf.Burn, damage, 0f, -1f, caster);
        target.Thing.TakeDamage(damageInfo);
        
        // Chance to set on fire
        if (Rand.Chance(0.3f) && target.Thing is Pawn targetPawn)
        {
            var fireHediff = HediffMaker.MakeHediff(HediffDefOf.Burn, targetPawn);
            targetPawn.health.AddHediff(fireHediff);
        }
        
        // Visual
        CultivationVFX.PlayEffect("fire_strike", target.Thing.DrawPos, caster.Map, 1f);
    }
    
    // Implement other elemental effects...
    
    protected override bool ValidateCustomRequirements(Pawn caster, LocalTargetInfo target)
    {
        // Dynamic skills always validate if base requirements are met
        return true;
    }
    
    protected override string GetCustomDisabledReason(Pawn caster, LocalTargetInfo target)
    {
        return "";
    }
}
```

### **Mod Settings Integration**

```csharp
/// <summary>
/// Settings integration for cultivation system customization
/// </summary>
public class CultivationModSettings : ModSettings
{
    #region Settings Fields
    public static bool enableEnhancedEffects = true;
    public static float skillPowerMultiplier = 1f;
    public static float qiRegenMultiplier = 1f;
    public static bool enableAutoSkillLearning = true;
    public static bool enableMasterySystem = true;
    public static int maxConcurrentSkills = 5;
    
    public static bool debugMode = false;
    public static bool showPerformanceMetrics = false;
    public static CultivationThemes.CultivationTheme uiTheme = CultivationThemes.CultivationTheme.Classic;
    #endregion
    
    #region Settings UI
    public void DoWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        
        // Gameplay settings
        listing.Label("Gameplay Settings", -1f, "Core gameplay modifications");
        listing.CheckboxLabeled("Enhanced Visual Effects", ref enableEnhancedEffects);
        listing.CheckboxLabeled("Auto Skill Learning", ref enableAutoSkillLearning);
        listing.CheckboxLabeled("Mastery System", ref enableMasterySystem);
        
        // Power scaling
        listing.Label($"Skill Power Multiplier: {skillPowerMultiplier:P0}", -1f, 
                     "Global multiplier for all skill damage/effects");
        skillPowerMultiplier = listing.Slider(skillPowerMultiplier, 0.5f, 2f);
        
        listing.Label($"Qi Regeneration Multiplier: {qiRegenMultiplier:P0}", -1f,
                     "Global multiplier for qi regeneration rate");
        qiRegenMultiplier = listing.Slider(qiRegenMultiplier, 0.5f, 3f);
        
        listing.Label($"Max Concurrent Skills: {maxConcurrentSkills}", -1f,
                     "Maximum number of skills that can be used simultaneously");
        maxConcurrentSkills = (int)listing.Slider(maxConcurrentSkills, 1, 10);
        
        listing.Gap();
        
        // UI settings
        listing.Label("Interface Settings", -1f, "User interface customization");
        
        if (listing.ButtonTextLabeled("UI Theme:", uiTheme.ToString()))
        {
            var themeOptions = new List<FloatMenuOption>();
            foreach (CultivationThemes.CultivationTheme theme in Enum.GetValues(typeof(CultivationThemes.CultivationTheme)))
            {
                themeOptions.Add(new FloatMenuOption(theme.ToString(), () =>
                {
                    uiTheme = theme;
                    CultivationThemes.currentTheme = theme;
                }));
            }
            Find.WindowStack.Add(new FloatMenu(themeOptions));
        }
        
        listing.Gap();
        
        // Developer settings
        listing.Label("Developer Settings", -1f, "Advanced debugging and development tools");
        listing.CheckboxLabeled("Debug Mode", ref debugMode);
        listing.CheckboxLabeled("Performance Metrics", ref showPerformanceMetrics);
        
        if (debugMode)
        {
            if (listing.ButtonText("Clear All Caches"))
            {
                CultivationCache.RebuildCache();
                Messages.Message("Cultivation caches cleared", MessageTypeDefOf.TaskCompletion);
            }
            
            if (listing.ButtonText("Force Garbage Collection"))
            {
                CultivationMemoryManager.PerformGarbageCollection();
                Messages.Message("Garbage collection forced", MessageTypeDefOf.TaskCompletion);
            }
            
            if (listing.ButtonText("Generate Performance Report"))
            {
                CultivationPerformanceMonitor.LogPerformanceReport();
                CultivationLogAnalyzer.AnalyzePerformance();
            }
        }
        
        listing.End();
    }
    
    public override void ExposeData()
    {
        Scribe_Values.Look(ref enableEnhancedEffects, "enableEnhancedEffects", true);
        Scribe_Values.Look(ref skillPowerMultiplier, "skillPowerMultiplier", 1f);
        Scribe_Values.Look(ref qiRegenMultiplier, "qiRegenMultiplier", 1f);
        Scribe_Values.Look(ref enableAutoSkillLearning, "enableAutoSkillLearning", true);
        Scribe_Values.Look(ref enableMasterySystem, "enableMasterySystem", true);
        Scribe_Values.Look(ref maxConcurrentSkills, "maxConcurrentSkills", 5);
        Scribe_Values.Look(ref debugMode, "debugMode", false);
        Scribe_Values.Look(ref showPerformanceMetrics, "showPerformanceMetrics", false);
        Scribe_Values.Look(ref uiTheme, "uiTheme", CultivationThemes.CultivationTheme.Classic);
    }
    #endregion
    
    #region Settings Application
    public static void ApplySettings()
    {
        // Apply theme changes
        CultivationThemes.currentTheme = uiTheme;
        
        // Update performance monitoring
        if (showPerformanceMetrics != CultivationPerformanceMonitor.IsEnabled)
        {
            CultivationPerformanceMonitor.SetEnabled(showPerformanceMetrics);
        }
        
        // Apply debug mode changes
        if (debugMode)
        {
            CultivationDebugRenderer.EnableDebugMode();
        }
        else
        {
            CultivationDebugRenderer.DisableDebugMode();
        }
    }
    #endregion
    
    // Implement required interface methods (minimal implementation)...
}

/// <summary>
/// Mod class for settings integration
/// </summary>
public class TuTienMod : Mod
{
    private CultivationModSettings settings;
    
    public TuTienMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<CultivationModSettings>();
    }
    
    public override void DoSettingsWindowContents(Rect inRect)
    {
        settings.DoWindowContents(inRect);
    }
    
    public override string SettingsCategory()
    {
        return "Tu Tiên Cultivation";
    }
    
    public override void WriteSettings()
    {
        base.WriteSettings();
        CultivationModSettings.ApplySettings();
    }
}
```

---

## 📊 **API Usage Examples**

### **External Mod Integration**

```csharp
/// <summary>
/// Example of how other mods can integrate with Tu Tiên
/// </summary>
public class ExternalModIntegration
{
    /// <summary>
    /// Check if a pawn has cultivation abilities
    /// </summary>
    public static bool IsCultivator(Pawn pawn)
    {
        return pawn.GetComp<CultivationComp>() != null;
    }
    
    /// <summary>
    /// Get a cultivator's current power level
    /// </summary>
    public static int GetCultivationLevel(Pawn pawn)
    {
        var comp = pawn.GetComp<CultivationComp>();
        return comp?.cultivationData?.GetTotalRank() ?? 0;
    }
    
    /// <summary>
    /// Grant cultivation points to a pawn
    /// </summary>
    public static void GrantCultivationPoints(Pawn pawn, float points, string source)
    {
        var comp = pawn.GetComp<CultivationComp>();
        comp?.AddCultivationProgress(points, source);
    }
    
    /// <summary>
    /// Check if pawn can use a specific skill
    /// </summary>
    public static bool CanUseCultivationSkill(Pawn pawn, string skillName)
    {
        var comp = pawn.GetComp<CultivationComp>();
        return comp?.CanUseSkill(skillName) ?? false;
    }
    
    /// <summary>
    /// Force teach a skill to a pawn
    /// </summary>
    public static bool TeachSkill(Pawn pawn, string skillName)
    {
        var comp = pawn.GetComp<CultivationComp>();
        return comp?.LearnSkill(skillName, false) ?? false;
    }
    
    /// <summary>
    /// Get all skills available to a pawn
    /// </summary>
    public static List<string> GetAvailableSkills(Pawn pawn)
    {
        var comp = pawn.GetComp<CultivationComp>();
        return comp?.GetAllAvailableSkills() ?? new List<string>();
    }
    
    /// <summary>
    /// Apply qi cost modification
    /// </summary>
    public static void ModifyQiCosts(Func<Pawn, string, float, float> modifier)
    {
        // This would integrate with the plugin system
        var plugin = new QiCostModifierPlugin(modifier);
        CultivationPluginManager.RegisterPlugin(plugin);
    }
}

/// <summary>
/// Example quest integration using cultivation system
/// </summary>
public class CultivationQuestIntegration
{
    /// <summary>
    /// Create a quest that requires cultivation level
    /// </summary>
    public static Quest CreateCultivationQuest()
    {
        var quest = QuestUtility.GenerateQuestAndMakeAvailable(
            QuestScriptDefOf.OpportunitySite_ItemStash, 
            Find.World.worldObjects.Settlements.RandomElement().Tile
        );
        
        // Add cultivation requirement
        var cultivationReq = new QuestPart_RequireCultivationLevel
        {
            requiredRealm = CultivationRealm.FoundationBuilding,
            requiredStage = 5,
            quest = quest
        };
        
        quest.AddPart(cultivationReq);
        
        return quest;
    }
}

/// <summary>
/// Custom quest part requiring cultivation level
/// </summary>
public class QuestPart_RequireCultivationLevel : QuestPart
{
    public CultivationRealm requiredRealm;
    public int requiredStage;
    
    public override bool QuestPartReserves(Pawn p)
    {
        var comp = p.GetComp<CultivationComp>();
        if (comp?.cultivationData == null) return false;
        
        return comp.cultivationData.currentRealm >= requiredRealm &&
               (comp.cultivationData.currentRealm > requiredRealm || 
                comp.cultivationData.currentStage >= requiredStage);
    }
    
    public override string ExtraInspectString(ISelectable target)
    {
        return $"Requires cultivation: {requiredRealm} Stage {requiredStage}+";
    }
}
```

---

**API Reference Version**: 2.0  
**Plugin Support**: Full  
**External Integration**: Compatible  
**Last Updated**: September 2025
