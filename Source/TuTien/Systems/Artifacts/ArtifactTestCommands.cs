using System.Linq;
using RimWorld;
using Verse;

namespace TuTien
{
    /// <summary>
    /// Simple test commands for spawning artifacts (access via dev console)
    /// </summary>
    public static class ArtifactTestCommands
    {
        public static void SpawnIronSword()
        {
            SpawnArtifact("TuTien_IronCultivationSword");
        }
        
        public static void SpawnClothRobe()
        {
            SpawnArtifact("TuTien_ClothCultivationRobe");
        }
        
        public static void SpawnSpiritBow()
        {
            SpawnArtifact("TuTien_SpiritHunterBow");
        }
        
        public static void SpawnDragonCrown()
        {
            SpawnArtifact("TuTien_DragonCrown");
        }
        
        public static void SpawnImmortalTalisman()
        {
            SpawnArtifact("TuTien_ImmortalTalisman");
        }
        
        public static void SpawnAllArtifacts()
        {
            SpawnIronSword();
            SpawnClothRobe();
            SpawnSpiritBow();
            SpawnDragonCrown();
            SpawnImmortalTalisman();
        }
        
        private static void SpawnArtifact(string defName)
        {
            if (Find.CurrentMap == null)
            {
                Log.Error($"No current map to spawn {defName}");
                return;
            }

            var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (thingDef == null)
            {
                Log.Error($"Could not find ThingDef: {defName}");
                Log.Message("Available artifact ThingDefs:");
                var availableDefs = DefDatabase<ThingDef>.AllDefs
                    .Where(def => def.defName.Contains("TuTien_"))
                    .ToList();
                foreach (var def in availableDefs)
                {
                    Log.Message($"  - {def.defName}");
                }
                return;
            }

            var thing = ThingMaker.MakeThing(thingDef);
            var spawnCell = Find.CurrentMap.Center;
            if (!spawnCell.Standable(Find.CurrentMap))
            {
                spawnCell = CellFinder.RandomClosewalkCellNear(spawnCell, Find.CurrentMap, 5);
            }

            GenSpawn.Spawn(thing, spawnCell, Find.CurrentMap);
            Log.Message($"✅ Spawned {thing.Label} at {spawnCell}");
        }
        
        public static void ListArtifactDefs()
        {
            Log.Message("=== Available Artifact Definitions ===");
            
            // List CultivationArtifactDefs
            var artifactDefs = DefDatabase<CultivationArtifactDef>.AllDefs.ToList();
            Log.Message($"\nCultivationArtifactDefs ({artifactDefs.Count}):");
            foreach (var def in artifactDefs)
            {
                Log.Message($"  - {def.defName}: {def.label} ({def.rarity} {def.artifactType})");
            }
            
            // List ThingDefs with CultivationArtifactExtension
            var thingDefs = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.HasModExtension<CultivationArtifactExtension>())
                .ToList();
            Log.Message($"\nThingDefs with CultivationArtifactExtension ({thingDefs.Count}):");
            foreach (var def in thingDefs)
            {
                Log.Message($"  - {def.defName}: {def.label}");
            }
        }
    }
}
