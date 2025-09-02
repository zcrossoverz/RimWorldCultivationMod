using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.Abilities
{
    public class AbilityEffect_Heal
    {
        public float amount = 50f;
        
        public void Apply(CultivationAbilityDef abilityDef, Pawn caster, LocalTargetInfo target)
        {
            if (target.Thing is Pawn targetPawn)
            {
                // Heal the target
                var hediffSet = targetPawn.health.hediffSet;
                var injuries = new List<Hediff_Injury>();
                hediffSet.GetHediffs<Hediff_Injury>(ref injuries, null);
                
                float healingLeft = amount;
                
                foreach (var injury in injuries)
                {
                    if (healingLeft <= 0) break;
                    
                    var healAmount = Mathf.Min(healingLeft, injury.Severity);
                    injury.Heal(healAmount);
                    healingLeft -= healAmount;
                }
                
                // Visual effects
                FleckMaker.ThrowMetaIcon(targetPawn.Position, targetPawn.Map, FleckDefOf.Heart);
                
                Messages.Message($"{caster.LabelShort} healed {targetPawn.LabelShort} for {amount} HP!", MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}
