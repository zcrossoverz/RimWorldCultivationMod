# Implementation Guide - Hướng Dẫn Chi Tiết

## 🎯 **Step-by-Step Implementation Examples**

### 1. 🔥 **Thêm Thuộc Tính Tu Tiên Mới (New Cultivation Attribute)**

#### Example: Thêm "Spiritual Power" attribute

**Step 1: Core Data Structure**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Core\CultivationDataEnhanced.cs
public partial class CultivationDataEnhanced : IExposable
{
    // Thêm thuộc tính mới
    public float spiritualPower = 0f;
    public float maxSpiritualPower = 100f;
    
    public void ExposeData()
    {
        // ... existing code ...
        Scribe_Values.Look(ref spiritualPower, "spiritualPower", 0f);
        Scribe_Values.Look(ref maxSpiritualPower, "maxSpiritualPower", 100f);
    }
}
```

**Step 2: Processing Logic**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Core\CultivationCompEnhanced.cs
private void ProcessEnhancedCultivation()
{
    // ... existing code ...
    
    // Spiritual Power regeneration
    float spiritRegen = GetSpiritualPowerRegen();
    data.spiritualPower = Math.Min(data.spiritualPower + spiritRegen, data.maxSpiritualPower);
}

private float GetSpiritualPowerRegen()
{
    var data = EnhancedData;
    float baseRegen = 0.1f; // Base regeneration
    
    // Realm multiplier
    float realmMultiplier = (int)data.currentRealm * 0.5f;
    
    return baseRegen * (1f + realmMultiplier);
}
```

**Step 3: UI Integration** 
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Core\CultivationCompEnhanced.cs
private string GetEnhancedInspectString()
{
    // ... existing code ...
    sb.AppendLine($"Spiritual Power: {data.spiritualPower:F1}/{data.maxSpiritualPower:F1}");
}
```

**Step 4: Utility Functions**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Core\CultivationDataEnhanced.cs
public bool HasEnoughSpiritualPower(float required)
{
    return spiritualPower >= required;
}

public bool ConsumeSpiritualPower(float amount)
{
    if (spiritualPower >= amount)
    {
        spiritualPower -= amount;
        return true;
    }
    return false;
}
```

---

### 2. ⚡ **Thêm Skill/Ability Mới (New Skill)**

#### Example: "Lightning Strike" skill

**Step 1: Tạo Effect Class**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Abilities\AbilityEffect_LightningStrike.cs
using Verse;
using RimWorld;
using UnityEngine;

namespace TuTien.Abilities
{
    public class AbilityEffect_LightningStrike
    {
        public float damage = 150f;
        public float stunDuration = 2f;
        public float radius = 2f;
        
        public void Apply(CultivationAbilityDef abilityDef, Pawn caster, LocalTargetInfo target)
        {
            // Visual effects
            FleckMaker.ThrowLightningGlow(target.Cell.ToVector3(), caster.Map, 2f);
            
            // Find targets in radius
            var targets = GetPawnsInRadius(target.Cell, radius, caster.Map)
                .Where(p => p != caster && p.HostileTo(caster));
                
            foreach (var targetPawn in targets)
            {
                // Deal damage
                var dinfo = new DamageInfo(DamageDefOf.Burn, damage, 0f, -1f, caster);
                targetPawn.TakeDamage(dinfo);
                
                // Apply stun
                var stunHediff = HediffMaker.MakeHediff(HediffDefOf.Paralysis, targetPawn);
                stunHediff.Severity = stunDuration;
                targetPawn.health.AddHediff(stunHediff);
            }
            
            // Sound effect
            SoundDefOf.Thunder_OnMap.PlayOneShot(new TargetInfo(target.Cell, caster.Map));
        }
        
