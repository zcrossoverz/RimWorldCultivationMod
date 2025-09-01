# 🚀 Tu Tiên Refactor Roadmap - From 1/10 to 10/10

## 📋 **PHASE 1: QUICK WINS (Week 1-2)**

### **Step 1.1: Replace Lists with HashSets (2 hours)** ⚡
**Goal**: 80% performance boost cho skill lookups

#### **Files to modify:**
```csharp
// CultivationDataEnhanced.cs
❌ public List<CultivationSkillDef> unlockedSkills;
✅ public HashSet<string> unlockedSkillNames;
   public Dictionary<string, CultivationSkillDef> skillCache;

❌ public bool HasSkill(CultivationSkillDef skill) => unlockedSkills.Contains(skill);
✅ public bool HasSkill(string skillName) => unlockedSkillNames.Contains(skillName);
```

#### **Implementation:**
1. Update `CultivationDataEnhanced.cs`
2. Update `ExposeData()` methods
3. Update all `HasSkill()` calls
4. Test save/load compatibility

---

### **Step 1.2: Implement Basic Caching (4 hours)** ⚡
**Goal**: 50% reduction trong expensive calculations

#### **Create CultivationCache.cs:**
```csharp
public static class CultivationCache
{
    private static Dictionary<string, object> cache = new Dictionary<string, object>();
    private static Dictionary<string, int> lastUpdateTick = new Dictionary<string, int>();
    
    public static T GetOrCompute<T>(string key, Func<T> computer, int cacheFor = 60)
    {
        if (ShouldRecalculate(key, cacheFor))
        {
            cache[key] = computer();
            lastUpdateTick[key] = Find.TickManager.TicksGame;
        }
        return (T)cache[key];
    }
    
    private static bool ShouldRecalculate(string key, int cacheFor)
    {
        if (!cache.ContainsKey(key)) return true;
        var lastUpdate = lastUpdateTick.GetValueOrDefault(key, 0);
        return Find.TickManager.TicksGame - lastUpdate > cacheFor;
    }
    
    public static void ClearCache() => cache.Clear();
}
```

#### **Usage Example:**
```csharp
// CultivationComp.cs
public CultivationStats GetStats()
{
    return CultivationCache.GetOrCompute(
        $"stats_{pawn.ThingID}", 
        () => CalculateStatsExpensive(),
        cacheFor: 30 // Cache for 30 ticks
    );
}
```

---

### **Step 1.3: Separate UI from Logic (6 hours)** 🏗️
**Goal**: Clean separation of concerns

#### **Create CultivationUI.cs:**
```csharp
public static class CultivationUI
{
    public static string GetInspectString(CultivationDataEnhanced data)
    {
        var sb = new StringBuilder();
        
        // Realm info
        AppendRealmInfo(sb, data);
        
        // Skill info
        AppendSkillInfo(sb, data);
        
        // Stats info
        AppendStatsInfo(sb, data);
        
        return sb.ToString();
    }
    
    private static void AppendRealmInfo(StringBuilder sb, CultivationDataEnhanced data)
    {
        sb.AppendLine($"Realm: {data.currentRealm} Stage {data.currentStage}");
        sb.AppendLine($"Progress: {data.cultivationProgress:F1}%");
    }
    
    // ... other UI methods
}
```

#### **Update CultivationComp.cs:**
```csharp
public override string CompInspectStringExtra()
{
    return CultivationUI.GetInspectString(cultivationData);
}
```

---

## 📋 **PHASE 2: REGISTRY SYSTEM (Week 3)**

### **Step 2.1: Create Base Registry (8 hours)** 🔧
**Goal**: Auto-discovery thay vì hard-coded registration

