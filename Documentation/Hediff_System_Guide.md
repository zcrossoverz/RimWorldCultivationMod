# RimWorld Hediff System - Complete Guide

## Tổng quan về Hediff System

**Hediff** (Health Difference) là hệ thống quản lý tất cả các trạng thái sức khỏe, buff, debuff, bệnh tật, và thay đổi thống kê trong RimWorld.

## 1. Định nghĩa Hediff trong XML

### 1.1. Hediff cơ bản (Basic Hediff)

```xml
<!-- File: Defs/HediffDefs/CultivationHediffs.xml -->
<Defs>
  <HediffDef>
    <defName>QiEnhancement</defName>
    <hediffClass>HediffWithComps</hediffClass>
    <label>qi enhancement</label>
    <description>Enhanced by cultivation energy, improving physical capabilities.</description>
    <labelNoun>qi enhancement</labelNoun>
    <defaultLabelColor>(0.2, 0.8, 1)</defaultLabelColor>
    
    <!-- Hiển thị -->
    <scenarioCanAdd>false</scenarioCanAdd>
    <maxSeverity>1.0</maxSeverity>
    <isBad>false</isBad>
    <tendable>false</tendable>
    
    <!-- Thống kê -->
    <stages>
      <li>
        <statOffsets>
          <MoveSpeed>0.2</MoveSpeed>
          <WorkSpeedGlobal>0.15</WorkSpeedGlobal>
        </statOffsets>
        <statFactors>
          <MeleeDamageFactor>1.2</MeleeDamageFactor>
          <MeleeHitChance>1.1</MeleeHitChance>
        </statFactors>
        <capacityOffsets>
          <Consciousness>0.1</Consciousness>
          <Manipulation>0.05</Manipulation>
        </capacityOffsets>
      </li>
    </stages>
  </HediffDef>
</Defs>
```

### 1.2. Hediff với nhiều mức độ (Multi-stage Hediff)

```xml
<HediffDef>
  <defName>CultivationRealm</defName>
  <hediffClass>TuTien.CultivationRealmHediff</hediffClass>
  <label>cultivation realm</label>
  <description>Current cultivation realm and its effects.</description>
  <labelNoun>cultivation realm</labelNoun>
  <defaultLabelColor>(1, 0.8, 0.2)</defaultLabelColor>
  
  <maxSeverity>9.0</maxSeverity>
  <scenarioCanAdd>false</scenarioCanAdd>
  <isBad>false</isBad>
  
  <stages>
    <!-- Qi Gathering Realm (1-3) -->
    <li>
      <minSeverity>1.0</minSeverity>
      <label>qi gathering (early)</label>
      <statOffsets>
        <MoveSpeed>0.1</MoveSpeed>
      </statOffsets>
    </li>
    <li>
      <minSeverity>2.0</minSeverity>
      <label>qi gathering (middle)</label>
      <statOffsets>
        <MoveSpeed>0.15</MoveSpeed>
        <WorkSpeedGlobal>0.05</WorkSpeedGlobal>
      </statOffsets>
    </li>
    <li>
      <minSeverity>3.0</minSeverity>
      <label>qi gathering (late)</label>
      <statOffsets>
        <MoveSpeed>0.2</MoveSpeed>
        <WorkSpeedGlobal>0.1</WorkSpeedGlobal>
      </statOffsets>
      <statFactors>
        <MeleeDamageFactor>1.1</MeleeDamageFactor>
      </statFactors>
    </li>
    
    <!-- Foundation Realm (4-6) -->
    <li>
      <minSeverity>4.0</minSeverity>
      <label>foundation (early)</label>
      <statOffsets>
        <MoveSpeed>0.25</MoveSpeed>
        <WorkSpeedGlobal>0.15</WorkSpeedGlobal>
      </statOffsets>
      <statFactors>
        <MeleeDamageFactor>1.2</MeleeDamageFactor>
        <MeleeHitChance>1.05</MeleeHitChance>
      </statFactors>
    </li>
    <!-- Thêm các stage khác... -->
  </stages>
</HediffDef>
```

### 1.3. Hediff tạm thời (Temporary Hediff)

