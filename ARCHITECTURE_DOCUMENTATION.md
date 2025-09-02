# Tu Tiên - Cultivation Mod - Kiến Trúc Code & Documentation

## 📋 Tổng Quan Kiến Trúc (Architecture Overview)

### 🏗️ Cấu Trúc Thư Mục Chính
```
TuTien/
├── Core/                    # Hệ thống cốt lõi tu tiên
├── Abilities/              # Hệ thống skill/ability
├── Systems/                # Các hệ thống phụ trợ
├── Patches/                # Harmony patches cho RimWorld
├── UI/                     # Giao diện người dùng
├── Items/                  # Vật phẩm tu tiên
├── Buildings/              # Công trình tu tiên
└── Utilities/              # Tiện ích chung
```

---

## 🔄 Flow Code Chính (Main Code Flow)

### 1. 🎯 **Initialization Flow**
```
Game Start → TuTienMod.cs (ModBase)
    ↓
Pawn Spawn → CultivationComp/CultivationCompEnhanced
    ↓
Data Init → CultivationData/CultivationDataEnhanced
    ↓
Skill Setup → CultivationSkillManager
    ↓
UI Integration → CompAbilityUser (abilities)
```

### 2. 🔄 **Update Cycle (CompTick)**
```
Every Tick:
CultivationCompEnhanced.CompTick()
    ↓
ProcessCultivation() → ProcessEnhancedCultivation() / ProcessLegacyCultivation()
    ↓
1. TechniqueSynergyManager.ApplySynergyEffects()
2. SkillSynergyManager.ApplySynergyEffects()
3. Qi Regeneration
4. Environmental Cultivation (if meditating)
5. UpdateProgressTracking()
```

---

## 🏛️ Chi Tiết Từng Component

### 1. 🔥 **Core System - Hệ Thống Cốt Lõi**

#### `CultivationCompEnhanced.cs` - Component chính
- **Nhiệm vụ**: Quản lý toàn bộ trạng thái tu tiên của pawn
- **Chức năng**:
  - Dual data support (Legacy + Enhanced)
  - Event-driven cache management
  - Performance optimization
  - Data validation

#### `CultivationData.cs` - Dữ liệu legacy
- **Nhiệm vụ**: Cấu trúc dữ liệu tu tiên cũ (tương thích ngược)
- **Chức năng**:
  - Basic realm/stage tracking
  - Qi management
  - Skill unlocking

#### `CultivationDataEnhanced.cs` - Dữ liệu mới
- **Nhiệm vụ**: Cấu trúc dữ liệu tu tiên tối ưu
- **Chức năng**:
  - Memory-optimized storage
  - Advanced progress tracking
  - Affinities & resistances
  - Resource pools

#### `CultivationTechnique.cs` - Công pháp tu tiên
- **Nhiệm vụ**: Base class cho tất cả các công pháp
- **Chức năng**:
  - Mastery levels
  - Prerequisites
  - Practice mechanics

---

### 2. ⚡ **Abilities System - Hệ Thống Kỹ Năng**

#### `CompAbilityUser.cs` - Ability manager
- **Nhiệm vụ**: Quản lý abilities của pawn
- **Chức năng**:
  - Ability instances
  - Cooldown tracking
  - UI commands

#### `CultivationAbilityDef.cs` - Definition abilities
- **Nhiệm vụ**: Định nghĩa abilities trong XML
- **Chức năng**:
  - Qi costs
  - Targeting
  - Effects framework

#### `AbilityEffect_*.cs` - Effect classes
- **Nhiệm vụ**: Implement các hiệu ứng ability
- **Chức năng**:
  - `AbilityEffect_LaunchProjectile`: Bắn projectile
  - `AbilityEffect_Heal`: Hồi máu

#### `SwordQiProjectile.cs` - Custom projectile
- **Nhiệm vụ**: Projectile xuyên phá với penetration
- **Chức năng**:
  - Penetrates obstacles
  - Multi-target hits (max 5)
  - Explosion finish (300 damage each)

---

### 3. 🏺 **Artifacts System - Hệ Thống Pháp Bảo**

#### `CultivationArtifactComp.cs` - Artifact component
- **Nhiệm vụ**: Quản lý pháp bảo và effects
- **Chức năng**:
  - Auto-targeting
  - Buff application
  - Qi integration

#### `CultivationArtifactDef.cs` - Artifact definitions
- **Nhiệm vụ**: Định nghĩa pháp bảo trong XML
- **Chức năng**:
  - Stats bonuses
  - Special abilities
  - Requirements

---

### 4. 🎯 **Systems - Hệ Thống Phụ Trợ**

