using Verse;
using TuTien.Core;

namespace TuTien.Utils
{
    /// <summary>
    /// Simple event subscriber for testing cultivation events
    /// Automatically subscribes to all events and logs them for verification
    /// </summary>
    public static class CultivationEventLogger
    {
        private static bool isInitialized = false;
        
        /// <summary>
        /// Initialize event subscriptions (called from mod constructor)
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;
            
            // Subscribe to all cultivation events for logging
            CultivationEvents.OnRealmChanged += OnRealmChanged;
            CultivationEvents.OnStageChanged += OnStageChanged;
            CultivationEvents.OnBreakthroughAttempt += OnBreakthroughAttempt;
            CultivationEvents.OnBreakthroughResult += OnBreakthroughResult;
            CultivationEvents.OnSkillUsed += OnSkillUsed;
            CultivationEvents.OnSkillUnlocked += OnSkillUnlocked;
            CultivationEvents.OnSkillCooldownExpired += OnSkillCooldownExpired;
            CultivationEvents.OnQiChanged += OnQiChanged;
            CultivationEvents.OnTuViChanged += OnTuViChanged;
            CultivationEvents.OnQiDepleted += OnQiDepleted;
            CultivationEvents.OnQiRestored += OnQiRestored;
            CultivationEvents.OnStatsRecalculated += OnStatsRecalculated;
            CultivationEvents.OnAutoCultivationStarted += OnAutoCultivationStarted;
            CultivationEvents.OnAutoCultivationStopped += OnAutoCultivationStopped;
            
            isInitialized = true;
            Log.Message("[TuTien Event Logger] Event subscriptions initialized successfully");
        }
        
        /// <summary>
        /// Cleanup event subscriptions
        /// </summary>
        public static void Cleanup()
        {
            if (!isInitialized) return;
            
            CultivationEvents.OnRealmChanged -= OnRealmChanged;
            CultivationEvents.OnStageChanged -= OnStageChanged;
            CultivationEvents.OnBreakthroughAttempt -= OnBreakthroughAttempt;
            CultivationEvents.OnBreakthroughResult -= OnBreakthroughResult;
            CultivationEvents.OnSkillUsed -= OnSkillUsed;
            CultivationEvents.OnSkillUnlocked -= OnSkillUnlocked;
            CultivationEvents.OnSkillCooldownExpired -= OnSkillCooldownExpired;
            CultivationEvents.OnQiChanged -= OnQiChanged;
            CultivationEvents.OnTuViChanged -= OnTuViChanged;
            CultivationEvents.OnQiDepleted -= OnQiDepleted;
            CultivationEvents.OnQiRestored -= OnQiRestored;
            CultivationEvents.OnStatsRecalculated -= OnStatsRecalculated;
            CultivationEvents.OnAutoCultivationStarted -= OnAutoCultivationStarted;
            CultivationEvents.OnAutoCultivationStopped -= OnAutoCultivationStopped;
            
            isInitialized = false;
            Log.Message("[TuTien Event Logger] Event subscriptions cleaned up");
        }
        
        #region Event Handlers
        
        private static void OnRealmChanged(Pawn pawn, CultivationRealm oldRealm, CultivationRealm newRealm)
        {
            Log.Message($"[EVENT TEST] ✅ OnRealmChanged: {pawn.Name.ToStringShort} {oldRealm} → {newRealm}");
        }
        
        private static void OnStageChanged(Pawn pawn, int oldStage, int newStage)
        {
            Log.Message($"[EVENT TEST] ✅ OnStageChanged: {pawn.Name.ToStringShort} Stage {oldStage} → {newStage}");
        }
        
        private static void OnBreakthroughAttempt(Pawn pawn, bool isAutoCultivation)
        {
            Log.Message($"[EVENT TEST] ✅ OnBreakthroughAttempt: {pawn.Name.ToStringShort} (auto: {isAutoCultivation})");
        }
        
        private static void OnBreakthroughResult(Pawn pawn, bool success)
        {
            Log.Message($"[EVENT TEST] ✅ OnBreakthroughResult: {pawn.Name.ToStringShort} {(success ? "SUCCESS" : "FAILED")}");
        }
        
        private static void OnSkillUsed(Pawn pawn, CultivationSkillDef skill)
        {
            Log.Message($"[EVENT TEST] ✅ OnSkillUsed: {pawn.Name.ToStringShort} used {skill.LabelCap}");
        }
        
        private static void OnSkillUnlocked(Pawn pawn, CultivationSkillDef skill)
        {
            Log.Message($"[EVENT TEST] ✅ OnSkillUnlocked: {pawn.Name.ToStringShort} unlocked {skill.LabelCap}");
        }
        
        private static void OnSkillCooldownExpired(Pawn pawn, CultivationSkillDef skill)
        {
            Log.Message($"[EVENT TEST] ✅ OnSkillCooldownExpired: {pawn.Name.ToStringShort} - {skill.LabelCap} ready");
        }
        
        private static void OnQiChanged(Pawn pawn, float oldQi, float newQi)
        {
            Log.Message($"[EVENT TEST] ✅ OnQiChanged: {pawn.Name.ToStringShort} Qi {oldQi:F1} → {newQi:F1}");
        }
        
        private static void OnTuViChanged(Pawn pawn, float oldTuVi, float newTuVi)
        {
            Log.Message($"[EVENT TEST] ✅ OnTuViChanged: {pawn.Name.ToStringShort} TuVi {oldTuVi:F1} → {newTuVi:F1}");
        }
        
        private static void OnQiDepleted(Pawn pawn)
        {
            Log.Message($"[EVENT TEST] ✅ OnQiDepleted: {pawn.Name.ToStringShort} has no Qi left!");
        }
        
        private static void OnQiRestored(Pawn pawn)
        {
            Log.Message($"[EVENT TEST] ✅ OnQiRestored: {pawn.Name.ToStringShort} Qi fully restored!");
        }
        
        private static void OnStatsRecalculated(Pawn pawn, CultivationData data)
        {
            Log.Message($"[EVENT TEST] ✅ OnStatsRecalculated: {pawn.Name.ToStringShort} stats updated");
        }
        
        private static void OnAutoCultivationStarted(Pawn pawn)
        {
            Log.Message($"[EVENT TEST] ✅ OnAutoCultivationStarted: {pawn.Name.ToStringShort} began auto-cultivation");
        }
        
        private static void OnAutoCultivationStopped(Pawn pawn)
        {
            Log.Message($"[EVENT TEST] ✅ OnAutoCultivationStopped: {pawn.Name.ToStringShort} stopped auto-cultivation");
        }
        
        #endregion
        
        #region Statistics and Debug Info
        
        public static void LogEventStatistics()
        {
            var subscriberCount = CultivationEvents.GetEventSubscriberCount();
            Log.Message($"[EVENT TEST] Event system status:");
            Log.Message($"  - Initialized: {isInitialized}");
            Log.Message($"  - Total subscribers: {subscriberCount}");
            Log.Message($"  - Logger active: {(subscriberCount > 0 ? "YES" : "NO")}");
        }
        
        #endregion
    }
}
