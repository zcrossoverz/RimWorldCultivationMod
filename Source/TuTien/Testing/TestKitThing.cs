using Verse;
using RimWorld;
using System.Collections.Generic;

namespace TuTien.Testing
{
    /// <summary>
    /// ✅ Test Kit Thing - Có thể craft và sử dụng trong game
    /// </summary>
    public class TestKitThing : ThingWithComps
    {
        protected override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);
            SpawnTestArtifacts(ingester.Position, ingester.Map);
        }
        
        // Khi click chuột phải
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            yield return new FloatMenuOption("Spawn Test Artifacts", delegate
            {
                SpawnTestArtifacts(selPawn.Position, selPawn.Map);
                this.Destroy();
            });
        }
        
        public override string GetInspectString()
        {
            return "Right-click to spawn test artifacts!";
        }
        
        private void SpawnTestArtifacts(IntVec3 center, Map map)
        {
            if (map == null) return;
            
            var spawnedCount = 0;
            Messages.Message("Đang spawn Tu Tiên test artifacts...", MessageTypeDefOf.NeutralEvent);
            
            // Spawn cultivation sword
            var swordDef = DefDatabase<ThingDef>.GetNamed("TuTien_CultivationSword", false);
            if (swordDef != null)
            {
                var sword = ThingMaker.MakeThing(swordDef);
                if (GenPlace.TryPlaceThing(sword, center, map, ThingPlaceMode.Near))
                {
                    spawnedCount++;
                }
            }
            
            // Spawn cultivation robe
            var robeDef = DefDatabase<ThingDef>.GetNamed("TuTien_CultivationRobe", false);
            if (robeDef != null)
            {
                var robe = ThingMaker.MakeThing(robeDef);
                if (GenPlace.TryPlaceThing(robe, center + new IntVec3(1, 0, 0), map, ThingPlaceMode.Near))
                {
                    spawnedCount++;
                }
            }
            
            /*
            // Spawn staff
            var staffDef = DefDatabase<ThingDef>.GetNamed("TuTien_TestStaff", false);
            if (staffDef != null)
            {
                var staff = ThingMaker.MakeThing(staffDef);
                if (GenPlace.TryPlaceThing(staff, center + new IntVec3(-1, 0, 0), map, ThingPlaceMode.Near))
                {
                    spawnedCount++;
                }
            }
            */
            
            // Test EmergencyTest
            EmergencyTest.TestBasicSpawning();
            
            Messages.Message($"✅ Đã spawn {spawnedCount} Tu Tiên artifacts!", MessageTypeDefOf.PositiveEvent);
            
            // Log artifact info
            var count = 0;
            Log.Message("[Tu Tiên] === ARTIFACT INFO ===");
            foreach (var def in DefDatabase<CultivationArtifactDef>.AllDefs)
            {
                count++;
                Log.Message($"[Tu Tiên] {count}. {def.defName}: {def.label} ({def.rarity}) - ELO: {def.eloRange.min}-{def.eloRange.max}");
            }
        }
    }
}