```xml
<HediffDef>
  <defName>QiOverload</defName>
  <hediffClass>HediffWithComps</hediffClass>
  <label>qi overload</label>
  <description>Too much qi energy causing temporary fatigue.</description>
  <labelNoun>qi overload</labelNoun>
  <defaultLabelColor>(1, 0.5, 0)</defaultLabelColor>
  
  <maxSeverity>1.0</maxSeverity>
  <scenarioCanAdd>false</scenarioCanAdd>
  <isBad>true</isBad>
  
  <!-- Tự động biến mất sau thời gian -->
  <comps>
    <li Class="HediffCompProperties_Disappears">
      <disappearsAfterTicks>30000</disappearsAfterTicks> <!-- 5 minutes -->
      <showRemainingTime>true</showRemainingTime>
    </li>
  </comps>
  
  <stages>
    <li>
      <statOffsets>
        <MoveSpeed>-0.3</MoveSpeed>
        <WorkSpeedGlobal>-0.2</WorkSpeedGlobal>
      </statOffsets>
      <capacityOffsets>
        <Consciousness>-0.2</Consciousness>
      </capacityOffsets>
    </li>
  </stages>
</HediffDef>
```

## 2. Custom Hediff Classes trong C#

### 2.1. Hediff cơ bản với logic tùy chỉnh

```csharp
// File: Source/TuTien/Hediffs/CultivationRealmHediff.cs
using Verse;
using RimWorld;

namespace TuTien
{
    public class CultivationRealmHediff : HediffWithComps
    {
        public override string LabelInBrackets
        {
            get
            {
                var comp = pawn.GetComp<CultivationComp>();
                if (comp?.cultivationData != null)
                {
                    return $"{comp.cultivationData.currentRealm} Stage {comp.cultivationData.currentStage}";
                }
                return base.LabelInBrackets;
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            // Update severity based on cultivation data
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData != null)
            {
                // Calculate severity from realm and stage
                float newSeverity = (int)comp.cultivationData.currentRealm + 
                                  (comp.cultivationData.currentStage - 1) * 0.33f;
                
                if (Math.Abs(this.Severity - newSeverity) > 0.01f)
                {
                    this.Severity = newSeverity;
                }
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            Log.Message($"[TuTien] Added cultivation realm hediff to {pawn.Name}");
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            Log.Message($"[TuTien] Removed cultivation realm hediff from {pawn.Name}");
        }
    }
}
```

### 2.2. Hediff với Component tùy chỉnh

```csharp
// File: Source/TuTien/Hediffs/QiShieldHediff.cs
using Verse;
using RimWorld;

namespace TuTien
{
    public class QiShieldHediff : HediffWithComps
    {
        private int shieldHitPoints = 100;
        private int maxShieldHitPoints = 100;

        public int ShieldHitPoints => shieldHitPoints;
        public float ShieldPercent => (float)shieldHitPoints / maxShieldHitPoints;

        public override string LabelInBrackets => $"{shieldHitPoints}/{maxShieldHitPoints}";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shieldHitPoints, "shieldHitPoints", 100);
            Scribe_Values.Look(ref maxShieldHitPoints, "maxShieldHitPoints", 100);
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            // Set initial shield based on cultivation level
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData != null)
            {
                maxShieldHitPoints = (int)(comp.cultivationData.maxQi * 0.5f);
                shieldHitPoints = maxShieldHitPoints;
            }
        }

        public bool AbsorbDamage(ref DamageInfo dinfo)
        {
            if (shieldHitPoints <= 0) return false;

            int damage = Mathf.RoundToInt(dinfo.Amount);
            shieldHitPoints = Mathf.Max(0, shieldHitPoints - damage);

            // Visual effect
            EffectMaker.MakeEffect(EffecterDefOf.Shield_Break, pawn.Position, pawn.Map);

            if (shieldHitPoints <= 0)
            {
                // Shield broken
                Messages.Message($"{pawn.Name}'s qi shield has been shattered!", 
                    MessageTypeDefOf.NegativeHealthEvent);
                pawn.health.RemoveHediff(this);
                return false;
            }

            // Partially absorbed
            dinfo.SetAmount(damage * 0.5f); // Reduce damage by 50%
            return true;
        }

        public override void Tick()
        {
            base.Tick();

            // Shield regeneration every 2 seconds
            if (pawn.IsHashIntervalTick(120))
            {
                if (shieldHitPoints < maxShieldHitPoints)
                {
                    var comp = pawn.GetComp<CultivationComp>();
                    if (comp?.cultivationData != null && comp.cultivationData.currentQi > 10)
                    {
                        shieldHitPoints = Mathf.Min(maxShieldHitPoints, shieldHitPoints + 5);
                        comp.cultivationData.currentQi -= 2; // Cost qi to regenerate
                    }
                }
            }
        }
    }
}
```

## 3. Cách thêm Hediff bằng C# Script

### 3.1. Thêm Hediff đơn giản

