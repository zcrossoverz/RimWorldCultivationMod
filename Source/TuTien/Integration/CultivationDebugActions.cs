using System.Linq;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien.Integration
{
    /// <summary>
    /// Debug utilities for testing cultivation events and systems
    /// </summary>
    public static class CultivationDebugActions
    {
        /// <summary>
        /// Test event performance on first available colonist
        /// </summary>
        public static void TestEventPerformance()
        {
            if (Find.CurrentMap?.mapPawns?.FreeColonists == null || Find.CurrentMap.mapPawns.FreeColonists.Count == 0)
            {
                Log.Error("[TuTien Debug] No colonists found for testing");
                return;
            }
            
            var pawn = Find.CurrentMap.mapPawns.FreeColonists.First();
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null)
            {
                // Create cultivation data for testing
                comp.cultivationData = new CultivationData(pawn);
                Log.Message($"[TuTien Debug] Created cultivation data for {pawn.Name.ToStringShort}");
            }
            
            Log.Message($"[TuTien Debug] Testing events on {pawn.Name.ToStringShort}...");
            
            // Fire some test events
            for (int i = 0; i < 10; i++)
            {
                CultivationEvents.TriggerQiChanged(pawn, 50f + i, 51f + i);
                CultivationEvents.TriggerStageChanged(pawn, 1, 2);
                CultivationEvents.TriggerBreakthroughAttempt(pawn, i % 2 == 0);
                CultivationEvents.TriggerBreakthroughResult(pawn, i % 3 == 0);
            }
            
            Log.Message("[TuTien Debug] Event test completed - 40 events fired");
        }
    }
}
