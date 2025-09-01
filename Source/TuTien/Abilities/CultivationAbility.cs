using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using TuTien;

/// <summary>
/// Instance of a cultivation ability that can be cast
/// Global namespace for easy access
/// </summary>
public class CultivationAbility
{
    public CultivationAbilityDef def;
    public TuTien.Abilities.CompAbilityUser comp;

    public CultivationAbility(CultivationAbilityDef def, TuTien.Abilities.CompAbilityUser comp)
    {
        this.def = def;
        this.comp = comp;
    }

    public bool CanCast => comp.CanCastAbility(def);

    public void TryCast(LocalTargetInfo target)
    {
        if (!CanCast) return;

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
        if (def.effects == null) return;

        foreach (var effect in def.effects)
        {
            switch (effect.effectType.ToLower())
            {
                case "damage":
                    ApplyDamage(target, effect);
                    break;
                case "heal":
                    ApplyHealing(target, effect);
                    break;
            }
        }
    }

    private void ApplyDamage(LocalTargetInfo target, AbilityEffectDef effect)
    {
        if (target.Thing is Pawn targetPawn)
        {
            var damageTypeDef = DefDatabase<DamageDef>.GetNamed(effect.damageType, false) ?? DamageDefOf.Cut;
            var damageInfo = new DamageInfo(damageTypeDef, effect.magnitude);
            targetPawn.TakeDamage(damageInfo);
            Messages.Message($"Dealt {effect.magnitude} {effect.damageType} damage to {targetPawn.LabelShort}!", MessageTypeDefOf.NeutralEvent);
        }
    }

    private void ApplyHealing(LocalTargetInfo target, AbilityEffectDef effect)
    {
        if (target.Thing is Pawn targetPawn)
        {
            // Get injuries using proper RimWorld 1.6 API
            var injuryList = new List<Hediff_Injury>();
            targetPawn.health.hediffSet.GetHediffs(ref injuryList, (Hediff_Injury h) => h.CanHealNaturally());
            
            if (injuryList.Any())
            {
                var injury = injuryList.RandomElement();
                injury.Heal(effect.magnitude);
                Messages.Message($"Healed {effect.magnitude} damage on {targetPawn.LabelShort}!", MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message($"{targetPawn.LabelShort} has no wounds to heal!", MessageTypeDefOf.NeutralEvent);
            }
        }
    }

    public Command GetCommand()
    {
        if (def.category == "Combat")
        {
            // Combat abilities use targeting
            return new Command_Target
            {
                defaultLabel = def.abilityLabel ?? def.label,
                defaultDesc = $"{def.abilityDescription ?? def.description}\n\nQi Cost: {def.qiCost}\nCooldown: {def.cooldownTicks} ticks",
                icon = ContentFinder<Texture2D>.Get(def.iconPath, false) ?? BaseContent.BadTex,
                targetingParams = new TargetingParameters
                {
                    canTargetPawns = true,
                    canTargetSelf = false,
                    canTargetBuildings = false,
                    canTargetItems = false
                },
                action = (target) => TryCast(target)
            };
        }
        else
        {
            // Support abilities self-cast
            return new Command_Action
            {
                defaultLabel = def.abilityLabel ?? def.label,
                defaultDesc = $"{def.abilityDescription ?? def.description}\n\nQi Cost: {def.qiCost}\nCooldown: {def.cooldownTicks} ticks",
                icon = ContentFinder<Texture2D>.Get(def.iconPath, false) ?? BaseContent.BadTex,
                action = () => TryCastSimple()
            };
        }
    }

    private void TryCastSimple()
    {
        if (!CanCast) 
        {
            Messages.Message("Cannot cast ability - insufficient Qi or on cooldown", MessageTypeDefOf.RejectInput);
            return;
        }

        // Cast on self for now (simple implementation)
        if (comp.parent is Pawn caster)
        {
            TryCast(new LocalTargetInfo(caster));
        }
    }
}