#### **Create CultivationRegistry.cs:**
```csharp
public static class CultivationRegistry
{
    private static Dictionary<string, Type> skillWorkers = new Dictionary<string, Type>();
    private static Dictionary<string, Type> techniqueWorkers = new Dictionary<string, Type>();
    private static Dictionary<string, Type> effectFactories = new Dictionary<string, Type>();
    
    static CultivationRegistry()
    {
        AutoDiscoverWorkers();
    }
    
    private static void AutoDiscoverWorkers()
    {
        var assembly = Assembly.GetAssembly(typeof(CultivationRegistry));
        
        // Auto-discover skill workers
        var skillWorkerTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(CultivationSkillWorker)) && !t.IsAbstract);
            
        foreach (var type in skillWorkerTypes)
        {
            var workerName = type.Name.Replace("Worker", "");
            skillWorkers[workerName] = type;
            Log.Message($"[TuTien Registry] Registered skill worker: {workerName}");
        }
        
        // Auto-discover technique workers
        var techniqueWorkerTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(CultivationTechniqueWorker)) && !t.IsAbstract);
            
        foreach (var type in techniqueWorkerTypes)
        {
            var workerName = type.Name.Replace("Worker", "");
            techniqueWorkers[workerName] = type;
            Log.Message($"[TuTien Registry] Registered technique worker: {workerName}");
        }
    }
    
    public static CultivationSkillWorker GetSkillWorker(string skillType)
    {
        if (skillWorkers.TryGetValue(skillType, out Type workerType))
        {
            return (CultivationSkillWorker)Activator.CreateInstance(workerType);
        }
        Log.Warning($"[TuTien Registry] No worker found for skill: {skillType}");
        return null;
    }
    
    public static CultivationTechniqueWorker GetTechniqueWorker(string techniqueType)
    {
        if (techniqueWorkers.TryGetValue(techniqueType, out Type workerType))
        {
            return (CultivationTechniqueWorker)Activator.CreateInstance(workerType);
        }
        Log.Warning($"[TuTien Registry] No worker found for technique: {techniqueType}");
        return null;
    }
}
```

---

### **Step 2.2: Update Existing Workers (4 hours)** 🔄
**Goal**: Tất cả workers sử dụng registry

#### **Update CultivationSkillDef.cs:**
```csharp
public class CultivationSkillDef : Def
{
    public string workerClass; // Auto-populated từ defName
    
    private CultivationSkillWorker workerInt;
    public CultivationSkillWorker Worker
    {
        get
        {
            if (workerInt == null)
            {
                var workerType = workerClass ?? defName;
                workerInt = CultivationRegistry.GetSkillWorker(workerType);
                if (workerInt == null)
                {
                    Log.Error($"[TuTien] Could not create worker for skill: {defName}");
                    workerInt = new DefaultSkillWorker(); // Fallback
                }
            }
            return workerInt;
        }
    }
    
    public bool CanExecute(Pawn pawn) => Worker?.CanExecute(pawn, this) ?? false;
    public void Execute(Pawn pawn) => Worker?.Execute(pawn, this);
}
```

---

## 📋 **PHASE 3: EFFECT SYSTEM (Week 4)**

### **Step 3.1: Create Effect Factory (12 hours)** 🏭
**Goal**: XML-driven effects thay vì hard-coded

#### **Create ISkillEffect.cs:**
```csharp
public abstract class SkillEffect
{
    public abstract string EffectType { get; }
    public abstract void Apply(Pawn pawn, CultivationSkillDef skill);
    public virtual bool CanApply(Pawn pawn, CultivationSkillDef skill) => true;
}

public class DamageEffect : SkillEffect
{
    public override string EffectType => "Damage";
    public DamageType damageType = DamageType.Physical;
    public float baseDamage = 10f;
    public float scalingFactor = 0.1f;
    
    public override void Apply(Pawn pawn, CultivationSkillDef skill)
    {
        var targets = FindTargets(pawn);
        var damage = CalculateDamage(pawn, skill);
        
        foreach (var target in targets)
        {
            ApplyDamageToTarget(target, damage);
        }
    }
    
    private float CalculateDamage(Pawn pawn, CultivationSkillDef skill)
    {
        var cultivationData = pawn.GetComp<CultivationComp>()?.cultivationData;
        if (cultivationData == null) return baseDamage;
        
        var cultivationLevel = (int)cultivationData.currentRealm * 10 + cultivationData.currentStage;
        return baseDamage + (cultivationLevel * scalingFactor);
    }
}

public class HediffEffect : SkillEffect
{
    public override string EffectType => "Hediff";
    public string hediffDef;
    public float duration = 60f;
    public float severity = 1f;
    
    public override void Apply(Pawn pawn, CultivationSkillDef skill)
    {
        var hediff = DefDatabase<HediffDef>.GetNamed(hediffDef);
        var hediffInstance = HediffMaker.MakeHediff(hediff, pawn);
        hediffInstance.Severity = severity;
        pawn.health.AddHediff(hediffInstance);
    }
}
```

