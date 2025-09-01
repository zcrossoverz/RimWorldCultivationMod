using Verse;
using Verse.AI;
using RimWorld;

namespace TuTien
{
    public class WorkGiver_Cultivate : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(TuTienDefOf.CultivationMeditationSpot);

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // Check if pawn has cultivation component
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null)
                return false;

            // Check if cultivation spot is available
            if (t.def != TuTienDefOf.CultivationMeditationSpot)
                return false;

            // Check if pawn can reserve the spot
            if (!pawn.CanReserve(t, 1, -1, null, forced))
                return false;

            // Check if pawn should cultivate (not at max level, has qi)
            return comp.cultivationData.CanCultivate();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(TuTienDefOf.MeditateCultivation, t);
        }
    }
}
