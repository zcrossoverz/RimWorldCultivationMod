using Verse;

namespace TuTien
{
    /// <summary>
    /// Unified interface for all cultivation skill workers
    /// Solves the dual inheritance problem between Core and SkillWorkers namespaces
    /// </summary>
    public interface ICultivationSkillWorker
    {
        /// <summary>Execute the skill effect</summary>
        void Execute(Pawn pawn, CultivationSkillDef skill);
        
        /// <summary>The skill definition this worker handles</summary>
        CultivationSkillDef def { get; set; }
    }
}