#### **Create EffectFactory.cs:**
```csharp
public static class EffectFactory
{
    private static Dictionary<string, Type> effectTypes = new Dictionary<string, Type>();
    
    static EffectFactory()
    {
        RegisterEffect<DamageEffect>();
        RegisterEffect<HediffEffect>();
        RegisterEffect<AreaEffect>();
        RegisterEffect<StatModifierEffect>();
        // Auto-discover more effects
        AutoDiscoverEffects();
    }
    
    public static void RegisterEffect<T>() where T : SkillEffect, new()
    {
        var instance = new T();
        effectTypes[instance.EffectType] = typeof(T);
        Log.Message($"[TuTien] Registered effect type: {instance.EffectType}");
    }
    
    public static SkillEffect CreateEffect(XmlNode xmlNode)
    {
        var effectClass = xmlNode.Attributes["Class"]?.Value;
        if (effectClass == null)
        {
            Log.Error("[TuTien] Effect missing Class attribute");
            return null;
        }
        
        if (!effectTypes.TryGetValue(effectClass, out Type effectType))
        {
            Log.Error($"[TuTien] Unknown effect type: {effectClass}");
            return null;
        }
        
        return (SkillEffect)DirectXmlToObject.ObjectFromXml(xmlNode, effectType);
    }
}
```

---

### **Step 3.2: Update XML Loading (6 hours)** 📄
**Goal**: Skills load effects từ XML

#### **Update CultivationSkillDef.cs:**
```csharp
public class CultivationSkillDef : Def
{
    public List<SkillEffect> effects = new List<SkillEffect>();
    
    public override void ResolveReferences()
    {
        base.ResolveReferences();
        
        // Effects sẽ được load automatically từ XML
        foreach (var effect in effects)
        {
            if (effect == null)
            {
                Log.Error($"[TuTien] Null effect in skill: {defName}");
            }
        }
    }
}
```

#### **XML Example:**
```xml
<CultivationSkillDef>
  <defName>ThunderPalm</defName>
  <label>Thunder Palm</label>
  <effects>
    <li Class="DamageEffect">
      <damageType>Lightning</damageType>
      <baseDamage>50</baseDamage>
      <scalingFactor>0.2</scalingFactor>
    </li>
    <li Class="HediffEffect">
      <hediffDef>LightningStun</hediffDef>
      <duration>180</duration>
      <severity>0.5</severity>
    </li>
  </effects>
</CultivationSkillDef>
```

---

## 📋 **PHASE 4: MODULAR DATA (Week 5)**

### **Step 4.1: Component System (10 hours)** 🧩
**Goal**: Break down monolithic CultivationDataEnhanced

#### **Create ICultivationComponent.cs:**
```csharp
public interface ICultivationComponent : IExposable
{
    string ComponentName { get; }
    void Initialize(Pawn pawn);
    void Update(float deltaTime);
    void OnRealmChanged(CultivationRealm newRealm);
    void OnStageChanged(int newStage);
}

public class CultivationProgress : ICultivationComponent
{
    public string ComponentName => "Progress";
    public CultivationRealm currentRealm = CultivationRealm.Mortal;
    public int currentStage = 1;
    public float cultivationProgress = 0f;
    public float qi = 0f;
    public float maxQi = 100f;
    
    public void Initialize(Pawn pawn) { }
    public void Update(float deltaTime) { }
    public void OnRealmChanged(CultivationRealm newRealm) { }
    public void OnStageChanged(int newStage) { }
    
    public void ExposeData()
    {
        Scribe_Values.Look(ref currentRealm, "currentRealm");
        Scribe_Values.Look(ref currentStage, "currentStage");
        Scribe_Values.Look(ref cultivationProgress, "cultivationProgress");
        Scribe_Values.Look(ref qi, "qi");
        Scribe_Values.Look(ref maxQi, "maxQi");
    }
}

public class CultivationSkills : ICultivationComponent
{
    public string ComponentName => "Skills";
    public HashSet<string> unlockedSkills = new HashSet<string>();
    public Dictionary<string, float> skillCooldowns = new Dictionary<string, float>();
    public Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    
    // ... implementation
}
```