        private IEnumerable<Pawn> GetPawnsInRadius(IntVec3 center, float radius, Map map)
        {
            var cells = GenRadial.RadialCellsAround(center, radius, true);
            foreach (var cell in cells)
            {
                var things = cell.GetThingList(map);
                foreach (var thing in things)
                {
                    if (thing is Pawn pawn)
                        yield return pawn;
                }
            }
        }
    }
}
```

**Step 2: XML Definition**
```xml
<!-- File: d:\RimWorld\Mods\TuTien\Defs\CultivationAbilityDefs_Basic.xml -->
<TuTien.CultivationAbilityDef>
  <defName>TuTien_LightningStrike</defName>
  <label>Lightning Strike</label>
  <description>Call down lightning to strike enemies in an area.</description>
  <iconPath>UI/Icons/CultivationAbilities/LightningStrike</iconPath>
  <targetType>GroundTarget</targetType>
  <qiCost>75</qiCost>
  <cooldownTicks>480</cooldownTicks>
  <range>20</range>
  <effects>
    <li Class="TuTien.Abilities.AbilityEffect_LightningStrike">
      <damage>150</damage>
      <stunDuration>2</stunDuration>
      <radius>2</radius>
    </li>
  </effects>
</TuTien.CultivationAbilityDef>
```

**Step 3: Requirements (Optional)**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Abilities\CompAbilityUser.cs
// Thêm vào GetGizmosExtra() method để check requirements
if (ability.def.defName == "TuTien_LightningStrike")
{
    var cultivationComp = pawn.GetComp<CultivationCompEnhanced>();
    if (cultivationComp?.EnhancedData?.currentRealm < CultivationRealm.FoundationEstablishment)
    {
        command.Disable("Requires Foundation Establishment realm");
    }
}
```

---

### 3. 🏺 **Thêm Artifact Mới (New Artifact)**

#### Example: "Thunder Sword" artifact

**Step 1: CultivationArtifactDef**
```xml
<!-- File: d:\RimWorld\Mods\TuTien\Defs\CultivationArtifactDefs.xml -->
<TuTien.CultivationArtifactDef>
  <defName>ThunderSword</defName>
  <label>Thunder Sword</label>
  <description>A legendary sword infused with lightning qi.</description>
  <rarity>Legendary</rarity>
  <requiredRealm>FoundationEstablishment</requiredRealm>
  <requiredStage>5</requiredStage>
  <qiCapacity>500</qiCapacity>
  <qiRegenRate>5.0</qiRegenRate>
  <abilities>
    <li>TuTien_LightningStrike</li>
    <li>TuTien_ThunderSlash</li>
  </abilities>
  <effects>
    <li Class="TuTien.Systems.Effects.CultivationEffect_StatBonus">
      <statDef>MeleeHitChance</statDef>
      <value>0.3</value>
    </li>
    <li Class="TuTien.Systems.Effects.CultivationEffect_ElementalBonus">
      <element>Lightning</element>
      <damageMultiplier>1.5</damageMultiplier>
    </li>
  </effects>
</TuTien.CultivationArtifactDef>
```

**Step 2: ThingDef**
```xml
<!-- File: d:\RimWorld\Mods\TuTien\Defs\ThingDefs_CultivationArtifacts.xml -->
<ThingDef ParentName="BaseMeleeWeapon_Sharp_Quality">
  <defName>TuTien_ThunderSword</defName>
  <label>thunder sword</label>
  <description>A cultivation sword crackling with lightning energy.</description>
  <graphicData>
    <texPath>Things/Item/Equipment/WeaponMelee/ThunderSword</texPath>
    <graphicClass>Graphic_Single</graphicClass>
  </graphicData>
  <statBases>
    <WorkToMake>15000</WorkToMake>
    <Mass>2.5</Mass>
  </statBases>
  <tools>
    <li>
      <label>blade</label>
      <capacities>
        <li>Cut</li>
        <li>Stab</li>
      </capacities>
      <power>25</power>
      <cooldownTime>1.5</cooldownTime>
    </li>
  </tools>
  <comps>
    <li Class="TuTien.Systems.Artifacts.CultivationArtifactCompProperties">
      <artifactDef>ThunderSword</artifactDef>
    </li>
  </comps>
</ThingDef>
```

---

### 4. 🌟 **Thêm Buff/Debuff Mới (New Buff/Debuff)**

