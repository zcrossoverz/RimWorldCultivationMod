# 🎨 UI Integration & Visual System

## 📖 **Table of Contents**

1. [UI System Overview](#ui-system-overview)
2. [Unified Gizmo Architecture](#unified-gizmo-architecture)
3. [Visual Effects System](#visual-effects-system)
4. [User Experience Design](#user-experience-design)
5. [Performance Optimization](#performance-optimization)
6. [Customization Guide](#customization-guide)

---

## 🎨 **UI System Overview**

The Tu Tiên UI system provides seamless integration of cultivation skills with RimWorld's native interface through a unified gizmo management system.

```
UI Integration Architecture:
┌─────────────────────────────────────────────────────────────┐
│                    UI SYSTEM OVERVIEW                       │
├─────────────────┬─────────────────┬─────────────────────────┤
│   INPUT LAYER   │  PROCESSING     │    OUTPUT LAYER         │
│                 │    LAYER        │                         │
│ ┌─────────────┐ │ ┌─────────────┐ │ ┌─────────────────────┐ │
│ │ Player      │ │ │ Harmony     │ │ │ Gizmo Display       │ │
│ │ Actions     │ │ │ Patches     │ │ │ System              │ │
│ │             │ │ │             │ │ │                     │ │
│ │ • Click     │ │ │ • Intercept │ │ │ • Skill icons       │ │
│ │ • Hover     │ │ │ • Process   │ │ │ • Tooltips          │ │
│ │ • Target    │◄┼►│ • Validate  │◄┼►│ • Cooldown timers   │ │
│ │ • Hotkeys   │ │ │ • Generate  │ │ │ • Disabled states   │ │
│ └─────────────┘ │ └─────────────┘ │ └─────────────────────┘ │
└─────────────────┴─────────────────┴─────────────────────────┘
                           │
                           ▼
                ┌─────────────────────┐
                │   FEEDBACK SYSTEM   │
                │                     │
                │ • Visual effects    │
                │ • Sound effects     │
                │ • Messages          │
                │ • Screen shakes     │
                └─────────────────────┘
```

---

## 🔧 **Unified Gizmo Architecture**

### **Core Patch System**

```csharp
/// <summary>
/// Central UI integration point for all cultivation systems
/// </summary>
[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public static class Pawn_GetGizmos_UnifiedPatch
{
    private static readonly Dictionary<Pawn, List<Gizmo>> gizmoCache = 
        new Dictionary<Pawn, List<Gizmo>>();
    
    private static int lastCacheFrame = -1;
    
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
    {
        // Return original gizmos first
        foreach (var gizmo in __result)
            yield return gizmo;
        
        // Add cultivation gizmos with caching
        foreach (var cultivationGizmo in GetCachedCultivationGizmos(__instance))
            yield return cultivationGizmo;
    }
    
    private static IEnumerable<Gizmo> GetCachedCultivationGizmos(Pawn pawn)
    {
        var currentFrame = Time.frameCount;
        
        // Use cached results if available (same frame)
        if (lastCacheFrame == currentFrame && gizmoCache.TryGetValue(pawn, out var cached))
        {
            return cached;
        }
        
        // Generate new gizmos
        var gizmos = GenerateCultivationGizmos(pawn).ToList();
        
        // Update cache
        gizmoCache[pawn] = gizmos;
        lastCacheFrame = currentFrame;
        
        // Cleanup old cache entries
        if (currentFrame % 300 == 0) // Every 5 seconds
        {
            CleanupGizmoCache();
        }
        
        return gizmos;
    }
    
    private static IEnumerable<Gizmo> GenerateCultivationGizmos(Pawn pawn)
    {
        var comp = pawn.GetComp<CultivationComp>();
        if (comp?.cultivationData == null) yield break;
        
        // 1. Direct Known Skills (CultivationSkillDef)
        foreach (var skillGizmo in GetKnownSkillGizmos(pawn, comp))
            yield return skillGizmo;
            
        // 2. Known Abilities (CultivationAbilityDef)
        foreach (var abilityGizmo in GetKnownAbilityGizmos(pawn, comp))
            yield return abilityGizmo;
            
        // 3. Artifact-Granted Skills (Dual System Support)
        foreach (var artifactGizmo in GetArtifactSkillGizmos(pawn, comp))
            yield return artifactGizmo;
    }
}
```

### **Smart Gizmo Generation**

```csharp
/// <summary>
/// Advanced gizmo factory with smart features
/// </summary>
public static class CultivationGizmoFactory
{
    #region Skill Gizmo Creation
    public static Command_Action CreateSkillGizmo(CultivationSkillDef skillDef, Pawn pawn, CultivationComp comp)
    {
        var worker = skillDef.GetSkillWorker();
        
        return new Command_Action
        {
            defaultLabel = skillDef.label,
            defaultDesc = BuildSkillDescription(skillDef, pawn, comp),
            icon = GetSkillIcon(skillDef),
            iconDrawScale = GetIconScale(skillDef),
            
            action = () => ExecuteSkillWithTargeting(skillDef, pawn, worker),
            
            // Smart disable logic
            Disabled = !CanUseSkill(skillDef, pawn, comp),
            disabledReason = GetSkillDisabledReason(skillDef, pawn, comp),
            
            // Visual enhancements
            shrinkable = true,
            groupKey = GetSkillGroupKey(skillDef),
            order = GetSkillOrder(skillDef)
        };
    }
    
    private static string BuildSkillDescription(CultivationSkillDef skillDef, Pawn pawn, CultivationComp comp)
    {
        var sb = new StringBuilder();
        
        // Base description
        sb.AppendLine(skillDef.description);
        sb.AppendLine();
        
        // Requirements
        sb.AppendLine($"Realm Requirement: {skillDef.requiredRealm}");
        sb.AppendLine($"Stage Requirement: {skillDef.requiredStage}");
        sb.AppendLine($"Qi Cost: {skillDef.qiCost}");
        
        if (skillDef.cooldownTicks > 0)
            sb.AppendLine($"Cooldown: {(skillDef.cooldownTicks / 60f):F1}s");
        
        if (skillDef.range > 0)
            sb.AppendLine($"Range: {skillDef.range}");
        
        // Current status
        sb.AppendLine();
        sb.AppendLine("--- Current Status ---");
        sb.AppendLine($"Your Realm: {comp.cultivationData.currentRealm} (Stage {comp.cultivationData.currentStage})");
        sb.AppendLine($"Available Qi: {comp.cultivationData.currentQi:F1}/{comp.cultivationData.maxQi:F1}");
        
        // Cooldown status
        if (comp.skillCooldowns.TryGetValue(skillDef.defName, out int cooldown) && cooldown > 0)
        {
            sb.AppendLine($"Cooldown: {(cooldown / 60f):F1}s remaining");
        }
        
        // Mastery information
        if (comp.skillMasteryData?.TryGetValue(skillDef.defName, out var mastery) == true)
        {
            sb.AppendLine();
            sb.AppendLine("--- Mastery ---");
            sb.AppendLine($"Rank: {mastery.MasteryRank} ({mastery.masteryLevel:P1})");
            sb.AppendLine($"Usage: {mastery.usageCount} times");
            sb.AppendLine($"Success Rate: {mastery.SuccessRate:P1}");
            sb.AppendLine($"Power Bonus: +{(mastery.GetMasteryBonus() - 1f):P1}");
        }
        
        return sb.ToString();
    }
    #endregion
    
    #region Ability Gizmo Creation
    public static Command_Action CreateAbilityGizmo(CultivationAbilityDef abilityDef, Pawn pawn, CultivationComp comp)
    {
        var abilityUser = pawn.GetComp<CompAbilityUser>();
        var ability = new CultivationAbility(abilityDef, abilityUser);
        
        return new Command_Action
        {
            defaultLabel = abilityDef.abilityLabel ?? abilityDef.label,
            defaultDesc = BuildAbilityDescription(abilityDef, pawn, comp),
            icon = GetAbilityIcon(abilityDef),
            iconDrawScale = GetIconScale(abilityDef),
            
            action = () => ExecuteAbilityWithTargeting(abilityDef, pawn, ability),
            
            // Enhanced disable logic
            Disabled = !ability.CanCast(),
            disabledReason = ability.GetDisabledReason(),
            
            // Visual enhancements
            shrinkable = true,
            groupKey = GetAbilityGroupKey(abilityDef),
            order = GetAbilityOrder(abilityDef)
        };
    }
    
    private static string BuildAbilityDescription(CultivationAbilityDef abilityDef, Pawn pawn, CultivationComp comp)
    {
        var sb = new StringBuilder();
        
        // Base description
        sb.AppendLine(abilityDef.abilityDescription ?? abilityDef.description);
        sb.AppendLine();
        
        // Effects summary
        if (abilityDef.effects?.Count > 0)
        {
            sb.AppendLine("--- Effects ---");
            foreach (var effect in abilityDef.effects)
            {
                if (effect is AbilityEffectDef genericEffect)
                {
                    sb.AppendLine($"• {genericEffect.effectType}: {genericEffect.magnitude}");
                    if (!string.IsNullOrEmpty(genericEffect.damageType))
                        sb.AppendLine($"  Damage Type: {genericEffect.damageType}");
                    if (genericEffect.duration > 0)
                        sb.AppendLine($"  Duration: {(genericEffect.duration / 60f):F1}s");
                }
                else
                {
                    sb.AppendLine($"• {effect.GetType().Name}");
                }
            }
            sb.AppendLine();
        }
        
        // Requirements and costs
        sb.AppendLine("--- Requirements ---");
        sb.AppendLine($"Realm: {abilityDef.requiredRealm}");
        sb.AppendLine($"Stage: {abilityDef.requiredStage}");
        sb.AppendLine($"Qi Cost: {abilityDef.qiCost}");
        sb.AppendLine($"Cooldown: {(abilityDef.cooldownTicks / 60f):F1}s");
        sb.AppendLine($"Range: {abilityDef.range}");
        sb.AppendLine($"Target Type: {abilityDef.targetType}");
        
        return sb.ToString();
    }
    #endregion
    
    #region Visual Enhancements
    private static Texture2D GetSkillIcon(CultivationSkillDef skillDef)
    {
        // Try to get custom icon first
        if (skillDef.uiIcon != null) return skillDef.uiIcon;
        
        // Fallback to element-based icons
        return skillDef.associatedElement switch
        {
            QiType.Fire => ContentFinder<Texture2D>.Get("UI/Icons/Fire"),
            QiType.Water => ContentFinder<Texture2D>.Get("UI/Icons/Water"),
            QiType.Earth => ContentFinder<Texture2D>.Get("UI/Icons/Earth"),
            QiType.Metal => ContentFinder<Texture2D>.Get("UI/Icons/Metal"),
            QiType.Wood => ContentFinder<Texture2D>.Get("UI/Icons/Wood"),
            QiType.Lightning => ContentFinder<Texture2D>.Get("UI/Icons/Lightning"),
            _ => BaseContent.BadTex
        };
    }
    
    private static float GetIconScale(Def def)
    {
        // Scale icons based on power/rarity
        if (def is CultivationSkillDef skillDef)
        {
            return skillDef.requiredRealm switch
            {
                CultivationRealm.FoundationBuilding => 0.9f,
                CultivationRealm.CoreFormation => 1.0f,
                CultivationRealm.NascentSoul => 1.1f,
                _ => 1.2f
            };
        }
        
        return 1.0f;
    }
    
    private static int GetSkillGroupKey(CultivationSkillDef skillDef)
    {
        // Group skills by category for better organization
        return skillDef.category?.GetHashCode() ?? 0;
    }
    
    private static float GetSkillOrder(CultivationSkillDef skillDef)
    {
        // Order by realm requirement, then by stage
        return (int)skillDef.requiredRealm * 10 + skillDef.requiredStage;
    }
    #endregion
}
```

---

## ✨ **Visual Effects System**

### **Effect Management Architecture**

```csharp
/// <summary>
/// Centralized visual effects management for cultivation abilities
/// </summary>
public static class CultivationVFX
{
    private static readonly Dictionary<string, VFXProfile> effectProfiles = 
        new Dictionary<string, VFXProfile>();
    
    static CultivationVFX()
    {
        InitializeDefaultProfiles();
    }
    
    private static void InitializeDefaultProfiles()
    {
        // Qi-based effects
        effectProfiles["qi_burst"] = new VFXProfile
        {
            primaryFleck = FleckDefOf.PsycastPsychicEffect,
            particleCount = 15,
            baseColor = Color.cyan,
            duration = 60,
            spreadRadius = 2f
        };
        
        // Elemental effects
        effectProfiles["fire_strike"] = new VFXProfile
        {
            primaryFleck = FleckDefOf.FireGlow,
            particleCount = 8,
            baseColor = Color.red,
            duration = 30,
            spreadRadius = 1f
        };
        
        effectProfiles["lightning_strike"] = new VFXProfile
        {
            primaryFleck = FleckDefOf.LightningGlow,
            particleCount = 12,
            baseColor = Color.yellow,
            duration = 20,
            spreadRadius = 1.5f
        };
        
        // Defensive effects
        effectProfiles["shield_activation"] = new VFXProfile
        {
            primaryFleck = FleckDefOf.PsycastPsychicEffect,
            particleCount = 20,
            baseColor = ColorLibrary.Cyan,
            duration = 90,
            spreadRadius = 2.5f,
            pattern = VFXPattern.Circle
        };
    }
    
    public static void PlayEffect(string effectName, Vector3 position, Map map, float intensity = 1f)
    {
        if (!effectProfiles.TryGetValue(effectName, out var profile)) return;
        
        switch (profile.pattern)
        {
            case VFXPattern.Single:
                PlaySingleEffect(profile, position, map, intensity);
                break;
                
            case VFXPattern.Circle:
                PlayCircleEffect(profile, position, map, intensity);
                break;
                
            case VFXPattern.Line:
                PlayLineEffect(profile, position, map, intensity);
                break;
                
            case VFXPattern.Explosion:
                PlayExplosionEffect(profile, position, map, intensity);
                break;
        }
    }
    
    private static void PlayCircleEffect(VFXProfile profile, Vector3 center, Map map, float intensity)
    {
        int particleCount = Mathf.RoundToInt(profile.particleCount * intensity);
        float radius = profile.spreadRadius * intensity;
        
        for (int i = 0; i < particleCount; i++)
        {
            float angle = (360f / particleCount) * i;
            var offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );
            
            var particlePos = center + offset;
            
            // Create fleck with variation
            var fleck = FleckMaker.GetDataStatic(particlePos, map, profile.primaryFleck);
            fleck.rotationRate = Rand.Range(-45f, 45f);
            fleck.Scale = Rand.Range(0.8f, 1.2f) * intensity;
            fleck.exactLifetime = profile.duration;
            
            FleckMaker.ThrowGlow(particlePos, map, fleck.Scale, profile.baseColor);
        }
    }
    
    [System.Serializable]
    public class VFXProfile
    {
        public FleckDef primaryFleck;
        public int particleCount = 10;
        public Color baseColor = Color.white;
        public int duration = 60;
        public float spreadRadius = 1f;
        public VFXPattern pattern = VFXPattern.Single;
        public SoundDef sound;
    }
    
    public enum VFXPattern
    {
        Single,    // Single effect at target
        Circle,    // Circular pattern around point
        Line,      // Line from caster to target
        Explosion  // Radial explosion effect
    }
}
```

### **Skill-Specific Visual Effects**

```csharp
/// <summary>
/// Specialized visual effects for different skill types
/// </summary>
public static class SkillVFXSpecialized
{
    public static void CreateQiPunchEffect(Pawn caster, LocalTargetInfo target)
    {
        // Impact burst at target
        CultivationVFX.PlayEffect("qi_burst", target.Thing.DrawPos, caster.Map, 1.2f);
        
        // Qi trail from caster to target
        CreateQiTrail(caster.DrawPos, target.Thing.DrawPos, caster.Map);
        
        // Screen shake for impact
        Find.CameraDriver.shaker.DoShake(1f);
    }
    
    public static void CreateQiShieldEffect(Pawn pawn)
    {
        // Shield dome effect
        CultivationVFX.PlayEffect("shield_activation", pawn.DrawPos, pawn.Map, 1f);
        
        // Persistent shield glow
        var comp = pawn.GetComp<CultivationComp>();
        if (comp != null)
        {
            comp.StartPersistentEffect("shield_glow", 300); // 5 seconds
        }
    }
    
    public static void CreateSwordStrikeEffect(Pawn caster, LocalTargetInfo target)
    {
        // Sword qi emanation
        FleckMaker.ThrowLightningGlow(target.Cell.ToVector3(), caster.Map, 1.5f);
        
        // Cutting trail effect
        var positions = GetLinePositions(caster.DrawPos, target.Thing.DrawPos, 5);
        foreach (var pos in positions)
        {
            FleckMaker.ThrowAirPuffUp(pos, caster.Map);
        }
        
        // Impact sparks
        FleckMaker.ThrowMicroSparks(target.Thing.DrawPos, caster.Map);
    }
    
    private static void CreateQiTrail(Vector3 start, Vector3 end, Map map)
    {
        var positions = GetLinePositions(start, end, 8);
        
        for (int i = 0; i < positions.Count; i++)
        {
            float delay = i * 0.1f; // Stagger the trail
            
            // Create delayed fleck
            var fleck = FleckMaker.GetDataStatic(positions[i], map, FleckDefOf.PsycastPsychicEffect);
            fleck.exactLifetime = 30;
            fleck.Scale = 0.8f;
            
            // Delayed spawn
            map.flecks.CreateFleck(fleck);
        }
    }
    
    private static List<Vector3> GetLinePositions(Vector3 start, Vector3 end, int count)
    {
        var positions = new List<Vector3>();
        
        for (int i = 0; i <= count; i++)
        {
            float t = (float)i / count;
            positions.Add(Vector3.Lerp(start, end, t));
        }
        
        return positions;
    }
}
```

---

## 🎯 **User Experience Design**

### **Smart Targeting System**

```csharp
/// <summary>
/// Intelligent targeting system for different ability types
/// </summary>
public class Targeter_CultivationAbility : Targeter
{
    private readonly CultivationAbilityDef abilityDef;
    private readonly CultivationAbility ability;
    
    public Targeter_CultivationAbility(CultivationAbilityDef def, CultivationAbility ability)
    {
        this.abilityDef = def;
        this.ability = ability;
        
        ConfigureTargeting();
    }
    
    private void ConfigureTargeting()
    {
        switch (abilityDef.targetType)
        {
            case AbilityTargetType.Touch:
                targetValidator = (LocalTargetInfo target) => 
                    IsValidTouchTarget(target);
                break;
                
            case AbilityTargetType.Ranged:
                targetValidator = (LocalTargetInfo target) => 
                    IsValidRangedTarget(target);
                break;
                
            case AbilityTargetType.AreaAroundSelf:
                targetValidator = (LocalTargetInfo target) => 
                    IsValidAreaTarget(target);
                break;
                
            case AbilityTargetType.Enemy:
                targetValidator = (LocalTargetInfo target) => 
                    IsValidEnemyTarget(target);
                break;
        }
        
        // Set highlighting
        highlightAction = (LocalTargetInfo target) => 
            HighlightValidTargets(target);
        
        // Set confirmation action
        action = (LocalTargetInfo target) => 
            ConfirmAndExecute(target);
    }
    
    private bool IsValidTouchTarget(LocalTargetInfo target)
    {
        var caster = ability.comp.parent as Pawn;
        
        // Range check
        if (caster.Position.DistanceTo(target.Cell) > abilityDef.range)
            return false;
        
        // Line of sight check
        if (abilityDef.requiresLineOfSight && 
            !GenSight.LineOfSight(caster.Position, target.Cell, caster.Map))
            return false;
        
        // Target type validation
        switch (abilityDef.targetType)
        {
            case AbilityTargetType.Enemy:
                return target.Thing is Pawn targetPawn && targetPawn.HostileTo(caster);
                
            case AbilityTargetType.Ally:
                return target.Thing is Pawn targetPawn && !targetPawn.HostileTo(caster);
                
            default:
                return target.IsValid;
        }
    }
    
    private void HighlightValidTargets(LocalTargetInfo mouseTarget)
    {
        var caster = ability.comp.parent as Pawn;
        
        // Highlight range
        GenDraw.DrawRadiusRing(caster.Position, abilityDef.range);
        
        // Highlight valid targets in range
        var cellsInRange = GenRadial.RadialCellsAround(caster.Position, abilityDef.range, true);
        
        foreach (var cell in cellsInRange)
        {
            var thingsInCell = cell.GetThingList(caster.Map);
            foreach (var thing in thingsInCell)
            {
                if (IsValidTarget(new LocalTargetInfo(thing)))
                {
                    GenDraw.DrawTargetHighlight(new LocalTargetInfo(thing));
                }
            }
        }
        
        // Show area of effect if applicable
        if (HasAreaEffect())
        {
            DrawAreaOfEffect(mouseTarget);
        }
    }
    
    private void DrawAreaOfEffect(LocalTargetInfo target)
    {
        // Find AOE effects in ability
        foreach (var effect in abilityDef.effects)
        {
            if (effect is AbilityEffectDef genericEffect && 
                genericEffect.effectType.ToLower() == "aoe")
            {
                GenDraw.DrawRadiusRing(target.Cell, genericEffect.radius);
                break;
            }
        }
    }
}
```

### **Enhanced Feedback Systems**

```csharp
/// <summary>
/// Rich feedback system for cultivation actions
/// </summary>
public static class CultivationFeedback
{
    public static void ShowSkillFeedback(Pawn caster, string skillName, LocalTargetInfo target, bool success)
    {
        if (success)
        {
            // Success feedback
            Messages.Message($"{caster.LabelShort} executed {skillName}!", 
                           MessageTypeDefOf.PositiveEvent);
            
            // Floating text at target
            MoteMaker.ThrowText(target.Thing.DrawPos, target.Thing.Map, 
                              skillName, Color.cyan, 3.5f);
        }
        else
        {
            // Failure feedback
            Messages.Message($"{caster.LabelShort} failed to execute {skillName}!", 
                           MessageTypeDefOf.RejectInput);
            
            // Failure visual
            FleckMaker.ThrowMetaIcon(caster.Position, caster.Map, FleckDefOf.IncapIcon);
        }
    }
    
    public static void ShowDamageNumbers(Thing target, float damage, DamageDef damageType)
    {
        // Floating damage numbers
        var color = GetDamageColor(damageType);
        MoteMaker.ThrowText(target.DrawPos + Vector3.up, target.Map, 
                          $"{damage:F0}", color, 2f);
        
        // Screen shake based on damage
        var shakeIntensity = Mathf.Clamp(damage / 50f, 0.1f, 2f);
        Find.CameraDriver.shaker.DoShake(shakeIntensity);
    }
    
    public static void ShowHealingNumbers(Thing target, float healing)
    {
        // Green floating text for healing
        MoteMaker.ThrowText(target.DrawPos + Vector3.up, target.Map, 
                          $"+{healing:F0}", Color.green, 2f);
        
        // Healing sparkles
        FleckMaker.ThrowMetaIcon(target.Position, target.Map, FleckDefOf.Heart);
    }
    
    public static void ShowCriticalHit(Thing target, float damage)
    {
        // Special critical hit effects
        MoteMaker.ThrowText(target.DrawPos + Vector3.up, target.Map, 
                          $"CRITICAL! {damage:F0}", Color.yellow, 4f);
        
        // Enhanced visual effects
        FleckMaker.ThrowExplosionCell(target.Position, target.Map, FleckDefOf.ExplosionFlash, Color.yellow);
        
        // Stronger screen shake
        Find.CameraDriver.shaker.DoShake(2f);
    }
    
    private static Color GetDamageColor(DamageDef damageType)
    {
        if (damageType == DamageDefOf.Cut) return Color.red;
        if (damageType == DamageDefOf.Blunt) return Color.yellow;
        if (damageType == DamageDefOf.Burn) return Color.orange;
        if (damageType == DamageDefOf.Stab) return Color.magenta;
        return Color.white;
    }
}

[System.Serializable]
public class VFXProfile
{
    public FleckDef primaryFleck;
    public int particleCount = 10;
    public Color baseColor = Color.white;
    public int duration = 60;
    public float spreadRadius = 1f;
    public VFXPattern pattern = VFXPattern.Single;
    public SoundDef sound;
    public float screenShakeIntensity = 0f;
}
```

---

## 🎛️ **Customization Guide**

### **Adding Custom UI Elements**

#### **1. Custom Inspect Tabs**

```csharp
/// <summary>
/// Custom inspect tab for cultivation information
/// </summary>
public class InspectTab_Cultivation : InspectTabBase
{
    protected override string GetLabel(Thing thing) => "Cultivation";
    
    protected override void FillTab()
    {
        var pawn = SelThing as Pawn;
        var comp = pawn?.GetComp<CultivationComp>();
        if (comp?.cultivationData == null) return;
        
        var rect = new Rect(0f, 0f, size.x, size.y);
        var listing = new Listing_Standard();
        listing.Begin(rect);
        
        // Basic info section
        DrawBasicInfo(listing, comp);
        
        // Skills section
        DrawSkillsSection(listing, comp);
        
        // Artifacts section
        DrawArtifactsSection(listing, pawn);
        
        listing.End();
    }
    
    private void DrawBasicInfo(Listing_Standard listing, CultivationComp comp)
    {
        var data = comp.cultivationData;
        
        listing.Label($"Cultivation Realm: {data.currentRealm}");
        listing.Label($"Current Stage: {data.currentStage}");
        
        // Qi bar
        var qiRect = listing.GetRect(20f);
        Widgets.FillableBar(qiRect, data.currentQi / data.maxQi, 
                           Widgets.WindowBGFillColor, Color.cyan, true);
        Widgets.Label(qiRect, $"Qi: {data.currentQi:F1}/{data.maxQi:F1}");
        
        // Progress bar
        var progressRect = listing.GetRect(20f);
        var progressPercent = data.cultivationPoints / data.GetRequiredPoints();
        Widgets.FillableBar(progressRect, progressPercent,
                           Widgets.WindowBGFillColor, Color.gold, true);
        Widgets.Label(progressRect, $"Progress: {progressPercent:P1}");
        
        listing.Gap();
    }
    
    private void DrawSkillsSection(Listing_Standard listing, CultivationComp comp)
    {
        listing.Label("Known Skills:");
        
        foreach (var skillName in comp.knownSkills)
        {
            var skillRect = listing.GetRect(25f);
            
            // Skill icon and name
            var iconRect = new Rect(skillRect.x, skillRect.y, 24f, 24f);
            var labelRect = new Rect(skillRect.x + 30f, skillRect.y, skillRect.width - 30f, skillRect.height);
            
            // Get skill info
            var skillDef = CultivationCache.GetSkillDef(skillName);
            var abilityDef = CultivationCache.GetAbilityDef(skillName);
            
            Texture2D icon = BaseContent.BadTex;
            string label = skillName;
            
            if (skillDef != null)
            {
                icon = skillDef.uiIcon ?? BaseContent.BadTex;
                label = skillDef.label;
            }
            else if (abilityDef != null)
            {
                icon = abilityDef.uiIcon ?? BaseContent.BadTex;
                label = abilityDef.abilityLabel ?? abilityDef.label;
            }
            
            // Draw icon and label
            Widgets.DrawTextureFitted(iconRect, icon, 1f);
            Widgets.Label(labelRect, label);
            
            // Mastery info
            if (comp.skillMasteryData?.TryGetValue(skillName, out var mastery) == true)
            {
                var masteryRect = new Rect(labelRect.x, labelRect.y + 12f, labelRect.width, 12f);
                Widgets.Label(masteryRect, $"Mastery: {mastery.MasteryRank} ({mastery.masteryLevel:P0})");
            }
        }
    }
}
```

#### **2. Custom Gizmo Styles**

```csharp
/// <summary>
/// Stylized gizmo commands for cultivation abilities
/// </summary>
public class Command_CultivationSkill : Command_Action
{
    public CultivationSkillDef skillDef;
    public Pawn caster;
    public float masteryLevel = 0f;
    
    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        // Custom background based on skill element
        var bgColor = GetElementColor(skillDef.associatedElement);
        var originalColor = GUI.color;
        
        // Glow effect for ready skills
        if (!Disabled)
        {
            GUI.color = Color.Lerp(originalColor, bgColor, 0.3f);
            
            // Pulse effect for high mastery
            if (masteryLevel > 0.8f)
            {
                var pulse = Mathf.Sin(Time.time * 3f) * 0.2f + 0.8f;
                GUI.color = Color.Lerp(GUI.color, Color.white, pulse * 0.3f);
            }
        }
        
        var result = base.GizmoOnGUI(topLeft, maxWidth, parms);
        
        // Draw mastery indicators
        if (masteryLevel > 0f)
        {
            DrawMasteryIndicator(topLeft, masteryLevel);
        }
        
        // Draw cooldown overlay
        if (GetCooldownPercent() > 0f)
        {
            DrawCooldownOverlay(topLeft, maxWidth);
        }
        
        GUI.color = originalColor;
        return result;
    }
    
    private void DrawMasteryIndicator(Vector2 topLeft, float mastery)
    {
        var indicatorRect = new Rect(topLeft.x + 2f, topLeft.y + 2f, 8f, 60f * mastery);
        
        // Mastery bar on left side
        var color = Color.Lerp(Color.gray, Color.gold, mastery);
        Widgets.DrawBoxSolid(indicatorRect, color);
    }
    
    private void DrawCooldownOverlay(Vector2 topLeft, float maxWidth)
    {
        var cooldownPercent = GetCooldownPercent();
        var overlayRect = new Rect(topLeft.x, topLeft.y, maxWidth, 75f);
        
        // Semi-transparent overlay
        Widgets.DrawBoxSolid(overlayRect, new Color(0f, 0f, 0f, 0.5f * cooldownPercent));
        
        // Cooldown text
        var remainingTicks = GetRemainingCooldownTicks();
        var timeText = $"{(remainingTicks / 60f):F1}s";
        var textRect = new Rect(topLeft.x, topLeft.y + 55f, maxWidth, 20f);
        
        var oldColor = GUI.color;
        GUI.color = Color.white;
        Widgets.Label(textRect, timeText);
        GUI.color = oldColor;
    }
    
    private Color GetElementColor(QiType element)
    {
        return element switch
        {
            QiType.Fire => Color.red,
            QiType.Water => Color.blue,
            QiType.Earth => new Color(0.6f, 0.4f, 0.2f),
            QiType.Metal => Color.gray,
            QiType.Wood => Color.green,
            QiType.Lightning => Color.yellow,
            _ => Color.cyan
        };
    }
}
```

### **Accessibility Features**

```csharp
/// <summary>
/// Accessibility enhancements for cultivation UI
/// </summary>
public static class CultivationAccessibility
{
    public static bool colorBlindMode = false;
    public static bool highContrastMode = false;
    public static bool showDetailedTooltips = true;
    public static float uiScale = 1f;
    
    public static Color GetAccessibleColor(Color originalColor, string context)
    {
        if (!colorBlindMode) return originalColor;
        
        // Convert to colorblind-friendly alternatives
        return context switch
        {
            "fire" => new Color(1f, 0.5f, 0f), // Orange instead of red
            "water" => new Color(0f, 0.8f, 1f), // Light blue
            "earth" => new Color(0.8f, 0.6f, 0.4f), // Brown
            "success" => new Color(0f, 0.8f, 0.2f), // Bright green
            "danger" => new Color(1f, 0.3f, 0f), // Orange-red
            _ => originalColor
        };
    }
    
    public static string EnhanceTooltip(string baseTooltip, object context)
    {
        if (!showDetailedTooltips) return baseTooltip;
        
        var sb = new StringBuilder(baseTooltip);
        
        // Add keyboard shortcuts
        if (context is CultivationSkillDef skillDef)
        {
            sb.AppendLine();
            sb.AppendLine($"Hotkey: {GetSkillHotkey(skillDef.defName)}");
        }
        
        // Add performance impact info
        sb.AppendLine();
        sb.AppendLine("Performance Impact: Low");
        
        return sb.ToString();
    }
    
    private static string GetSkillHotkey(string skillDefName)
    {
        // Map skills to hotkeys (configurable)
        return skillDefName switch
        {
            "QiPunch" => "Q",
            "QiShield" => "E", 
            "Ability_SwordStrike" => "R",
            _ => "None"
        };
    }
}
```

---

## 📊 **Performance Optimization**

### **UI Performance Best Practices**

#### **1. Gizmo Caching System**

```csharp
public static class GizmoPerformanceManager
{
    private static readonly Dictionary<(Pawn, int), List<Gizmo>> frameCache = 
        new Dictionary<(Pawn, int), List<Gizmo>>();
    
    private static readonly HashSet<Pawn> dirtyPawns = new HashSet<Pawn>();
    
    public static void MarkPawnDirty(Pawn pawn)
    {
        dirtyPawns.Add(pawn);
    }
    
    public static List<Gizmo> GetCachedGizmos(Pawn pawn, Func<List<Gizmo>> generator)
    {
        var currentFrame = Time.frameCount;
        var key = (pawn, currentFrame);
        
        // Return cached if available and pawn not dirty
        if (!dirtyPawns.Contains(pawn) && frameCache.TryGetValue(key, out var cached))
        {
            return cached;
        }
        
        // Generate new gizmos
        var gizmos = generator();
        
        // Cache result
        frameCache[key] = gizmos;
        dirtyPawns.Remove(pawn);
        
        // Cleanup old cache entries
        CleanupFrameCache(currentFrame);
        
        return gizmos;
    }
    
    private static void CleanupFrameCache(int currentFrame)
    {
        if (currentFrame % 60 != 0) return; // Cleanup every second
        
        var keysToRemove = frameCache.Keys.Where(k => currentFrame - k.Item2 > 60).ToList();
        foreach (var key in keysToRemove)
        {
            frameCache.Remove(key);
        }
    }
}
```

#### **2. Efficient Icon Loading**

```csharp
public static class IconManager
{
    private static readonly Dictionary<string, Texture2D> iconCache = 
        new Dictionary<string, Texture2D>();
    
    private static readonly Dictionary<string, Texture2D> generatedIcons = 
        new Dictionary<string, Texture2D>();
    
    public static Texture2D GetSkillIcon(CultivationSkillDef skillDef)
    {
        var key = $"skill_{skillDef.defName}";
        
        if (iconCache.TryGetValue(key, out var cached))
            return cached;
        
        // Try to load custom icon
        Texture2D icon = null;
        if (!string.IsNullOrEmpty(skillDef.iconPath))
        {
            icon = ContentFinder<Texture2D>.Get(skillDef.iconPath, false);
        }
        
        // Fallback to generated icon
        if (icon == null)
        {
            icon = GenerateSkillIcon(skillDef);
        }
        
        iconCache[key] = icon;
        return icon;
    }
    
    private static Texture2D GenerateSkillIcon(CultivationSkillDef skillDef)
    {
        var key = $"generated_skill_{skillDef.defName}";
        
        if (generatedIcons.TryGetValue(key, out var cached))
            return cached;
        
        // Generate procedural icon based on skill properties
        var baseIcon = GetElementBaseIcon(skillDef.associatedElement);
        var colorizedIcon = ColorizeIcon(baseIcon, GetElementColor(skillDef.associatedElement));
        
        generatedIcons[key] = colorizedIcon;
        return colorizedIcon;
    }
    
    private static Texture2D GetElementBaseIcon(QiType element)
    {
        return element switch
        {
            QiType.Fire => ContentFinder<Texture2D>.Get("UI/Icons/BaseFlame"),
            QiType.Water => ContentFinder<Texture2D>.Get("UI/Icons/BaseWave"),
            QiType.Earth => ContentFinder<Texture2D>.Get("UI/Icons/BaseRock"),
            QiType.Metal => ContentFinder<Texture2D>.Get("UI/Icons/BaseMetal"),
            QiType.Wood => ContentFinder<Texture2D>.Get("UI/Icons/BaseLeaf"),
            QiType.Lightning => ContentFinder<Texture2D>.Get("UI/Icons/BaseBolt"),
            _ => BaseContent.BadTex
        };
    }
    
    private static Texture2D ColorizeIcon(Texture2D baseTexture, Color tintColor)
    {
        // Create colorized version of base texture
        // Implementation would involve texture manipulation
        // For now, return base texture
        return baseTexture;
    }
}
```

### **Theme System**

```csharp
/// <summary>
/// Customizable theme system for cultivation UI
/// </summary>
public static class CultivationThemes
{
    public static CultivationTheme currentTheme = CultivationTheme.Classic;
    
    public enum CultivationTheme
    {
        Classic,    // Default cyan/blue theme
        Fire,       // Red/orange theme
        Nature,     // Green/brown theme
        Lightning,  // Yellow/purple theme
        Ice,        // Blue/white theme
        Dark        // Black/red theme
    }
    
    public static ThemeProfile GetThemeProfile(CultivationTheme theme)
    {
        return theme switch
        {
            CultivationTheme.Classic => new ThemeProfile
            {
                primaryColor = Color.cyan,
                secondaryColor = Color.blue,
                accentColor = Color.white,
                backgroundTint = new Color(0.1f, 0.3f, 0.4f, 0.8f)
            },
            
            CultivationTheme.Fire => new ThemeProfile
            {
                primaryColor = Color.red,
                secondaryColor = Color.yellow,
                accentColor = Color.orange,
                backgroundTint = new Color(0.4f, 0.2f, 0.1f, 0.8f)
            },
            
            CultivationTheme.Nature => new ThemeProfile
            {
                primaryColor = Color.green,
                secondaryColor = new Color(0.4f, 0.8f, 0.2f),
                accentColor = Color.yellow,
                backgroundTint = new Color(0.2f, 0.4f, 0.1f, 0.8f)
            },
            
            _ => GetThemeProfile(CultivationTheme.Classic)
        };
    }
    
    [System.Serializable]
    public class ThemeProfile
    {
        public Color primaryColor;
        public Color secondaryColor;
        public Color accentColor;
        public Color backgroundTint;
        public float glowIntensity = 1f;
        public float animationSpeed = 1f;
    }
    
    public static void ApplyThemeToGizmo(Command_Action gizmo, CultivationTheme theme)
    {
        var profile = GetThemeProfile(theme);
        
        // Modify gizmo appearance based on theme
        // This would require extending the gizmo rendering system
        // For now, we can modify color properties
        
        if (gizmo is Command_CultivationSkill cultSkill)
        {
            cultSkill.themeProfile = profile;
        }
    }
}
```

---

## 🔧 **Integration Testing UI**

### **Debug UI Panel**

```csharp
/// <summary>
/// In-game debug panel for cultivation system testing
/// </summary>
public class Window_CultivationDebug : Window
{
    private Vector2 scrollPos = Vector2.zero;
    private Pawn selectedPawn;
    
    public override Vector2 InitialSize => new Vector2(600f, 500f);
    
    public override void DoWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        var scrollRect = new Rect(0f, 0f, inRect.width - 20f, inRect.height - 20f);
        
        Widgets.BeginScrollView(inRect, ref scrollPos, scrollRect);
        listing.Begin(scrollRect);
        
        // Pawn selection
        DrawPawnSelector(listing);
        
        if (selectedPawn != null)
        {
            listing.Gap();
            DrawCultivationInfo(listing);
            
            listing.Gap();
            DrawSkillTesting(listing);
            
            listing.Gap();
            DrawArtifactTesting(listing);
        }
        
        listing.End();
        Widgets.EndScrollView();
    }
    
    private void DrawPawnSelector(Listing_Standard listing)
    {
        listing.Label("Select Pawn for Testing:");
        
        foreach (var pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
        {
            if (pawn.GetComp<CultivationComp>() != null)
            {
                if (listing.ButtonText($"{pawn.LabelShort} ({pawn.Faction?.Name ?? "None"})"))
                {
                    selectedPawn = pawn;
                }
            }
        }
    }
    
    private void DrawCultivationInfo(Listing_Standard listing)
    {
        var comp = selectedPawn.GetComp<CultivationComp>();
        var data = comp.cultivationData;
        
        listing.Label($"Cultivation Status: {selectedPawn.LabelShort}");
        listing.Label($"Realm: {data.currentRealm} (Stage {data.currentStage})");
        listing.Label($"Qi: {data.currentQi:F1}/{data.maxQi:F1}");
        
        // Quick actions
        if (listing.ButtonText("Add 50 Qi"))
        {
            data.currentQi = Mathf.Min(data.maxQi, data.currentQi + 50f);
        }
        
        if (listing.ButtonText("Advance Stage"))
        {
            if (data.currentStage < 9)
            {
                data.currentStage++;
                data.cultivationPoints = 0f;
            }
            else if (data.currentRealm < CultivationRealm.Immortal)
            {
                data.currentRealm = (CultivationRealm)((int)data.currentRealm + 1);
                data.currentStage = 1;
                data.cultivationPoints = 0f;
            }
        }
    }
    
    private void DrawSkillTesting(Listing_Standard listing)
    {
        listing.Label("Skill Testing:");
        
        var comp = selectedPawn.GetComp<CultivationComp>();
        
        // Test all known skills
        foreach (var skillName in comp.GetAllAvailableSkills())
        {
            var skillDef = CultivationCache.GetSkillDef(skillName);
            var abilityDef = CultivationCache.GetAbilityDef(skillName);
            
            string testLabel = $"Test {skillName}";
            if (skillDef != null) testLabel += " (Skill)";
            else if (abilityDef != null) testLabel += " (Ability)";
            
            if (listing.ButtonText(testLabel))
            {
                TestSkillExecution(skillName, selectedPawn);
            }
        }
        
        // Add new skill for testing
        if (listing.ButtonText("Grant Test Skill"))
        {
            comp.knownSkills.Add("QiPunch");
            MarkGizmosCacheInvalid(selectedPawn);
        }
    }
    
    private void TestSkillExecution(string skillName, Pawn pawn)
    {
        // Create dummy target for testing
        var target = new LocalTargetInfo(pawn); // Self-target for testing
        
        var skillDef = CultivationCache.GetSkillDef(skillName);
        if (skillDef != null)
        {
            var worker = skillDef.GetSkillWorker();
            if (worker.CanExecute(pawn, target))
            {
                worker.ExecuteSkillEffect(pawn, target);
                Messages.Message($"Executed skill: {skillName}", MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                Messages.Message($"Cannot execute skill: {worker.GetDisabledReason(pawn, target)}", 
                               MessageTypeDefOf.RejectInput);
            }
        }
        
        var abilityDef = CultivationCache.GetAbilityDef(skillName);
        if (abilityDef != null)
        {
            var abilityUser = pawn.GetComp<CompAbilityUser>();
            var ability = new CultivationAbility(abilityDef, abilityUser);
            
            if (ability.CanCast(target))
            {
                ability.TryCast(target);
                Messages.Message($"Executed ability: {skillName}", MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                Messages.Message($"Cannot execute ability: {ability.GetDisabledReason()}", 
                               MessageTypeDefOf.RejectInput);
            }
        }
    }
}
```

---

## 🎯 **Best Practices Summary**

### **UI Design Guidelines**

1. **Consistency**: Use consistent styling across all cultivation gizmos
2. **Feedback**: Provide immediate visual and audio feedback for all actions
3. **Accessibility**: Support colorblind users and different UI scales
4. **Performance**: Cache expensive operations and limit UI updates
5. **Extensibility**: Design UI components to be easily extended by other mods

### **Implementation Checklist**

- [ ] **Gizmo Creation**
  - [ ] Proper icon assignment
  - [ ] Descriptive tooltips
  - [ ] Correct disabled states
  - [ ] Appropriate grouping/ordering

- [ ] **Visual Effects**
  - [ ] Skill-appropriate effects
  - [ ] Performance-optimized particles
  - [ ] Consistent color scheme
  - [ ] Proper timing and duration

- [ ] **User Feedback**
  - [ ] Success/failure messages
  - [ ] Floating damage/healing numbers
  - [ ] Audio cues for important events
  - [ ] Clear error messages

- [ ] **Accessibility**
  - [ ] Colorblind-friendly color choices
  - [ ] Keyboard shortcuts support
  - [ ] High contrast mode compatibility
  - [ ] Scalable UI elements

---

**UI System Version**: 2.0  
**Last Updated**: September 2025  
**Tested With**: RimWorld 1.6