#### **Update CultivationDataEnhanced.cs:**
```csharp
public class CultivationDataEnhanced : IExposable
{
    public List<ICultivationComponent> components = new List<ICultivationComponent>();
    
    // Quick access properties
    public CultivationProgress Progress => GetComponent<CultivationProgress>();
    public CultivationSkills Skills => GetComponent<CultivationSkills>();
    public CultivationStats Stats => GetComponent<CultivationStats>();
    
    public T GetComponent<T>() where T : ICultivationComponent
    {
        return components.OfType<T>().FirstOrDefault();
    }
    
    public void AddComponent<T>() where T : ICultivationComponent, new()
    {
        if (GetComponent<T>() == null)
        {
            var component = new T();
            components.Add(component);
        }
    }
    
    public void Initialize(Pawn pawn)
    {
        // Default components
        AddComponent<CultivationProgress>();
        AddComponent<CultivationSkills>();
        AddComponent<CultivationStats>();
        
        foreach (var component in components)
        {
            component.Initialize(pawn);
        }
    }
    
    public void Update(float deltaTime)
    {
        foreach (var component in components)
        {
            component.Update(deltaTime);
        }
    }
    
    public void ExposeData()
    {
        Scribe_Collections.Look(ref components, "components");
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            // Ensure default components exist
            if (GetComponent<CultivationProgress>() == null)
                AddComponent<CultivationProgress>();
        }
    }
}
```

---

## 📋 **PHASE 5: PLUGIN SYSTEM (Week 6)**

### **Step 5.1: Extension Points (12 hours)** 🔌
**Goal**: Third-party extensibility

#### **Create ICultivationExtension.cs:**
```csharp
public interface ICultivationExtension
{
    string ExtensionName { get; }
    Version Version { get; }
    List<string> Dependencies { get; }
    
    void PreInitialize();
    void Initialize();
    void PostInitialize();
    void RegisterComponents(CultivationRegistry registry);
}

[AttributeUsage(AttributeTargets.Class)]
public class CultivationExtensionAttribute : Attribute
{
    public string Name { get; }
    public string Version { get; }
    
    public CultivationExtensionAttribute(string name, string version = "1.0.0")
    {
        Name = name;
        Version = version;
    }
}
```

#### **Create ExtensionManager.cs:**
```csharp
public static class ExtensionManager
{
    private static List<ICultivationExtension> loadedExtensions = new List<ICultivationExtension>();
    private static bool initialized = false;
    
    public static void LoadAllExtensions()
    {
        if (initialized) return;
        
        Log.Message("[TuTien] Loading cultivation extensions...");
        
        var extensions = DiscoverExtensions();
        
        // Sort by dependencies
        var sortedExtensions = SortByDependencies(extensions);
        
        foreach (var extension in sortedExtensions)
        {
            try
            {
                LoadExtension(extension);
            }
            catch (Exception e)
            {
                Log.Error($"[TuTien] Failed to load extension {extension.ExtensionName}: {e}");
            }
        }
        
        initialized = true;
        Log.Message($"[TuTien] Loaded {loadedExtensions.Count} extensions");
    }
    
    private static List<ICultivationExtension> DiscoverExtensions()
    {
        var extensions = new List<ICultivationExtension>();
        
        // Search in current assembly
        var assembly = Assembly.GetAssembly(typeof(ExtensionManager));
        var extensionTypes = assembly.GetTypes()
            .Where(t => t.GetInterface(typeof(ICultivationExtension).Name) != null)
            .Where(t => !t.IsAbstract && !t.IsInterface);
            
        foreach (var type in extensionTypes)
        {
            var instance = (ICultivationExtension)Activator.CreateInstance(type);
            extensions.Add(instance);
        }
        
        // TODO: Search in other assemblies/mods
        
        return extensions;
    }
    
    private static void LoadExtension(ICultivationExtension extension)
    {
        Log.Message($"[TuTien] Loading extension: {extension.ExtensionName} v{extension.Version}");
        
        extension.PreInitialize();
        extension.Initialize();
        extension.RegisterComponents(CultivationRegistry.Instance);
        extension.PostInitialize();
        
        loadedExtensions.Add(extension);
    }
}
```

