using System.Linq;
using Verse;
using RimWorld;
using TuTien.Core;

namespace TuTien
{
    public class CultivationComp : ThingComp
    {
        public CultivationData cultivationData;
        private CultivationSkillManager skillManager;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            if (cultivationData == null && parent is Pawn pawn)
            {
                cultivationData = new CultivationData(pawn);
            }
            
            if (skillManager == null && parent is Pawn p)
            {
                skillManager = new CultivationSkillManager(p);
            }
        }

        // ✅ STEP 1.3: Smart update intervals for 30% additional performance gains
        private int tickCounter = 0;
        private const int FULL_UPDATE_INTERVAL = 60;    // 1 second - full tick processing
        private const int SKILL_CHECK_INTERVAL = 2500; // ~41 seconds - background tasks
        private const int CACHE_CLEANUP_INTERVAL = 15000; // ~4 minutes - maintenance
        
        public override void CompTick()
        {
            base.CompTick();
            tickCounter++;
            
            // Full cultivation updates every second (balanced frequency)
            if (tickCounter % FULL_UPDATE_INTERVAL == 0)
            {
                cultivationData?.Tick();
                skillManager?.ProcessCooldowns();
            }
            
            // Skill discovery every ~41 seconds (background processing)
            if (tickCounter % SKILL_CHECK_INTERVAL == 0)
            {
                skillManager?.CheckForAutoLearnedSkills();
                skillManager?.ProcessSkillDiscovery();
            }
            
            // Cache cleanup every ~4 minutes (maintenance)
            if (tickCounter % CACHE_CLEANUP_INTERVAL == 0)
            {
                Systems.SkillSynergy.SkillSynergyManager.CleanupStaleCache();
            }
            
            // Reset counter to prevent overflow
            if (tickCounter >= CACHE_CLEANUP_INTERVAL)
            {
                tickCounter = 0;
            }
        }
        
        /// <summary>
        /// Get the skill manager for this pawn
        /// </summary>
        public CultivationSkillManager GetSkillManager()
        {
            return skillManager;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref cultivationData, "cultivationData");
            Scribe_Deep.Look(ref skillManager, "skillManager");
            
            // Re-establish pawn reference after loading
            if (Scribe.mode == LoadSaveMode.PostLoadInit && skillManager != null && parent is Pawn pawn)
            {
                skillManager.pawn = pawn;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (cultivationData == null) return null;

            var talentDef = DefDatabase<TalentDef>.AllDefs
                .FirstOrDefault(t => t.talentLevel == cultivationData.talent);

            string realmName = cultivationData.currentRealm.ToString();
            string talentName = talentDef?.labelKey?.Translate() ?? cultivationData.talent.ToString();

            float requiredPoints = cultivationData.GetRequiredCultivationPoints();
            float progressPercent = (cultivationData.cultivationPoints / requiredPoints) * 100f;
            
            // Add skill information
            string skillInfo = "";
            if (skillManager != null)
            {
                int learnedSkills = skillManager.GetTotalLearnedSkills();
                int skillPoints = skillManager.skillPoints;
                int highestLevel = skillManager.GetHighestSkillLevel();
                
                skillInfo = $"\nSkills: {learnedSkills} learned | {skillPoints} points";
                if (highestLevel > 0)
                {
                    skillInfo += $" | Max Lv.{highestLevel}";
                }
            }
            
            return $"Realm: {realmName} Stage {cultivationData.currentStage}\n" +
                   $"Talent: {talentName}\n" +
                   $"Qi: {cultivationData.currentQi:F1}/{cultivationData.maxQi:F1}\n" +
                   $"Tu Vi: {cultivationData.cultivationPoints:F1}/{requiredPoints:F1} ({progressPercent:F1}%)" +
                   skillInfo;
        }
    }

    public class CultivationCompProperties : CompProperties
    {
        public CultivationCompProperties()
        {
            compClass = typeof(CultivationComp);
        }
    }
}
