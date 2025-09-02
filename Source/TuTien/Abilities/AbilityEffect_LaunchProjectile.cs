using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.Abilities
{
    public class AbilityEffect_LaunchProjectile
    {
        public ThingDef projectileDef;
        public float range = 25f;
        public bool targetGround = false;
        
        public void Apply(CultivationAbilityDef abilityDef, Pawn caster, LocalTargetInfo target)
        {
            if (projectileDef == null) return;
            
            var startPos = caster.Position.ToVector3Shifted();
            var targetPos = target.Cell.ToVector3Shifted();
            
            // Calculate direction and spawn projectile
            var direction = (targetPos - startPos).normalized;
            var angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            
            var projectile = (Projectile)GenSpawn.Spawn(projectileDef, caster.Position, caster.Map);
            projectile.Launch(caster, target, target, ProjectileHitFlags.IntendedTarget);
            
            // Visual effects
            FleckMaker.ThrowMicroSparks(caster.DrawPos, caster.Map);
        }
    }
}
