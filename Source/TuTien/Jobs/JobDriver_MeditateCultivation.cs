using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace TuTien
{
    public class JobDriver_MeditateCultivation : JobDriver
    {
        private const int MeditationDuration = 6000; // 100 seconds in ticks

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Go to the meditation spot
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            // Start meditation
            Toil meditate = new Toil();
            meditate.tickAction = delegate
            {
                // Just meditate - Qi regeneration handled in CultivationData.Tick()
                // Show meditation effects
                if (pawn.IsHashIntervalTick(120))
                {
                    FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, 0.3f);
                }
            };
            meditate.defaultCompleteMode = ToilCompleteMode.Never; // Continue until interrupted
            meditate.WithProgressBar(TargetIndex.A, () => 
            {
                var comp = pawn.GetComp<CultivationComp>();
                return comp?.cultivationData?.currentQi / comp?.cultivationData?.maxQi ?? 0f;
            });
            meditate.AddFinishAction(delegate
            {
                // Give small mood bonus after meditation
                pawn.needs.mood?.thoughts?.memories?.TryGainMemory(TuTienDefOf.MeditatedCultivation);
            });
            yield return meditate;
        }
    }

    public class CultivationJobExtension : DefModExtension
    {
        public float cultivationPowerMultiplier = 1f;
    }
}
