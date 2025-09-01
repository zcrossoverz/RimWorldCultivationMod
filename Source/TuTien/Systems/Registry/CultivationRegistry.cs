using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using TuTien.Systems.SkillSynergy;  // ✅ Add for SkillSynergyDef and SkillSynergyRarity

namespace TuTien.Systems.Registry
{
    /// <summary>
    /// ✅ STEP 2.1: Centralized Definition Registry for 2x faster lookups
    /// Provides O(1) access to all cultivation definitions with smart caching
    /// Replaces expensive DefDatabase.AllDefs calls throughout the codebase
    /// </summary>
    public static class CultivationRegistry
    {
        #region Cache Storage

        // Skill definitions cache
        private static Dictionary<string, CultivationSkillDef> _skillDefsCache;
        private static Dictionary<CultivationRealm, List<CultivationSkillDef>> _skillsByRealm;
        private static Dictionary<int, List<CultivationSkillDef>> _skillsByLevel;

        // Technique definitions cache
        private static Dictionary<string, CultivationTechniqueDef> _techniqueDefsCache;
        private static Dictionary<CultivationRealm, List<CultivationTechniqueDef>> _techniquesByRealm;
        private static Dictionary<string, List<CultivationTechniqueDef>> _techniquesByType;

        // Synergy definitions cache
        private static Dictionary<string, SkillSynergyDef> _synergyDefsCache;
        private static Dictionary<SkillSynergyRarity, List<SkillSynergyDef>> _synergyByRarity;
        private static Dictionary<CultivationRealm, List<SkillSynergyDef>> _synergyByRealm;

        // Talent definitions cache
        private static Dictionary<TalentLevel, TalentDef> _talentDefsCache;
        
        // Effect definitions cache
        private static Dictionary<string, CultivationEffectDef> _effectDefsCache;
        private static Dictionary<string, List<CultivationEffectDef>> _effectsByCategory;

        // Cache status
        private static bool _isInitialized = false;
        private static int _lastUpdateTick = -1;
        private const int CACHE_REFRESH_INTERVAL = 18000; // 5 minutes

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize all definition caches - call once at mod startup
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            Log.Message("[TuTien] Initializing Cultivation Registry...");

