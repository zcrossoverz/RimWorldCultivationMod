using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.Abilities
{
    public class SwordQiProjectile : Projectile
    {
        private int hitCount = 0;
        private const int maxHits = 5;
        
        protected override void Tick()
        {
            base.Tick();
            
            // Check for things to damage along the path
            var cellsInRadius = GenRadial.RadialCellsAround(Position, 1.9f, true);
            
            foreach (var cell in cellsInRadius)
            {
                if (!cell.InBounds(Map)) continue;
                
                var things = cell.GetThingList(Map);
                foreach (var thing in things)
                {
                    if (thing == launcher) continue; // Don't hit the caster
                    
                    // Hit pawns and structures
                    if (thing is Pawn pawn || thing.def.category == ThingCategory.Building)
                    {
                        DamageTarget(thing);
                        hitCount++;
                        
                        if (hitCount >= maxHits)
                        {
                            Explode();
                            return;
                        }
                    }
                }
            }
        }
        
        private void DamageTarget(Thing target)
        {
            var damageInfo = new DamageInfo(
                def.projectile.damageDef, 
                def.projectile.GetDamageAmount(launcher, null),
                def.projectile.GetArmorPenetration(launcher, null),
                0f, // angle
                launcher,
                null,
                equipmentDef,
                DamageInfo.SourceCategory.ThingOrUnknown,
                intendedTarget.Thing
            );
            
            target.TakeDamage(damageInfo);
            
            // Visual effect
            FleckMaker.ThrowMicroSparks(target.DrawPos, Map);
        }
        
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (hitThing != null && hitThing != launcher)
            {
                DamageTarget(hitThing);
                hitCount++;
            }
            
            if (hitCount >= maxHits || def.projectile.explosionRadius > 0)
            {
                Explode();
            }
            else
            {
                // Continue flying through
                return;
            }
        }
        
        private void Explode()
        {
            GenExplosion.DoExplosion(
                Position,
                Map,
                2f,
                DamageDefOf.Bomb,
                launcher,
                100
            );
            
            Destroy();
        }
    }
}
