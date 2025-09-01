using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TuTien.Abilities
{
    /// <summary>
    /// Component that gives abilities to the wearer when equipment is equipped
    /// Used for Pháp Bảo (cultivation artifacts)
    /// </summary>
    public class CompAbilityGiver : ThingComp
    {
        public CompAbilityGiverProperties Props => (CompAbilityGiverProperties)props;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            
            var abilityComp = pawn.GetComp<CompAbilityUser>();
            if (abilityComp != null && Props.abilitiesToGive != null)
            {
                foreach (var abilityDefName in Props.abilitiesToGive)
                {
                    var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamed(abilityDefName, false);
                    if (abilityDef != null)
                    {
                        abilityComp.AddAbility(abilityDef);
                    }
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            
            var abilityComp = pawn.GetComp<CompAbilityUser>();
            if (abilityComp != null && Props.abilitiesToGive != null)
            {
                foreach (var abilityDefName in Props.abilitiesToGive)
                {
                    var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamed(abilityDefName, false);
                    if (abilityDef != null)
                    {
                        abilityComp.RemoveAbility(abilityDef);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Properties for CompAbilityGiver
    /// </summary>
    public class CompAbilityGiverProperties : CompProperties
    {
        public List<string> abilitiesToGive = new List<string>();

        public CompAbilityGiverProperties()
        {
            compClass = typeof(CompAbilityGiver);
        }
    }
}
