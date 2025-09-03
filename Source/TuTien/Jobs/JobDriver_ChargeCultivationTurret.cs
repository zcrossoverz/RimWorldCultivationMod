using System.Collections.Generic;
using RimWorld;
using TuTien.Core;
using Verse;
using Verse.AI;

namespace TuTien
{
    public class JobDriver_ChargeCultivationTurret : JobDriver
    {
        private Building_CultivationTurret turret => (Building_CultivationTurret)this.job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.turret, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            
            Toil chargeToil = new Toil();
            chargeToil.tickAction = delegate()
            {
                var cultivationComp = pawn.TryGetComp<CultivationComp>();
                if (cultivationComp == null)
                {
                    this.EndJobWith(JobCondition.Incompletable);
                    return;
                }
                
                var qiComp = turret.QiComp;
                if (qiComp == null || qiComp.IsFull)
                {
                    this.EndJobWith(JobCondition.Succeeded);
                    return;
                }
                
                // Transfer Qi from cultivator to turret
                if (this.ticksLeftThisToil % 60 == 0) // Every 60 ticks (1 second)
                {
                    int qiToTransfer = UnityEngine.Mathf.Min(20, qiComp.MaxQi - qiComp.CurrentQi);
                    
                    // Check if cultivator has enough Qi
                    float cultivatorQi = cultivationComp.cultivationData?.currentQi ?? 0f;
                    if (qiToTransfer > cultivatorQi)
                    {
                        qiToTransfer = (int)cultivatorQi;
                    }
                    
                    if (qiToTransfer > 0 && cultivationComp.cultivationData?.currentRealm != CultivationRealm.Mortal)
                    {
                        bool success = qiComp.TryAddQi(qiToTransfer);
                        if (success)
                        {
                            // Deduct Qi from cultivator
                            cultivationComp.cultivationData.currentQi -= qiToTransfer;
                            if (cultivationComp.cultivationData.currentQi < 0)
                                cultivationComp.cultivationData.currentQi = 0;
                                
                            // Log.Message($"[TuTien] {pawn.LabelShort} nạp {qiToTransfer} Qi vào turret. Turret Qi: {qiComp.CurrentQi}/{qiComp.MaxQi}, Cultivator Qi: {cultivationComp.cultivationData.currentQi:F0}/{cultivationComp.cultivationData.maxQi}");
                        }
                        
                        // Visual effect
                        FleckMaker.ThrowDustPuff(turret.Position.ToVector3(), turret.Map, 1f);
                        
                        // Stop if cultivator runs out of Qi
                        if (cultivationComp.cultivationData.currentQi <= 0)
                        {
                            this.EndJobWith(JobCondition.Succeeded);
                            return;
                        }
                    }
                }
            };
            chargeToil.defaultCompleteMode = ToilCompleteMode.Never;
            chargeToil.WithProgressBar(TargetIndex.A, () => turret.QiComp?.QiPct ?? 0f);
            chargeToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return chargeToil;
        }
    }
}