#### Registry System (`Systems/Registry/`)
- **CultivationRegistry.cs**: Central data registry
- **Nhiệm vụ**: Quản lý definitions và cache

#### Synergy System (`Systems/*Synergy/`)
- **TechniqueSynergyManager.cs**: Tương tác giữa công pháp
- **SkillSynergyManager.cs**: Tương tác giữa kỹ năng

#### Effects System (`Systems/Effects/`)
- **CultivationEffectManager.cs**: Quản lý hiệu ứng tu tiên

---

## 🛠️ Hướng Dẫn Implementation

### 🆕 **Thêm Thuộc Tính Mới (New Attribute)**

#### Bước 1: Thêm vào CultivationData
```csharp
// d:\RimWorld\Mods\TuTien\Source\TuTien\Core\CultivationDataEnhanced.cs
public class CultivationDataEnhanced : IExposable
{
    // Thêm thuộc tính mới
    public float newAttribute = 0f;
    
    public void ExposeData()
    {
        // Thêm vào save/load
        Scribe_Values.Look(ref newAttribute, "newAttribute", 0f);
    }
}
```

#### Bước 2: Thêm vào UI
```csharp
// d:\RimWorld\Mods\TuTien\Source\TuTien\Core\CultivationCompEnhanced.cs
private string GetEnhancedInspectString()
{
    sb.AppendLine($"New Attribute: {data.newAttribute:F1}");
}
```

#### Bước 3: Thêm logic xử lý
```csharp
// d:\RimWorld\Mods\TuTien\Source\TuTien\Core\CultivationCompEnhanced.cs
private void ProcessEnhancedCultivation()
{
    // Logic xử lý thuộc tính mới
    data.newAttribute += calculateIncrease();
}
```

---

### 🔮 **Thêm Buff Mới (New Buff)**

#### Bước 1: Tạo HediffDef
```xml
<!-- d:\RimWorld\Mods\TuTien\Defs\HediffDefs_Cultivation.xml -->
<HediffDef>
  <defName>TuTien_NewBuff</defName>
  <label>new buff</label>
  <description>Description of new buff</description>
  <hediffClass>TuTien.Hediffs.Hediff_CultivationBuff</hediffClass>
  <defaultLabelColor>(0.8, 1.0, 0.8)</defaultLabelColor>
  <isBad>false</isBad>
  <maxSeverity>10.0</maxSeverity>
  <stages>
    <li>
      <statOffsets>
        <MoveSpeed>0.5</MoveSpeed>
      </statOffsets>
    </li>
  </stages>
</HediffDef>
```

#### Bước 2: Tạo Hediff Class
```csharp
// d:\RimWorld\Mods\TuTien\Source\TuTien\Hediffs\Hediff_NewBuff.cs
namespace TuTien.Hediffs
{
    public class Hediff_NewBuff : Hediff_CultivationBuff
    {
        public override void PostTick()
        {
            base.PostTick();
            // Custom logic
        }
    }
}
```

#### Bước 3: Apply Buff
```csharp
// Trong ability effect hoặc cultivation processing
var hediff = HediffMaker.MakeHediff(TuTienDefOf.TuTien_NewBuff, targetPawn);
hediff.Severity = 1.0f;
targetPawn.health.AddHediff(hediff);
```

---

### ⚔️ **Thêm Skill/Ability Mới (New Skill)**

#### Bước 1: Tạo AbilityDef
```xml
<!-- d:\RimWorld\Mods\TuTien\Defs\CultivationAbilityDefs_Basic.xml -->
<TuTien.CultivationAbilityDef>
  <defName>TuTien_NewSkill</defName>
  <label>New Skill</label>
  <description>Description of new skill</description>
  <targetType>Enemy</targetType>
  <qiCost>30</qiCost>
  <cooldownTicks>180</cooldownTicks>
  <range>15</range>
  <effects>
    <li Class="TuTien.Abilities.AbilityEffect_NewSkill">
      <damage>100</damage>
      <radius>3</radius>
    </li>
  </effects>
</TuTien.CultivationAbilityDef>
```

#### Bước 2: Tạo Effect Class
```csharp
// d:\RimWorld\Mods\TuTien\Source\TuTien\Abilities\AbilityEffect_NewSkill.cs
namespace TuTien.Abilities
{
    public class AbilityEffect_NewSkill
    {
        public float damage = 100f;
        public float radius = 3f;
        
        public void Apply(CultivationAbilityDef abilityDef, Pawn caster, LocalTargetInfo target)
        {
            // Implementation logic
            var targets = GetTargetsInRadius(target.Cell, radius, caster.Map);
            foreach (var t in targets)
            {
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Cut, damage, 0f, -1f, caster);
                t.TakeDamage(dinfo);
            }
        }
    }
}
```