#### Example: "Lightning Affinity" buff

**Step 1: HediffDef**
```xml
<!-- File: d:\RimWorld\Mods\TuTien\Defs\HediffDefs_Cultivation.xml -->
<HediffDef>
  <defName>TuTien_LightningAffinity</defName>
  <label>lightning affinity</label>
  <description>Enhanced connection to lightning qi.</description>
  <hediffClass>TuTien.Hediffs.Hediff_LightningAffinity</hediffClass>
  <defaultLabelColor>(1.0, 1.0, 0.6)</defaultLabelColor>
  <isBad>false</isBad>
  <maxSeverity>5.0</maxSeverity>
  <stages>
    <li>
      <minSeverity>1</minSeverity>
      <statOffsets>
        <MoveSpeed>0.2</MoveSpeed>
      </statOffsets>
      <capMods>
        <li>
          <capacity>Manipulation</capacity>
          <offset>0.1</offset>
        </li>
      </capMods>
    </li>
    <li>
      <minSeverity>3</minSeverity>
      <statOffsets>
        <MoveSpeed>0.5</MoveSpeed>
      </statOffsets>
    </li>
  </stages>
</HediffDef>
```

**Step 2: Custom Hediff Class**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Hediffs\Hediff_LightningAffinity.cs
using Verse;
using RimWorld;

namespace TuTien.Hediffs
{
    public class Hediff_LightningAffinity : Hediff
    {
        public override void PostTick()
        {
            base.PostTick();
            
            // Special lightning effects
            if (Find.TickManager.TicksGame % 60 == 0) // Every second
            {
                // Chance for lightning sparks
                if (Rand.Chance(0.1f))
                {
                    FleckMaker.ThrowMicroSparks(pawn.DrawPos, pawn.Map);
                }
            }
        }
        
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            // Add message when buff is applied
            Messages.Message($"{pawn.Name} gains lightning affinity!", 
                MessageTypeDefOf.PositiveEvent);
        }
    }
}
```

**Step 3: Application Logic**
```csharp
// Trong ability effect hoặc artifact logic
public void ApplyLightningAffinity(Pawn target, float severity = 1f)
{
    var existing = target.health.hediffSet.GetFirstHediffOfDef(TuTienDefOf.TuTien_LightningAffinity);
    if (existing != null)
    {
        // Stack with existing
        existing.Severity = Math.Min(existing.Severity + severity, 5f);
    }
    else
    {
        // Create new
        var hediff = HediffMaker.MakeHediff(TuTienDefOf.TuTien_LightningAffinity, target);
        hediff.Severity = severity;
        target.health.AddHediff(hediff);
    }
}
```

---

### 5. 🎨 **Thêm Visual Effects**

#### Example: Lightning skill với particle effects

**Custom Projectile với Effects:**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Abilities\LightningQiProjectile.cs
using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.Abilities
{
    public class LightningQiProjectile : Projectile
    {
        private int hitCount = 0;
        private const int maxHits = 3;
        
        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            IntVec3 position = base.Position;
            
            // Lightning visual effects
            for (int i = 0; i < 5; i++)
            {
                FleckMaker.ThrowLightningGlow(position.ToVector3(), map, 1.5f);
                FleckMaker.ThrowMicroSparks(position.ToVector3(), map);
            }
            
            // Chain lightning logic
            if (hitCount < maxHits && hitThing is Pawn hitPawn)
            {
                DealDamage(hitPawn);
                
                // Find next target for chain
                var nextTarget = FindNearestEnemy(hitPawn, 5f);
                if (nextTarget != null)
                {
                    ChainToTarget(nextTarget);
                }
            }
            
            base.Impact(hitThing);
        }
        
        private void ChainToTarget(Pawn target)
        {
            hitCount++;
            
            // Create chain effect
            var newProjectile = (LightningQiProjectile)GenSpawn.Spawn(def, Position, Map);
            newProjectile.hitCount = hitCount;
            newProjectile.Launch(launcher, target, target, ProjectileHitFlags.IntendedTarget);
        }
    }
}
```

