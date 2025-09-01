# ⚡ Quick Artifact Testing Guide

## 🚀 Hướng dẫn test nhanh artifacts

### Status: ✅ **READY TO TEST**
- Artifact Definitions: **LOADED** (5 artifacts)  
- ThingDefs: **FIXED** (removed component errors)
- Recipes: **FIXED** (using GeneralLaborSpeed) 
- DefOf: **CREATED** (proper naming)

---

## 🎯 **TEST COMMANDS** (Dev Console)

### Method 1: Individual Spawning
```
ArtifactTestCommands.SpawnIronSword()        // Common sword
ArtifactTestCommands.SpawnClothRobe()        // Common robe  
ArtifactTestCommands.SpawnSpiritBow()        // Rare bow
ArtifactTestCommands.SpawnDragonCrown()      // Epic crown
ArtifactTestCommands.SpawnImmortalTalisman() // Legendary talisman
```

### Method 2: Spawn All At Once
```
ArtifactTestCommands.SpawnAllArtifacts()     // Spawn all 5 artifacts
```

### Method 3: Debug Info
```
ArtifactTestCommands.ListArtifactDefs()      // List all definitions
ArtifactDebugCommands.ListAllArtifactDefs()  // Detailed info
```

---

## 🔧 **HOW TO TEST**

### Step 1: Start RimWorld 
- Enable **Developer Mode** (Settings)
- Load game with Tu Tiên mod
- Start or load any map

### Step 2: Open Dev Console
- Press **`~`** key (tilde)
- Type one of the commands above
- Press **Enter**

### Step 3: Check Results
- Items will spawn at **map center**
- Check console for success messages
- Equip artifacts on pawns to test

---

## 🎮 **Expected Results**

### ✅ **What Should Work:**
- [x] Artifacts spawn successfully
- [x] Items appear in game with correct names
- [x] Can be picked up and equipped
- [x] Quality system works (Poor-Legendary)
- [x] Correct ThingDef-ArtifactDef linking

### 🔄 **Phase 2 Features (Coming Next):**
- [ ] ELO-based stat generation
- [ ] Auto-combat behavior
- [ ] Qi pool management
- [ ] Buff application
- [ ] Component integration

---

## 🐛 **If Problems Occur:**

### ThingDef Not Found:
```
ArtifactTestCommands.ListArtifactDefs()  // Check what's available
```

### Console Errors:
- Check Player.log for detailed errors
- Look for "TuTien" messages
- Most common: missing dependencies

### Items Don't Spawn:
- Make sure you're on a valid map
- Check map center is accessible
- Try different spawn location

---

## 📊 **Current Test Status:**

| Component | Status | Notes |
|-----------|--------|-------|
| **ArtifactDefs** | ✅ LOADED | 5 definitions working |
| **ThingDefs** | ✅ FIXED | Component errors removed |  
| **Recipes** | ✅ FIXED | Using correct stats |
| **DefOf** | ✅ CREATED | Proper name matching |
| **Registry** | ✅ FIXED | NullReference resolved |
| **Spawning** | ✅ READY | Test commands created |
| **Components** | 🔄 PHASE 2 | ELO, AI, buffs coming |

---

## 🚀 **Next Steps After Testing:**

1. **Verify Basic Spawning** ✅ (Current)
2. **Enable Component System** 🔄 (Next)
3. **Test ELO Generation** 📋 (Next)  
4. **Test Auto-Combat** 📋 (Next)
5. **Test Qi Management** 📋 (Next)

Ready to test! Try the commands above and let me know what happens! 🎉
