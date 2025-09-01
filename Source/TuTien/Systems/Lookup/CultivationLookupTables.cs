using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using TuTien.Systems.Registry;
using TuTien.Systems.SkillSynergy;  // ✅ Add for SkillSynergyDef

namespace TuTien.Systems.Lookup
{
    /// <summary>
    /// ✅ STEP 2.2: Efficient Lookup Tables for 2x faster specialized queries
    /// Pre-computed lookup tables for common skill/technique combinations
    /// Replaces expensive LINQ queries with O(1) dictionary lookups
    /// </summary>
    public static class CultivationLookupTables
    {
        #region Lookup Tables

        // Skill lookup tables by common criteria
        private static Dictionary<CultivationRealm, Dictionary<int, List<CultivationSkillDef>>> _skillsByRealmAndStage;
        private static Dictionary<string, List<CultivationSkillDef>> _skillsByElementType;
        private static Dictionary<string, List<CultivationSkillDef>> _skillsByCategory;
        
        // Technique lookup tables
        private static Dictionary<string, List<CultivationTechniqueDef>> _techniquesByElement;
        private static Dictionary<CultivationRealm, List<CultivationTechniqueDef>> _techniquesByRealm;
        
        // Synergy lookup tables
        private static Dictionary<CultivationRealm, List<SkillSynergyDef>> _synergyByRealmRequirement;
        private static Dictionary<int, List<SkillSynergyDef>> _synergyBySkillCount;
        private static Dictionary<string, List<SkillSynergyDef>> _synergyByFirstSkill;
        
        // Cache status
        private static bool _tablesBuilt = false;
        private static int _lastRebuildTick = -1;
        private const int REBUILD_INTERVAL = 36000; // 10 minutes

        #endregion

        #region Initialization

        /// <summary>
        /// Build all lookup tables - call after registry initialization
        /// </summary>
        public static void BuildLookupTables()
        {
            if (_tablesBuilt && !ShouldRebuild()) return;

            Log.Message("[TuTien] Building Lookup Tables...");

            try
            {
                // Ensure registry is ready
                CultivationRegistry.Initialize();
                
                BuildSkillLookupTables();
                BuildTechniqueLookupTables();
                BuildSynergyLookupTables();

                _tablesBuilt = true;
                _lastRebuildTick = Find.TickManager?.TicksGame ?? 0;

                Log.Message($"[TuTien] Lookup tables built: {_skillsByRealmAndStage.Count} realm-stage combinations, " +
                          $"{_skillsByElementType.Count} element types, {_techniquesByElement.Count} technique elements");
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien] Failed to build lookup tables: {ex}");
            }
        }

        /// <summary>
        /// Force rebuild all lookup tables
        /// </summary>
        public static void RebuildTables()
        {
            _tablesBuilt = false;
            ClearTables();
            BuildLookupTables();
        }

        /// <summary>
        /// Clear all lookup tables
        /// </summary>
        private static void ClearTables()
        {
            _skillsByRealmAndStage?.Clear();
            _skillsByElementType?.Clear();
            _skillsByCategory?.Clear();
            _techniquesByElement?.Clear();
            _techniquesByRealm?.Clear();
            _synergyByRealmRequirement?.Clear();
            _synergyBySkillCount?.Clear();
            _synergyByFirstSkill?.Clear();
        }

        /// <summary>
        /// Check if tables should be rebuilt
        /// </summary>
        private static bool ShouldRebuild()
        {
            if (Find.TickManager?.TicksGame == null) return false;
            return Find.TickManager.TicksGame - _lastRebuildTick > REBUILD_INTERVAL;
        }

        #endregion

        #region Skill Lookup Tables

        /// <summary>
        /// Build skill lookup tables organized by common query patterns
        /// </summary>
        private static void BuildSkillLookupTables()
        {
            _skillsByRealmAndStage = new Dictionary<CultivationRealm, Dictionary<int, List<CultivationSkillDef>>>();
            _skillsByElementType = new Dictionary<string, List<CultivationSkillDef>>();
            _skillsByCategory = new Dictionary<string, List<CultivationSkillDef>>();

            foreach (var skill in CultivationRegistry.AllSkillDefs)
            {
                // Index by realm and stage combination
                if (!_skillsByRealmAndStage.ContainsKey(skill.requiredRealm))
                    _skillsByRealmAndStage[skill.requiredRealm] = new Dictionary<int, List<CultivationSkillDef>>();
                
                if (!_skillsByRealmAndStage[skill.requiredRealm].ContainsKey(skill.requiredStage))
                    _skillsByRealmAndStage[skill.requiredRealm][skill.requiredStage] = new List<CultivationSkillDef>();
                
                _skillsByRealmAndStage[skill.requiredRealm][skill.requiredStage].Add(skill);

                // Index by element type (parse from skill name)
                var elementType = GetElementTypeFromSkillName(skill.defName);
                if (!_skillsByElementType.ContainsKey(elementType))
                    _skillsByElementType[elementType] = new List<CultivationSkillDef>();
                _skillsByElementType[elementType].Add(skill);

                // Index by category
                var category = GetCategoryFromSkillName(skill.defName);
                if (!_skillsByCategory.ContainsKey(category))
                    _skillsByCategory[category] = new List<CultivationSkillDef>();
                _skillsByCategory[category].Add(skill);
            }
        }

