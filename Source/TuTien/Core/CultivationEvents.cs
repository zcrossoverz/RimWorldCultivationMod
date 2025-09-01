using System;
using Verse;

namespace TuTien.Core
{
    /// <summary>
    /// Central event system for cultivation mod
    /// Provides loose coupling between components and extensibility for other mods
    /// </summary>
    public static class CultivationEvents
    {
        #region Realm and Stage Events
        
        /// <summary>
        /// Fired when a pawn's cultivation realm changes
        /// </summary>
        public static event Action<Pawn, CultivationRealm, CultivationRealm> OnRealmChanged;
        
        /// <summary>
        /// Fired when a pawn's cultivation stage changes within a realm
        /// </summary>
        public static event Action<Pawn, int, int> OnStageChanged;
        
        /// <summary>
        /// Fired when a pawn attempts a breakthrough (before result is determined)
        /// </summary>
        public static event Action<Pawn, bool> OnBreakthroughAttempt; // bool = isAutoCultivation
        
        /// <summary>
        /// Fired when a breakthrough attempt completes with result
        /// </summary>
        public static event Action<Pawn, bool> OnBreakthroughResult; // bool = success
        
        #endregion
        
        #region Skill Events
        
        /// <summary>
        /// Fired when a pawn uses a cultivation skill
        /// </summary>
        public static event Action<Pawn, CultivationSkillDef> OnSkillUsed;
        
        /// <summary>
        /// Fired when a pawn unlocks a new skill
        /// </summary>
        public static event Action<Pawn, CultivationSkillDef> OnSkillUnlocked;
        
        /// <summary>
        /// Fired when a skill's cooldown expires
        /// </summary>
        public static event Action<Pawn, CultivationSkillDef> OnSkillCooldownExpired;
        
        #endregion
        
        #region Resource Events
        
        /// <summary>
        /// Fired when a pawn's Qi changes significantly (>1 point change)
        /// </summary>
        public static event Action<Pawn, float, float> OnQiChanged; // oldQi, newQi
        
        /// <summary>
        /// Fired when a pawn's Tu Vi (cultivation points) changes
        /// </summary>
        public static event Action<Pawn, float, float> OnTuViChanged; // oldTuVi, newTuVi
        
        /// <summary>
        /// Fired when Qi is fully depleted
        /// </summary>
        public static event Action<Pawn> OnQiDepleted;
        
        /// <summary>
        /// Fired when Qi is fully restored
        /// </summary>
        public static event Action<Pawn> OnQiRestored;
        
        /// <summary>
        /// Fired when a pawn's talent changes
        /// </summary>
        public static event Action<Pawn, TalentLevel, TalentLevel> OnTalentChanged; // pawn, oldTalent, newTalent
        
        #endregion
        
        #region Technique Events
        
        /// <summary>
        /// Fired when a pawn learns a new cultivation technique
        /// </summary>
        public static event Action<Pawn, CultivationTechnique> OnTechniqueLearned;
        
        /// <summary>
        /// Fired when a pawn practices a cultivation technique
        /// </summary>
        public static event Action<Pawn, CultivationTechnique, float> OnTechniquePracticed; // pawn, technique, experienceGained
        
        /// <summary>
        /// Fired when a technique's mastery level advances
        /// </summary>
        public static event Action<Pawn, CultivationTechnique, TechniqueMasteryLevel, TechniqueMasteryLevel> OnTechniqueMasteryAdvanced; // pawn, technique, oldLevel, newLevel
        
        /// <summary>
        /// Fired when a pawn changes their active cultivation technique
        /// </summary>
        public static event Action<Pawn, CultivationTechnique, CultivationTechnique> OnTechniqueChanged; // pawn, oldTechnique, newTechnique
        
        #endregion
        
        #region System Events
        
        /// <summary>
        /// Fired when cultivation stats are recalculated
        /// </summary>
        public static event Action<Pawn, CultivationData> OnStatsRecalculated;
        
        /// <summary>
        /// Fired when a pawn starts auto-cultivation
        /// </summary>
        public static event Action<Pawn> OnAutoCultivationStarted;
        
        /// <summary>
        /// Fired when a pawn stops auto-cultivation
        /// </summary>
        public static event Action<Pawn> OnAutoCultivationStopped;
        
        #endregion
        
        #region Event Triggers
        
        /// <summary>
        /// Trigger realm changed event with safety checks
        /// </summary>
        public static void TriggerRealmChanged(Pawn pawn, CultivationRealm oldRealm, CultivationRealm newRealm)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} realm changed: {oldRealm} -> {newRealm}");
                
