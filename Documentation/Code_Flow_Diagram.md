# Tu Tien Mod - Code Flow Diagram

## 🎯 Tổng quan Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Game Events   │    │   User Actions  │    │   Tick System   │
│                 │    │                 │    │                 │
│ • Pawn Spawn    │────│ • Skill Click   │────│ • Cultivation   │
│ • Map Load      │    │ • Tab Open      │    │ • Regeneration  │
│ • Damage        │    │ • Breakthrough  │    │ • Cooldowns     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                    HARMONY PATCHES                              │
│                                                                 │
│ • Pawn_SpawnSetup_Patch     → Add CultivationComp              │
│ • Pawn_GetGizmos_Patch      → Show Skill Buttons               │
│ • Thing_GetInspectTabs_Patch → Add Cultivation Tab             │
│ • DamageWorker_Apply_Patch  → Qi Shield Protection             │
└─────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                   CORE COMPONENTS                               │
└─────────────────────────────────────────────────────────────────┘
```

## 📊 Component Flow Diagram

```
                           🧬 PAWN SPAWN
                                │
                                ▼
                    ┌───────────────────────┐
                    │  CultivationComp      │
                    │  (Auto-attached)      │
                    └───────────────────────┘
                                │
                                ▼
                    ┌───────────────────────┐
                    │  CultivationData      │
                    │  • currentRealm       │
                    │  • currentStage       │
                    │  • cultivationPoints  │
                    │  • currentQi          │
                    │  • maxQi              │
                    │  • talent             │
                    │  • unlockedSkills     │
                    │  • skillCooldowns     │
                    └───────────────────────┘
                                │
                    ┌───────────┼───────────┐
                    ▼           ▼           ▼
         ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
         │   UI TAB    │ │   GIZMOS    │ │   TICKING   │
         │             │ │             │ │             │
         │ Cultivation │ │ Skill       │ │ Regenerate  │
         │ Progress    │ │ Buttons     │ │ Qi & Points │
         │ Stats       │ │ Cooldowns   │ │ Update      │
         │ Breakthrough│ │ Passive     │ │ Cooldowns   │
         └─────────────┘ └─────────────┘ └─────────────┘
```

## 🔄 Skill System Flow

```
        👆 USER CLICKS SKILL GIZMO
                    │
                    ▼
        ┌───────────────────────┐
        │   CanUseSkill()       │
        │   • Check Qi Cost     │
        │   • Check Cooldown    │
        │   • Check Conditions  │
        └───────────────────────┘
                    │
         ┌──────────┼──────────┐
         ▼          ▼          ▼
    ❌ FAIL    ✅ SUCCESS   ⏳ COOLDOWN
         │          │          │
         ▼          ▼          ▼
   Show Error   UseSkill()   Show Timer
                    │
                    ▼
        ┌───────────────────────┐
        │  Skill Worker         │
        │  • QiPunchWorker      │
        │  • QiShieldWorker     │
        │  • QiHealingWorker    │
        │  • etc...             │
        └───────────────────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │  Execute()            │
        │  • Consume Qi         │
        │  • Apply Effects      │
        │  • Add Hediffs        │
        │  • Visual Effects     │
        │  • Set Cooldown       │
        └───────────────────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │  Update UI            │
        │  • Refresh Gizmos     │
        │  • Update Tab         │
        │  • Show Notifications │
        └───────────────────────┘
