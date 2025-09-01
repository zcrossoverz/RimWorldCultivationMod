using System;
using System.Linq;
using Verse;
using TuTien.Systems.Registry;
using TuTien.Systems.Artifacts;

namespace TuTien.Testing
{
    /// <summary>
    /// ✅ Phase 2 Complete: Artifact System Test
    /// Validates artifact generation, component integration, and registry functionality
    /// DISABLED: Manual testing only - no automatic startup testing
    /// </summary>
    public static class ArtifactSystemTest
    {
        // Disabled automatic testing - call manually via console
        // static ArtifactSystemTest()
        // {
        //     TestArtifactSystem();
        // }
        
        public static void TestArtifactSystem()
        {
            Log.Message("[TuTien] Testing Artifact System...");
            
            try
            {
                TestRegistryIntegration();
                TestArtifactGeneration();
                TestELOScaling();
                TestBuffGeneration();
                
                Log.Message("[TuTien] ✅ Artifact System Test COMPLETED!");
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien] Artifact System Test FAILED: {ex}");
            }
        }
        
        /// <summary>Test registry integration</summary>
        private static void TestRegistryIntegration()
        {
            Log.Message("[TuTien] Testing Registry Integration...");
            
            // Test artifact def lookup
            var allArtifacts = CultivationRegistry.AllArtifactDefs;
            Log.Message($"[TuTien] Registry contains {allArtifacts.Count()} artifact definitions");
            
            // Test rarity-based lookup
            foreach (ArtifactRarity rarity in Enum.GetValues(typeof(ArtifactRarity)))
            {
                var artifacts = CultivationRegistry.GetArtifactsByRarity(rarity);
                Log.Message($"[TuTien] Found {artifacts.Count} {rarity} artifacts");
            }
            
            // Test type-based lookup  
            foreach (ArtifactType type in Enum.GetValues(typeof(ArtifactType)))
            {
                var artifacts = CultivationRegistry.GetArtifactsByType(type);
                if (artifacts.Count > 0)
                {
                    Log.Message($"[TuTien] Found {artifacts.Count} {type} artifacts");
                }
            }
        }
        
        /// <summary>Test artifact generation</summary>
        private static void TestArtifactGeneration()
        {
            Log.Message("[TuTien] Testing Artifact Generation...");
            
            // Test generation for each rarity
            foreach (ArtifactRarity rarity in Enum.GetValues(typeof(ArtifactRarity)))
            {
                try
                {
                    var artifact = ArtifactGenerator.GenerateRandomArtifact(rarity);
                    if (ArtifactGenerator.ValidateArtifactData(artifact))
                    {
                        Log.Message($"[TuTien] ✅ Generated valid {rarity} artifact: " +
                                   $"ELO {artifact.eloRating:F0}, Damage {artifact.baseDamage:F1}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[TuTien] Failed to generate {rarity} artifact: {ex.Message}");
                }
            }
        }
        
        /// <summary>Test ELO scaling</summary>
        private static void TestELOScaling()
        {
            Log.Message("[TuTien] Testing ELO Scaling...");
            
            var testELOs = new float[] { 100f, 300f, 500f, 800f, 1200f };
            
            foreach (float testELO in testELOs)
            {
                // Test different rarities to find artifacts
                CultivationArtifactData testArtifact = null;
                
                foreach (ArtifactRarity rarity in Enum.GetValues(typeof(ArtifactRarity)))
                {
                    var artifacts = CultivationRegistry.GetArtifactsByRarity(rarity);
                    if (artifacts.Count > 0)
                    {
                        testArtifact = ArtifactGenerator.GenerateArtifactWithELO(artifacts.First(), testELO);
                        break;
                    }
                }
                
                if (testArtifact != null)
                {
                    Log.Message($"[TuTien] ELO {testELO:F0} -> Power {testArtifact.powerMultiplier:F2}x, " +
                               $"Damage {testArtifact.baseDamage:F1}, Qi {testArtifact.maxArtifactQi:F0}");
                }
            }
        }
        
        /// <summary>Test buff generation</summary>
        private static void TestBuffGeneration()
        {
            Log.Message("[TuTien] Testing Buff Generation...");
            
            // Test each artifact type
            foreach (ArtifactType type in Enum.GetValues(typeof(ArtifactType)))
            {
                var artifacts = CultivationRegistry.GetArtifactsByType(type);
                if (artifacts.Count > 0)
                {
                    var testArtifact = ArtifactGenerator.GenerateArtifact(artifacts.First());
                    Log.Message($"[TuTien] {type} artifact generated {testArtifact.activeBuffs.Count} buffs: " +
                               $"{string.Join(", ", testArtifact.activeBuffs.Select(b => b.buffType.ToString()))}");
                }
            }
            
            // Test ELO scaling for buffs
            var commonArtifacts = CultivationRegistry.GetArtifactsByRarity(ArtifactRarity.Common);
            if (commonArtifacts.Count > 0)
            {
                var lowELO = ArtifactGenerator.GenerateArtifactWithELO(commonArtifacts.First(), 150f);
                var highELO = ArtifactGenerator.GenerateArtifactWithELO(commonArtifacts.First(), 1000f);
                
                Log.Message($"[TuTien] Buff scaling test:");
                Log.Message($"[TuTien] - Low ELO (150): {lowELO.activeBuffs.Count} buffs");
                Log.Message($"[TuTien] - High ELO (1000): {highELO.activeBuffs.Count} buffs");
            }
        }
    }
}
