# Tu Tiên Mod - Installation Guide

## ✅ Build Complete!

Your Tu Tiên mod has been successfully compiled! The DLL is located at:
`D:\RimWorld\Mods\TuTien\1.5\Assemblies\TuTien.dll`

## 🎮 How to Install and Use in RimWorld:

### 1. **The mod is already in the correct location!**
   - Your mod is in: `D:\RimWorld\Mods\TuTien\`
   - RimWorld automatically detects mods in this folder

### 2. **Enable the mod in RimWorld:**
   1. Launch RimWorld
   2. Go to **Options** → **Mods**
   3. Find "Tu Tiên - Basic Pack" in the mod list
   4. **Enable** the mod by checking the box
   5. Make sure **Harmony** is also enabled (required dependency)
   6. Click **Apply** and restart RimWorld

### 3. **Mod Load Order (Important!):**
   ```
   1. Core
   2. Harmony
   3. Tu Tiên - Basic Pack
   4. (Other mods)
   ```

### 4. **Testing the Mod:**
   1. Start a new game or load an existing save
   2. Select any colonist/pawn
   3. Look for the new **"Cultivation"** tab in the inspection panel
   4. All pawns start as "Mortal" and can begin cultivation

## 🎯 How to Use the Cultivation System:

### **Basic Cultivation:**
1. **Select a pawn** → **Cultivation tab**
2. Click **"Cultivate"** button to start meditation
3. Qi accumulates and converts to Cultivation Points
4. When ready, click **"Breakthrough"** to advance

### **Realm Progression:**
- **Mortal** → **Qi Condensation** (3 stages)
- **Qi Condensation** → **Foundation Establishment** (3 stages)  
- **Foundation Establishment** → **Golden Core** (3 stages)

### **Active Abilities:**
- Unlocked abilities appear as **gizmo buttons** when pawn is selected
- Each ability costs **Qi** and has **cooldown**
- Higher stages = more powerful abilities

### **Talents:**
- **Common** (65%): Normal speed
- **Rare** (20%): 20% faster 
- **Genius** (10%): 50% faster
- **Heaven Chosen** (5%): 70% faster + 70% chance for techniques

## 🔧 Development Commands:

### **Rebuild the mod:**
```powershell
cd "D:\RimWorld\Mods\TuTien\Source\TuTien"
dotnet build --configuration Release
```

### **Quick build using batch file:**
```cmd
D:\RimWorld\Mods\TuTien\build.bat
```

## 🐛 Troubleshooting:

### **Mod not appearing:**
- Check that the mod folder is in `D:\RimWorld\Mods\`
- Ensure `About.xml` exists in the About folder
- Restart RimWorld completely

### **Compilation errors:**
- Make sure .NET SDK is installed
- Check that RimWorld assemblies exist in the referenced paths
- Verify Harmony mod is installed

### **Game crashes:**
- Check RimWorld logs: `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log`
- Disable other mods to test compatibility
- Make sure load order is correct (Harmony before Tu Tiên)

## 🎉 Features You Can Test:

1. **Start cultivation** with any pawn
2. **Watch Qi and Cultivation bars** fill up
3. **Attempt breakthroughs** (risk vs reward!)
4. **Use active abilities** in combat
5. **Check NPC enemies** for random cultivation techniques
6. **Experiment with different talents** on new pawns

The mod is fully functional and ready to play! Enjoy your cultivation journey in RimWorld!