---

### 6. 🔄 **Thêm Technique/Công Pháp Mới**

#### Example: "Lightning Body Technique"

**Step 1: Technique Definition**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\TechniqueWorkers\LightningBodyTechnique.cs
using TuTien.Core;

namespace TuTien.TechniqueWorkers
{
    public class LightningBodyTechnique : CultivationTechnique
    {
        public override CultivationTechniqueType TechniqueType => CultivationTechniqueType.BodyCultivation;
        
        public override void Practice(Pawn practitioner)
        {
            var cultivationComp = practitioner.GetComp<CultivationCompEnhanced>();
            if (cultivationComp?.EnhancedData == null) return;
            
            var data = cultivationComp.EnhancedData;
            
            // Consume Qi for practice
            if (!data.ConsumeQi(qiCostPerPractice)) return;
            
            // Gain mastery experience
            masteryExperience += GetPracticeExperienceGain(practitioner);
            
            // Check for mastery level up
            if (masteryExperience >= masteryExperienceRequired)
            {
                AdvanceMasteryLevel();
            }
            
            // Apply technique benefits based on mastery
            ApplyTechniqueBenefits(practitioner);
        }
        
        private void ApplyTechniqueBenefits(Pawn pawn)
        {
            switch (masteryLevel)
            {
                case TechniqueMasteryLevel.Novice:
                    // +10% move speed
                    ApplyStatBuff(pawn, StatDefOf.MoveSpeed, 0.1f);
                    break;
                    
                case TechniqueMasteryLevel.Adept:
                    // +20% move speed, lightning resistance
                    ApplyStatBuff(pawn, StatDefOf.MoveSpeed, 0.2f);
                    ApplyElementalResistance(pawn, "Lightning", 0.3f);
                    break;
                    
                case TechniqueMasteryLevel.Master:
                    // Full lightning body transformation
                    ApplyLightningBodyTransformation(pawn);
                    break;
            }
        }
    }
}
```

**Step 2: XML Definition**
```xml
<!-- File: d:\RimWorld\Mods\TuTien\Defs\CultivationTechniqueDefs.xml -->
<TuTien.CultivationTechniqueDef>
  <defName>LightningBodyTechnique</defName>
  <label>Lightning Body Technique</label>
  <description>Cultivate the body with lightning qi to gain speed and electrical resistance.</description>
  <techniqueType>BodyCultivation</techniqueType>
  <workerClass>TuTien.TechniqueWorkers.LightningBodyTechnique</workerClass>
  <minimumRealm>FoundationEstablishment</minimumRealm>
  <minimumStage>3</minimumStage>
  <basePracticeTime>300</basePracticeTime>
  <qiCostPerPractice>50</qiCostPerPractice>
  <practiceXpGain>15</practiceXpGain>
</TuTien.CultivationTechniqueDef>
```

---

### 7. 🎛️ **Thêm UI Panel Mới**

#### Example: Technique Management Panel

**Step 1: Dialog Class**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\UI\Dialog_TechniqueManagement.cs
using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.UI
{
    public class Dialog_TechniqueManagement : Window
    {
        private Pawn pawn;
        private Vector2 scrollPosition;
        
        public Dialog_TechniqueManagement(Pawn pawn)
        {
            this.pawn = pawn;
            doCloseX = true;
            draggable = true;
            resizeable = true;
        }
        
        public override Vector2 InitialSize => new Vector2(800f, 600f);
        
        public override void DoWindowContents(Rect inRect)
        {
            var cultivationComp = pawn.GetComp<CultivationCompEnhanced>();
            if (cultivationComp?.EnhancedData == null) return;
            
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, 200, 30), "Technique Management");
            
            Text.Font = GameFont.Small;
            var listRect = new Rect(0, 40, inRect.width, inRect.height - 80);
            
            DrawTechniqueList(listRect, cultivationComp);
            
            // Practice button
            if (Widgets.ButtonText(new Rect(0, inRect.height - 35, 120, 30), "Practice Selected"))
            {
                // Practice logic
            }
        }
        
        private void DrawTechniqueList(Rect rect, CultivationCompEnhanced comp)
        {
            // Implementation for technique list with scrolling
        }
    }
}
```

