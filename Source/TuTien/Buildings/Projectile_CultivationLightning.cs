using RimWorld;
using UnityEngine;
using Verse;

namespace TuTien
{
    public class Projectile_CultivationLightning : Bullet
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            // Lightning effect
            if (this.Map != null)
            {
                // Create lightning visual effect
                FleckMaker.ThrowLightningGlow(this.Position.ToVector3(), this.Map, 2f);
                
                // Additional electric damage
                if (hitThing is Pawn pawn)
                {
                    // Electric stun effect
                    pawn.stances?.stunner?.StunFor(60, this.launcher);
                }
            }
            
            base.Impact(hitThing, blockedByShield);
        }
    }
}