#### Bước 3: Integration với CompAbilityUser
Abilities sẽ tự động được load và integrate thông qua `CompAbilityUser.cs`.

---

### 🏺 **Thêm Artifact Mới (New Artifact)**

#### Bước 1: Tạo CultivationArtifactDef
```xml
<!-- d:\RimWorld\Mods\TuTien\Defs\CultivationArtifactDefs.xml -->
<TuTien.CultivationArtifactDef>
  <defName>NewArtifact</defName>
  <label>New Artifact</label>
  <description>Description</description>
  <rarity>Legendary</rarity>
  <effects>
    <li Class="TuTien.Systems.Effects.CultivationEffect_StatBonus">
      <statDef>MoveSpeed</statDef>
      <value>2.0</value>
    </li>
  </effects>
  <abilities>
    <li>TuTien_NewSkill</li>
  </abilities>
</TuTien.CultivationArtifactDef>
```

#### Bước 2: Tạo ThingDef
```xml
<!-- d:\RimWorld\Mods\TuTien\Defs\ThingDefs_CultivationArtifacts.xml -->
<ThingDef ParentName="BaseMeleeWeapon_Sharp_Quality">
  <defName>TuTien_NewArtifact</defName>
  <label>new artifact</label>
  <description>A powerful cultivation artifact</description>
  <comps>
    <li Class="TuTien.Systems.Artifacts.CultivationArtifactCompProperties">
      <artifactDef>NewArtifact</artifactDef>
    </li>
  </comps>
</ThingDef>
```

---

## 🎯 **Data Flow Diagram**

### Tu Tiên Data Flow:
```
Pawn Creation
    ↓
CultivationComp/CultivationCompEnhanced attached
    ↓
CultivationData initialized
    ↓
CompAbilityUser attached (if has abilities)
    ↓
Equipment → CultivationArtifactComp (if artifact)
    ↓
Every Tick: ProcessCultivation()
    ↓
1. Synergy calculations
2. Qi regeneration  
3. Environmental effects
4. Progress tracking
    ↓
UI Updates (inspect strings, gizmos)
```

### Ability Usage Flow:
```
Player clicks ability gizmo
    ↓
Command_CastAbilityWithCooldown.ProcessInput()
    ↓
Check Qi cost, cooldown, range
    ↓
CultivationAbility.Activate()
    ↓
AbilityEffect_*.Apply() 
    ↓
Visual effects, damage, healing, etc.
    ↓
Start cooldown timer
```

---

## 📝 **Các Interface & Base Classes Quan Trọng**

### Core Interfaces:
- `IExposable` - Save/Load functionality
- `ThingComp` - RimWorld component system
- `Def` - RimWorld definition system

### Key Base Classes:
- `CultivationCompEnhanced` - Main cultivation component
- `CultivationTechnique` - Base for all techniques
- `CultivationArtifactComp` - Artifact management
- `AbilityEffect_*` - Ability effects framework

---

## 🔧 **Configuration Files**

### XML Definitions:
- `CultivationAbilityDefs_Basic.xml` - Ability definitions
- `CultivationArtifactDefs.xml` - Artifact definitions  
- `ThingDefs_CultivationArtifacts.xml` - Item definitions
- `HediffDefs_Cultivation.xml` - Buff/debuff definitions

---

## 🚀 **Quick Implementation Guide**

### Để thêm feature mới:

1. **Identify Type**: Attribute/Buff/Skill/Artifact?
2. **Core Data**: Thêm vào `CultivationDataEnhanced`
3. **Processing**: Thêm logic vào `ProcessEnhancedCultivation()`
4. **UI**: Update `GetEnhancedInspectString()`
5. **XML**: Tạo definitions tương ứng
6. **Testing**: Use debug mode để test

### Template Files để copy:
- **New Ability**: Copy `AbilityEffect_Heal.cs`
- **New Buff**: Copy existing `Hediff_*.cs` 
- **New Attribute**: Add to `CultivationDataEnhanced`
- **New Artifact**: Copy existing artifact definition

---

## 🎮 **Current System Status**

### ✅ **Hoàn Thành:**
- Dual data system (Legacy + Enhanced)
- Sword Qi projectile với penetration
- Visual cooldown system
- Artifact integration
- Synergy systems
- Performance optimization

### 🔄 **Đang Phát Triển:**
- Advanced techniques
- More artifact types
- Complex ability combinations

### 🎯 **Next Steps:**
- Enhanced UI panels
- More visual effects
- Balancing system
