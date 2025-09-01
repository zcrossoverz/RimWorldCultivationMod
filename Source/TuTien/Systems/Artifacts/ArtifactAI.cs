using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TuTien.Systems.Artifacts
{
    /// <summary>
    /// ✅ Task 2.1: AI component for artifact auto-targeting and behavior
    /// Handles intelligent targeting, auto-attacks, and skill casting
    /// </summary>
    public class ArtifactAI
    {
        #region Data
        private CultivationArtifactComp parentComp;
        private Thing artifact;
        private CultivationArtifactData artifactData;
        
        // Cached references for performance
        private Pawn owner;
        private Map cachedMap;
        private int lastTargetScanTick = -1;
        private List<Thing> cachedTargets = new List<Thing>();
        
        // AI state
        private Thing currentTarget;
        private int lastAttackTick = -1;
        private bool isEnabled = true;
        #endregion

        #region Constructor & Setup
        public ArtifactAI(CultivationArtifactComp comp)
        {
            parentComp = comp;
            artifact = comp.parent;
            artifactData = comp.ArtifactData;
        }
        
        public void Cleanup()
        {
            currentTarget = null;
            cachedTargets.Clear();
            owner = null;
        }
        #endregion

        #region Main Update
        /// <summary>
        /// Main AI update method - called every second
        /// </summary>
        public void Update()
        {
            if (!ShouldBeActive())
            {
                currentTarget = null;
                return;
            }
            
            UpdateOwnerReference();
            
            // Scan for targets periodically
            int currentTick = Find.TickManager.TicksGame;
            if (currentTick - lastTargetScanTick > 180) // Every 3 seconds
            {
                ScanForTargets();
                lastTargetScanTick = currentTick;
            }
            
            // Select and engage target
            UpdateTargeting();
            TryExecuteAutoAttack();
            
            // Handle special abilities
            TryExecuteSpecialAbilities();
        }
        
        /// <summary>Check if AI should be active</summary>
        private bool ShouldBeActive()
        {
            return isEnabled && 
                   artifact.Spawned && 
                   artifactData.IsEffectivelyActive &&
                   owner != null &&
                   !owner.Downed &&
                   !owner.Dead;
        }
        
        /// <summary>Update cached owner reference</summary>
        private void UpdateOwnerReference()
        {
            var currentOwner = parentComp.EquippedPawn;
            if (owner != currentOwner)
            {
                owner = currentOwner;
                cachedMap = owner?.Map;
                currentTarget = null; // Reset target when owner changes
            }
        }
        #endregion

        #region Targeting System
        /// <summary>Scan area for valid targets</summary>
        private void ScanForTargets()
        {
            cachedTargets.Clear();
            
            if (owner == null || cachedMap == null) return;
            
            float detectionRadius = artifactData.artifactDef?.detectionRadius ?? 12f;
            
            // Find all potential targets in range
            var potentialTargets = GenRadial.RadialDistinctThingsAround(
                owner.Position, cachedMap, detectionRadius, false)
                .Where(IsValidTarget)
                .ToList();
            
            cachedTargets.AddRange(potentialTargets);
            
            // Sort by priority
            cachedTargets.Sort(CompareTargetPriority);
        }
        
        /// <summary>Check if a thing is a valid target</summary>
        private bool IsValidTarget(Thing target)
        {
            if (target == owner || target == artifact) return false;
            
            // Check if it's a hostile pawn
            if (target is Pawn pawn)
            {
                return pawn.HostileTo(owner) && 
                       !pawn.Downed && 
                       !pawn.Dead &&
                       GenSight.LineOfSight(owner.Position, pawn.Position, cachedMap, true);
            }
            
            // Check if it's a hostile building/turret
            if (target is Building building)
            {
                return building.HostileTo(owner) &&
                       GenSight.LineOfSight(owner.Position, building.Position, cachedMap, true);
            }
            
            return false;
        }
        
        /// <summary>Compare target priority for sorting</summary>
        private int CompareTargetPriority(Thing a, Thing b)
        {
            // Prioritize by distance (closer = higher priority)
            float distA = owner.Position.DistanceTo(a.Position);
            float distB = owner.Position.DistanceTo(b.Position);
            
            // Prioritize pawns over buildings
            int typeA = a is Pawn ? 0 : 1;
            int typeB = b is Pawn ? 0 : 1;
            
            if (typeA != typeB) return typeA.CompareTo(typeB);
            
            return distA.CompareTo(distB);
        }
        
        /// <summary>Update current target selection</summary>
        private void UpdateTargeting()
        {
            // Validate current target
            if (currentTarget != null && !IsValidTarget(currentTarget))
            {
                currentTarget = null;
            }
            
            // Select new target if needed
            if (currentTarget == null && cachedTargets.Count > 0)
            {
                currentTarget = cachedTargets.FirstOrDefault();
            }
        }
        #endregion

        #region Combat Actions
        /// <summary>Try to execute auto-attack on current target</summary>
        private void TryExecuteAutoAttack()
        {
            if (currentTarget == null || !CanAttackTarget(currentTarget)) return;
            
            int currentTick = Find.TickManager.TicksGame;
            if (currentTick - lastAttackTick < artifactData.cooldownTicks) return;
            
            // Check Qi cost
            if (!artifactData.HasEnoughQi(artifactData.qiCostPerAttack))
            {
                // Try to absorb Qi first
                if (!artifactData.TryAbsorbQiFromPawn(artifactData.qiCostPerAttack * 2f))
                {
                    return; // Not enough Qi
                }
            }
            
            ExecuteAttack(currentTarget);
            lastAttackTick = currentTick;
        }
        
        /// <summary>Check if we can attack a target</summary>
        private bool CanAttackTarget(Thing target)
        {
            if (target == null || owner == null) return false;
            
            float distance = owner.Position.DistanceTo(target.Position);
            return distance <= artifactData.range;
        }
        
        /// <summary>Execute an attack on the target</summary>
        private void ExecuteAttack(Thing target)
        {
            // Consume Qi
            artifactData.TryConsumeQi(artifactData.qiCostPerAttack);
            
            // Create attack effect
            CreateAttackEffect(target);
            
            // Deal damage if target is a pawn
            if (target is Pawn targetPawn)
            {
                DealDamageToTarget(targetPawn);
            }
            
            Log.Message($"[TuTien] {artifact.def.defName} auto-attacked {target.LabelShort}");
        }
        
        /// <summary>Create visual attack effect</summary>
        private void CreateAttackEffect(Thing target)
        {
            // Create projectile or beam effect
            if (owner?.Map != null)
            {
                // TODO: Create proper visual effects in Phase 3
                // For now, just log the attack
                Log.Message($"[TuTien] Artifact attack effect: {owner.Position} -> {target.Position}");
            }
        }
        
        /// <summary>Deal damage to target pawn</summary>
        private void DealDamageToTarget(Pawn target)
        {
            float damage = artifactData.EffectiveDamage;
            
            // Apply accuracy check
            if (Rand.Value > artifactData.accuracy)
            {
                Log.Message($"[TuTien] Artifact attack missed {target.LabelShort}");
                return;
            }
            
            // Critical hit check
            bool isCritical = Rand.Value <= artifactData.criticalChance;
            if (isCritical)
            {
                damage *= 2f;
                Log.Message($"[TuTien] Critical hit on {target.LabelShort}!");
            }
            
            // Create damage info
            var damageInfo = new DamageInfo(
                DamageDefOf.Cut, // TODO: Make this configurable
                damage,
                0f, // Armor penetration
                -1f, // Angle
                owner, // Instigator
                null, // Hit part
                artifact.def // Weapon def
            );
            
            target.TakeDamage(damageInfo);
        }
        #endregion

        #region Special Abilities
        /// <summary>Try to execute special artifact abilities</summary>
        private void TryExecuteSpecialAbilities()
        {
            if (artifactData.artifactDef?.autoSkills == null) return;
            
            foreach (string skillDefName in artifactData.artifactDef.autoSkills)
            {
                TryExecuteSkill(skillDefName);
            }
        }
        
        /// <summary>Try to execute a specific skill</summary>
        private void TryExecuteSkill(string skillDefName)
        {
            // Check cooldown
            int currentTick = Find.TickManager.TicksGame;
            if (artifactData.skillCooldowns.TryGetValue(skillDefName, out int cooldownEnd) && 
                currentTick < cooldownEnd)
            {
                return;
            }
            
            // TODO: Implement skill execution in Phase 3
            // This will integrate with the existing skill system
            
            // Set cooldown (placeholder - 30 seconds)
            artifactData.skillCooldowns[skillDefName] = currentTick + 1800;
        }
        #endregion

        #region Public Interface
        /// <summary>Enable or disable AI</summary>
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            if (!enabled)
            {
                currentTarget = null;
            }
        }
        
        /// <summary>Force target a specific thing</summary>
        public void ForceTarget(Thing target)
        {
            if (IsValidTarget(target))
            {
                currentTarget = target;
            }
        }
        
        /// <summary>Get current target for UI display</summary>
        public Thing GetCurrentTarget() => currentTarget;
        
        /// <summary>Get number of available targets</summary>
        public int GetAvailableTargetCount() => cachedTargets.Count;
        
        /// <summary>Check if AI is currently enabled</summary>
        public bool IsEnabled() => isEnabled;
        #endregion
    }
}
