using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TuTien.SkillWorkers
{
    public abstract class CultivationSkillWorker : ICultivationSkillWorker
    {
        public CultivationSkillDef def { get; set; }


        /// <summary>
        /// Main execution method - handles costs and cooldown automatically
        /// </summary>
        public virtual void Execute(Pawn pawn, CultivationSkillDef skill)
        {
            // Execute the skill effect first
            ExecuteSkillEffect(pawn, skill);
            
            // Then apply costs and cooldown
            ApplyCostsAndCooldown(pawn, skill);
        }

        /// <summary>
        /// Override this method to implement skill effects
        /// </summary>
        protected abstract void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill);

        /// <summary>
        /// Apply cooldown and costs after skill execution
        /// </summary>
        protected virtual void ApplyCostsAndCooldown(Pawn pawn, CultivationSkillDef skill)
        {
            var comp = pawn.GetComp<CultivationComp>();
            var cultivationData = comp?.cultivationData;
            
            if (cultivationData != null)
            {
                // Apply Qi cost
                cultivationData.currentQi -= skill.qiCost;
                
                // Apply cooldown
                if (skill.cooldownHours > 0)
                {
                    int cooldownTicks = Mathf.RoundToInt(skill.cooldownHours * GenDate.TicksPerHour);
                    cultivationData.skillCooldowns[skill.defName] = cooldownTicks;
                    
                    // Also set in SkillManager for sync
                    var skillManager = comp.GetSkillManager();
                    if (skillManager != null)
                    {
                        skillManager.SetSkillCooldown(skill.defName, cooldownTicks);
                    }
                    
                    Log.Message($"[TuTien] Applied costs & cooldown: {skill.defName} = {skill.cooldownHours}h ({cooldownTicks} ticks)");
                }
            }
        }
    }

    // Qi Condensation Stage 1 Skills
    public class QiForceSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // +10/15/20% melee damage for 5 seconds
            var comp = pawn.GetComp<CultivationComp>();
            int stage = comp?.cultivationData?.currentStage ?? 1;

            float damageBonus = 0.1f + (stage - 1) * 0.05f;

            var hediff = HediffMaker.MakeHediff(TuTienDefOf.QiForceHediff, pawn);
            hediff.Severity = damageBonus;
            pawn.health.AddHediff(hediff);

            // Visual effect
            FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, 1f);

            // User message
            Messages.Message($"{pawn.Name.ToStringShort} used Qi Punch!", MessageTypeDefOf.PositiveEvent);
            
            // Apply costs and cooldown
            ApplyCostsAndCooldown(pawn, skill);
        }
    }

    public class QiWaveSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // Ranged qi projectile
            var comp = pawn.GetComp<CultivationComp>();
            int stage = comp?.cultivationData?.currentStage ?? 1;
            
            float damage = 15f + (stage - 1) * 10f;
            
            var target = GetNearestHostile(pawn);
            if (target != null)
            {
                // Simple damage effect instead of projectile for now
                var dinfo = new DamageInfo(DamageDefOf.Blunt, damage, 0f, -1f, pawn);
                target.TakeDamage(dinfo);
                FleckMaker.ThrowDustPuff(target.Position, pawn.Map, 1f);
            }
        }

        private Pawn GetNearestHostile(Pawn caster)
        {
            return caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p.HostileTo(caster) && p.Position.DistanceTo(caster.Position) <= 20f)
                .OrderBy(p => p.Position.DistanceTo(caster.Position))
                .FirstOrDefault();
        }
    }

    public class QiAbsorptionSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // Damage reduction for 10 seconds
            var comp = pawn.GetComp<CultivationComp>();
            int stage = comp?.cultivationData?.currentStage ?? 1;
            
            float reduction = 0.1f + (stage - 1) * 0.05f;
            
            var hediff = HediffMaker.MakeHediff(TuTienDefOf.QiShieldHediff, pawn);
            hediff.Severity = reduction;
            pawn.health.AddHediff(hediff);
            
            // Visual effect
            FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, 1f);
            
            // User message
            Messages.Message($"{pawn.Name.ToStringShort} activated Qi Shield!", MessageTypeDefOf.PositiveEvent);
        }
    }

    // Foundation Establishment Skills
    public class QiBarrierSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // Block next 1-3 attacks
            var comp = pawn.GetComp<CultivationComp>();
            int stage = comp?.cultivationData?.currentStage ?? 1;
            
            int blockedAttacks = stage;
            
            var hediff = HediffMaker.MakeHediff(TuTienDefOf.QiBarrierHediff, pawn);
            hediff.Severity = blockedAttacks;
            pawn.health.AddHediff(hediff);
        }
    }

    public class QiSwordSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // AOE sword qi attack
            var comp = pawn.GetComp<CultivationComp>();
            int stage = comp?.cultivationData?.currentStage ?? 1;
            
            float damage = 25f + (stage - 1) * 15f;
            float radius = 2f + (stage - 1) * 1f;
            
            var targets = GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, radius, true)
                .OfType<Pawn>()
                .Where(p => p.HostileTo(pawn));

            foreach (var target in targets)
            {
                var dinfo = new DamageInfo(DamageDefOf.Cut, damage, 0f, -1f, pawn);
                target.TakeDamage(dinfo);
                
                // Visual effect
                FleckMaker.ThrowDustPuff(target.Position, pawn.Map, 1.5f);
            }
        }
    }

    public class QiPurificationSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // Remove debuffs
            var comp = pawn.GetComp<CultivationComp>();
            int stage = comp?.cultivationData?.currentStage ?? 1;
            
            var hediffsToRemove = pawn.health.hediffSet.hediffs
                .Where(h => h.def.isBad && h.def != HediffDefOf.BloodLoss)
                .Take(stage)
                .ToList();

            foreach (var hediff in hediffsToRemove)
            {
                pawn.health.RemoveHediff(hediff);
            }

            FleckMaker.ThrowHeatGlow(pawn.Position, pawn.Map, 1f);
        }
    }

    // Golden Core Skills
    public class QiBurstSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // +50% damage for 10 seconds
            var comp = pawn.GetComp<CultivationComp>();
            int stage = comp?.cultivationData?.currentStage ?? 1;
            
            float damageBonus = 0.5f + (stage - 1) * 0.1f;
            
            var hediff = HediffMaker.MakeHediff(TuTienDefOf.QiBurstHediff, pawn);
            hediff.Severity = damageBonus;
            pawn.health.AddHediff(hediff);
        }
    }

    public class QiRestorationSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // Healing effect
            var comp = pawn.GetComp<CultivationComp>();
            int stage = comp?.cultivationData?.currentStage ?? 1;
            
            // Instant healing based on stage
            var injuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(h => h.CanHealNaturally() || h.CanHealFromTending())
                .ToList();
                
            if (injuries.Any())
            {
                // Heal some injuries
                foreach (var injury in injuries.Take(stage))
                {
                    injury.Heal(5f + stage * 2f); // Heal more based on stage
                }
                Messages.Message($"{pawn.Name.ToStringShort} used Qi Healing to mend injuries!", MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                // No injuries, restore health
                var healthHediff = pawn.health.hediffSet.hediffs
                    .FirstOrDefault(h => h.def == HediffDefOf.WoundInfection);
                    
                if (healthHediff != null)
                {
                    healthHediff.Heal(10f);
                    Messages.Message($"{pawn.Name.ToStringShort} used Qi Healing to cure illness!", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message($"{pawn.Name.ToStringShort} used Qi Healing, but has no wounds to heal.", MessageTypeDefOf.NeutralEvent);
                }
            }
            
            // Apply restoration hediff for continued healing
            var hediff = HediffMaker.MakeHediff(TuTienDefOf.QiRestorationHediff, pawn);
            hediff.Severity = 0.05f + (stage - 1) * 0.02f;
            pawn.health.AddHediff(hediff);
            
            // Visual effect
            FleckMaker.ThrowLightningGlow(pawn.Position.ToVector3(), pawn.Map, 1f);
        }
    }

    public class ElementalAttackSkillWorker : CultivationSkillWorker
    {
        protected override void ExecuteSkillEffect(Pawn pawn, CultivationSkillDef skill)
        {
            // Elemental attack based on technique
            var comp = pawn.GetComp<CultivationComp>();
            var data = comp?.cultivationData;
            if (data == null) return;

            var technique = data.knownTechniques.FirstOrDefault();
            if (technique == null) return;

            // Launch elemental projectile based on technique type
            var target = GetNearestHostile(pawn);
            if (target != null)
            {
                ThingDef projectileDef = GetProjectileForTechnique(technique);
                // Simple damage effect for now
                var dinfo = new DamageInfo(DamageDefOf.Burn, 50f, 0f, -1f, pawn);
                target.TakeDamage(dinfo);
                FleckMaker.ThrowFireGlow(target.Position.ToVector3(), pawn.Map, 1f);
            }
        }

        private ThingDef GetProjectileForTechnique(CultivationTechniqueDef technique)
        {
            // Return appropriate projectile based on technique
            switch (technique.defName)
            {
                case "SwordQiTechnique":
                    return TuTienDefOf.SwordQiProjectile;
                case "LightningTechnique":
                    return TuTienDefOf.LightningProjectile;
                case "FireTechnique":
                    return TuTienDefOf.FireProjectile;
                case "IceTechnique":
                    return TuTienDefOf.IceProjectile;
                case "EarthTechnique":
                    return TuTienDefOf.EarthProjectile;
                default:
                    return TuTienDefOf.QiWaveProjectile;
            }
        }

        private Pawn GetNearestHostile(Pawn caster)
        {
            return caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p.HostileTo(caster) && p.Position.DistanceTo(caster.Position) <= 25f)
                .OrderBy(p => p.Position.DistanceTo(caster.Position))
                .FirstOrDefault();
        }
    }
}