```

## ⚡ Breakthrough System Flow

```
        💪 USER CLICKS BREAKTHROUGH
                    │
                    ▼
        ┌───────────────────────┐
        │   CanBreakthrough()   │
        │   • Check Tu Vi       │
        │   • Check Cooldown    │
        │   • Check Conditions  │
        └───────────────────────┘
                    │
         ┌──────────┼──────────┐
         ▼          ▼          ▼
    ❌ FAIL    ✅ READY    ⏳ COOLDOWN
         │          │          │
         ▼          ▼          ▼
   Show Error  AttemptBreak   Show Timer
                    │
                    ▼
        ┌───────────────────────┐
        │  Calculate Chance     │
        │  • Talent Modifier    │
        │  • Stage Difficulty   │
        │  • Random Factor      │
        └───────────────────────┘
                    │
         ┌──────────┼──────────┐
         ▼          ▼          ▼
    ❌ FAILED   ✅ SUCCESS  ⚠️ CRITICAL
         │          │          │
         ▼          ▼          ▼
   ┌─────────┐ ┌─────────┐ ┌─────────┐
   │ Damage  │ │Advance  │ │Qi Dev.  │
   │ Pawn    │ │Stage    │ │Serious  │
   │ Cooldown│ │Unlock   │ │Injuries │
   │         │ │Skills   │ │         │
   └─────────┘ └─────────┘ └─────────┘
         │          │          │
         └──────────┼──────────┘
                    ▼
        ┌───────────────────────┐
        │  Show Result Dialog   │
        │  • Stats Comparison   │
        │  • New Skills         │
        │  • Effects Summary    │
        └───────────────────────┘
```

## 🎮 User Interface Flow

```
                    🖱️ CLICK PAWN
                         │
                         ▼
        ┌─────────────────────────────────┐
        │     Pawn Inspector Tabs         │
        │  Bio | Gear | Social | Health  │
        │            CULTIVATION          │ ← Injected by Patch
        └─────────────────────────────────┘
                         │
                         ▼
        ┌─────────────────────────────────┐
        │      ITab_Cultivation           │
        │   → CultivationUI.DrawTab()     │
        └─────────────────────────────────┘
                         │
                         ▼
        ┌─────────────────────────────────┐
        │      DrawCultivationTab()       │
        │                                 │
        │  ┌─ Realm & Stage Info          │
        │  ┌─ Progress Bars (Tu Vi, Qi)   │
        │  ┌─ Breakthrough Button         │
        │  ┌─ Skills List:                │
        │     ├─ Active Skills Section    │
        │     └─ Passive Skills Section   │
        │  ┌─ Cultivation Button          │
        └─────────────────────────────────┘
```

## 🔧 Gizmo System Flow

```
                  🎮 EVERY FRAME UPDATE
                         │
                         ▼
        ┌─────────────────────────────────┐
        │   Pawn_GetGizmos_Patch          │
        │   • Check if Colonist           │
        │   • Get CultivationComp         │
        └─────────────────────────────────┘
                         │
                         ▼
        ┌─────────────────────────────────┐
        │   GetCultivationGizmos()        │
        │                                 │
        │  ┌─ Cultivate Button            │
        │  ┌─ Breakthrough Button         │
        │  ┌─ Active Skill Buttons        │
        │  └─ Passive Skill Info Buttons  │
        └─────────────────────────────────┘
                         │
                         ▼
        ┌─────────────────────────────────┐
        │   For Each Active Skill:        │
        │                                 │
        │  ┌─ Command_CultivationSkill    │
        │  ┌─ Calculate Cooldown %        │
        │  ┌─ Set Button Color            │
        │  └─ Draw Cooldown Overlay       │
        └─────────────────────────────────┘
                         │
                         ▼
        ┌─────────────────────────────────┐
        │   Render on Screen              │
        │   (Bottom-right gizmo bar)      │
        └─────────────────────────────────┘
```

## 📈 Tick System Flow

```
                  ⏰ EVERY GAME TICK (60 per second)
                         │
                         ▼
        ┌─────────────────────────────────┐
        │    All CultivationComp          │
        │    CompTick() called            │
        └─────────────────────────────────┘
                         │
                         ▼
        ┌─────────────────────────────────┐
        │   CultivationData.Tick()        │
        │                                 │
        │  Every 60 ticks (1 second):     │
        │  ┌─ Regenerate Qi               │
        │  ┌─ Update Skill Cooldowns      │
        │  ┌─ Check Auto-Cultivation      │
        │                                 │
        │  Every 2500 ticks (1 hour):     │
        │  ┌─ Generate Tu Vi Points       │
        │  ┌─ Apply Passive Skills        │
        │  └─ Update Hediffs              │
        └─────────────────────────────────┘
