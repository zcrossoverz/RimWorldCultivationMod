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

        public override void CompTick()
        {
            base.CompTick();
            cultivationData?.Tick();
            skillManager?.ProcessCooldowns();
            
            // Process skill discovery and auto-learning every so often
            if (Find.TickManager.TicksGame % 2500 == 0) // Every ~41 seconds
            {
                skillManager?.CheckForAutoLearnedSkills();
                skillManager?.ProcessSkillDiscovery();
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
