using System;
using System.Linq;
using Verse;
using TuTien.Systems.Registry;

namespace TuTien.Testing
{
    /// <summary>
    /// ✅ Simple test to verify that effect definitions can be loaded
    /// This can be called from dev mode to test the system
    /// DISABLED: Manual testing only - no automatic startup testing
    /// </summary>
    public static class EffectSystemTest
    {
        // Disabled automatic testing - call manually via console
        // static EffectSystemTest()
        // {
        //     TestEffectLoading();
        // }
        
        public static void TestEffectLoading()
        {
            try
            {
                Log.Message("[TuTien] Testing Effect Definition Loading...");
                
                // Test direct DefDatabase access
                var allEffects = DefDatabase<CultivationEffectDef>.AllDefs;
                int effectCount = allEffects.ToList().Count;
                Log.Message($"[TuTien] Found {effectCount} effect definitions:");
                
                foreach (var effect in allEffects)
                {
                    Log.Message($"[TuTien] - {effect.defName}: {effect.label} (Class: {effect.effectClass}, Category: {effect.category})");
                }
                
                // Test registry access
                var ironBody = CultivationRegistry.GetEffectDef("CultivationEffect_IronBody");
                if (ironBody != null)
                {
                    Log.Message($"[TuTien] ✅ Registry Access Test PASSED: {ironBody.label}");
                }
                else
                {
                    Log.Warning("[TuTien] ❌ Registry Access Test FAILED: Could not find IronBody effect");
                }
                
                // Test category filtering
                var defensiveEffects = CultivationRegistry.GetEffectsByCategory("Defense");
                int defenseCount = defensiveEffects.ToList().Count;
                Log.Message($"[TuTien] Found {defenseCount} defensive effects");
                
                Log.Message("[TuTien] ✅ Effect System Test COMPLETED!");
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien] ❌ Effect System Test FAILED: {ex}");
            }
        }
    }
}
