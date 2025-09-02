using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using TuTien;

namespace TuTien.Abilities
{
    /// <summary>
    /// Instance of a cultivation ability that can be cast
    /// With proper cooldown visual like technique skills
    /// </summary>
    public class CultivationAbility
    {
        public TuTien.CultivationAbilityDef def;
        public CompAbilityUser comp;

        public CultivationAbility(TuTien.CultivationAbilityDef def, CompAbilityUser comp)
        {
            this.def = def;
            this.comp = comp;
    }

    public bool CanCast => comp.CanCastAbility(def);

    public int CooldownRemaining => comp.GetCooldownRemaining(def.defName);

    public void TryCast(LocalTargetInfo target)
    {
        if (!CanCast) 
        {
            Log.Message($"[DEBUG] Cannot cast - CanCast returned false");
            Messages.Message("Cannot cast ability - insufficient Qi or on cooldown", MessageTypeDefOf.RejectInput);
            return;
        }

        // Consume Qi if on a pawn
        if (comp.parent is Pawn pawn)
        {
            var cultivationData = pawn.GetCultivationData();
            if (cultivationData != null)
            {
                cultivationData.currentQi -= def.qiCost;
                Messages.Message($"Used {def.qiCost} Qi. Remaining: {cultivationData.currentQi:F1}", MessageTypeDefOf.NeutralEvent);
            }
        }

        // Apply effects
        ApplyEffects(target);

        // Start cooldown
        comp.StartCooldown(def);

        Messages.Message($"Successfully cast {def.abilityLabel}!", MessageTypeDefOf.PositiveEvent);
    }

        private void ApplyEffects(LocalTargetInfo target)
        {
            Log.Message($"[DEBUG] ApplyEffects called with {def.effects?.Count ?? 0} effects");

            if (def.effects == null) return;

            foreach (var effect in def.effects)
            {
                // Handle different effect types using reflection/casting
                if (effect is AbilityEffect_LaunchProjectile projectileEffect)
                {
                    projectileEffect.Apply(def, comp.parent as Pawn, target);
                }
                else if (effect is AbilityEffect_Heal healEffect)
                {
                    healEffect.Apply(def, comp.parent as Pawn, target);
                }
                else if (effect is AbilityEffect_CorpseRevival revivalEffect)
                {
                    Log.Message($"[DEBUG] Applying corpse revival effect");
                    revivalEffect.Apply(def, comp.parent as Pawn, target);
                }
                else
                {
                    Log.Warning($"Unknown ability effect type: {effect.GetType()}");
                }
            }
        Log.Message($"[DEBUG] ApplyEffects completed");
    }

    public Command GetCommand()
    {
        var command = new Command_CastAbilityWithCooldown
        {
            ability = this,
            defaultLabel = def.abilityLabel ?? def.label,
            defaultDesc = GetAbilityDescription(),
            icon = ContentFinder<Texture2D>.Get(def.iconPath, false) ?? BaseContent.BadTex,
            Disabled = !CanCast  // Set disabled based on ability state
        };
        
        // Set targeting parameters based on ability type
        if (def.category == "Combat" || def.category == "Forbidden")
        {
            command.targetingParams = GetTargetingParameters();
            command.action = (target) => {
                Log.Message($"[DEBUG] Command action triggered! Target: {target}");
                TryCast(target);
            };
        }
        else
        {
            // Support abilities self-cast
            command.selfCastAction = () => TryCastSimple();
        }

        return command;
    }
    
    private TargetingParameters GetTargetingParameters()
    {
        var targeting = new TargetingParameters();

            // Configure based on ability defName for special cases
            if (def.defName == "TuTien_CorpseRevival")
            {
                // Special targeting for corpse revival
                targeting.canTargetPawns = true;
                targeting.canTargetSelf = false;
                targeting.canTargetBuildings = true;
                targeting.canTargetItems = true;
                // targeting.canTargetLocations = true;

                // Add validator to check what's being targeted
                        targeting.validator = (TargetInfo target) => 
                        {
                            // Accept anything for now
                            return true;
                        };
            }
            else
            {
                // Standard combat targeting
                targeting.canTargetPawns = true;
                targeting.canTargetSelf = false;
                targeting.canTargetBuildings = false;
                targeting.canTargetItems = false;
            }
        
        return targeting;
    }

    private string GetAbilityDescription()
    {
        var desc = def.abilityDescription ?? def.description;
        var cooldownText = CooldownRemaining > 0 ? $"\n\nCooldown: {CooldownRemaining} ticks remaining" : "";
        return $"{desc}\n\nQi Cost: {def.qiCost}{cooldownText}";
    }

    private void TryCastSimple()
    {
        if (!CanCast) 
        {
            Messages.Message("Cannot cast ability - insufficient Qi or on cooldown", MessageTypeDefOf.RejectInput);
            return;
        }

        // Cast on self
        if (comp.parent is Pawn caster)
        {
            TryCast(new LocalTargetInfo(caster));
        }
    }
}
}
