# Testing Cultivation Artifacts - Quick Guide

## ✅ Artifacts Created Successfully!

Hệ thống artifact đã được triển khai đầy đủ với:
- **5 Artifact Definitions** (Common → Legendary)
- **5 ThingDef Integrations** 
- **5 Crafting Recipes**
- **Debug Commands** để test
- **Complete Component System** với ELO ratings & auto-combat

## 🎯 Available Test Artifacts

### 1. Iron Cultivation Sword (Common)
- **ThingDef**: `TuTien_IronCultivationSword`
- **Features**: Auto-attacks, basic damage, low Qi pool
- **Craft**: Steel (50) + Stone blocks (20) at Smithy
- **Requirements**: Crafting 6

### 2. Cloth Cultivation Robe (Common)  
- **ThingDef**: `TuTien_ClothCultivationRobe`
- **Features**: Qi efficiency boost, defensive buffs
- **Craft**: Cloth (80) + Gold (5) at Tailoring Bench
- **Requirements**: Crafting 4

### 3. Spirit Hunter Bow (Rare)
- **ThingDef**: `TuTien_SpiritHunterBow`
- **Features**: Long-range auto-targeting, spirit arrows
- **Craft**: Wood (60) + Steel (25) + Silver (20) at Smithy  
- **Requirements**: Crafting 10

### 4. Crown of Thunder Dragon (Epic)
- **ThingDef**: `TuTien_DragonCrown`
- **Features**: Lightning abilities, massive cultivation bonuses
- **Craft**: Gold (100) + Jade (25) + Steel (75) at Smithy
- **Requirements**: Crafting 15

### 5. Talisman of Immortal Essence (Legendary)
- **ThingDef**: `TuTien_ImmortalTalisman`  
- **Features**: God-tier abilities, multiple auto-skills
- **Craft**: Gold (200) + Jade (50) + Plasteel (30) + Uranium (10)
- **Requirements**: Crafting 20, Fabrication research

## 🛠️ How to Test

### Method 1: Debug Console (Quickest)
1. Enable Dev Mode (Settings > Developer mode)
2. Open Debug Console (`~` key)
3. Run commands:
   ```
   ArtifactDebugCommands.SpawnRandomArtifact()
   ArtifactDebugCommands.SpawnSpecificArtifact("TuTien_IronCultivationSword")
   ArtifactDebugCommands.TestArtifactELOGeneration()
   ArtifactDebugCommands.ListAllArtifactDefs()
   ```

### Method 2: In-Game Crafting
1. Build required crafting stations
2. Gather materials
3. Craft artifacts from recipes
4. Equip them on cultivators
5. Watch auto-combat in action!

### Method 3: XML Spawning
Add to any event or scenario:
```xml
<li Class="ThingSpawnParams">
  <thingDef>TuTien_SpiritHunterBow</thingDef>
  <count>1</count>
</li>
```

## ⚡ Expected Features 

### ELO System in Action
- Each artifact gets hidden ELO rating (100-1200)
- Higher ELO = better stats, more buffs
- Cross-rarity overlaps (high Common > low Rare)
- Bell curve distribution for natural feel

### Auto-Combat Behavior
- **Swords**: Melee auto-attacks on nearby enemies
- **Bows**: Long-range smart targeting
- **Defensive**: Shields, barriers, healing
- **Aura**: Area buffs for allies

### Qi Management
- Artifacts consume Qi for abilities
- Absorb Qi from wearer's cultivation
- Efficiency varies by artifact quality
- Empty Qi = reduced effectiveness

### Buff Generation
- 1-8 buffs per artifact based on rarity
- Combat, cultivation, defense, aura types
- ELO-scaled effectiveness
- Stackable with other artifacts

## 🔧 Technical Architecture

### File Structure
```
Defs/
├── CultivationArtifactDefs_Basic.xml    # Artifact definitions
├── ThingDefs_CultivationArtifacts.xml   # RimWorld integration  
└── RecipeDefs_CultivationArtifacts.xml  # Crafting recipes

Source/TuTien/Systems/Artifacts/
├── CultivationArtifactDef.cs            # Core definitions
├── CultivationArtifactData.cs           # ELO-based data
├── CultivationArtifactComp.cs           # ThingComp integration
├── ArtifactGenerator.cs                 # ELO generation logic
├── ArtifactAI.cs                        # Auto-targeting system
├── ArtifactBuff.cs                      # 25+ buff types
├── CultivationArtifactExtension.cs      # ThingDef linking
└── ArtifactDebugCommands.cs             # Testing utilities
```

### Performance Optimizations
- **O(1) lookups** via registry caches
- **Smart update intervals** (30/60 ticks)
- **Event-driven** instead of constant polling
- **Cached references** to avoid repeated queries

## 📊 Testing Checklist

- [ ] Artifacts spawn successfully
- [ ] ELO ratings generate in correct ranges
- [ ] ThingDef-ArtifactDef linking works
- [ ] Equipping triggers component activation
- [ ] Auto-combat engages enemies
- [ ] Qi pools drain with ability usage
- [ ] Buffs apply and stack properly
- [ ] Crafting recipes produce artifacts
- [ ] Quality system integrates smoothly
- [ ] Performance remains stable

## 🚀 Next Phase Ideas

1. **Visual Effects**: Particle systems for abilities
2. **Sound Effects**: Audio feedback for activations
3. **Advanced AI**: Formation tactics, priority targeting
4. **Artifact Evolution**: Upgrade system with materials
5. **Set Bonuses**: Multi-artifact synergies
6. **Legendary Events**: World events around powerful artifacts

---
**Status**: ✅ READY FOR TESTING - All core systems implemented and compiled successfully!