---

### **Step 5.2: Example Extension (4 hours)** 🔥
**Goal**: Demo extension system

#### **Create IcePhoenixExtension.cs:**
```csharp
[CultivationExtension("IcePhoenix", "1.0.0")]
public class IcePhoenixExtension : ICultivationExtension
{
    public string ExtensionName => "Ice Phoenix Cultivation";
    public Version Version => new Version(1, 0, 0);
    public List<string> Dependencies => new List<string>();
    
    public void PreInitialize()
    {
        Log.Message("[IcePhoenix] Pre-initializing Ice Phoenix extension");
    }
    
    public void Initialize()
    {
        Log.Message("[IcePhoenix] Initializing Ice Phoenix extension");
    }
    
    public void PostInitialize()
    {
        Log.Message("[IcePhoenix] Post-initializing Ice Phoenix extension");
    }
    
    public void RegisterComponents(CultivationRegistry registry)
    {
        // Register custom skills
        registry.RegisterSkillWorker<IcePhoenixTransformationWorker>();
        registry.RegisterSkillWorker<IcePhoenixBreathWorker>();
        
        // Register custom effects
        registry.RegisterEffect<FreezeEffect>();
        registry.RegisterEffect<PhoenixRebornEffect>();
        
        // Register custom components
        registry.RegisterComponent<IcePhoenixBloodline>();
        
        Log.Message("[IcePhoenix] Registered Ice Phoenix components");
    }
}

public class IcePhoenixTransformationWorker : CultivationSkillWorker
{
    public override bool CanExecute(Pawn pawn, CultivationSkillDef skill)
    {
        var bloodline = pawn.GetComp<CultivationComp>()?.cultivationData?.GetComponent<IcePhoenixBloodline>();
        return bloodline?.isAwakened == true;
    }
    
    public override void Execute(Pawn pawn, CultivationSkillDef skill)
    {
        // Ice Phoenix transformation logic
        var hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("IcePhoenixForm"), pawn);
        pawn.health.AddHediff(hediff);
        
        Log.Message($"[IcePhoenix] {pawn.Name} transformed into Ice Phoenix form!");
    }
}
```

---

## 📋 **PHASE 6: ADVANCED FEATURES (Week 7)**

### **Step 6.1: Artifact Growth System (8 hours)** 📿
**Goal**: Weapons that evolve với cultivation

