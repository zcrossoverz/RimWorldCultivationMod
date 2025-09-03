# 🛠️ Development Guide & Best Practices

## 📖 **Table of Contents**

1. [Development Environment Setup](#development-environment-setup)
2. [Code Architecture Guidelines](#code-architecture-guidelines)
3. [Testing Framework](#testing-framework)
4. [Debugging Tools](#debugging-tools)
5. [Performance Optimization](#performance-optimization)
6. [Common Patterns](#common-patterns)
7. [Troubleshooting Guide](#troubleshooting-guide)

---

## 🔧 **Development Environment Setup**

### **Required Tools**

```
Development Stack:
┌─────────────────────────────────────────────────────────────┐
│                    DEVELOPMENT ENVIRONMENT                  │
├──────────────┬──────────────┬──────────────┬─────────────────┤
│   IDE SETUP  │  REFERENCES  │   TESTING    │    DEBUGGING    │
│              │              │              │                 │
│ ┌──────────┐ │ ┌──────────┐ │ ┌──────────┐ │ ┌─────────────┐ │
│ │ VS/Rider │ │ │ Assembly │ │ │ Unit     │ │ │ In-game     │ │
│ │          │ │ │ Refs     │ │ │ Tests    │ │ │ Console     │ │
│ │ • C#     │ │ │          │ │ │          │ │ │             │ │
│ │ • XML    │ │ │ • Core   │ │ │ • Skill  │ │ │ • Debug     │ │
│ │ • Debug  │◄┼►│ • UI     │◄┼►│ • Artifact│◄┼►│   Commands  │ │
│ │ • Git    │ │ │ • Harmony│ │ │ • System │ │ │ • Live Edit │ │
│ │ • Build  │ │ │ • Unity  │ │ │ • Logic  │ │ │ • Profiler  │ │
│ └──────────┘ │ └──────────┘ │ └──────────┘ │ └─────────────┘ │
└──────────────┴──────────────┴──────────────┴─────────────────┘
```

### **Project Structure**

```
TuTien Mod Structure:
TuTien/
├── About/
│   ├── About.xml
│   └── Preview.png
├── Assemblies/
│   └── TuTien.dll
├── Defs/
│   ├── CultivationSkillDefs/
│   ├── CultivationAbilityDefs/
│   ├── CultivationArtifactDefs/
│   ├── ThingDefs/
│   └── RecipeDefs/
├── Source/
│   ├── TuTien/
│   │   ├── Components/
│   │   ├── Skills/
│   │   ├── Artifacts/
│   │   ├── UI/
│   │   ├── Utils/
│   │   └── Patches/
│   └── TuTien.csproj
├── Textures/
│   ├── UI/
│   ├── Things/
│   └── Effects/
├── Documentation/
│   └── CultivationArtifacts/
└── Languages/
    └── English/
```

### **Visual Studio Setup**

```csharp
// .csproj Configuration
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <OutputPath>..\Assemblies</OutputPath>
    <LangVersion>9.0</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine">
      <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\UnityEngine.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\UnityEngine.CoreModule.dll</HintPath>
    </Reference>
    <Reference Include="0Harmony">
      <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\0Harmony.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

---

## 🏗️ **Code Architecture Guidelines**

### **Naming Conventions**

```csharp
/// <summary>
/// Comprehensive naming convention examples
/// </summary>
public class NamingConventions
{
    // Classes: PascalCase
    public class CultivationSkillWorker { }
    public class ArtifactQiBooster { }
    
    // Interfaces: IPascalCase
    public interface ICultivationPlugin { }
    public interface ISkillExecutor { }
    
    // Methods: PascalCase
    public void ExecuteSkill() { }
    public bool CanUseAbility() { }
    
    // Fields: camelCase (private), PascalCase (public)
    private CultivationData cultivationData;
    public CultivationRealm CurrentRealm;
    
    // Constants: UPPER_CASE
    public const string DEFAULT_SKILL_NAME = "BasicCultivation";
    public const float MAX_QI_MULTIPLIER = 10f;
    
    // DefName conventions: Category_SpecificName
    // Skills: "Skill_QiPunch", "Skill_Fireball"
    // Abilities: "Ability_SwordStrike", "Ability_Heal"
    // Artifacts: "Artifact_QiSword", "Artifact_CultivationManual"
    
    // XML node naming: lowercase with underscores
    // <cultivation_skill>, <auto_skills>, <qi_cost>
}
```

### **Component Design Pattern**

```csharp
/// <summary>
/// Standard component template for cultivation features
/// </summary>
public abstract class CultivationComponentBase : ThingComp
{
    #region Abstract/Virtual Members
    protected abstract void InitializeComponent();
    protected virtual void OnComponentEnabled() { }
    protected virtual void OnComponentDisabled() { }
    protected abstract void UpdateComponent();
    #endregion
    
    #region Core Framework
    protected CultivationComp cultivationComp;
    protected bool isInitialized = false;
    private int lastUpdateTick = -1;
    
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        
        cultivationComp = parent.GetComp<CultivationComp>();
        if (cultivationComp == null)
        {
            Log.Error($"{GetType().Name} requires CultivationComp but none found on {parent}");
            return;
        }
        
        InitializeComponent();
        isInitialized = true;
        OnComponentEnabled();
    }
    
    public override void CompTick()
    {
        if (!isInitialized) return;
        
        // Throttle updates to reduce performance impact
        var currentTick = Find.TickManager.TicksGame;
        if (currentTick - lastUpdateTick >= GetUpdateInterval())
        {
            UpdateComponent();
            lastUpdateTick = currentTick;
        }
    }
    
    protected virtual int GetUpdateInterval() => 60; // Default: 1 second
    #endregion
    
    #region Validation
    protected bool ValidateRequirements(out string error)
    {
        error = "";
        
        if (cultivationComp?.cultivationData == null)
        {
            error = "No cultivation data available";
            return false;
        }
        
        return ValidateSpecificRequirements(out error);
    }
    
    protected virtual bool ValidateSpecificRequirements(out string error)
    {
        error = "";
        return true;
    }
    #endregion
    
    #region Event Handling
    protected void RegisterForEvents()
    {
        CultivationEventManager.OnSkillUsed += OnSkillUsed;
        CultivationEventManager.OnBreakthrough += OnBreakthrough;
    }
    
    protected void UnregisterFromEvents()
    {
        CultivationEventManager.OnSkillUsed -= OnSkillUsed;
        CultivationEventManager.OnBreakthrough -= OnBreakthrough;
    }
    
    protected virtual void OnSkillUsed(Pawn pawn, string skillName, bool success) { }
    protected virtual void OnBreakthrough(Pawn pawn, CultivationRealm oldRealm, int oldStage, 
                                        CultivationRealm newRealm, int newStage) { }
    #endregion
}
```

### **Error Handling Standards**

```csharp
/// <summary>
/// Standardized error handling for cultivation system
/// </summary>
public static class CultivationErrorHandler
{
    public enum ErrorLevel
    {
        Warning,
        Error,
        Critical
    }
    
    public static void LogError(string message, Exception ex = null, ErrorLevel level = ErrorLevel.Error)
    {
        var formattedMessage = $"[Tu Tiên] {level}: {message}";
        
        if (ex != null)
        {
            formattedMessage += $"\nException: {ex.Message}\nStack: {ex.StackTrace}";
        }
        
        switch (level)
        {
            case ErrorLevel.Warning:
                Log.Warning(formattedMessage);
                break;
            case ErrorLevel.Error:
                Log.Error(formattedMessage);
                break;
            case ErrorLevel.Critical:
                Log.Error(formattedMessage);
                // Could also show in-game message for critical errors
                if (Current.Game != null)
                {
                    Messages.Message($"Tu Tiên Critical Error: {message}", MessageTypeDefOf.NegativeEvent);
                }
                break;
        }
    }
    
    public static T SafeExecute<T>(Func<T> action, T defaultValue = default, string context = "Unknown")
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            LogError($"Error in {context}", ex);
            return defaultValue;
        }
    }
    
    public static void SafeExecute(Action action, string context = "Unknown")
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            LogError($"Error in {context}", ex);
        }
    }
}

/// <summary>
/// Usage example of error handling
/// </summary>
public class SafeCultivationOperations
{
    public void ExecuteSkillSafely(Pawn pawn, string skillName)
    {
        CultivationErrorHandler.SafeExecute(() =>
        {
            var comp = pawn.GetComp<CultivationComp>();
            if (comp == null)
            {
                throw new InvalidOperationException("Pawn lacks cultivation component");
            }
            
            var skillDef = CultivationCache.GetSkillDef(skillName);
            if (skillDef == null)
            {
                throw new ArgumentException($"Skill not found: {skillName}");
            }
            
            // Execute skill logic...
            
        }, $"ExecuteSkill({skillName})");
    }
}
```

---

## 🧪 **Testing Framework**

### **Unit Test Structure**

```csharp
/// <summary>
/// Unit testing framework for cultivation features
/// </summary>
[TestFixture]
public class CultivationSystemTests
{
    private Pawn testPawn;
    private CultivationComp testComp;
    
    [SetUp]
    public void SetUp()
    {
        // Create test pawn with cultivation component
        testPawn = CreateTestPawn();
        testComp = testPawn.GetComp<CultivationComp>();
        Assert.NotNull(testComp, "Test pawn should have cultivation component");
    }
    
    [Test]
    public void TestSkillLearning()
    {
        // Arrange
        var skillName = "QiPunch";
        var initialSkillCount = testComp.knownSkills.Count;
        
        // Act
        testComp.LearnSkill(skillName);
        
        // Assert
        Assert.Contains(skillName, testComp.knownSkills.ToList());
        Assert.AreEqual(initialSkillCount + 1, testComp.knownSkills.Count);
    }
    
    [Test]
    public void TestQiConsumption()
    {
        // Arrange
        var initialQi = testComp.cultivationData.currentQi;
        var consumeAmount = 10f;
        
        // Act
        var result = testComp.ConsumeQi(consumeAmount);
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(initialQi - consumeAmount, testComp.cultivationData.currentQi);
    }
    
    [Test]
    public void TestInsufficientQi()
    {
        // Arrange
        testComp.cultivationData.currentQi = 5f;
        var consumeAmount = 10f;
        
        // Act
        var result = testComp.ConsumeQi(consumeAmount);
        
        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(5f, testComp.cultivationData.currentQi); // Should remain unchanged
    }
    
    [TestCase(CultivationRealm.QiCondensation, 1, 100f)]
    [TestCase(CultivationRealm.FoundationBuilding, 5, 400f)]
    [TestCase(CultivationRealm.CoreFormation, 9, 1152f)]
    public void TestAdvancementRequirements(CultivationRealm realm, int stage, float expectedPoints)
    {
        // Arrange
        testComp.cultivationData.currentRealm = realm;
        testComp.cultivationData.currentStage = stage;
        
        // Act
        var requiredPoints = testComp.cultivationData.GetRequiredPoints();
        
        // Assert
        Assert.AreEqual(expectedPoints, requiredPoints, 0.1f);
    }
    
    private Pawn CreateTestPawn()
    {
        // Create a minimal test pawn with cultivation component
        var pawnDef = PawnKindDefOf.Colonist.race;
        var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
        
        // Add cultivation component if not present
        if (pawn.GetComp<CultivationComp>() == null)
        {
            var compProps = new CultivationCompProperties();
            var comp = new CultivationComp();
            comp.props = compProps;
            pawn.AllComps.Add(comp);
            comp.parent = pawn;
            comp.Initialize(compProps);
        }
        
        return pawn;
    }
}

/// <summary>
/// Integration tests for skill execution system
/// </summary>
[TestFixture]
public class SkillExecutionTests
{
    [Test]
    public void TestQiPunchExecution()
    {
        // Arrange
        var pawn = CreateTestPawn();
        var target = CreateTestTarget();
        var skillDef = DefDatabase<CultivationSkillDef>.GetNamed("QiPunch");
        
        // Act
        var worker = skillDef.GetSkillWorker();
        var canExecute = worker.CanExecute(pawn, target);
        
        // Assert
        Assert.IsTrue(canExecute);
        
        // Execute and verify effects
        var initialHealth = target.Thing.HitPoints;
        worker.ExecuteSkillEffect(pawn, target);
        
        // Target should take damage
        Assert.Less(target.Thing.HitPoints, initialHealth);
    }
    
    private LocalTargetInfo CreateTestTarget()
    {
        // Create a test target (could be a dummy pawn or object)
        var target = PawnGenerator.GeneratePawn(PawnKindDefOf.Raider, Faction.OfPirates);
        return new LocalTargetInfo(target);
    }
}
```

### **Test Data Generation**

```csharp
/// <summary>
/// Test data factory for consistent test scenarios
/// </summary>
public static class CultivationTestDataFactory
{
    public static CultivationData CreateTestCultivationData(CultivationRealm realm = CultivationRealm.QiCondensation, 
                                                           int stage = 1, float qi = 100f)
    {
        return new CultivationData
        {
            currentRealm = realm,
            currentStage = stage,
            currentQi = qi,
            maxQi = qi,
            cultivationPoints = 0f,
            lastCultivationTime = 0,
            primaryElement = QiType.Neutral,
            elementalExperience = new Dictionary<string, float>(),
            skillUsageCount = new Dictionary<string, int>()
        };
    }
    
    public static CultivationSkillDef CreateTestSkillDef(string defName, CultivationRealm requiredRealm = CultivationRealm.QiCondensation,
                                                        int requiredStage = 1, float qiCost = 10f)
    {
        return new CultivationSkillDef
        {
            defName = defName,
            label = $"Test {defName}",
            description = $"Test skill: {defName}",
            requiredRealm = requiredRealm,
            requiredStage = requiredStage,
            qiCost = qiCost,
            cooldownTicks = 60,
            range = 5f,
            associatedElement = QiType.Neutral,
            skillWorkerClass = typeof(SkillWorker_QiPunch)
        };
    }
    
    public static CultivationArtifactDef CreateTestArtifactDef(string defName, List<string> autoSkills = null)
    {
        return new CultivationArtifactDef
        {
            defName = defName,
            artifactName = $"Test {defName}",
            description = $"Test artifact: {defName}",
            autoSkills = autoSkills ?? new List<string> { "QiPunch" },
            qiBonus = 50f,
            realmBonus = 0,
            stageBonus = 0
        };
    }
    
    /// <summary>
    /// Create complete test scenario with pawn, skills, and artifacts
    /// </summary>
    public static TestScenario CreateCompleteScenario()
    {
        var scenario = new TestScenario();
        
        // Create test pawn
        scenario.Pawn = CreateTestCultivator();
        
        // Create test skills
        scenario.Skills = new List<CultivationSkillDef>
        {
            CreateTestSkillDef("TestSkill_Basic"),
            CreateTestSkillDef("TestSkill_Advanced", CultivationRealm.FoundationBuilding, 3, 25f)
        };
        
        // Create test artifacts
        scenario.Artifacts = new List<CultivationArtifactDef>
        {
            CreateTestArtifactDef("TestArtifact_Sword", new List<string> { "TestSkill_Basic" }),
            CreateTestArtifactDef("TestArtifact_Manual", new List<string> { "TestSkill_Advanced" })
        };
        
        return scenario;
    }
    
    private static Pawn CreateTestCultivator()
    {
        var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
        
        // Ensure cultivation component
        var comp = pawn.GetComp<CultivationComp>();
        if (comp == null)
        {
            // Add component logic here
        }
        
        return pawn;
    }
    
    public class TestScenario
    {
        public Pawn Pawn { get; set; }
        public List<CultivationSkillDef> Skills { get; set; } = new List<CultivationSkillDef>();
        public List<CultivationArtifactDef> Artifacts { get; set; } = new List<CultivationArtifactDef>();
    }
}
```

---

## 🐛 **Debugging Tools**

### **In-Game Debug Console Commands**

```csharp
/// <summary>
/// Debug commands for testing cultivation system in-game
/// </summary>
public static class CultivationDebugCommands
{
    [DebugAction("Tu Tiên", "Grant Skill", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void GrantSkill()
    {
        var pawn = UI.MouseCell().GetFirstPawn(Find.CurrentMap);
        if (pawn?.GetComp<CultivationComp>() == null) return;
        
        var options = new List<DebugMenuOption>();
        
        // Add all available skills
        foreach (var skillDef in DefDatabase<CultivationSkillDef>.AllDefs)
        {
            options.Add(new DebugMenuOption(skillDef.label, DebugMenuOptionMode.Action, () =>
            {
                pawn.GetComp<CultivationComp>().LearnSkill(skillDef.defName);
            }));
        }
        
        // Add all available abilities
        foreach (var abilityDef in DefDatabase<CultivationAbilityDef>.AllDefs)
        {
            options.Add(new DebugMenuOption(abilityDef.abilityLabel ?? abilityDef.label, DebugMenuOptionMode.Action, () =>
            {
                pawn.GetComp<CultivationComp>().LearnSkill(abilityDef.defName);
            }));
        }
        
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
    
    [DebugAction("Tu Tiên", "Set Realm", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void SetRealm()
    {
        var pawn = UI.MouseCell().GetFirstPawn(Find.CurrentMap);
        var comp = pawn?.GetComp<CultivationComp>();
        if (comp?.cultivationData == null) return;
        
        var options = new List<DebugMenuOption>();
        
        foreach (CultivationRealm realm in Enum.GetValues(typeof(CultivationRealm)))
        {
            options.Add(new DebugMenuOption(realm.ToString(), DebugMenuOptionMode.Action, () =>
            {
                comp.cultivationData.currentRealm = realm;
                comp.cultivationData.currentStage = 1;
                comp.cultivationData.cultivationPoints = 0f;
                Messages.Message($"{pawn.LabelShort} set to {realm}", MessageTypeDefOf.TaskCompletion);
            }));
        }
        
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
    
    [DebugAction("Tu Tiên", "Add Qi", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void AddQi()
    {
        var pawn = UI.MouseCell().GetFirstPawn(Find.CurrentMap);
        var comp = pawn?.GetComp<CultivationComp>();
        if (comp?.cultivationData == null) return;
        
        comp.cultivationData.currentQi = comp.cultivationData.maxQi;
        Messages.Message($"{pawn.LabelShort} qi restored", MessageTypeDefOf.TaskCompletion);
    }
    
    [DebugAction("Tu Tiên", "Force Breakthrough", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ForceBreakthrough()
    {
        var pawn = UI.MouseCell().GetFirstPawn(Find.CurrentMap);
        var comp = pawn?.GetComp<CultivationComp>();
        if (comp?.cultivationData == null) return;
        
        var requiredPoints = comp.cultivationData.GetRequiredPoints();
        comp.cultivationData.cultivationPoints = requiredPoints;
        comp.CheckForBreakthrough();
    }
    
    [DebugAction("Tu Tiên", "Show Debug Info", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ShowDebugInfo()
    {
        var pawn = UI.MouseCell().GetFirstPawn(Find.CurrentMap);
        var comp = pawn?.GetComp<CultivationComp>();
        if (comp?.cultivationData == null) return;
        
        var info = GetDebugInfo(comp);
        Find.WindowStack.Add(new Dialog_MessageBox(info));
    }
    
    private static string GetDebugInfo(CultivationComp comp)
    {
        var sb = new StringBuilder();
        var data = comp.cultivationData;
        
        sb.AppendLine($"=== Cultivation Debug Info ===");
        sb.AppendLine($"Realm: {data.currentRealm} Stage {data.currentStage}");
        sb.AppendLine($"Qi: {data.currentQi:F1}/{data.maxQi:F1}");
        sb.AppendLine($"Progress: {data.GetCultivationProgress():P1}");
        sb.AppendLine($"Required Points: {data.GetRequiredPoints():F1}");
        sb.AppendLine($"Current Points: {data.cultivationPoints:F1}");
        
        sb.AppendLine();
        sb.AppendLine($"Known Skills ({comp.knownSkills.Count}):");
        foreach (var skill in comp.knownSkills)
        {
            sb.AppendLine($"  - {skill}");
        }
        
        sb.AppendLine();
        sb.AppendLine($"Active Cooldowns ({comp.skillCooldowns.Count(kvp => kvp.Value > 0)}):");
        foreach (var cooldown in comp.skillCooldowns.Where(kvp => kvp.Value > 0))
        {
            sb.AppendLine($"  - {cooldown.Key}: {cooldown.Value} ticks");
        }
        
        return sb.ToString();
    }
}
```

### **Visual Debug Overlays**

```csharp
/// <summary>
/// Visual debug overlays for cultivation system
/// </summary>
public static class CultivationDebugRenderer
{
    private static bool showQiOverlay = false;
    private static bool showSkillRanges = false;
    private static bool showCultivationProgress = false;
    
    [DebugAction("Tu Tiên", "Toggle Qi Overlay", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ToggleQiOverlay()
    {
        showQiOverlay = !showQiOverlay;
        Messages.Message($"Qi overlay: {(showQiOverlay ? "ON" : "OFF")}", MessageTypeDefOf.TaskCompletion);
    }
    
    [DebugAction("Tu Tiên", "Toggle Skill Ranges", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ToggleSkillRanges()
    {
        showSkillRanges = !showSkillRanges;
        Messages.Message($"Skill ranges: {(showSkillRanges ? "ON" : "OFF")}", MessageTypeDefOf.TaskCompletion);
    }
    
    public static void DrawDebugOverlays()
    {
        if (!Prefs.DevMode) return;
        
        if (showQiOverlay) DrawQiOverlay();
        if (showSkillRanges) DrawSkillRanges();
        if (showCultivationProgress) DrawCultivationProgress();
    }
    
    private static void DrawQiOverlay()
    {
        var map = Find.CurrentMap;
        if (map == null) return;
        
        foreach (var pawn in map.mapPawns.AllPawnsSpawned)
        {
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null) continue;
            
            var pos = pawn.DrawPos;
            var data = comp.cultivationData;
            
            // Draw qi bar above pawn
            var barRect = new Rect(pos.x - 0.5f, pos.z + 1f, 1f, 0.1f);
            var qiPercent = data.GetQiPercent();
            
            // Background
            GenUI.DrawTextureWithMaterial(barRect, BaseContent.WhiteTex, null, Color.black);
            
            // Qi bar
            var fillRect = new Rect(barRect.x, barRect.y, barRect.width * qiPercent, barRect.height);
            GenUI.DrawTextureWithMaterial(fillRect, BaseContent.WhiteTex, null, Color.cyan);
            
            // Text
            var labelPos = new Vector3(pos.x, pos.y + 2f, pos.z);
            GenMapUI.DrawThingLabel(labelPos, $"{data.currentQi:F0}/{data.maxQi:F0}", Color.white);
        }
    }
    
    private static void DrawSkillRanges()
    {
        var selectedPawn = Find.Selector.SingleSelectedThing as Pawn;
        var comp = selectedPawn?.GetComp<CultivationComp>();
        if (comp == null) return;
        
        // Draw range circles for all known skills
        foreach (var skillName in comp.knownSkills)
        {
            var skillDef = CultivationCache.GetSkillDef(skillName);
            var abilityDef = CultivationCache.GetAbilityDef(skillName);
            
            var range = skillDef?.range ?? abilityDef?.range ?? 0f;
            if (range > 0f)
            {
                GenDraw.DrawRadiusRing(selectedPawn.Position, range);
            }
        }
    }
    
    private static void DrawCultivationProgress()
    {
        var map = Find.CurrentMap;
        if (map == null) return;
        
        foreach (var pawn in map.mapPawns.AllPawnsSpawned)
        {
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null) continue;
            
            var pos = pawn.DrawPos;
            var data = comp.cultivationData;
            var progress = data.GetCultivationProgress();
            
            // Draw progress circle
            var numSegments = 20;
            var radius = 0.8f;
            var filledSegments = Mathf.RoundToInt(numSegments * progress);
            
            for (int i = 0; i < numSegments; i++)
            {
                var angle = (360f / numSegments) * i;
                var segmentPos = pos + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );
                
                var color = i < filledSegments ? Color.gold : Color.gray;
                GenDraw.DrawTargetHighlight(new LocalTargetInfo(segmentPos.ToIntVec3()));
            }
        }
    }
}
```

---

## ⚡ **Performance Optimization**

### **Caching Strategies**

```csharp
/// <summary>
/// Advanced caching system for cultivation operations
/// </summary>
public static class CultivationCache
{
    #region Def Caches
    private static readonly Dictionary<string, CultivationSkillDef> skillDefCache = 
        new Dictionary<string, CultivationSkillDef>();
    
    private static readonly Dictionary<string, CultivationAbilityDef> abilityDefCache = 
        new Dictionary<string, CultivationAbilityDef>();
    
    private static readonly Dictionary<string, CultivationArtifactDef> artifactDefCache = 
        new Dictionary<string, CultivationArtifactDef>();
    
    private static bool cachesInitialized = false;
    
    static CultivationCache()
    {
        InitializeCaches();
    }
    
    private static void InitializeCaches()
    {
        if (cachesInitialized) return;
        
        CultivationPerformanceMonitor.StartTiming("InitializeCaches");
        
        // Cache all skill defs
        foreach (var skillDef in DefDatabase<CultivationSkillDef>.AllDefs)
        {
            skillDefCache[skillDef.defName] = skillDef;
        }
        
        // Cache all ability defs
        foreach (var abilityDef in DefDatabase<CultivationAbilityDef>.AllDefs)
        {
            abilityDefCache[abilityDef.defName] = abilityDef;
        }
        
        // Cache all artifact defs
        foreach (var artifactDef in DefDatabase<CultivationArtifactDef>.AllDefs)
        {
            artifactDefCache[artifactDef.defName] = artifactDef;
        }
        
        cachesInitialized = true;
        CultivationPerformanceMonitor.EndTiming("InitializeCaches");
        
        Log.Message($"[Tu Tiên] Cached {skillDefCache.Count} skills, {abilityDefCache.Count} abilities, {artifactDefCache.Count} artifacts");
    }
    
    public static CultivationSkillDef GetSkillDef(string defName)
    {
        if (!cachesInitialized) InitializeCaches();
        return skillDefCache.GetValueOrDefault(defName);
    }
    
    public static CultivationAbilityDef GetAbilityDef(string defName)
    {
        if (!cachesInitialized) InitializeCaches();
        return abilityDefCache.GetValueOrDefault(defName);
    }
    
    public static CultivationArtifactDef GetArtifactDef(string defName)
    {
        if (!cachesInitialized) InitializeCaches();
        return artifactDefCache.GetValueOrDefault(defName);
    }
    #endregion
    
    #region Component Caches
    private static readonly Dictionary<Pawn, CultivationComp> compCache = 
        new Dictionary<Pawn, CultivationComp>();
    
    private static int lastCompCacheCleanup = 0;
    
    public static CultivationComp GetCultivationComp(Pawn pawn)
    {
        if (pawn == null) return null;
        
        // Check cache first
        if (compCache.TryGetValue(pawn, out var cached))
        {
            return cached;
        }
        
        // Get from pawn and cache
        var comp = pawn.GetComp<CultivationComp>();
        if (comp != null)
        {
            compCache[pawn] = comp;
        }
        
        // Periodic cache cleanup
        var currentTick = Find.TickManager.TicksGame;
        if (currentTick - lastCompCacheCleanup > 3600) // Every minute
        {
            CleanupComponentCache();
            lastCompCacheCleanup = currentTick;
        }
        
        return comp;
    }
    
    private static void CleanupComponentCache()
    {
        var keysToRemove = new List<Pawn>();
        
        foreach (var kvp in compCache)
        {
            var pawn = kvp.Key;
            if (pawn == null || pawn.Destroyed || !pawn.Spawned)
            {
                keysToRemove.Add(pawn);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            compCache.Remove(key);
        }
    }
    #endregion
    
    #region Skill Worker Cache
    private static readonly Dictionary<Type, SkillWorkerBase> skillWorkerCache = 
        new Dictionary<Type, SkillWorkerBase>();
    
    public static T GetSkillWorker<T>() where T : SkillWorkerBase, new()
    {
        var type = typeof(T);
        if (!skillWorkerCache.TryGetValue(type, out var worker))
        {
            worker = new T();
            skillWorkerCache[type] = worker;
        }
        
        return (T)worker;
    }
    #endregion
    
    #region Cache Statistics
    public static CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            skillDefCount = skillDefCache.Count,
            abilityDefCount = abilityDefCache.Count,
            artifactDefCount = artifactDefCache.Count,
            componentCacheSize = compCache.Count,
            skillWorkerCacheSize = skillWorkerCache.Count,
            isInitialized = cachesInitialized
        };
    }
    
    public class CacheStatistics
    {
        public int skillDefCount;
        public int abilityDefCount;
        public int artifactDefCount;
        public int componentCacheSize;
        public int skillWorkerCacheSize;
        public bool isInitialized;
        
        public override string ToString()
        {
            return $"Cache Stats - Skills: {skillDefCount}, Abilities: {abilityDefCount}, " +
                   $"Artifacts: {artifactDefCount}, Components: {componentCacheSize}, " +
                   $"Workers: {skillWorkerCacheSize}, Init: {isInitialized}";
        }
    }
    #endregion
}
```

### **Memory Management**

```csharp
/// <summary>
/// Memory management utilities for cultivation system
/// </summary>
public static class CultivationMemoryManager
{
    private static readonly List<WeakReference> trackedObjects = new List<WeakReference>();
    private static int lastGCCheck = 0;
    
    public static void TrackObject(object obj)
    {
        trackedObjects.Add(new WeakReference(obj));
    }
    
    public static void PerformGarbageCollection()
    {
        var currentTick = Find.TickManager.TicksGame;
        
        // Only check every 10 seconds
        if (currentTick - lastGCCheck < 600) return;
        lastGCCheck = currentTick;
        
        // Clean up dead references
        trackedObjects.RemoveAll(wr => !wr.IsAlive);
        
        // Force GC if memory pressure is high
        var memoryBefore = GC.GetTotalMemory(false);
        if (memoryBefore > 100 * 1024 * 1024) // 100MB threshold
        {
            Log.Warning($"[Tu Tiên] High memory usage ({memoryBefore / (1024 * 1024)}MB), forcing GC");
            GC.Collect(0, GCCollectionMode.Optimized);
            
            var memoryAfter = GC.GetTotalMemory(true);
            Log.Message($"[Tu Tiên] GC freed {(memoryBefore - memoryAfter) / (1024 * 1024)}MB");
        }
    }
    
    public static MemoryReport GetMemoryReport()
    {
        return new MemoryReport
        {
            totalMemory = GC.GetTotalMemory(false),
            trackedObjects = trackedObjects.Count(wr => wr.IsAlive),
            gen0Collections = GC.CollectionCount(0),
            gen1Collections = GC.CollectionCount(1),
            gen2Collections = GC.CollectionCount(2)
        };
    }
    
    public class MemoryReport
    {
        public long totalMemory;
        public int trackedObjects;
        public int gen0Collections;
        public int gen1Collections;
        public int gen2Collections;
        
        public override string ToString()
        {
            return $"Memory: {totalMemory / (1024 * 1024)}MB, Objects: {trackedObjects}, " +
                   $"GC: G0={gen0Collections}, G1={gen1Collections}, G2={gen2Collections}";
        }
    }
}
```

---

## 🔧 **Common Patterns**

### **Skill Implementation Template**

```csharp
/// <summary>
/// Template for implementing new cultivation skills
/// </summary>
// Step 1: Create XML definition
/*
<CultivationSkillDef>
  <defName>Skill_NewSkill</defName>
  <label>new skill</label>
  <description>A new cultivation skill.</description>
  <requiredRealm>QiCondensation</requiredRealm>
  <requiredStage>1</requiredStage>
  <qiCost>15</qiCost>
  <cooldownTicks>120</cooldownTicks>
  <range>3</range>
  <associatedElement>Fire</associatedElement>
  <skillWorkerClass>SkillWorker_NewSkill</skillWorkerClass>
</CultivationSkillDef>
*/

// Step 2: Implement skill worker
public class SkillWorker_NewSkill : SkillWorkerBase
{
    public override bool CanExecute(Pawn caster, LocalTargetInfo target)
    {
        // Check base requirements first
        if (!base.CanExecute(caster, target)) return false;
        
        // Add specific requirements
        if (target.Thing == null)
        {
            SetDisabledReason("No target");
            return false;
        }
        
        // Range check
        if (caster.Position.DistanceTo(target.Cell) > skillDef.range)
        {
            SetDisabledReason("Out of range");
            return false;
        }
        
        return true;
    }
    
    public override void ExecuteSkillEffect(Pawn caster, LocalTargetInfo target)
    {
        // Consume qi and start cooldown
        base.ExecuteSkillEffect(caster, target);
        
        // Skill-specific effects
        var targetThing = target.Thing;
        if (targetThing != null)
        {
            // Apply damage
            var damage = CalculateDamage(caster);
            var damageInfo = new DamageInfo(DamageDefOf.Burn, damage, 0f, -1f, caster);
            targetThing.TakeDamage(damageInfo);
            
            // Visual effects
            CultivationVFX.PlayEffect("fire_strike", target.Thing.DrawPos, caster.Map, 1f);
            
            // Sound effect
            SoundDefOf.Explosion_Bomb.PlayOneShot(new TargetInfo(target.Cell, caster.Map));
        }
        
        // Record mastery progress
        RecordSkillUsage(caster, true);
    }
    
    private float CalculateDamage(Pawn caster)
    {
        var baseDamage = 20f;
        
        // Realm scaling
        var realmMultiplier = 1f + (int)GetCultivationRealm(caster) * 0.2f;
        
        // Mastery bonus
        var masteryBonus = GetMasteryBonus(caster, skillDef.defName);
        
        return baseDamage * realmMultiplier * masteryBonus;
    }
}

// Step 3: Register for auto-loading (if needed)
// No additional registration needed - DefDatabase handles XML loading

// Step 4: Add to starting skills or artifact grants (optional)
// In CultivationCompProperties or CultivationArtifactDef
```

### **Artifact Implementation Template**

```csharp
/// <summary>
/// Template for implementing new cultivation artifacts
/// </summary>
// Step 1: Create ThingDef
/*
<ThingDef ParentName="BaseWeapon">
  <defName>NewCultivationArtifact</defName>
  <label>new cultivation artifact</label>
  <description>A powerful cultivation artifact.</description>
  <comps>
    <li Class="CultivationArtifactCompProperties">
      <cultivationArtifactDef>CultivationArtifact_NewArtifact</cultivationArtifactDef>
    </li>
  </comps>
</ThingDef>
*/

// Step 2: Create CultivationArtifactDef
/*
<CultivationArtifactDef>
  <defName>CultivationArtifact_NewArtifact</defName>
  <artifactName>new artifact</artifactName>
  <description>A mystical artifact that enhances cultivation.</description>
  <autoSkills>
    <li>Skill_NewSkill</li>
  </autoSkills>
  <qiBonus>25</qiBonus>
  <realmBonus>0</realmBonus>
  <stageBonus>1</stageBonus>
  <qiRegenMultiplier>1.2</qiRegenMultiplier>
</CultivationArtifactDef>
*/

// Step 3: Optional custom component for special effects
public class CultivationArtifactComp_NewArtifact : CultivationArtifactComp
{
    protected override void OnEquippedSpecialEffects(Pawn pawn)
    {
        base.OnEquippedSpecialEffects(pawn);
        
        // Custom effects when equipped
        Messages.Message($"{pawn.LabelShort} feels the power of the {def.artifactName}!", 
                        MessageTypeDefOf.PositiveEvent);
        
        // Grant temporary bonuses
        ApplyTemporaryEffects(pawn);
    }
    
    protected override void OnUnequippedSpecialEffects(Pawn pawn)
    {
        base.OnUnequippedSpecialEffects(pawn);
        
        // Remove temporary effects
        RemoveTemporaryEffects(pawn);
    }
    
    private void ApplyTemporaryEffects(Pawn pawn)
    {
        // Could apply hediffs, mood bonuses, etc.
    }
    
    private void RemoveTemporaryEffects(Pawn pawn)
    {
        // Remove any temporary effects
    }
}
```

---

## 🚨 **Troubleshooting Guide**

### **Common Issues and Solutions**

| Issue | Symptoms | Cause | Solution |
|-------|----------|-------|----------|
| **Skills not showing in gizmos** | No skill buttons visible | Missing harmony patch or component | Verify AbilityUIPatches applied, check CultivationComp exists |
| **"Skill not found" errors** | Red error in logs | DefName mismatch | Check XML defName matches C# references |
| **Infinite cooldowns** | Skills permanently disabled | Cooldown not reducing | Verify CompTick is running, check ReduceCooldowns() |
| **Qi not regenerating** | Qi stays at zero | Regen calculation issue | Check GetQiRegenRate() and cultivation data |
| **Artifacts not granting skills** | Equipment doesn't add gizmos | autoSkills not processed | Verify artifact comp notification system |
| **Save/load corruption** | Data lost on reload | ExposeData incomplete | Check all IExposable implementations |

### **Debugging Workflow**

```csharp
/// <summary>
/// Step-by-step debugging workflow for cultivation issues
/// </summary>
public static class CultivationDebugWorkflow
{
    public static void DiagnoseSkillIssue(Pawn pawn, string skillName)
    {
        var report = new StringBuilder();
        report.AppendLine($"=== Skill Diagnosis: {skillName} ===");
        
        // Step 1: Check component
        var comp = pawn.GetComp<CultivationComp>();
        if (comp == null)
        {
            report.AppendLine("❌ CRITICAL: No CultivationComp found");
            LogReport(report.ToString());
            return;
        }
        report.AppendLine("✅ CultivationComp exists");
        
        // Step 2: Check skill definition
        var skillDef = CultivationCache.GetSkillDef(skillName);
        var abilityDef = CultivationCache.GetAbilityDef(skillName);
        if (skillDef == null && abilityDef == null)
        {
            report.AppendLine($"❌ CRITICAL: Skill definition not found: {skillName}");
            LogReport(report.ToString());
            return;
        }
        report.AppendLine($"✅ Definition found: {(skillDef != null ? "Skill" : "Ability")}");
        
        // Step 3: Check known skills
        if (!comp.knownSkills.Contains(skillName))
        {
            report.AppendLine($"⚠️ WARNING: Skill not in known skills");
            report.AppendLine($"Known skills: {string.Join(", ", comp.knownSkills)}");
        }
        else
        {
            report.AppendLine("✅ Skill is known");
        }
        
        // Step 4: Check requirements
        var canLearn = comp.CanLearnSkill(skillName, out string reason);
        report.AppendLine($"Can learn: {canLearn} ({reason})");
        
        // Step 5: Check gizmo generation
        var availableSkills = comp.GetAllAvailableSkills();
        if (availableSkills.Contains(skillName))
        {
            report.AppendLine("✅ Skill in available skills");
        }
        else
        {
            report.AppendLine("❌ ERROR: Skill not in available skills");
        }
        
        // Step 6: Check cooldowns
        if (comp.skillCooldowns.TryGetValue(skillName, out int cooldown) && cooldown > 0)
        {
            report.AppendLine($"⚠️ Skill on cooldown: {cooldown} ticks");
        }
        
        LogReport(report.ToString());
    }
    
    public static void DiagnoseGizmoIssue(Pawn pawn)
    {
        var report = new StringBuilder();
        report.AppendLine($"=== Gizmo Diagnosis: {pawn.LabelShort} ===");
        
        // Check harmony patch
        var harmonyId = "tutien.cultivation";
        var harmony = new HarmonyLib.Harmony(harmonyId);
        var patches = HarmonyLib.Harmony.GetAllPatchedMethods();
        
        var gizmoPatched = patches.Any(m => m.Name == "GetGizmos" && m.DeclaringType == typeof(Pawn));
        report.AppendLine($"Harmony gizmo patch active: {gizmoPatched}");
        
        // Check component and data
        var comp = pawn.GetComp<CultivationComp>();
        if (comp?.cultivationData == null)
        {
            report.AppendLine("❌ No cultivation component or data");
            LogReport(report.ToString());
            return;
        }
        
        // Generate gizmos manually for testing
        try
        {
            var gizmos = AbilityUIPatches.GenerateCultivationGizmos(pawn, comp).ToList();
            report.AppendLine($"✅ Generated {gizmos.Count} gizmos successfully");
            
            foreach (var gizmo in gizmos)
            {
                report.AppendLine($"  - {gizmo.GetType().Name}: {gizmo.defaultLabel}");
            }
        }
        catch (Exception ex)
        {
            report.AppendLine($"❌ Gizmo generation failed: {ex.Message}");
        }
        
        LogReport(report.ToString());
    }
    
    private static void LogReport(string report)
    {
        Log.Message($"[Tu Tiên Debug]\n{report}");
        
        // Also show in-game if in dev mode
        if (Prefs.DevMode && Current.Game != null)
        {
            Find.WindowStack.Add(new Dialog_MessageBox(report));
        }
    }
}
```

### **Log Analysis Tools**

```csharp
/// <summary>
/// Tools for analyzing cultivation system logs
/// </summary>
public static class CultivationLogAnalyzer
{
    public static void AnalyzePerformance()
    {
        var report = new StringBuilder();
        report.AppendLine("=== Performance Analysis ===");
        
        // Cache statistics
        var cacheStats = CultivationCache.GetStatistics();
        report.AppendLine($"Cache: {cacheStats}");
        
        // Memory statistics
        var memoryStats = CultivationMemoryManager.GetMemoryReport();
        report.AppendLine($"Memory: {memoryStats}");
        
        // Performance metrics
        CultivationPerformanceMonitor.LogPerformanceReport();
        
        Log.Message(report.ToString());
    }
    
    public static void FindErrorPatterns()
    {
        // Analyze recent log entries for patterns
        var errorPatterns = new Dictionary<string, int>();
        
        // This would analyze Unity log for Tu Tiên errors
        // and identify common patterns
        
        Log.Message("[Tu Tiên] Error pattern analysis complete");
    }
}
```

---

## 🎯 **Development Checklist**

### **New Feature Development**

- [ ] **Planning Phase**
  - [ ] Design document created
  - [ ] XML structure defined
  - [ ] C# class hierarchy planned
  - [ ] Performance impact assessed

- [ ] **Implementation Phase**
  - [ ] XML definitions created
  - [ ] C# classes implemented
  - [ ] Integration points identified
  - [ ] Error handling added

- [ ] **Testing Phase**
  - [ ] Unit tests written
  - [ ] Integration tests passed
  - [ ] Performance benchmarks met
  - [ ] Save/load functionality verified

- [ ] **Documentation Phase**
  - [ ] Code comments added
  - [ ] User documentation updated
  - [ ] API documentation generated
  - [ ] Examples provided

### **Quality Assurance**

- [ ] **Code Quality**
  - [ ] Follows naming conventions
  - [ ] Proper error handling
  - [ ] Performance optimized
  - [ ] Memory leaks checked

- [ ] **Compatibility**
  - [ ] RimWorld version tested
  - [ ] Other mods compatibility verified
  - [ ] Save compatibility maintained
  - [ ] Multiplayer compatibility (if applicable)

---

**Development Guide Version**: 2.0  
**Last Updated**: September 2025  
**Compatibility**: RimWorld 1.6+
