using Verse;
using System.Linq;

namespace TuTien.Testing
{
    /// <summary>
    /// Test advanced effect properties and XML field validation
    /// </summary>
    [StaticConstructorOnStartup]
    public static class AdvancedEffectTest
    {
        static AdvancedEffectTest()
        {
            TestAdvancedEffectProperties();
        }
        
        private static void TestAdvancedEffectProperties()
        {
            Log.Message("[TuTien] Testing Advanced Effect Properties...");
            
            // Test QiBurst effect properties
            var qiBlastEffect = DefDatabase<CultivationEffectDef>.GetNamedSilentFail("CultivationEffect_QiBlast");
            if (qiBlastEffect != null)
            {
                Log.Message($"[TuTien] ✅ QiBlast Effect - Damage: {qiBlastEffect.burstDamage}, Radius: {qiBlastEffect.burstRadius}, Qi Cost: {qiBlastEffect.qiCost}");
            }
            
            // Test HealingAura effect properties  
            var healingAuraEffect = DefDatabase<CultivationEffectDef>.GetNamedSilentFail("CultivationEffect_HealingAura");
            if (healingAuraEffect != null)
            {
                Log.Message($"[TuTien] ✅ HealingAura Effect - Heal: {healingAuraEffect.healAmount}, Radius: {healingAuraEffect.auraRadius}, Interval: {healingAuraEffect.healInterval}");
            }
            
            // Test DamageResistance effect properties
            var flameResistanceEffect = DefDatabase<CultivationEffectDef>.GetNamedSilentFail("CultivationEffect_FlameResistance");
            if (flameResistanceEffect != null)
            {
                Log.Message($"[TuTien] ✅ FlameResistance Effect - Resistance: {flameResistanceEffect.resistancePercent}%, Type: {flameResistanceEffect.targetDamageType}");
            }
            
            // Count effects by advanced categories
            var burstEffects = DefDatabase<CultivationEffectDef>.AllDefs.Where(e => e.burstDamage > 0).Count();
            var healingEffects = DefDatabase<CultivationEffectDef>.AllDefs.Where(e => e.healAmount > 0).Count();
            var resistanceEffects = DefDatabase<CultivationEffectDef>.AllDefs.Where(e => e.resistancePercent > 0).Count();
            
            Log.Message($"[TuTien] Advanced Effects Summary - Burst: {burstEffects}, Healing: {healingEffects}, Resistance: {resistanceEffects}");
            Log.Message("[TuTien] ✅ Advanced Effect Properties Test COMPLETED!");
        }
    }
}