#### **Create IArtifactGrowth.cs:**
```csharp
public interface IArtifactGrowth
{
    List<GrowthStage> GrowthStages { get; }
    GrowthStage CurrentStage { get; set; }
    void CheckGrowth(Pawn owner);
    void ApplyStageEffects(Pawn owner, GrowthStage stage);
}

public class GrowthStage
{
    public string stageName;
    public CultivationRealm minRealm;
    public int minStage;
    public List<StatModifier> statBonuses = new List<StatModifier>();
    public List<string> newSkills = new List<string>();
    public List<string> newEffects = new List<string>();
    
    public bool CanActivate(CultivationDataEnhanced data)
    {
        return (int)data.Progress.currentRealm >= (int)minRealm &&
               data.Progress.currentStage >= minStage;
    }
}

public class CultivationWeaponComp : ThingComp, IArtifactGrowth
{
    public List<GrowthStage> GrowthStages => Props.growthStages;
    public GrowthStage CurrentStage { get; set; }
    
    public CultivationWeaponCompProperties Props => (CultivationWeaponCompProperties)props;
    
    public override void CompTick()
    {
        if (parent.Spawned && parent.holdingOwner?.Owner is Pawn owner)
        {
            CheckGrowth(owner);
        }
    }
    
    public void CheckGrowth(Pawn owner)
    {
        var cultivation = owner.GetComp<CultivationComp>()?.cultivationData;
        if (cultivation == null) return;
        
        var targetStage = GrowthStages
            .Where(s => s.CanActivate(cultivation))
            .OrderByDescending(s => (int)s.minRealm * 10 + s.minStage)
            .FirstOrDefault();
            
        if (targetStage != CurrentStage)
        {
            CurrentStage = targetStage;
            ApplyStageEffects(owner, targetStage);
        }
    }
    
    public void ApplyStageEffects(Pawn owner, GrowthStage stage)
    {
        if (stage == null) return;
        
        // Apply stat bonuses to weapon
        // Grant new skills to owner
        // Add visual effects
        
        Messages.Message(
            $"{parent.Label} has evolved to {stage.stageName}!",
            owner,
            MessageTypeDefOf.PositiveEvent
        );
    }
}
```

---

### **Step 6.2: Complex Interactions (6 hours)** ⚔️
**Goal**: Element interactions, skill combos

#### **Create InteractionSystem.cs:**
```csharp
public static class CultivationInteractions
{
    private static Dictionary<string, ElementalReaction> elementalReactions = new Dictionary<string, ElementalReaction>();
    private static Dictionary<List<string>, string> skillCombos = new Dictionary<List<string>, string>();
    
    static CultivationInteractions()
    {
        InitializeElementalReactions();
        InitializeSkillCombos();
    }
    
    private static void InitializeElementalReactions()
    {
        // Wu Xing cycle reactions
        RegisterElementalReaction("Fire", "Water", new ElementalReaction
        {
            reactionType = ReactionType.Conflict,
            fireBonus = -0.3f,
            waterBonus = 0.2f,
            description = "Water extinguishes fire"
        });
        
        RegisterElementalReaction("Wood", "Fire", new ElementalReaction
        {
            reactionType = ReactionType.Synergy,
            woodBonus = -0.1f,
            fireBonus = 0.3f,
            description = "Wood feeds fire"
        });
        
        // ... more reactions
    }
    
    public static ElementalReaction GetElementalReaction(string element1, string element2)
    {
        var key = $"{element1}_{element2}";
        if (elementalReactions.TryGetValue(key, out var reaction))
            return reaction;
            
        // Try reverse
        key = $"{element2}_{element1}";
        if (elementalReactions.TryGetValue(key, out reaction))
        {
            return reaction.GetReversed();
        }
        
        return null;
    }
    
    public static string GetSkillCombo(List<string> skills)
    {
        var sortedSkills = skills.OrderBy(s => s).ToList();
        
        foreach (var combo in skillCombos)
        {
            if (combo.Key.SequenceEqual(sortedSkills))
                return combo.Value;
        }
        
        return null;
    }
}
```

---

## 📋 **PHASE 7: DYNAMIC UI (Week 8)**

### **Step 7.1: Component-based UI (10 hours)** 🖥️
**Goal**: Auto-generated UI từ data

