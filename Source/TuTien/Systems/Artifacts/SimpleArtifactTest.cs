using System.Linq;
using RimWorld;
using Verse;

namespace TuTien
{
    /// <summary>
    /// Emergency simple test commands - bypass all complex systems
    /// </summary>
    public static class SimpleArtifactTest
    {
        public static void TestBasicSpawn()
        {
            if (Find.CurrentMap == null)
            {
                Log.Error("No current map found");
                return;
            }

            Log.Message("=== Testing Basic Artifact Spawn ===");
            
            // Try to find any ThingDef with "TuTien" in the name
            var allThingDefs = DefDatabase<ThingDef>.AllDefs.ToList();
            var tutienThingDefs = allThingDefs.Where(def => def.defName.Contains("TuTien")).ToList();
            
            Log.Message($"Found {tutienThingDefs.Count} TuTien ThingDefs:");
            foreach (var def in tutienThingDefs)
            {
                Log.Message($"  - {def.defName}: {def.label} ({def.thingClass})");
            }
            
            // Try spawning the first available one
            if (tutienThingDefs.Any())
            {
                var firstDef = tutienThingDefs.First();
                try
                {
                    var thing = ThingMaker.MakeThing(firstDef);
                    var spawnCell = Find.CurrentMap.Center;
                    if (!spawnCell.Standable(Find.CurrentMap))
                    {
                        spawnCell = CellFinder.RandomClosewalkCellNear(spawnCell, Find.CurrentMap, 5);
                    }
                    
                    GenSpawn.Spawn(thing, spawnCell, Find.CurrentMap);
                    Log.Message($"✅ SUCCESS: Spawned {thing.Label} at {spawnCell}");
                }
                catch (System.Exception ex)
                {
                    Log.Error($"❌ FAILED to spawn {firstDef.defName}: {ex.Message}");
                }
            }
            else
            {
                Log.Error("❌ No TuTien ThingDefs found!");
                
                // Debug: List some example ThingDefs to see what's available
                Log.Message("Sample available ThingDefs:");
                var sampleDefs = allThingDefs.Take(10).ToList();
                foreach (var def in sampleDefs)
                {
                    Log.Message($"  - {def.defName}: {def.label}");
                }
            }
        }
        
        public static void TestArtifactDefLoading()
        {
            Log.Message("=== Testing CultivationArtifactDef Loading ===");
            
            var artifactDefs = DefDatabase<CultivationArtifactDef>.AllDefs.ToList();
            Log.Message($"Found {artifactDefs.Count} CultivationArtifactDefs:");
            
            foreach (var def in artifactDefs)
            {
                Log.Message($"  - {def.defName}: {def.label} ({def.rarity} {def.artifactType})");
                Log.Message($"    ELO Range: {def.eloRange.min}-{def.eloRange.max}");
                Log.Message($"    Required: Realm {def.requiredRealmLevel}, Stage {def.requiredStage}");
            }
            
            if (artifactDefs.Count == 5)
            {
                Log.Message("✅ All 5 artifact definitions loaded successfully!");
            }
            else
            {
                Log.Warning($"❌ Expected 5 artifact definitions, found {artifactDefs.Count}");
            }
        }
        
        public static void TestAllSystems()
        {
            Log.Message("=== RUNNING COMPREHENSIVE ARTIFACT TEST ===");
            TestArtifactDefLoading();
            TestBasicSpawn();
            Log.Message("=== TEST COMPLETED ===");
        }
    }
}
