using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using TuTien.Systems.Registry;

namespace TuTien.Systems.Artifacts
{
    /// <summary>
    /// ✅ Task 2.1: Core artifact component system
    /// ThingComp that manages cultivation artifacts - handles Qi, auto-targeting, and buff application
    /// Integrates with existing RimWorld equipment system
    /// </summary>
    public class CultivationArtifactComp : ThingComp
    {
        #region Data Storage
        /// <summary>Runtime artifact data with generated stats and state</summary>
        private CultivationArtifactData _artifactData;
        
        /// <summary>AI component for auto-targeting and behavior</summary>
        private ArtifactAI _aiComponent;
        
        /// <summary>Whether component has been initialized</summary>
        private bool _isInitialized = false;
        
        /// <summary>Cached pawn reference for performance</summary>
        private Pawn _cachedEquippedPawn;
        
        /// <summary>Last tick when component was updated</summary>
        private int _lastUpdateTick = -1;
        #endregion

        #region Properties
        /// <summary>Get component properties</summary>
        public CultivationArtifactCompProperties Props => (CultivationArtifactCompProperties)props;
        
        /// <summary>Get artifact data, generating if needed</summary>
        public CultivationArtifactData ArtifactData
        {
            get
            {
                if (_artifactData == null)
                {
                    _artifactData = GenerateArtifactData();
                }
                return _artifactData;
            }
        }
        
        /// <summary>Get the pawn currently equipping this artifact</summary>
        public Pawn EquippedPawn => _cachedEquippedPawn ?? FindEquippedPawn();
        
        /// <summary>Check if artifact is currently equipped and active</summary>
        public bool IsEquippedAndActive => EquippedPawn != null && ArtifactData.IsEffectivelyActive;
        
        /// <summary>Get AI component for advanced behaviors</summary>
        public ArtifactAI AI => _aiComponent;
        #endregion

        #region RimWorld Component Lifecycle
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            if (!respawningAfterLoad)
            {
                InitializeArtifact();
            }
            else
            {
                // Post-load setup
                RestoreAfterLoad();
            }
            
            // Always create AI component
            _aiComponent = new ArtifactAI(this);
            _isInitialized = true;
            
            Log.Message($"[TuTien] Artifact component initialized: {parent.def.defName}");
        }
        
        /// <summary>
        /// ✅ Task 2.1: Smart update system following existing patterns
        /// Uses same performance optimization as CultivationComp
        /// </summary>
        public override void CompTick()
        {
            if (!_isInitialized || !parent.Spawned) return;
            
            int currentTick = Find.TickManager.TicksGame;
            
            // Smart update intervals (following CultivationComp pattern)
            if (currentTick % 30 == 0) // 0.5 second intervals
            {
                UpdateArtifact();
                _lastUpdateTick = currentTick;
            }
            
            // Qi absorption every 10 ticks (more frequent for responsiveness)
            if (currentTick % 10 == 0)
            {
                TryAbsorbQi();
            }
            
            // AI update every 60 ticks (1 second)
            if (currentTick % 60 == 0 && _aiComponent != null)
            {
                _aiComponent.Update();
            }
            
            // Buff updates every 30 ticks
            if (currentTick % 30 == 15) // Offset to spread load
            {
                UpdateBuffs();
            }
        }
        
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            // Clean up when artifact is destroyed
            if (_cachedEquippedPawn != null)
            {
                OnUnequippedBy(_cachedEquippedPawn);
            }
            
            _aiComponent?.Cleanup();
            base.PostDestroy(mode, previousMap);
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            
            Scribe_Deep.Look(ref _artifactData, "artifactData");
            Scribe_Values.Look(ref _isInitialized, "isInitialized");
            Scribe_Values.Look(ref _lastUpdateTick, "lastUpdateTick");
            Scribe_References.Look(ref _cachedEquippedPawn, "cachedEquippedPawn");
            