#### **Create CultivationUIComponent.cs:**
```csharp
public abstract class CultivationUIComponent
{
    public abstract string ComponentName { get; }
    public abstract int Priority { get; }
    public abstract bool ShouldShow(CultivationDataEnhanced data);
    public abstract float GetRequiredHeight(CultivationDataEnhanced data);
    public abstract void Draw(Rect rect, CultivationDataEnhanced data);
}

public class RealmProgressUIComponent : CultivationUIComponent
{
    public override string ComponentName => "RealmProgress";
    public override int Priority => 1;
    
    public override bool ShouldShow(CultivationDataEnhanced data) => true;
    
    public override float GetRequiredHeight(CultivationDataEnhanced data) => 60f;
    
    public override void Draw(Rect rect, CultivationDataEnhanced data)
    {
        var progress = data.Progress;
        
        // Draw realm label
        var realmRect = new Rect(rect.x, rect.y, rect.width, 20f);
        Widgets.Label(realmRect, $"Realm: {progress.currentRealm} Stage {progress.currentStage}");
        
        // Draw progress bar
        var progressRect = new Rect(rect.x, rect.y + 25f, rect.width, 20f);
        Widgets.FillableBar(progressRect, progress.cultivationProgress / 100f);
        
        // Draw Qi bar
        var qiRect = new Rect(rect.x, rect.y + 50f, rect.width, 20f);
        Widgets.FillableBar(qiRect, progress.qi / progress.maxQi, Texture2D.blueTexture);
    }
}

public class CultivationUIManager
{
    private static List<CultivationUIComponent> components = new List<CultivationUIComponent>();
    
    static CultivationUIManager()
    {
        RegisterComponent(new RealmProgressUIComponent());
        RegisterComponent(new SkillsUIComponent());
        RegisterComponent(new StatsUIComponent());
        RegisterComponent(new TechniquesUIComponent());
        AutoDiscoverComponents();
    }
    
    public static void RegisterComponent(CultivationUIComponent component)
    {
        components.Add(component);
        components = components.OrderBy(c => c.Priority).ToList();
    }
    
    public static string DrawCultivationInfo(CultivationDataEnhanced data)
    {
        var sb = new StringBuilder();
        
        foreach (var component in components.Where(c => c.ShouldShow(data)))
        {
            // For inspect string, we convert UI components to text
            sb.AppendLine($"=== {component.ComponentName} ===");
            // Component-specific text generation
        }
        
        return sb.ToString();
    }
}
```

---

## 📋 **PHASE 8: CONFIGURATION SYSTEM (Week 9)**

### **Step 8.1: XML Configuration (6 hours)** ⚙️
**Goal**: Everything configurable via XML

#### **Create CultivationSettingsDef.cs:**
```csharp
public class CultivationSettingsDef : Def
{
    // Global settings
    public float globalCultivationSpeedMultiplier = 1.0f;
    public float globalQiRegenerationMultiplier = 1.0f;
    public bool enableDebugging = false;
    
    // Gameplay settings
    public int maxSkillsPerRealm = 5;
    public int maxTechniquesPerPawn = 3;
    public float breakthroughFailurePenalty = 0.1f;
    
    // Balance settings
    public List<RealmSettings> realmSettings = new List<RealmSettings>();
    
    public static CultivationSettingsDef Settings
    {
        get
        {
            return DefDatabase<CultivationSettingsDef>.GetNamed("Default") ?? 
                   DefDatabase<CultivationSettingsDef>.AllDefs.FirstOrDefault();
        }
    }
}

public class RealmSettings
{
    public CultivationRealm realm;
    public float baseProgressRequired = 100f;
    public float progressPerStage = 100f;
    public List<string> availableSkills = new List<string>();
}
```

#### **XML Example:**
```xml
<CultivationSettingsDef>
  <defName>Default</defName>
  <globalCultivationSpeedMultiplier>1.0</globalCultivationSpeedMultiplier>
  <globalQiRegenerationMultiplier>1.0</globalQiRegenerationMultiplier>
  <maxSkillsPerRealm>5</maxSkillsPerRealm>
  
  <realmSettings>
    <li>
      <realm>Mortal</realm>
      <baseProgressRequired>100</baseProgressRequired>
      <progressPerStage>50</progressPerStage>
      <availableSkills>
        <li>QiPunch</li>
        <li>QiShield</li>
        <li>QiHealing</li>
      </availableSkills>
    </li>
    <li>
      <realm>QiCondensation</realm>
      <baseProgressRequired>200</baseProgressRequired>
      <progressPerStage>100</progressPerStage>
      <availableSkills>
        <li>EnhancedPhysique</li>
        <li>QiSense</li>
        <li>IronBody</li>
      </availableSkills>
    </li>
  </realmSettings>
</CultivationSettingsDef>
```

---

## 📋 **PHASE 9: OPTIMIZATION (Week 10)**