        /// <summary>
        /// Get skills available for specific realm and stage - O(1) lookup
        /// </summary>
        public static List<CultivationSkillDef> GetSkillsForRealmAndStage(CultivationRealm realm, int stage)
        {
            EnsureTablesBuilt();
            
            if (_skillsByRealmAndStage.TryGetValue(realm, out var stageDict) &&
                stageDict.TryGetValue(stage, out var skills))
            {
                return skills;
            }
            
            return new List<CultivationSkillDef>();
        }

        /// <summary>
        /// Get all skills of a specific element type - O(1) lookup
        /// </summary>
        public static List<CultivationSkillDef> GetSkillsByElement(string elementType)
        {
            EnsureTablesBuilt();
            return _skillsByElementType.TryGetValue(elementType, out var skills) ? skills : new List<CultivationSkillDef>();
        }

        /// <summary>
        /// Get skills available up to and including a specific realm/stage combination
        /// </summary>
        public static List<CultivationSkillDef> GetSkillsUpToLevel(CultivationRealm maxRealm, int maxStage)
        {
            EnsureTablesBuilt();
            
            var availableSkills = new List<CultivationSkillDef>();
            
            foreach (var realmKvp in _skillsByRealmAndStage)
            {
                if (realmKvp.Key > maxRealm) continue;
                
                foreach (var stageKvp in realmKvp.Value)
                {
                    if (realmKvp.Key == maxRealm && stageKvp.Key > maxStage) continue;
                    
                    availableSkills.AddRange(stageKvp.Value);
                }
            }
            
            return availableSkills;
        }

        #endregion

        #region Technique Lookup Tables

        /// <summary>
        /// Build technique lookup tables
        /// </summary>
        private static void BuildTechniqueLookupTables()
        {
            _techniquesByElement = new Dictionary<string, List<CultivationTechniqueDef>>();
            _techniquesByRealm = new Dictionary<CultivationRealm, List<CultivationTechniqueDef>>();

            foreach (var technique in CultivationRegistry.AllTechniqueDefs)
            {
                // Index by element type
                var elementType = GetElementTypeFromTechniqueName(technique.defName);
                if (!_techniquesByElement.ContainsKey(elementType))
                    _techniquesByElement[elementType] = new List<CultivationTechniqueDef>();
                _techniquesByElement[elementType].Add(technique);

                // Index by estimated realm requirement (based on name patterns)
                var estimatedRealm = EstimateRealmFromTechniqueName(technique.defName);
                if (!_techniquesByRealm.ContainsKey(estimatedRealm))
                    _techniquesByRealm[estimatedRealm] = new List<CultivationTechniqueDef>();
                _techniquesByRealm[estimatedRealm].Add(technique);
            }
        }

        /// <summary>
        /// Get techniques by element type - O(1) lookup
        /// </summary>
        public static List<CultivationTechniqueDef> GetTechniquesByElement(string elementType)
        {
            EnsureTablesBuilt();
            return _techniquesByElement.TryGetValue(elementType, out var techniques) ? techniques : new List<CultivationTechniqueDef>();
        }

        /// <summary>
        /// Get techniques suitable for a realm - O(1) lookup
        /// </summary>
        public static List<CultivationTechniqueDef> GetTechniquesForRealm(CultivationRealm realm)
        {
            EnsureTablesBuilt();
            return _techniquesByRealm.TryGetValue(realm, out var techniques) ? techniques : new List<CultivationTechniqueDef>();
        }

        #endregion

        #region Synergy Lookup Tables

        /// <summary>
        /// Build synergy lookup tables for fast querying
        /// </summary>
        private static void BuildSynergyLookupTables()
        {
            _synergyByRealmRequirement = new Dictionary<CultivationRealm, List<SkillSynergyDef>>();
            _synergyBySkillCount = new Dictionary<int, List<SkillSynergyDef>>();
            _synergyByFirstSkill = new Dictionary<string, List<SkillSynergyDef>>();

            foreach (var synergy in CultivationRegistry.AllSynergyDefs)
            {
                // Index by realm requirement
                if (!_synergyByRealmRequirement.ContainsKey(synergy.requiredRealm))
                    _synergyByRealmRequirement[synergy.requiredRealm] = new List<SkillSynergyDef>();
                _synergyByRealmRequirement[synergy.requiredRealm].Add(synergy);

                // Index by required skill count
                int skillCount = synergy.requiredSkills?.Count ?? 0;
                if (!_synergyBySkillCount.ContainsKey(skillCount))
                    _synergyBySkillCount[skillCount] = new List<SkillSynergyDef>();
                _synergyBySkillCount[skillCount].Add(synergy);

                // Index by first required skill (for quick elimination)
                if (synergy.requiredSkills?.Count > 0)
                {
                    string firstSkill = synergy.requiredSkills[0].defName;
                    if (!_synergyByFirstSkill.ContainsKey(firstSkill))
                        _synergyByFirstSkill[firstSkill] = new List<SkillSynergyDef>();
                    _synergyByFirstSkill[firstSkill].Add(synergy);
                }
            }
        }