                OnRealmChanged?.Invoke(pawn, oldRealm, newRealm);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnRealmChanged event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger stage changed event with safety checks
        /// </summary>
        public static void TriggerStageChanged(Pawn pawn, int oldStage, int newStage)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} stage changed: {oldStage} -> {newStage}");
                
                OnStageChanged?.Invoke(pawn, oldStage, newStage);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnStageChanged event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger breakthrough attempt event
        /// </summary>
        public static void TriggerBreakthroughAttempt(Pawn pawn, bool isAutoCultivation)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} attempting breakthrough (auto: {isAutoCultivation})");
                
                OnBreakthroughAttempt?.Invoke(pawn, isAutoCultivation);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnBreakthroughAttempt event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger breakthrough result event
        /// </summary>
        public static void TriggerBreakthroughResult(Pawn pawn, bool success)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} breakthrough result: {(success ? "SUCCESS" : "FAILED")}");
                
                OnBreakthroughResult?.Invoke(pawn, success);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnBreakthroughResult event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger skill used event
        /// </summary>
        public static void TriggerSkillUsed(Pawn pawn, CultivationSkillDef skill)
        {
            if (pawn == null || skill == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} used skill: {skill.LabelCap}");
                
                OnSkillUsed?.Invoke(pawn, skill);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnSkillUsed event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger skill unlocked event
        /// </summary>
        public static void TriggerSkillUnlocked(Pawn pawn, CultivationSkillDef skill)
        {
            if (pawn == null || skill == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} unlocked skill: {skill.LabelCap}");
                
                OnSkillUnlocked?.Invoke(pawn, skill);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnSkillUnlocked event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger skill cooldown expired event
        /// </summary>
        public static void TriggerSkillCooldownExpired(Pawn pawn, CultivationSkillDef skill)
        {
            if (pawn == null || skill == null) return;
            
            try
            {
                OnSkillCooldownExpired?.Invoke(pawn, skill);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnSkillCooldownExpired event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger Qi changed event (only for significant changes)
        /// </summary>
        public static void TriggerQiChanged(Pawn pawn, float oldQi, float newQi)
        {
            if (pawn == null) return;
            
            // Only trigger for significant changes (>1 point) to avoid spam
            if (Math.Abs(newQi - oldQi) < 1f) return;
            
            try
            {
                OnQiChanged?.Invoke(pawn, oldQi, newQi);
                
                // Trigger depletion/restoration events
                if (oldQi > 0 && newQi <= 0)
                    TriggerQiDepleted(pawn);
                else if (oldQi < pawn.GetComp<CultivationComp>()?.cultivationData?.maxQi && 
                         newQi >= pawn.GetComp<CultivationComp>()?.cultivationData?.maxQi)
                    TriggerQiRestored(pawn);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnQiChanged event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger Tu Vi changed event
        /// </summary>
        public static void TriggerTuViChanged(Pawn pawn, float oldTuVi, float newTuVi)
        {
            if (pawn == null) return;
            
            // Only trigger for changes >0.1 to avoid micro-changes spam
            if (Math.Abs(newTuVi - oldTuVi) < 0.1f) return;
            
            try
            {
                OnTuViChanged?.Invoke(pawn, oldTuVi, newTuVi);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnTuViChanged event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger talent changed event
        /// </summary>
        public static void TriggerTalentChanged(Pawn pawn, TalentLevel oldTalent, TalentLevel newTalent)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} talent changed: {oldTalent} -> {newTalent}");
                
                OnTalentChanged?.Invoke(pawn, oldTalent, newTalent);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnTalentChanged event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger Qi depleted event
        /// </summary>
        public static void TriggerQiDepleted(Pawn pawn)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} Qi depleted");
                
                OnQiDepleted?.Invoke(pawn);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnQiDepleted event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger Qi restored event
        /// </summary>
        public static void TriggerQiRestored(Pawn pawn)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} Qi restored");
                
                OnQiRestored?.Invoke(pawn);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnQiRestored event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger technique changed event
        /// <summary>
        /// Trigger technique learned event with safety checks
        /// </summary>
        public static void TriggerTechniqueLearned(Pawn pawn, CultivationTechnique technique)
        {
            if (pawn == null || technique == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} learned technique: {technique.label}");
                
                OnTechniqueLearned?.Invoke(pawn, technique);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnTechniqueLearned event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger technique practiced event with safety checks
        /// </summary>
        public static void TriggerTechniquePracticed(Pawn pawn, CultivationTechnique technique, float experienceGained)
        {
            if (pawn == null || technique == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} practiced {technique.label}, gained {experienceGained:F1} experience");
                
                OnTechniquePracticed?.Invoke(pawn, technique, experienceGained);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnTechniquePracticed event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger technique mastery advanced event with safety checks
        /// </summary>
        public static void TriggerTechniqueMasteryAdvanced(Pawn pawn, CultivationTechnique technique, TechniqueMasteryLevel oldLevel, TechniqueMasteryLevel newLevel)
        {
            if (pawn == null || technique == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} advanced {technique.label} from {oldLevel} to {newLevel}");
                
                OnTechniqueMasteryAdvanced?.Invoke(pawn, technique, oldLevel, newLevel);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnTechniqueMasteryAdvanced event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger technique changed event with safety checks
        /// </summary>
        public static void TriggerTechniqueChanged(Pawn pawn, CultivationTechnique oldTechnique, CultivationTechnique newTechnique)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} technique changed: {oldTechnique?.label ?? "None"} -> {newTechnique?.label ?? "None"}");
                
                OnTechniqueChanged?.Invoke(pawn, oldTechnique, newTechnique);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnTechniqueChanged event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger stats recalculated event
        /// </summary>
        public static void TriggerStatsRecalculated(Pawn pawn, CultivationData data)
        {
            if (pawn == null || data == null) return;
            
            try
            {
                OnStatsRecalculated?.Invoke(pawn, data);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnStatsRecalculated event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger auto cultivation started event
        /// </summary>
        public static void TriggerAutoCultivationStarted(Pawn pawn)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} started auto cultivation");
                
                OnAutoCultivationStarted?.Invoke(pawn);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnAutoCultivationStarted event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Trigger auto cultivation stopped event
        /// </summary>
        public static void TriggerAutoCultivationStopped(Pawn pawn)
        {
            if (pawn == null) return;
            
            try
            {
                if (Prefs.DevMode)
                    Log.Message($"[TuTien Events] {pawn.Name.ToStringShort} stopped auto cultivation");
                
                OnAutoCultivationStopped?.Invoke(pawn);
            }
            catch (Exception ex)
            {
                Log.Error($"[TuTien Events] Error in OnAutoCultivationStopped event: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get count of subscribers for debugging
        /// </summary>
        public static int GetEventSubscriberCount()
        {
            int count = 0;
            
            count += OnRealmChanged?.GetInvocationList().Length ?? 0;
            count += OnStageChanged?.GetInvocationList().Length ?? 0;
            count += OnBreakthroughAttempt?.GetInvocationList().Length ?? 0;
            count += OnBreakthroughResult?.GetInvocationList().Length ?? 0;
            count += OnSkillUsed?.GetInvocationList().Length ?? 0;
            count += OnSkillUnlocked?.GetInvocationList().Length ?? 0;
            count += OnSkillCooldownExpired?.GetInvocationList().Length ?? 0;
            count += OnQiChanged?.GetInvocationList().Length ?? 0;
            count += OnTuViChanged?.GetInvocationList().Length ?? 0;
            count += OnQiDepleted?.GetInvocationList().Length ?? 0;
            count += OnQiRestored?.GetInvocationList().Length ?? 0;
            count += OnTechniqueChanged?.GetInvocationList().Length ?? 0;
            count += OnTechniqueLearned?.GetInvocationList().Length ?? 0;
            count += OnTechniquePracticed?.GetInvocationList().Length ?? 0;
            count += OnTechniqueMasteryAdvanced?.GetInvocationList().Length ?? 0;
            count += OnStatsRecalculated?.GetInvocationList().Length ?? 0;
            count += OnAutoCultivationStarted?.GetInvocationList().Length ?? 0;
            count += OnAutoCultivationStopped?.GetInvocationList().Length ?? 0;
            
            return count;
        }
        
        /// <summary>
        /// Clear all event subscriptions (for cleanup/testing)
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            OnRealmChanged = null;
            OnStageChanged = null;
            OnBreakthroughAttempt = null;
            OnBreakthroughResult = null;
            OnSkillUsed = null;
            OnSkillUnlocked = null;
            OnSkillCooldownExpired = null;
            OnQiChanged = null;
            OnTuViChanged = null;
            OnQiDepleted = null;
            OnQiRestored = null;
            OnTechniqueChanged = null;
            OnTechniqueLearned = null;
            OnTechniquePracticed = null;
            OnTechniqueMasteryAdvanced = null;
            OnStatsRecalculated = null;
            OnAutoCultivationStarted = null;
            OnAutoCultivationStopped = null;
            
            if (Prefs.DevMode)
                Log.Message("[TuTien Events] All event subscriptions cleared");
        }
        
        #endregion
    }
}
