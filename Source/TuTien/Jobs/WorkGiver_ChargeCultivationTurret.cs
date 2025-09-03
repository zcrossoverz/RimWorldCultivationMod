using RimWorld;
using TuTien.Core;
using Verse;
using Verse.AI;

namespace TuTien
{
    public class WorkGiver_ChargeCultivationTurret : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(TuTienDefOf.CultivationLightningTurret);

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            // Only cultivators can charge turrets
            return pawn.TryGetComp<CultivationComp>() == null;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_CultivationTurret turret))
                return false;

            var qiComp = turret.QiComp;
            if (qiComp == null || qiComp.IsFull)
                return false;

            var cultivationComp = pawn.TryGetComp<CultivationComp>();
            if (cultivationComp?.cultivationData?.currentRealm == CultivationRealm.Mortal)
                return false;

            return pawn.CanReserve(turret, 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(TuTienDefOf.ChargeCultivationTurret, t);
        }
    }
}