### **Step 9.1: Object Pooling (8 hours)** 🏊
**Goal**: Reduce garbage collection

#### **Create ObjectPool.cs:**
```csharp
public class ObjectPool<T> where T : new()
{
    private readonly ConcurrentQueue<T> objects = new ConcurrentQueue<T>();
    private readonly Func<T> objectGenerator = () => new T();
    
    public T Get()
    {
        if (objects.TryDequeue(out T item))
            return item;
        return objectGenerator();
    }
    
    public void Return(T item)
    {
        if (item != null)
            objects.Enqueue(item);
    }
}

public static class CultivationPools
{
    public static ObjectPool<StringBuilder> StringBuilders = new ObjectPool<StringBuilder>();
    public static ObjectPool<List<CultivationSkillDef>> SkillLists = new ObjectPool<List<CultivationSkillDef>>();
    
    public static StringBuilder GetStringBuilder()
    {
        var sb = StringBuilders.Get();
        sb.Clear();
        return sb;
    }
    
    public static void ReturnStringBuilder(StringBuilder sb)
    {
        StringBuilders.Return(sb);
    }
}
```

---

### **Step 9.2: Performance Monitoring (4 hours)** 📊
**Goal**: Track performance metrics

#### **Create PerformanceMonitor.cs:**
```csharp
public static class CultivationPerformanceMonitor
{
    private static Dictionary<string, PerformanceMetric> metrics = new Dictionary<string, PerformanceMetric>();
    
    public static void StartMeasure(string operation)
    {
        if (!metrics.ContainsKey(operation))
            metrics[operation] = new PerformanceMetric(operation);
            
        metrics[operation].StartMeasure();
    }
    
    public static void EndMeasure(string operation)
    {
        if (metrics.ContainsKey(operation))
            metrics[operation].EndMeasure();
    }
    
    public static void LogPerformanceReport()
    {
        var sb = CultivationPools.GetStringBuilder();
        sb.AppendLine("=== Cultivation Performance Report ===");
        
        foreach (var metric in metrics.Values.OrderByDescending(m => m.AverageTime))
        {
            sb.AppendLine($"{metric.Operation}: {metric.AverageTime:F2}ms (calls: {metric.CallCount})");
        }
        
        Log.Message(sb.ToString());
        CultivationPools.ReturnStringBuilder(sb);
    }
}
```

---

## 🎯 **SUCCESS METRICS**

### **Before Refactor (Current: 1/10):**
```
✅ Add new skill: 5-8 steps, 4-6 files, hard-coded worker
✅ Add new stat: 6-10 steps, 5-8 files, manual UI
✅ Add new buff: 3-5 steps, basic hediff only
✅ Add new artifact: 8-12 steps, no cultivation effects
✅ Performance: ~2ms/tick, 50MB memory
```

### **After Refactor (Target: 10/10):**
```
🚀 Add new skill: 1 XML file, auto-generated worker
🚀 Add new stat: 1 XML file, auto-UI integration
🚀 Add new buff: 1 XML file, complex stages & events  
🚀 Add new artifact: 1 XML file, growth system
🚀 Performance: ~0.2ms/tick, 20MB memory
🚀 Plugin system: Third-party extensions
🚀 Configuration: Everything XML-configurable
```

---

## 📅 **TIMELINE SUMMARY**

| Week | Phase | Hours | Goal |
|------|-------|-------|------|
| 1-2 | Quick Wins | 12h | Performance boost |
| 3 | Registry | 12h | Auto-discovery |
| 4 | Effects | 18h | XML-driven effects |
| 5 | Components | 10h | Modular data |
| 6 | Plugins | 16h | Extensibility |
| 7 | Artifacts | 14h | Advanced features |
| 8 | Dynamic UI | 10h | Auto-generated UI |
| 9 | Config | 6h | XML configuration |
| 10 | Optimization | 12h | Performance tuning |

**Total: ~110 hours over 10 weeks = 11h/week average**

**Result: Tu Tiên mod becomes the most extensible RimWorld cultivation mod ever! 🏆**
