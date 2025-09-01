using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TuTien.Abilities
{
    /// <summary>
    /// Component that gives entities the ability to use cultivation abilities
    /// Can be attached to Pawns, Items, or Buildings
    /// </summary>
    public class CompAbilityUser : ThingComp
    {
        private List<CultivationAbility> abilities = new List<CultivationAbility>();
        private Dictionary<string, int> cooldowns = new Dictionary<string, int>();

        public CompAbilityUserProperties Props => (CompAbilityUserProperties)props;

        public List<CultivationAbility> Abilities => abilities;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            // Initialize abilities from properties
            if (Props.initialAbilities != null)
            {
                foreach (var abilityDefName in Props.initialAbilities)
                {
                    var abilityDef = DefDatabase<CultivationAbilityDef>.GetNamed(abilityDefName, false);
                    if (abilityDef != null)
                    {
                        AddAbility(abilityDef);
                    }
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            
            // Reduce cooldowns
            var keys = cooldowns.Keys.ToList();
            foreach (var key in keys)
            {
                if (cooldowns[key] > 0)
                {
                    cooldowns[key]--;
                }
            }
        }

        public void AddAbility(CultivationAbilityDef abilityDef)
        {
            if (abilities.Any(a => a.def == abilityDef)) return;
            
            var ability = new CultivationAbility(abilityDef, this);
            abilities.Add(ability);
        }

        public void RemoveAbility(CultivationAbilityDef abilityDef)
        {
            abilities.RemoveAll(a => a.def == abilityDef);
        }

        public bool CanCastAbility(CultivationAbilityDef abilityDef)
        {
            // Check cooldown
            if (cooldowns.ContainsKey(abilityDef.defName) && cooldowns[abilityDef.defName] > 0)
                return false;

            // Check Qi cost (if this is on a pawn)
            if (parent is Pawn pawn)
            {
                var cultivationComp = pawn.GetComp<CultivationComp>();
                if (cultivationComp?.cultivationData?.currentQi < abilityDef.qiCost)
                    return false;
            }

            return true;
        }

        public void StartCooldown(CultivationAbilityDef abilityDef)
        {
            cooldowns[abilityDef.defName] = abilityDef.cooldownTicks;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref cooldowns, "cooldowns", LookMode.Value, LookMode.Value);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (abilities == null) abilities = new List<CultivationAbility>();
            }
        }
    }

    /// <summary>
    /// Properties for CompAbilityUser
    /// </summary>
    public class CompAbilityUserProperties : CompProperties
    {
        public List<string> initialAbilities = new List<string>();

        public CompAbilityUserProperties()
        {
            compClass = typeof(CompAbilityUser);
        }
    }
}
