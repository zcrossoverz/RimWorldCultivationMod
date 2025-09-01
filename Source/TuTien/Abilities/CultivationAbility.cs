using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using TuTien;

/// <summary>
/// Instance of a cultivation ability that can be cast
/// With proper cooldown visual like technique skills
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

    public int CooldownRemaining => comp.GetCooldownRemaining(def.defName);

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
            // Combat abilities use targeting with cooldown
            return new Command_CastAbilityWithCooldown
            {
                ability = this,
                defaultLabel = def.abilityLabel ?? def.label,
                defaultDesc = GetAbilityDescription(),
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
            // Support abilities self-cast with cooldown
            return new Command_CastAbilityWithCooldown
            {
                ability = this,
                defaultLabel = def.abilityLabel ?? def.label,
                defaultDesc = GetAbilityDescription(),
                icon = ContentFinder<Texture2D>.Get(def.iconPath, false) ?? BaseContent.BadTex,
                selfCastAction = () => TryCastSimple()
            };
        }
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

/// <summary>
/// Custom command class with cooldown visual like technique skills
/// </summary>
public class Command_CastAbilityWithCooldown : Command_Target
{
    public CultivationAbility ability;
    public System.Action selfCastAction;

    public override void ProcessInput(Event ev)
    {
        if (selfCastAction != null)
        {
            // Self-cast ability
            selfCastAction.Invoke();
        }
        else
        {
            // Targeted ability
            base.ProcessInput(ev);
        }
    }

    public override bool InheritInteractionsFrom(Gizmo other)
    {
        return false;
    }

    public override void GizmoUpdateOnMouseover()
    {
        base.GizmoUpdateOnMouseover();
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var result = base.GizmoOnGUI(topLeft, maxWidth, parms);
        
        // Draw cooldown overlay like technique skills
        if (ability.CooldownRemaining > 0)
        {
            var rect = new Rect(topLeft.x, topLeft.y, 75f, 75f);
            var progress = 1f - (float)ability.CooldownRemaining / ability.def.cooldownTicks;
            
            // Draw cooldown overlay
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            Widgets.DrawTextureFitted(rect, BaseContent.GreyTex, progress);
            GUI.color = Color.white;
            
            // Draw cooldown text
            var cooldownText = (ability.CooldownRemaining / 60f).ToString("F1") + "s";
            var textRect = new Rect(rect.x, rect.y + rect.height - 20f, rect.width, 20f);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(textRect, cooldownText);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }
        
        // Disable if can't cast
        if (!ability.CanCast)
        {
            GUI.color = new Color(1f, 1f, 1f, 0.4f);
        }
        
        return result;
    }
}