        /// <summary>
        /// Get synergies available for a specific realm - O(1) lookup
        /// </summary>
        public static List<SkillSynergyDef> GetSynergiesForRealm(CultivationRealm realm)
        {
            EnsureTablesBuilt();
            return _synergyByRealmRequirement.TryGetValue(realm, out var synergies) ? synergies : new List<SkillSynergyDef>();
        }

        /// <summary>
        /// Get synergies that require a specific number of skills
        /// </summary>
        public static List<SkillSynergyDef> GetSynergiesBySkillCount(int skillCount)
        {
            EnsureTablesBuilt();
            return _synergyBySkillCount.TryGetValue(skillCount, out var synergies) ? synergies : new List<SkillSynergyDef>();
        }

        /// <summary>
        /// Get synergies that require a specific skill as their first requirement
        /// </summary>
        public static List<SkillSynergyDef> GetSynergiesWithFirstSkill(string skillDefName)
        {
            EnsureTablesBuilt();
            return _synergyByFirstSkill.TryGetValue(skillDefName, out var synergies) ? synergies : new List<SkillSynergyDef>();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Extract element type from skill name
        /// </summary>
        private static string GetElementTypeFromSkillName(string skillName)
        {
            if (skillName.Contains("Fire")) return "Fire";
            if (skillName.Contains("Ice")) return "Ice";
            if (skillName.Contains("Lightning")) return "Lightning";
            if (skillName.Contains("Earth")) return "Earth";
            if (skillName.Contains("Sword")) return "Sword";
            if (skillName.Contains("Body")) return "Body";
            if (skillName.Contains("Soul")) return "Soul";
            return "General";
        }

        /// <summary>
        /// Extract category from skill name
        /// </summary>
        private static string GetCategoryFromSkillName(string skillName)
        {
            if (skillName.Contains("Combat")) return "Combat";
            if (skillName.Contains("Healing")) return "Healing";
            if (skillName.Contains("Utility")) return "Utility";
            if (skillName.Contains("Movement")) return "Movement";
            if (skillName.Contains("Qi")) return "Qi";
            return "General";
        }

        /// <summary>
        /// Extract element type from technique name
        /// </summary>
        private static string GetElementTypeFromTechniqueName(string techniqueName)
        {
            return GetElementTypeFromSkillName(techniqueName); // Same logic
        }

        /// <summary>
        /// Estimate realm requirement from technique name
        /// </summary>
        private static CultivationRealm EstimateRealmFromTechniqueName(string techniqueName)
        {
            if (techniqueName.Contains("Basic") || techniqueName.Contains("Novice")) 
                return CultivationRealm.Mortal;
            if (techniqueName.Contains("Qi") || techniqueName.Contains("Intermediate")) 
                return CultivationRealm.QiCondensation;
            if (techniqueName.Contains("Foundation") || techniqueName.Contains("Advanced")) 
                return CultivationRealm.FoundationEstablishment;
            if (techniqueName.Contains("Core") || techniqueName.Contains("Master")) 
                return CultivationRealm.GoldenCore;
            
            return CultivationRealm.Mortal; // Default
        }

        /// <summary>
        /// Ensure lookup tables are built before use
        /// </summary>
        private static void EnsureTablesBuilt()
        {
            if (!_tablesBuilt)
            {
                BuildLookupTables();
            }
            else if (ShouldRebuild())
            {
                RebuildTables();
            }
        }

        /// <summary>
        /// Get lookup table statistics for debugging
        /// </summary>
        public static string GetLookupStats()
        {
            EnsureTablesBuilt();
            
            return $"Lookup Table Stats:\n" +
                   $"  Skill realm-stage combinations: {_skillsByRealmAndStage?.Count ?? 0}\n" +
                   $"  Skill element types: {_skillsByElementType?.Count ?? 0}\n" +
                   $"  Technique elements: {_techniquesByElement?.Count ?? 0}\n" +
                   $"  Synergy realm requirements: {_synergyByRealmRequirement?.Count ?? 0}\n" +
                   $"  Synergy skill count variations: {_synergyBySkillCount?.Count ?? 0}";
        }

        #endregion
    }
}