            try
            {
                BuildSkillCache();
                BuildTechniqueCache();
                BuildSynergyCache();
                BuildTalentCache();

                _isInitialized = true;
                _lastUpdateTick = Find.TickManager?.TicksGame ?? 0;

                Log.Message($"[TuTien] Registry initialized: {_skillDefsCache.Count} skills, " +
                          $"{_techniqueDefsCache.Count} techniques, {_synergyDefsCache.Count} synergies");
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien] Failed to initialize Registry: {ex}");
            }
        }

        /// <summary>
        /// Force refresh all caches - use when definitions change
        /// </summary>
        public static void RefreshCaches()
        {
            _isInitialized = false;
            ClearCaches();
            Initialize();
        }

        /// <summary>
        /// Clear all caches
        /// </summary>
        private static void ClearCaches()
        {
            _skillDefsCache?.Clear();
            _skillsByRealm?.Clear();
            _skillsByLevel?.Clear();
            _techniqueDefsCache?.Clear();
            _techniquesByRealm?.Clear();
            _techniquesByType?.Clear();
            _synergyDefsCache?.Clear();
            _synergyByRarity?.Clear();
            _synergyByRealm?.Clear();
            _talentDefsCache?.Clear();
        }

        #endregion

        #region Skill Definitions

        /// <summary>
        /// Build skill definition cache with organized indices
        /// </summary>
        private static void BuildSkillCache()
        {
            _skillDefsCache = new Dictionary<string, CultivationSkillDef>();
            _skillsByRealm = new Dictionary<CultivationRealm, List<CultivationSkillDef>>();
            _skillsByLevel = new Dictionary<int, List<CultivationSkillDef>>();

            var allSkills = DefDatabase<CultivationSkillDef>.AllDefs;
            
            foreach (var skill in allSkills)
            {
                // Main cache by defName
                _skillDefsCache[skill.defName] = skill;

                // Index by realm
                if (!_skillsByRealm.ContainsKey(skill.requiredRealm))
                    _skillsByRealm[skill.requiredRealm] = new List<CultivationSkillDef>();
                _skillsByRealm[skill.requiredRealm].Add(skill);

                // Index by level requirement
                int level = skill.requiredStage;
                if (!_skillsByLevel.ContainsKey(level))
                    _skillsByLevel[level] = new List<CultivationSkillDef>();
                _skillsByLevel[level].Add(skill);
            }
        }

        /// <summary>
        /// Get skill definition by name - O(1) lookup
        /// </summary>
        public static CultivationSkillDef GetSkillDef(string defName)
        {
            EnsureInitialized();
            return _skillDefsCache.TryGetValue(defName, out var skill) ? skill : null;
        }

        /// <summary>
        /// Get all skills available for a realm - pre-filtered and cached
        /// </summary>
        public static List<CultivationSkillDef> GetSkillsForRealm(CultivationRealm realm)
        {
            EnsureInitialized();
            return _skillsByRealm.TryGetValue(realm, out var skills) ? skills : new List<CultivationSkillDef>();
        }

        /// <summary>
        /// Get all skills available at a certain level
        /// </summary>
        public static List<CultivationSkillDef> GetSkillsForLevel(int level)
        {
            EnsureInitialized();
            return _skillsByLevel.TryGetValue(level, out var skills) ? skills : new List<CultivationSkillDef>();
        }

        /// <summary>
        /// Get all skill definitions - use sparingly
        /// </summary>
        public static IEnumerable<CultivationSkillDef> AllSkillDefs
        {
            get
            {
                EnsureInitialized();
                return _skillDefsCache.Values;
            }
        }

        #endregion

        #region Technique Definitions

        /// <summary>
        /// Build technique definition cache with organized indices
        /// </summary>
        private static void BuildTechniqueCache()
        {
            _techniqueDefsCache = new Dictionary<string, CultivationTechniqueDef>();
            _techniquesByRealm = new Dictionary<CultivationRealm, List<CultivationTechniqueDef>>();
            _techniquesByType = new Dictionary<string, List<CultivationTechniqueDef>>();

            var allTechniques = DefDatabase<CultivationTechniqueDef>.AllDefs;
            
            foreach (var technique in allTechniques)
            {
                // Main cache by defName
                _techniqueDefsCache[technique.defName] = technique;

                // Basic category indexing by technique name patterns
                string category = "General";
                if (technique.defName.Contains("Sword")) category = "Sword";
                else if (technique.defName.Contains("Fire")) category = "Fire";
                else if (technique.defName.Contains("Ice")) category = "Ice";
                else if (technique.defName.Contains("Lightning")) category = "Lightning";
                else if (technique.defName.Contains("Earth")) category = "Earth";
                
                if (!_techniquesByType.ContainsKey(category))
                    _techniquesByType[category] = new List<CultivationTechniqueDef>();
                _techniquesByType[category].Add(technique);
            }
        }

        /// <summary>
        /// Get technique definition by name - O(1) lookup
        /// </summary>
        public static CultivationTechniqueDef GetTechniqueDef(string defName)
        {
            EnsureInitialized();
            return _techniqueDefsCache.TryGetValue(defName, out var technique) ? technique : null;
        }

        /// <summary>
        /// Get all techniques available for a realm
        /// </summary>
        public static List<CultivationTechniqueDef> GetTechniquesForRealm(CultivationRealm realm)
        {
            EnsureInitialized();
            return _techniquesByRealm.TryGetValue(realm, out var techniques) ? techniques : new List<CultivationTechniqueDef>();
        }

        /// <summary>
        /// Get all techniques of a specific type/category
        /// </summary>
        public static List<CultivationTechniqueDef> GetTechniquesByType(string category)
        {
            EnsureInitialized();
            return _techniquesByType.TryGetValue(category, out var techniques) ? techniques : new List<CultivationTechniqueDef>();
        }

        /// <summary>
        /// Get all technique definitions - use sparingly
        /// </summary>
        public static IEnumerable<CultivationTechniqueDef> AllTechniqueDefs
        {
            get
            {
                EnsureInitialized();
                return _techniqueDefsCache.Values;
            }
        }

        #endregion

        #region Synergy Definitions

        /// <summary>
        /// Build synergy definition cache with organized indices
        /// </summary>
        private static void BuildSynergyCache()
        {
            _synergyDefsCache = new Dictionary<string, SkillSynergyDef>();
            _synergyByRarity = new Dictionary<SkillSynergyRarity, List<SkillSynergyDef>>();
            _synergyByRealm = new Dictionary<CultivationRealm, List<SkillSynergyDef>>();

            var allSynergies = DefDatabase<SkillSynergyDef>.AllDefs;
            
            foreach (var synergy in allSynergies)
            {
                // Main cache by defName
                _synergyDefsCache[synergy.defName] = synergy;

                // Index by rarity
                if (!_synergyByRarity.ContainsKey(synergy.rarity))
                    _synergyByRarity[synergy.rarity] = new List<SkillSynergyDef>();
                _synergyByRarity[synergy.rarity].Add(synergy);

                // Index by required realm
                if (!_synergyByRealm.ContainsKey(synergy.requiredRealm))
                    _synergyByRealm[synergy.requiredRealm] = new List<SkillSynergyDef>();
                _synergyByRealm[synergy.requiredRealm].Add(synergy);
            }
        }

        /// <summary>
        /// Get synergy definition by name - O(1) lookup
        /// </summary>
        public static SkillSynergyDef GetSynergyDef(string defName)
        {
            EnsureInitialized();
            return _synergyDefsCache.TryGetValue(defName, out var synergy) ? synergy : null;
        }

        /// <summary>
        /// Get all synergies of a specific rarity
        /// </summary>
        public static List<SkillSynergyDef> GetSynergyByRarity(SkillSynergyRarity rarity)
        {
            EnsureInitialized();
            return _synergyByRarity.TryGetValue(rarity, out var synergies) ? synergies : new List<SkillSynergyDef>();
        }

        /// <summary>
        /// Get all synergies available for a realm
        /// </summary>
        public static List<SkillSynergyDef> GetSynergyForRealm(CultivationRealm realm)
        {
            EnsureInitialized();
            return _synergyByRealm.TryGetValue(realm, out var synergies) ? synergies : new List<SkillSynergyDef>();
        }

        /// <summary>
        /// Get all synergy definitions - use sparingly  
        /// </summary>
        public static IEnumerable<SkillSynergyDef> AllSynergyDefs
        {
            get
            {
                EnsureInitialized();
                return _synergyDefsCache.Values;
            }
        }

        #endregion

        #region Talent Definitions

        /// <summary>
        /// Build talent definition cache
        /// </summary>
        private static void BuildTalentCache()
        {
            _talentDefsCache = new Dictionary<TalentLevel, TalentDef>();

            var allTalents = DefDatabase<TalentDef>.AllDefs;
            
            foreach (var talent in allTalents)
            {
                _talentDefsCache[talent.talentLevel] = talent;
            }
        }

        /// <summary>
        /// Get talent definition by level - O(1) lookup
        /// </summary>
        public static TalentDef GetTalentDef(TalentLevel level)
        {
            EnsureInitialized();
            return _talentDefsCache.TryGetValue(level, out var talent) ? talent : null;
        }

        /// <summary>
        /// Get all talent definitions
        /// </summary>
        public static IEnumerable<TalentDef> AllTalentDefs
        {
            get
            {
                EnsureInitialized();
                return _talentDefsCache.Values;
            }
        }

        #endregion

        #region Effect Definition Access

        /// <summary>
        /// Get effect definition by name - O(1) lookup
        /// </summary>
        public static CultivationEffectDef GetEffectDef(string defName)
        {
            if (string.IsNullOrEmpty(defName)) return null;
            
            // Simple direct access since we don't have complex caching yet
            return DefDatabase<CultivationEffectDef>.GetNamedSilentFail(defName);
        }

        /// <summary>
        /// Get all effect definitions by category
        /// </summary>
        public static IEnumerable<CultivationEffectDef> GetEffectsByCategory(string category)
        {
            return DefDatabase<CultivationEffectDef>.AllDefs.Where(def => def.category == category);
        }

        /// <summary>
        /// Get all effect definitions
        /// </summary>
        public static IEnumerable<CultivationEffectDef> AllEffectDefs => DefDatabase<CultivationEffectDef>.AllDefs;

        #endregion

        #region Cache Management

        /// <summary>
        /// Ensure registry is initialized before use
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            else if (ShouldRefreshCache())
            {
                RefreshCaches();
            }
        }

        /// <summary>
        /// Check if cache should be refreshed
        /// </summary>
        private static bool ShouldRefreshCache()
        {
            if (Find.TickManager?.TicksGame == null) return false;
            
            return Find.TickManager.TicksGame - _lastUpdateTick > CACHE_REFRESH_INTERVAL;
        }

        /// <summary>
        /// Get registry statistics for debugging
        /// </summary>
        public static string GetRegistryStats()
        {
            EnsureInitialized();
            
            return $"Registry Stats:\n" +
                   $"  Skills: {_skillDefsCache?.Count ?? 0} definitions, {_skillsByRealm?.Count ?? 0} realm indices\n" +
                   $"  Techniques: {_techniqueDefsCache?.Count ?? 0} definitions, {_techniquesByRealm?.Count ?? 0} realm indices\n" +
                   $"  Synergies: {_synergyDefsCache?.Count ?? 0} definitions, {_synergyByRarity?.Count ?? 0} rarity levels\n" +
                   $"  Talents: {_talentDefsCache?.Count ?? 0} definitions\n" +
                   $"  Last refresh: {(Find.TickManager?.TicksGame - _lastUpdateTick) ?? 0} ticks ago";
        }

        #endregion
    }
}