            // AI component will be recreated in PostSpawnSetup
        }
        #endregion

        #region Artifact Management
        /// <summary>Initialize new artifact with generated stats</summary>
        private void InitializeArtifact()
        {
            if (Props.artifactDef != null)
            {
                _artifactData = new CultivationArtifactData(Props.artifactDef);
            }
            else
            {
                // Fallback: try to find artifact def by thing def name
                var artifactDef = CultivationRegistry.GetArtifactDef(parent.def.defName);
                if (artifactDef != null)
                {
                    _artifactData = new CultivationArtifactData(artifactDef);
                }
                else
                {
                    Log.Warning($"[TuTien] No artifact definition found for {parent.def.defName}");
                    _artifactData = new CultivationArtifactData(); // Create default data
                }
            }
        }
        
        /// <summary>Generate artifact data if not already present</summary>
        private CultivationArtifactData GenerateArtifactData()
        {
            if (Props.artifactDef != null)
            {
                return ArtifactGenerator.GenerateArtifact(Props.artifactDef);
            }
            
            // Create default artifact data
            Log.Warning($"[TuTien] Creating default artifact data for {parent.def.defName}");
            return new CultivationArtifactData();
        }
        
        /// <summary>Restore state after loading from save</summary>
        private void RestoreAfterLoad()
        {
            if (_artifactData != null)
            {
                _artifactData.equippedBy = _cachedEquippedPawn;
                _artifactData.lastUpdateTick = _lastUpdateTick;
            }
        }
        
        /// <summary>Main update method</summary>
        private void UpdateArtifact()
        {
            var data = ArtifactData;
            
            // Update equipped pawn reference
            UpdateEquippedPawnReference();
            
            // Update artifact state
            data.lastUpdateTick = Find.TickManager.TicksGame;
            
            // Validate and fix any issues
            ValidateArtifactState();
        }
        
        /// <summary>Update equipped pawn reference if needed</summary>
        private void UpdateEquippedPawnReference()
        {
            var currentPawn = FindEquippedPawn();
            if (_cachedEquippedPawn != currentPawn)
            {
                // Equipment changed
                if (_cachedEquippedPawn != null)
                {
                    OnUnequippedBy(_cachedEquippedPawn);
                }
                
                _cachedEquippedPawn = currentPawn;
                ArtifactData.equippedBy = currentPawn;
                
                if (currentPawn != null)
                {
                    OnEquippedBy(currentPawn);
                }
            }
        }
        
        /// <summary>Validate artifact state and fix issues</summary>
        private void ValidateArtifactState()
        {
            var data = ArtifactData;
            
            // Clamp Qi values
            data.currentArtifactQi = Mathf.Clamp(data.currentArtifactQi, 0f, data.maxArtifactQi);
            
            // Update buff durations and remove expired buffs
            data.activeBuffs.RemoveAll(buff => buff.HasExpired);
            
            // Validate skill cooldowns
            var currentTick = Find.TickManager.TicksGame;
            var expiredCooldowns = data.skillCooldowns.Where(kvp => kvp.Value <= currentTick).Select(kvp => kvp.Key).ToList();
            foreach (var skill in expiredCooldowns)
            {
                data.skillCooldowns.Remove(skill);
            }
        }
        #endregion

        #region Equipment Integration
        /// <summary>Find the pawn currently equipping this artifact</summary>
        private Pawn FindEquippedPawn()
        {
            // Check if held by a pawn
            if (parent.holdingOwner?.Owner is Pawn pawn)
            {
                return pawn;
            }
            
            // Check if in equipment tracker
            if (parent.holdingOwner?.Owner is Pawn_EquipmentTracker tracker)
            {
                return tracker.pawn;
            }
            
            return null;
        }
        
        /// <summary>Called when artifact is equipped by a pawn</summary>
        public void OnEquippedBy(Pawn pawn)
        {
            Log.Message($"[TuTien] Artifact {parent.def.defName} equipped by {pawn.Name}");
            
            ArtifactData.equippedBy = pawn;
            ArtifactData.isActive = true;
            
            // Apply artifact buffs to pawn
            ApplyBuffsToPawn(pawn);
            
            // Subscribe to pawn events if needed
            // TODO: Add event subscriptions in Phase 3
        }
        
        /// <summary>Called when artifact is unequipped from a pawn</summary>
        public void OnUnequippedBy(Pawn pawn)
        {
            Log.Message($"[TuTien] Artifact {parent.def.defName} unequipped from {pawn.Name}");
            
            // Remove artifact buffs from pawn
            RemoveBuffsFromPawn(pawn);
            
            ArtifactData.equippedBy = null;
            ArtifactData.isActive = false;
            
            // Unsubscribe from pawn events
            // TODO: Add event unsubscriptions in Phase 3
        }
        
        /// <summary>Apply artifact buffs to equipped pawn</summary>
        private void ApplyBuffsToPawn(Pawn pawn)
        {
            // TODO: Implement buff application in Phase 3
            // This will integrate with the existing effect system
            Log.Message($"[TuTien] Applying {ArtifactData.activeBuffs.Count} buffs to {pawn.Name} (placeholder)");
        }
        
        /// <summary>Remove artifact buffs from pawn</summary>
        private void RemoveBuffsFromPawn(Pawn pawn)
        {
            // TODO: Implement buff removal in Phase 3
            Log.Message($"[TuTien] Removing {ArtifactData.activeBuffs.Count} buffs from {pawn.Name} (placeholder)");
        }
        #endregion

        #region Qi Management
        /// <summary>Try to absorb Qi from equipped pawn</summary>
        private void TryAbsorbQi()
        {
            var data = ArtifactData;
            if (data.equippedBy == null || data.currentArtifactQi >= data.maxArtifactQi * 0.95f)
                return;
            
            // Calculate absorption amount
            float absorptionAmount = data.qiAbsorptionRate;
            
            // Try to absorb from pawn
            if (data.TryAbsorbQiFromPawn(absorptionAmount))
            {
                data.lastAbsorptionTick = Find.TickManager.TicksGame;
            }
        }
        
        /// <summary>Update active buffs</summary>
        private void UpdateBuffs()
        {
            var data = ArtifactData;
            
            foreach (var buff in data.activeBuffs)
            {
                buff.Update();
            }
            
            // Remove expired buffs
            int removedCount = data.activeBuffs.RemoveAll(buff => buff.HasExpired);
            if (removedCount > 0)
            {
                Log.Message($"[TuTien] Removed {removedCount} expired buffs from artifact");
            }
        }
        #endregion

        #region Inspection & UI
        public override string CompInspectStringExtra()
        {
            if (!_isInitialized) return "Initializing...";
            
            var data = ArtifactData;
            var lines = new List<string>();
            
            // Basic info
            lines.Add($"ELO: {data.eloRating:F0} (Power: {data.powerMultiplier:F2}x)");
            lines.Add($"Qi: {data.currentArtifactQi:F0}/{data.maxArtifactQi:F0} ({data.QiPercentage:P0})");
            
            // Combat stats
            lines.Add($"Damage: {data.EffectiveDamage:F1}, Range: {data.range:F1}, Accuracy: {data.accuracy:P0}");
            
            // Status
            if (data.equippedBy != null)
            {
                lines.Add($"Equipped by: {data.equippedBy.Name.ToStringShort}");
            }
            else
            {
                lines.Add("Not equipped");
            }
            
            // Active buffs
            if (data.activeBuffs.Count > 0)
            {
                lines.Add($"Active buffs: {string.Join(", ", data.activeBuffs.Select(b => b.description))}");
            }
            
            return string.Join("\n", lines);
        }
        #endregion
    }

    /// <summary>
    /// ✅ Task 2.1: Component properties for cultivation artifacts
    /// Links ThingDef to CultivationArtifactDef
    /// </summary>
    public class CultivationArtifactCompProperties : CompProperties
    {
        /// <summary>Reference to artifact definition for stat generation</summary>
        public CultivationArtifactDef artifactDef;
        
        /// <summary>Override ELO range for this specific artifact (optional)</summary>
        public FloatRange? eloOverride;
        
        /// <summary>Additional buff categories for this artifact (optional)</summary>
        public List<string> bonusBuffCategories = new List<string>();
        
        public CultivationArtifactCompProperties()
        {
            compClass = typeof(CultivationArtifactComp);
        }
    }
}
