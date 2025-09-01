using System.Linq;
using RimWorld;
using Verse;

namespace TuTien
{
    /// <summary>
    /// Simple debug commands for testing cultivation artifacts
    /// Access via dev console: ArtifactDebugCommands.SpawnRandomArtifact()
    /// </summary>
    public static class ArtifactDebugCommands
    {
        public static void SpawnRandomArtifact()
        {
            if (Find.CurrentMap == null)
            {
                Log.Error("No current map to spawn artifact on");
                return;
            }

            // Get all artifact ThingDefs
            var artifactThingDefs = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.HasModExtension<CultivationArtifactExtension>())
                .ToList();

            if (!artifactThingDefs.Any())
            {
                Log.Error("No artifact ThingDefs found with CultivationArtifactExtension");
                return;
            }

            // Pick random artifact
            var randomDef = artifactThingDefs.RandomElement();
            var thing = ThingMaker.MakeThing(randomDef);
            
            // Spawn near player
            var spawnCell = Find.CurrentMap.Center;
            if (!spawnCell.Standable(Find.CurrentMap))
            {
                spawnCell = CellFinder.RandomClosewalkCellNear(spawnCell, Find.CurrentMap, 5);
            }

            GenSpawn.Spawn(thing, spawnCell, Find.CurrentMap);
            
            // Log creation details (commented out until comp loads)
            // var comp = thing.TryGetComp<CultivationArtifactComp>();
            // if (comp?.ArtifactData != null)
            // {
            //     var data = comp.ArtifactData;
            //     Log.Message($"Spawned {thing.Label} with ELO: {data.EloRating}, Power: {data.PowerMultiplier:F2}x, Qi Pool: {data.MaxQiPool}");
            // }
            // else
            // {
                Log.Message($"Spawned {thing.Label} (artifact data will initialize on equip)");
            // }
        }

        public static void SpawnSpecificArtifact(string defName)
        {
            if (Find.CurrentMap == null)
            {
                Log.Error("No current map");
                return;
            }

            var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (thingDef == null || !thingDef.HasModExtension<CultivationArtifactExtension>())
            {
                Log.Error($"No artifact ThingDef found with name: {defName}");
                return;
            }

            var thing = ThingMaker.MakeThing(thingDef);
            var spawnCell = Find.CurrentMap.Center;
            if (!spawnCell.Standable(Find.CurrentMap))
            {
                spawnCell = CellFinder.RandomClosewalkCellNear(spawnCell, Find.CurrentMap, 5);
            }

            GenSpawn.Spawn(thing, spawnCell, Find.CurrentMap);
            Log.Message($"Spawned {thing.Label}");
        }

        public static void TestArtifactELOGeneration()
        {
            Log.Message("=== Artifact ELO Generation Test ===");
            
            var artifactDefs = DefDatabase<CultivationArtifactDef>.AllDefs.ToList();
            foreach (var def in artifactDefs)
            {
                Log.Message($"\n{def.defName} ({def.rarity}):");
                
                // Generate 5 samples (commented out until generator loads)
                // for (int i = 0; i < 5; i++)
                // {
                //     var data = ArtifactGenerator.GenerateArtifactData(def);
                //     Log.Message($"  Sample {i + 1}: ELO {data.EloRating}, Power {data.PowerMultiplier:F2}x, Buffs: {data.Buffs.Count}");
                // }
                Log.Message($"  ELO Range: {def.eloRange.min}-{def.eloRange.max}");
            }
        }

        public static void ListEquippedArtifacts()
        {
            if (Find.CurrentMap == null)
            {
                Log.Error("No current map");
                return;
            }

            Log.Message("=== Equipped Artifacts ===");
            
            foreach (var pawn in Find.CurrentMap.mapPawns.FreeColonists)
            {
                var artifacts = pawn.equipment?.AllEquipmentListForReading?
                    .Where(eq => eq.def.HasModExtension<CultivationArtifactExtension>())
                    .ToList() ?? new System.Collections.Generic.List<ThingWithComps>();

                var apparelArtifacts = pawn.apparel?.WornApparel?
                    .Where(app => app.def.HasModExtension<CultivationArtifactExtension>())
                    .ToList() ?? new System.Collections.Generic.List<Apparel>();

                if (artifacts.Any() || apparelArtifacts.Any())
                {
                    Log.Message($"\n{pawn.Name}:");
                    
                    foreach (var artifact in artifacts)
                    {
                        // var comp = artifact.TryGetComp<CultivationArtifactComp>();
                        // var data = comp?.ArtifactData;
                        // if (data != null)
                        // {
                        //     Log.Message($"  Equipment: {artifact.Label} - ELO: {data.EloRating}, Qi: {data.CurrentQi}/{data.MaxQiPool}");
                        // }
                        Log.Message($"  Equipment: {artifact.Label} (artifact component will load)");
                    }
                    
                    foreach (var artifact in apparelArtifacts)
                    {
                        // var comp = artifact.TryGetComp<CultivationArtifactComp>();
                        // var data = comp?.ArtifactData;
                        // if (data != null)
                        // {
                        //     Log.Message($"  Apparel: {artifact.Label} - ELO: {data.EloRating}, Qi: {data.CurrentQi}/{data.MaxQiPool}");
                        // }
                        Log.Message($"  Apparel: {artifact.Label} (artifact component will load)");
                    }
                }
            }
        }

        public static void ListAllArtifactDefs()
        {
            Log.Message("=== Available Artifact ThingDefs ===");
            
            var artifactDefs = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.HasModExtension<CultivationArtifactExtension>())
                .ToList();

            foreach (var def in artifactDefs)
            {
                var extension = def.GetModExtension<CultivationArtifactExtension>();
                var artifactDef = extension?.GetArtifactDef();
                if (artifactDef != null)
                {
                    Log.Message($"{def.defName} -> {artifactDef.defName} ({artifactDef.rarity} {artifactDef.artifactType})");
                }
                else
                {
                    Log.Warning($"{def.defName} -> Invalid artifact extension");
                }
            }
        }
    }
}
