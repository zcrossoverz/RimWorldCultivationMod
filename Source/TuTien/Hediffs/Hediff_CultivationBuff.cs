using RimWorld;
using Verse;
using System.Linq;

namespace TuTien.Hediffs
{
    public class Hediff_CultivationBuff : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            UpdateBuffStats();
        }

        public override void Tick()
        {
            base.Tick();
            
            // Update every few seconds to ensure stats stay current
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                UpdateBuffStats();
            }
        }

        private void UpdateBuffStats()
        {
            var cultivationComp = pawn?.GetComp<CultivationComp>();
            if (cultivationComp?.cultivationData == null) return;

            var stageStats = cultivationComp.cultivationData.GetCurrentStageStats();
            ApplyStageStats(stageStats);
        }

        private void ApplyStageStats(CultivationStageStats stats)
        {
            // HP Multiplier - just set severity for now, actual stats handled by patch
            var totalBuff = (stats.maxHpMultiplier - 1f) + stats.moveSpeedOffset + (stats.meleeDamageMultiplier - 1f);
            this.Severity = UnityEngine.Mathf.Clamp(totalBuff, 0.1f, 2f);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            // Stats will naturally revert when hediff is removed
        }
    }
}
