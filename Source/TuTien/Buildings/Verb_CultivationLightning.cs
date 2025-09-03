using RimWorld;
using UnityEngine;
using Verse;

namespace TuTien
{
    public class Verb_CultivationLightning : Verb_Shoot
    {
        protected override bool TryCastShot()
        {
            if (this.caster is Building_CultivationTurret turret)
            {
                var qiComp = turret.QiComp;
                if (qiComp == null || !qiComp.CanShoot)
                {
                    return false;
                }
                
                // Consume Qi
                qiComp.TryConsumeQi(qiComp.QiPerShot);
            }
            
            bool result = base.TryCastShot();
            
            // Add lightning visual effects after shot
            if (result && this.currentTarget.IsValid)
            {
                Vector3 targetPos = this.currentTarget.Cell.ToVector3();
                FleckMaker.ThrowLightningGlow(targetPos, this.caster.Map, 2f);
                
                // Additional electric effect on target if it's a pawn
                if (this.currentTarget.Thing is Pawn targetPawn)
                {
                    // Electric stun effect will be applied by damage
                    FleckMaker.ThrowMicroSparks(targetPos, this.caster.Map);
                }
            }
            
            return result;
        }
    }
}