```csharp
public static void AddBuffToPawn(Pawn pawn, string hediffDefName, float severity = 1.0f)
{
    var hediffDef = DefDatabase<HediffDef>.GetNamed(hediffDefName);
    if (hediffDef != null)
    {
        var hediff = HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.Severity = severity;
        pawn.health.AddHediff(hediff);
        
        Log.Message($"Added {hediffDefName} to {pawn.Name} with severity {severity}");
    }
}

// Sử dụng
AddBuffToPawn(pawn, "QiEnhancement", 1.0f);
```

### 3.2. Thêm Hediff với thời gian tồn tại

```csharp
public static void AddTemporaryBuff(Pawn pawn, string hediffDefName, int durationTicks, float severity = 1.0f)
{
    var hediffDef = DefDatabase<HediffDef>.GetNamed(hediffDefName);
    if (hediffDef != null)
    {
        var hediff = HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.Severity = severity;
        
        // Thêm component tự động biến mất
        var comp = hediff.TryGetComp<HediffComp_Disappears>();
        if (comp != null)
        {
            comp.ticksToDisappear = durationTicks;
        }
        
        pawn.health.AddHediff(hediff);
    }
}

// Sử dụng: buff kéo dài 5 phút (18000 ticks)
AddTemporaryBuff(pawn, "QiBoost", 18000, 0.8f);
```

### 3.3. Thêm Hediff với vị trí cụ thể

```csharp
public static void AddHediffToBodyPart(Pawn pawn, string hediffDefName, BodyPartDef bodyPartDef, float severity = 1.0f)
{
    var hediffDef = DefDatabase<HediffDef>.GetNamed(hediffDefName);
    var bodyPart = pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.def == bodyPartDef);
    
    if (hediffDef != null && bodyPart != null)
    {
        var hediff = HediffMaker.MakeHediff(hediffDef, pawn, bodyPart);
        hediff.Severity = severity;
        pawn.health.AddHediff(hediff);
    }
}

// Sử dụng: thêm buff vào tay phải
AddHediffToBodyPart(pawn, "EnhancedArm", BodyPartDefOf.Arm, 1.0f);
```

### 3.4. Kiểm tra và cập nhật Hediff

```csharp
public static void UpdateOrAddHediff(Pawn pawn, string hediffDefName, float newSeverity)
{
    var hediffDef = DefDatabase<HediffDef>.GetNamed(hediffDefName);
    var existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
    
    if (existingHediff != null)
    {
        // Cập nhật severity
        existingHediff.Severity = newSeverity;
    }
    else
    {
        // Thêm mới
        var hediff = HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.Severity = newSeverity;
        pawn.health.AddHediff(hediff);
    }
}
```

### 3.5. Xóa Hediff

```csharp
public static void RemoveHediff(Pawn pawn, string hediffDefName)
{
    var hediffDef = DefDatabase<HediffDef>.GetNamed(hediffDefName);
    var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
    
    if (hediff != null)
    {
        pawn.health.RemoveHediff(hediff);
        Log.Message($"Removed {hediffDefName} from {pawn.Name}");
    }
}
```

## 4. Kịch bản ứng dụng thực tế

### 4.1. Skill Buff System

```csharp
public class CultivationSkillEffects
{
    public static void ApplyQiBoost(Pawn pawn, int duration = 18000)
    {
        // Remove existing boost
        RemoveHediff(pawn, "QiBoost");
        
        // Add new boost
        var hediffDef = DefDatabase<HediffDef>.GetNamed("QiBoost");
        var hediff = HediffMaker.MakeHediff(hediffDef, pawn) as HediffWithComps;
        
        // Set duration
        hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = duration;
        
        // Set severity based on cultivation level
        var comp = pawn.GetComp<CultivationComp>();
        float severity = comp?.cultivationData != null ? 
            (float)comp.cultivationData.currentRealm / 10f : 0.1f;
        hediff.Severity = severity;
        
        pawn.health.AddHediff(hediff);
        
        // Visual effect
        MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, "Qi Boost!", Color.cyan);
    }

    public static void ApplyQiOverload(Pawn pawn)
    {
        var hediffDef = DefDatabase<HediffDef>.GetNamed("QiOverload");
        var existing = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
        
        if (existing != null)
        {
            // Stack overload
            existing.Severity = Mathf.Min(1.0f, existing.Severity + 0.2f);
        }
        else
        {
            var hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            hediff.Severity = 0.3f;
            pawn.health.AddHediff(hediff);
        }
        
        Messages.Message($"{pawn.Name} is suffering from qi overload!", 
            MessageTypeDefOf.NegativeHealthEvent);
    }
}
```

### 4.2. Combat Integration

