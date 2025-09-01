using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien
{
    /// <summary>
    /// Enhanced cultivation progress tracking system
    /// </summary>
    [System.Serializable]
    public class CultivationProgress : IExposable
    {
        #region Progress Fields
        
        /// <summary>Total cultivation time in ticks</summary>
        public long totalCultivationTime = 0;
        
        /// <summary>Current realm progress (0.0 to 1.0)</summary>
        public float realmProgress = 0f;
        
        /// <summary>Current stage progress (0.0 to 1.0)</summary>
        public float stageProgress = 0f;
        
        /// <summary>Breakthrough attempts made</summary>
        public int breakthroughAttempts = 0;
        
        /// <summary>Successful breakthroughs</summary>
        public int successfulBreakthroughs = 0;
        
        /// <summary>Last meditation efficiency percentage</summary>
        public float lastMeditationEfficiency = 1f;
        
        /// <summary>Accumulated tribulation resistance</summary>
        public float tribulationResistance = 0f;
        
        /// <summary>Current cultivation momentum (affects gains)</summary>
        public float cultivationMomentum = 1f;
        
        #endregion
        
        #region Milestone Tracking
        
        /// <summary>Realms achieved and their achievement times</summary>
        public Dictionary<CultivationRealm, long> realmAchievements = new Dictionary<CultivationRealm, long>();
        
        /// <summary>Special milestones reached</summary>
        public HashSet<string> milestonesReached = new HashSet<string>();
        
        /// <summary>Personal cultivation records</summary>
        public Dictionary<string, float> personalRecords = new Dictionary<string, float>();
        
        #endregion
        
        #region Methods
        
        /// <summary>Add cultivation time and update momentum</summary>
        public void AddCultivationTime(int ticks, float efficiency = 1f)
        {
            totalCultivationTime += ticks;
            lastMeditationEfficiency = efficiency;
            
            // Update momentum based on consistency
            if (efficiency > 0.8f)
            {
                cultivationMomentum = Math.Min(cultivationMomentum + 0.01f, 2f);
            }
            else if (efficiency < 0.5f)
            {
                cultivationMomentum = Math.Max(cultivationMomentum - 0.02f, 0.5f);
            }
        }
        
        /// <summary>Record a breakthrough attempt</summary>
        public void RecordBreakthroughAttempt(bool success, CultivationRealm targetRealm = CultivationRealm.Mortal)
        {
            breakthroughAttempts++;
            if (success)
            {
                successfulBreakthroughs++;
                if (!realmAchievements.ContainsKey(targetRealm))
                {
                    realmAchievements[targetRealm] = Find.TickManager.TicksGame;
                }
            }
        }
        
        /// <summary>Get breakthrough success rate</summary>
        public float GetBreakthroughSuccessRate()
        {
            return breakthroughAttempts > 0 ? (float)successfulBreakthroughs / breakthroughAttempts : 0f;
        }
        
        /// <summary>Check if milestone should be awarded</summary>
        public bool ShouldAwardMilestone(string milestone)
        {
            return !milestonesReached.Contains(milestone);
        }
        
        /// <summary>Award a milestone</summary>
        public void AwardMilestone(string milestone)
        {
            milestonesReached.Add(milestone);
        }
        
        #endregion
        
        #region Serialization
        
        public void ExposeData()
        {
            Scribe_Values.Look(ref totalCultivationTime, "totalCultivationTime", 0L);
            Scribe_Values.Look(ref realmProgress, "realmProgress", 0f);
            Scribe_Values.Look(ref stageProgress, "stageProgress", 0f);
            Scribe_Values.Look(ref breakthroughAttempts, "breakthroughAttempts", 0);
            Scribe_Values.Look(ref successfulBreakthroughs, "successfulBreakthroughs", 0);
            Scribe_Values.Look(ref lastMeditationEfficiency, "lastMeditationEfficiency", 1f);
            Scribe_Values.Look(ref tribulationResistance, "tribulationResistance", 0f);
            Scribe_Values.Look(ref cultivationMomentum, "cultivationMomentum", 1f);
            
            Scribe_Collections.Look(ref realmAchievements, "realmAchievements");
            Scribe_Collections.Look(ref milestonesReached, "milestonesReached");
            Scribe_Collections.Look(ref personalRecords, "personalRecords");
            
            // Initialize collections if null after loading
            if (realmAchievements == null) realmAchievements = new Dictionary<CultivationRealm, long>();
            if (milestonesReached == null) milestonesReached = new HashSet<string>();
            if (personalRecords == null) personalRecords = new Dictionary<string, float>();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Enhanced cultivation affinities and resistances system
    /// </summary>
    [System.Serializable]
    public class CultivationAffinities : IExposable
    {
        #region Elemental Affinities
        
        /// <summary>Fire element affinity (0.0 to 2.0, 1.0 is neutral)</summary>
        public float fireAffinity = 1f;
        
        /// <summary>Water element affinity</summary>
        public float waterAffinity = 1f;
        
        /// <summary>Earth element affinity</summary>
        public float earthAffinity = 1f;
        
        /// <summary>Wood element affinity</summary>
        public float woodAffinity = 1f;
        
        /// <summary>Metal element affinity</summary>
        public float metalAffinity = 1f;
        
        /// <summary>Lightning element affinity</summary>
        public float lightningAffinity = 1f;
        
        /// <summary>Wind element affinity</summary>
        public float windAffinity = 1f;
        
        /// <summary>Ice element affinity</summary>
        public float iceAffinity = 1f;
        
        #endregion
        
        #region Cultivation Path Affinities
        
        /// <summary>Body cultivation affinity</summary>
        public float bodyCultivationAffinity = 1f;
        
        /// <summary>Soul cultivation affinity</summary>
        public float soulCultivationAffinity = 1f;
        
        /// <summary>Dual cultivation affinity</summary>
        public float dualCultivationAffinity = 1f;
        
        /// <summary>Alchemy affinity</summary>
        public float alchemyAffinity = 1f;
        
        /// <summary>Formation affinity</summary>
        public float formationAffinity = 1f;
        
        /// <summary>Weapon cultivation affinity</summary>
        public float weaponCultivationAffinity = 1f;
        
        #endregion
        
        #region Environmental Resistances
        
        /// <summary>Heat resistance (0.0 to 1.0)</summary>
        public float heatResistance = 0f;
        
        /// <summary>Cold resistance</summary>
        public float coldResistance = 0f;
        
        /// <summary>Poison resistance</summary>
        public float poisonResistance = 0f;
        
        /// <summary>Mental corruption resistance</summary>
        public float mentalCorruptionResistance = 0f;
        
        /// <summary>Spiritual pressure resistance</summary>
        public float spiritualPressureResistance = 0f;
        
        #endregion
        
        #region Methods
        
        /// <summary>Get affinity for a specific element</summary>
        public float GetElementalAffinity(string element)
        {
            return element.ToLower() switch
            {
                "fire" => fireAffinity,
                "water" => waterAffinity,
                "earth" => earthAffinity,
                "wood" => woodAffinity,
                "metal" => metalAffinity,
                "lightning" => lightningAffinity,
                "wind" => windAffinity,
                "ice" => iceAffinity,
                _ => 1f
            };
        }
        
        /// <summary>Get strongest elemental affinity</summary>
        public (string element, float affinity) GetStrongestElementalAffinity()
        {
            var affinities = new Dictionary<string, float>
            {
                ["fire"] = fireAffinity,
                ["water"] = waterAffinity,
                ["earth"] = earthAffinity,
                ["wood"] = woodAffinity,
                ["metal"] = metalAffinity,
                ["lightning"] = lightningAffinity,
                ["wind"] = windAffinity,
                ["ice"] = iceAffinity
            };
            
            var strongest = affinities.OrderByDescending(kvp => kvp.Value).First();
            return (strongest.Key, strongest.Value);
        }
        
        /// <summary>Apply environmental resistance</summary>
        public float ApplyEnvironmentalResistance(string damageType, float damage)
        {
            float resistance = damageType.ToLower() switch
            {
                "heat" or "fire" => heatResistance,
                "cold" or "ice" => coldResistance,
                "poison" or "toxic" => poisonResistance,
                "mental" or "psychic" => mentalCorruptionResistance,
                "spiritual" => spiritualPressureResistance,
                _ => 0f
            };
            
            return damage * (1f - resistance);
        }
        
        /// <summary>Randomly initialize affinities based on talent</summary>
        public void RandomizeAffinities(TalentLevel talent, System.Random random = null)
        {
            random ??= new System.Random();
            float variance = talent switch
            {
                TalentLevel.Common => 0.3f,
                TalentLevel.Rare => 0.5f,
                TalentLevel.Genius => 0.7f,
                TalentLevel.HeavenChosen => 1f,
                _ => 0.3f
            };
            
            // Randomize elemental affinities
            fireAffinity = Mathf.Clamp(1f + (float)(random.NextDouble() - 0.5) * variance, 0.5f, 2f);
            waterAffinity = Mathf.Clamp(1f + (float)(random.NextDouble() - 0.5) * variance, 0.5f, 2f);
            earthAffinity = Mathf.Clamp(1f + (float)(random.NextDouble() - 0.5) * variance, 0.5f, 2f);
            woodAffinity = Mathf.Clamp(1f + (float)(random.NextDouble() - 0.5) * variance, 0.5f, 2f);
            metalAffinity = Mathf.Clamp(1f + (float)(random.NextDouble() - 0.5) * variance, 0.5f, 2f);
            
            // Cultivation path affinities
            bodyCultivationAffinity = Mathf.Clamp(1f + (float)(random.NextDouble() - 0.5) * variance, 0.7f, 1.5f);
            soulCultivationAffinity = Mathf.Clamp(1f + (float)(random.NextDouble() - 0.5) * variance, 0.7f, 1.5f);
        }
        
        #endregion
        
        #region Serialization
        
        public void ExposeData()
        {
            // Elemental affinities
            Scribe_Values.Look(ref fireAffinity, "fireAffinity", 1f);
            Scribe_Values.Look(ref waterAffinity, "waterAffinity", 1f);
            Scribe_Values.Look(ref earthAffinity, "earthAffinity", 1f);
            Scribe_Values.Look(ref woodAffinity, "woodAffinity", 1f);
            Scribe_Values.Look(ref metalAffinity, "metalAffinity", 1f);
            Scribe_Values.Look(ref lightningAffinity, "lightningAffinity", 1f);
            Scribe_Values.Look(ref windAffinity, "windAffinity", 1f);
            Scribe_Values.Look(ref iceAffinity, "iceAffinity", 1f);
            
            // Cultivation path affinities
            Scribe_Values.Look(ref bodyCultivationAffinity, "bodyCultivationAffinity", 1f);
            Scribe_Values.Look(ref soulCultivationAffinity, "soulCultivationAffinity", 1f);
            Scribe_Values.Look(ref dualCultivationAffinity, "dualCultivationAffinity", 1f);
            Scribe_Values.Look(ref alchemyAffinity, "alchemyAffinity", 1f);
            Scribe_Values.Look(ref formationAffinity, "formationAffinity", 1f);
            Scribe_Values.Look(ref weaponCultivationAffinity, "weaponCultivationAffinity", 1f);
            
            // Environmental resistances
            Scribe_Values.Look(ref heatResistance, "heatResistance", 0f);
            Scribe_Values.Look(ref coldResistance, "coldResistance", 0f);
            Scribe_Values.Look(ref poisonResistance, "poisonResistance", 0f);
            Scribe_Values.Look(ref mentalCorruptionResistance, "mentalCorruptionResistance", 0f);
            Scribe_Values.Look(ref spiritualPressureResistance, "spiritualPressureResistance", 0f);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Memory-optimized cultivation resources pool
    /// </summary>
    [System.Serializable]
    public class CultivationResources : IExposable
    {
        #region Resource Pools
        
        /// <summary>Spiritual stones count</summary>
        public int spiritualStones = 0;
        
        /// <summary>Cultivation pills count</summary>
        public int cultivationPills = 0;
        
        /// <summary>Qi crystals count</summary>
        public int qiCrystals = 0;
        
        /// <summary>Technique scrolls count</summary>
        public int techniqueScrolls = 0;
        
        /// <summary>Formation materials count</summary>
        public int formationMaterials = 0;
        
        /// <summary>Alchemy ingredients count</summary>
        public int alchemyIngredients = 0;
        
        #endregion
        
        #region Resource Quality Tracking
        
        /// <summary>Average quality of spiritual stones (1-5 scale)</summary>
        public float avgSpiritualStoneQuality = 1f;
        
        /// <summary>Average quality of cultivation pills</summary>
        public float avgCultivationPillQuality = 1f;
        
        /// <summary>Highest quality resource ever obtained</summary>
        public float highestQualityAchieved = 1f;
        
        #endregion
        
        #region Methods
        
        /// <summary>Add resources with quality tracking</summary>
        public void AddResources(string resourceType, int amount, float quality = 1f)
        {
            switch (resourceType.ToLower())
            {
                case "spiritualstones":
                    UpdateQualityAverage(ref spiritualStones, ref avgSpiritualStoneQuality, amount, quality);
                    break;
                case "cultivationpills":
                    UpdateQualityAverage(ref cultivationPills, ref avgCultivationPillQuality, amount, quality);
                    break;
                case "qicrystals":
                    qiCrystals += amount;
                    break;
                case "techniquescrolls":
                    techniqueScrolls += amount;
                    break;
                case "formationmaterials":
                    formationMaterials += amount;
                    break;
                case "alchemyingredients":
                    alchemyIngredients += amount;
                    break;
            }
            
            if (quality > highestQualityAchieved)
            {
                highestQualityAchieved = quality;
            }
        }
        
        /// <summary>Use resources if available</summary>
        public bool TryUseResources(string resourceType, int amount)
        {
            var currentAmount = GetResourceAmount(resourceType);
            if (currentAmount >= amount)
            {
                AddResources(resourceType, -amount);
                return true;
            }
            return false;
        }
        
        /// <summary>Get current amount of a resource</summary>
        public int GetResourceAmount(string resourceType)
        {
            return resourceType.ToLower() switch
            {
                "spiritualstones" => spiritualStones,
                "cultivationpills" => cultivationPills,
                "qicrystals" => qiCrystals,
                "techniquescrolls" => techniqueScrolls,
                "formationmaterials" => formationMaterials,
                "alchemyingredients" => alchemyIngredients,
                _ => 0
            };
        }
        
        /// <summary>Update quality average when adding resources</summary>
        private void UpdateQualityAverage(ref int currentAmount, ref float currentAvgQuality, int addAmount, float newQuality)
        {
            if (addAmount > 0 && currentAmount >= 0)
            {
                float totalQuality = (currentAmount * currentAvgQuality) + (addAmount * newQuality);
                currentAmount += addAmount;
                currentAvgQuality = currentAmount > 0 ? totalQuality / currentAmount : 1f;
            }
            else
            {
                currentAmount = Math.Max(0, currentAmount + addAmount);
            }
        }
        
        /// <summary>Get total resource value</summary>
        public float GetTotalResourceValue()
        {
            return (spiritualStones * avgSpiritualStoneQuality) +
                   (cultivationPills * avgCultivationPillQuality * 2f) +
                   (qiCrystals * 5f) +
                   (techniqueScrolls * 10f) +
                   (formationMaterials * 3f) +
                   (alchemyIngredients * 2f);
        }
        
        #endregion
        
        #region Serialization
        
        public void ExposeData()
        {
            Scribe_Values.Look(ref spiritualStones, "spiritualStones", 0);
            Scribe_Values.Look(ref cultivationPills, "cultivationPills", 0);
            Scribe_Values.Look(ref qiCrystals, "qiCrystals", 0);
            Scribe_Values.Look(ref techniqueScrolls, "techniqueScrolls", 0);
            Scribe_Values.Look(ref formationMaterials, "formationMaterials", 0);
            Scribe_Values.Look(ref alchemyIngredients, "alchemyIngredients", 0);
            
            Scribe_Values.Look(ref avgSpiritualStoneQuality, "avgSpiritualStoneQuality", 1f);
            Scribe_Values.Look(ref avgCultivationPillQuality, "avgCultivationPillQuality", 1f);
            Scribe_Values.Look(ref highestQualityAchieved, "highestQualityAchieved", 1f);
        }
        
        #endregion
    }
}