```

## 🏗️ Data Structure Flow

```
                    💾 SAVE/LOAD SYSTEM
                         │
                         ▼
        ┌─────────────────────────────────┐
        │      CultivationComp            │
        │      ExposeData()               │
        └─────────────────────────────────┘
                         │
                         ▼
        ┌─────────────────────────────────┐
        │    CultivationData              │
        │    ExposeData()                 │
        │                                 │
        │ Scribe_Values.Look():           │
        │  • currentRealm                 │
        │  • currentStage                 │
        │  • cultivationPoints            │
        │  • currentQi, maxQi             │
        │  • talent                       │
        │                                 │
        │ Scribe_Collections.Look():      │
        │  • unlockedSkills               │
        │  • skillCooldowns               │
        │                                 │
        │ Custom Logic:                   │
        │  • Validate data                │
        │  • Migrate old saves            │
        │  • Initialize defaults          │
        └─────────────────────────────────┘
```

## 🎨 Visual Effects Flow

```
                   ✨ SKILL EXECUTION
                         │
                         ▼
        ┌─────────────────────────────────┐
        │     Skill Worker Execute        │
        │                                 │
        │  Visual Effects:                │
        │  ┌─ MoteMaker.ThrowText()       │
        │  ┌─ EffectMaker.MakeEffect()    │
        │  ┌─ FleckMaker.ThrowDustPuff()  │
        │  └─ Sound.PlayOneShotOnCamera() │
        └─────────────────────────────────┘
                         │
                         ▼
        ┌─────────────────────────────────┐
        │     Messages & Notifications    │
        │                                 │
        │  ┌─ Success Messages            │
        │  ┌─ Error Messages              │
        │  ┌─ Breakthrough Announcements  │
        │  └─ Combat Messages             │
        └─────────────────────────────────┘
```

## 🔍 Debug & Development Flow

```
                    🐛 DEVELOPMENT TOOLS
                         │
           ┌─────────────┼─────────────┐
           ▼             ▼             ▼
   ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
   │Debug Actions│ │Log Messages │ │Dev Commands │
   │             │ │             │ │             │
   │Add Skills   │ │Warnings     │ │God Mode     │
   │Set Realm    │ │Errors       │ │Skip Cooldown│
   │Test Breakthrough│ │Info      │ │Instant Max  │
   │Remove Hediffs│ │Trace       │ │             │
   └─────────────┘ └─────────────┘ └─────────────┘
                         │
                         ▼
                  📝 LOG OUTPUT
```

## 🔄 Integration Points

```
        🎯 RIMWORLD INTEGRATION POINTS
                         │
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│Health System│  │Stats System │  │Job System   │
│             │  │             │  │             │
│• Hediffs    │  │• StatOffsets│  │• Cultivation│
│• Damage     │  │• StatFactors│  │• Meditation │
│• Healing    │  │• Capacities │  │• Combat     │
└─────────────┘  └─────────────┘  └─────────────┘
        │                │                │
        ▼                ▼                ▼
        🎮 VANILLA RIMWORLD FEATURES
```

## 📋 Error Handling Flow

```
                  ⚠️ ERROR SCENARIOS
                         │
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│Null Checks  │  │Try-Catch    │  │Validation   │
│             │  │             │  │             │
│• Comp null  │  │• Skill exec │  │• Save data  │
│• Data null  │  │• UI drawing │  │• Def loading│
│• Pawn null  │  │• Patching   │  │• User input │
└─────────────┘  └─────────────┘  └─────────────┘
        │                │                │
        ▼                ▼                ▼
        📝 LOG ERROR & CONTINUE GRACEFULLY
```

## 🎯 Performance Considerations

```
                 🚀 OPTIMIZATION POINTS
                         │
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│Tick Optimization│ │UI Caching  │  │Memory Mgmt  │
│             │  │             │  │             │
│HashInterval │  │Static vars  │  │Object pools │
│Rare ticks   │  │Cached calc  │  │Dispose      │
│Skip inactive│  │Dirty flags  │  │GC friendly  │
└─────────────┘  └─────────────┘  └─────────────┘
```

Đây là sơ đồ flow hoàn chỉnh của mod Tu Tien! Mỗi phần đều kết nối với nhau tạo thành một hệ thống cultivation hoàn chỉnh và mượt mà trong RimWorld. 🏯✨
