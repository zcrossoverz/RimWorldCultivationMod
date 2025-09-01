using System;
using System.Linq;
using Verse;
using RimWorld;

namespace TuTien.Testing
{
    /// <summary>
    /// ✅ Ultra-Simple Emergency Testing
    /// Just tries to spawn basic artifacts to verify the system works
    /// </summary>
    public static class EmergencyTest
    {
        public static void TestBasicSpawning()
        {
            try
            {
                Log.Message("[TuTien] EMERGENCY TEST: Basic Artifact Spawning");
                
                // Test 1: Check if our definitions loaded
                var artifactDefs = DefDatabase<CultivationArtifactDef>.AllDefs?.ToList();
                Log.Message($"[TuTien] Found {artifactDefs?.Count ?? 0} artifact definitions");
                
                if (artifactDefs?.Any() == true)
                {
                    foreach (var def in artifactDefs.Take(3))
                    {
                        Log.Message($"[TuTien] - {def.defName}: {def.label} ({def.rarity})");
                    }
                }
                
                // Test 2: Check ThingDefs
                var simpleSword = DefDatabase<ThingDef>.GetNamed("TuTien_SimpleIronSword", false);
                var simpleRobe = DefDatabase<ThingDef>.GetNamed("TuTien_SimpleClothRobe", false);
                var simpleStaff = DefDatabase<ThingDef>.GetNamed("TuTien_SimpleSpiritStaff", false);
                
                Log.Message($"[TuTien] Simple ThingDefs - Sword: {simpleSword?.label ?? "NULL"}, Robe: {simpleRobe?.label ?? "NULL"}, Staff: {simpleStaff?.label ?? "NULL"}");
                
                // Test 3: Try to spawn if we have a map
                var map = Find.CurrentMap;
                if (map != null && (simpleSword != null || simpleRobe != null || simpleStaff != null))
                {
                    var spawnCount = 0;
                    
                    if (simpleSword != null)
                    {
                        var thing = ThingMaker.MakeThing(simpleSword);
                        if (GenPlace.TryPlaceThing(thing, map.Center + new IntVec3(0, 0, 0), map, ThingPlaceMode.Near))
                        {
                            spawnCount++;
                            Log.Message("[TuTien] ✅ Successfully spawned simple sword!");
                        }
                    }
                    
                    if (simpleRobe != null)
                    {
                        var thing = ThingMaker.MakeThing(simpleRobe);
                        if (GenPlace.TryPlaceThing(thing, map.Center + new IntVec3(1, 0, 0), map, ThingPlaceMode.Near))
                        {
                            spawnCount++;
                            Log.Message("[TuTien] ✅ Successfully spawned simple robe!");
                        }
                    }
                    
                    if (simpleStaff != null)
                    {
                        var thing = ThingMaker.MakeThing(simpleStaff);
                        if (GenPlace.TryPlaceThing(thing, map.Center + new IntVec3(-1, 0, 0), map, ThingPlaceMode.Near))
                        {
                            spawnCount++;
                            Log.Message("[TuTien] ✅ Successfully spawned simple staff!");
                        }
                    }
                    
                    if (spawnCount == 0)
                    {
                        Log.Warning("[TuTien] ❌ Failed to place any artifacts");
                    }
                    else
                    {
                        Log.Message($"[TuTien] ✅ Successfully spawned {spawnCount} artifacts!");
                    }
                }
                else
                {
                    Log.Message("[TuTien] No map available for spawning test or no ThingDefs loaded");
                }
                
                Log.Message("[TuTien] ✅ Emergency test completed!");
            }
            catch (Exception e)
            {
                Log.Error($"[TuTien] Emergency test failed: {e}");
            }
        }
    }
}