**Step 2: Gizmo Integration**
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Core\CultivationCompEnhanced.cs
public override IEnumerable<Gizmo> CompGetGizmosExtra()
{
    foreach (var gizmo in base.CompGetGizmosExtra())
        yield return gizmo;
        
    // Technique management button
    yield return new Command_Action
    {
        defaultLabel = "Techniques",
        defaultDesc = "Manage cultivation techniques",
        icon = ContentFinder<Texture2D>.Get("UI/Icons/Techniques"),
        action = () => Find.WindowStack.Add(new Dialog_TechniqueManagement(parent as Pawn))
    };
}
```

---

## 🔍 **Debug & Testing Tools**

### Debug Command
```csharp
// File: d:\RimWorld\Mods\TuTien\Source\TuTien\Testing\DebugActions.cs
[DebugAction("Tu Tien", "Add Lightning Affinity", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
public static void AddLightningAffinity(Pawn pawn)
{
    var hediff = HediffMaker.MakeHediff(TuTienDefOf.TuTien_LightningAffinity, pawn);
    hediff.Severity = 3f;
    pawn.health.AddHediff(hediff);
}
```

### Console Testing
```csharp
// Development console commands
[DebugAction("Tu Tien", "Test All Abilities", actionType = DebugActionType.ToolMapForPawns)]
public static void TestAllAbilities(Pawn pawn)
{
    var abilityComp = pawn.GetComp<CompAbilityUser>();
    if (abilityComp != null)
    {
        foreach (var ability in abilityComp.AllAbilities)
        {
            Log.Message($"Testing ability: {ability.def.label}");
            // Test logic
        }
    }
}
```

---

## 📊 **Performance Optimization Tips**

### 1. Cache Management
- Use event-driven cache invalidation
- Avoid frequent GetComp() calls
- Cache expensive calculations

### 2. Memory Optimization  
- Use object pooling for frequent allocations
- Implement proper disposal patterns
- Monitor GC pressure

### 3. Update Frequency
- Use different tick intervals for different systems
- Implement LOD (Level of Detail) for distant pawns
- Batch operations when possible

---

## 🎯 **Best Practices**

### Code Organization:
1. **Separation of Concerns**: Mỗi class có một responsibility rõ ràng
2. **Namespace Organization**: Theo chức năng (Core, Abilities, Systems, UI)
3. **Consistent Naming**: Prefix với `TuTien_` cho XML definitions

### Performance:
1. **Lazy Loading**: Chỉ load data khi cần thiết
2. **Event-Driven Updates**: Sử dụng events thay vì polling
3. **Caching**: Cache kết quả expensive calculations

### Compatibility:
1. **Dual System Support**: Hỗ trợ cả legacy và enhanced data
2. **Graceful Degradation**: Fallback khi component không có
3. **Save Compatibility**: Maintain backward compatibility

---

## 🔧 **Common Implementation Patterns**

### Pattern 1: Ability Effect
```csharp
public void Apply(CultivationAbilityDef abilityDef, Pawn caster, LocalTargetInfo target)
{
    // 1. Validate inputs
    // 2. Apply effects  
    // 3. Visual/audio feedback
    // 4. Handle side effects
}
```

### Pattern 2: Component Integration
```csharp
public override void PostSpawnSetup(bool respawningAfterLoad)
{
    base.PostSpawnSetup(respawningAfterLoad);
    // Initialize data
    // Subscribe to events
    // Validate integrity
}
```

### Pattern 3: Data Processing
```csharp
private void ProcessSomething()
{
    // 1. Get data
    // 2. Apply synergies
    // 3. Calculate changes
    // 4. Update state
    // 5. Trigger events
}
```

Đây là tài liệu architecture hoàn chỉnh! Bạn có thể sử dụng nó để implement bất kỳ feature mới nào trong hệ thống tu tiên.