```csharp
[HarmonyPatch(typeof(Thing), "TakeDamage")]
public static class Combat_TakeDamage_Patch
{
    public static bool Prefix(Thing __instance, DamageInfo dinfo)
    {
        if (__instance is Pawn pawn)
        {
            // Check for qi shield
            var qiShield = pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamed("QiShield")) as QiShieldHediff;
            
            if (qiShield != null)
            {
                if (qiShield.AbsorbDamage(ref dinfo))
                {
                    // Shield absorbed some/all damage
                    if (dinfo.Amount <= 0)
                    {
                        return false; // Completely blocked
                    }
                }
            }
        }
        
        return true; // Continue with original damage
    }
}
```

### 4.3. Cultivation Progression Effects

```csharp
public class CultivationProgressionEffects
{
    public static void OnRealmBreakthrough(Pawn pawn, CultivationRealm oldRealm, CultivationRealm newRealm)
    {
        // Update realm hediff
        UpdateOrAddHediff(pawn, "CultivationRealm", (float)newRealm);
        
        // Add temporary breakthrough euphoria
        AddTemporaryBuff(pawn, "BreakthroughEuphoria", 60000, 1.0f); // 1 day
        
        // Heal some injuries
        var injuries = pawn.health.hediffSet.GetHediffs<Hediff_Injury>();
        foreach (var injury in injuries.Take(2)) // Heal 2 wounds
        {
            injury.Heal(injury.Severity * 0.5f); // Heal 50%
        }
        
        // Special effects for major breakthroughs
        if ((int)newRealm > (int)oldRealm)
        {
            // Major realm breakthrough
            AddTemporaryBuff(pawn, "MajorBreakthroughAura", 180000, 1.0f); // 3 days
            
            // Visual explosion effect
            GenExplosion.DoExplosion(
                pawn.Position, pawn.Map, 3f, 
                DamageDefOf.EMP, // Non-damaging
                null, 0, -1f, null, null, null, null, null, 0f, 1,
                false, null, 0f, 1, 0.5f, false);
        }
    }
}
```

## 5. Debug và Testing

### 5.1. Debug Commands

```csharp
[DebugAction("Tu Tien", "Add Qi Enhancement", allowedGameStates = AllowedGameStates.PlayingOnMap)]
private static void AddQiEnhancement()
{
    Pawn pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
        .OfType<Pawn>().FirstOrDefault();
    
    if (pawn != null)
    {
        AddBuffToPawn(pawn, "QiEnhancement", 1.0f);
    }
}

[DebugAction("Tu Tien", "Remove All Cultivation Hediffs", allowedGameStates = AllowedGameStates.PlayingOnMap)]
private static void RemoveAllCultivationHediffs()
{
    Pawn pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
        .OfType<Pawn>().FirstOrDefault();
    
    if (pawn != null)
    {
        var cultivationHediffs = pawn.health.hediffSet.hediffs
            .Where(h => h.def.defName.Contains("Qi") || h.def.defName.Contains("Cultivation"))
            .ToList();
            
        foreach (var hediff in cultivationHediffs)
        {
            pawn.health.RemoveHediff(hediff);
        }
    }
}
```

## 6. Best Practices

### 6.1. Performance
- Sử dụng `IsHashIntervalTick()` cho logic định kỳ
- Tránh tính toán phức tạp trong `Tick()`
- Cache kết quả khi có thể

### 6.2. Save/Load
- Luôn implement `ExposeData()` cho custom hediffs
- Sử dụng `Scribe_Values` và `Scribe_References` đúng cách

### 6.3. Compatibility
- Kiểm tra null trước khi truy cập
- Sử dụng `DefDatabase<>.GetNamedSilentFail()` cho optional defs
- Test với các mod khác

## 7. Ví dụ hoàn chỉnh trong Tu Tien Mod

```csharp
// Trong skill worker
public override void Execute(Pawn pawn, CultivationSkillDef skill)
{
    // Remove existing shield
    var existingShield = pawn.health.hediffSet.GetFirstHediffOfDef(TuTienDefOf.QiShieldHediff);
    if (existingShield != null)
    {
        pawn.health.RemoveHediff(existingShield);
    }
    
    // Add new shield
    var shield = HediffMaker.MakeHediff(TuTienDefOf.QiShieldHediff, pawn) as QiShieldHediff;
    pawn.health.AddHediff(shield);
    
    // Visual effect
    MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, "Qi Shield!", Color.blue);
}
```

Đây là hướng dẫn hoàn chỉnh về hệ thống Hediff trong RimWorld. Bạn có thể áp dụng các pattern này cho bất kỳ loại buff/debuff nào trong mod Tu Tien!
