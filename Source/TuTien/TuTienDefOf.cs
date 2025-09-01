using Verse;
using RimWorld;

namespace TuTien
{
    [DefOf]
    public static class TuTienDefOf
    {
        // Hediffs
        public static HediffDef QiForceHediff;
        public static HediffDef QiShieldHediff;
        public static HediffDef QiBarrierHediff;
        public static HediffDef QiBurstHediff;
        public static HediffDef QiRestorationHediff;
        public static HediffDef QiDeviationHediff;
        public static HediffDef CultivationBuffHediff;

        // Projectiles
        public static ThingDef QiWaveProjectile;
        public static ThingDef SwordQiProjectile;
        public static ThingDef LightningProjectile;
        public static ThingDef FireProjectile;
        public static ThingDef IceProjectile;
        public static ThingDef EarthProjectile;

        // Talents
        public static TalentDef CommonTalent;
        public static TalentDef RareTalent;
        public static TalentDef GeniusTalent;
        public static TalentDef HeavenChosenTalent;

        // Techniques
        public static CultivationTechniqueDef SwordQiTechnique;
        public static CultivationTechniqueDef LightningTechnique;
        public static CultivationTechniqueDef FireTechnique;
        public static CultivationTechniqueDef IceTechnique;
        public static CultivationTechniqueDef EarthTechnique;

        // Buildings
        public static ThingDef CultivationMeditationSpot;

        // Jobs
        public static JobDef MeditateCultivation;

        // Thoughts
        public static ThoughtDef MeditatedCultivation;

        static TuTienDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TuTienDefOf));
        }
    }
}
