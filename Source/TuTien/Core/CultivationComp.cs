using System.Linq;
using Verse;
using RimWorld;

namespace TuTien
{
    public class CultivationComp : ThingComp
    {
        public CultivationData cultivationData;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            if (cultivationData == null && parent is Pawn pawn)
            {
                cultivationData = new CultivationData(pawn);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            cultivationData?.Tick();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref cultivationData, "cultivationData");
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
            
            return $"Realm: {realmName} Stage {cultivationData.currentStage}\n" +
                   $"Talent: {talentName}\n" +
                   $"Qi: {cultivationData.currentQi:F1}/{cultivationData.maxQi:F1}\n" +
                   $"Tu Vi: {cultivationData.cultivationPoints:F1}/{requiredPoints:F1} ({progressPercent:F1}%)";
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
