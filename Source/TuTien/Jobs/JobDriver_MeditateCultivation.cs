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
            // Go to target position (meditation spot or current position)
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            // Start meditation
            Toil meditate = new Toil();
            meditate.tickAction = delegate
            {
                // Add cultivation points during meditation
                var comp = pawn.GetComp<CultivationComp>();
                if (comp?.cultivationData != null)
                {
                    // Grant cultivation progress - faster when meditating
                    float baseGain = 0.1f; // Base cultivation per tick
                    float meditationBonus = 10.0f; // 2x speed when meditating
                    pawn.AddCultivationPoints(baseGain * meditationBonus);
                }

                // Show meditation effects
                if (pawn.IsHashIntervalTick(120))
                {
                    FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, 0.3f);
                }
            };
            meditate.defaultCompleteMode = ToilCompleteMode.Never; // Continue until interrupted
            
            // Show progress bar for cultivation progress
            meditate.WithProgressBar(TargetIndex.A, () => 
            {
                var comp = pawn.GetComp<CultivationComp>();
                if (comp?.cultivationData == null) return 0f;
                
                float required = comp.cultivationData.GetRequiredCultivationPoints();
                return required > 0 ? comp.cultivationData.cultivationPoints / required : 0f;
            });
            
            meditate.AddFinishAction(delegate
            {
                // Give small mood bonus after meditation (if ThoughtDef exists)
                // pawn.needs.mood?.thoughts?.memories?.TryGainMemory(TuTienDefOf.MeditatedCultivation);
            });
            yield return meditate;
        }
    }

    public class CultivationJobExtension : DefModExtension
    {
        public float cultivationPowerMultiplier = 1f;
    }
}
