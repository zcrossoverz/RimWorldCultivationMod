using Verse;
using RimWorld;

namespace TuTien
{
    public class CultivationSpotComp : ThingComp
    {
        public CultivationSpotCompProperties Props => (CultivationSpotCompProperties)props;

        public float CultivationBonus => Props.cultivationBonus;

        public override string CompInspectStringExtra()
        {
            return $"Cultivation bonus: +{(CultivationBonus - 1f) * 100f:F0}%";
        }
    }

    public class CultivationSpotCompProperties : CompProperties
    {
        public float cultivationBonus = 10f; // 10x bonus for meditation spot

        public CultivationSpotCompProperties()
        {
            compClass = typeof(CultivationSpotComp);
        }
    }
}
