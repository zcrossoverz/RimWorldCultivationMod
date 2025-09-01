using System.Linq;
using Verse;
using RimWorld;

namespace TuTien.TechniqueWorkers
{
    public class SwordQiTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            // Grant sword qi related skills
            var comp = pawn.GetComp<CultivationComp>();
            var data = comp?.cultivationData;
            if (data == null) return;

            // Add technique-specific skills
            var swordSkills = DefDatabase<CultivationSkillDef>.AllDefs
                .Where(s => s.defName.Contains("SwordQi"));
            
            foreach (var skill in swordSkills)
            {
                if (!data.unlockedSkills.Contains(skill))
                    data.unlockedSkills.Add(skill);
            }
        }
    }

    public class LightningTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationComp>();
            var data = comp?.cultivationData;
            if (data == null) return;

            var lightningSkills = DefDatabase<CultivationSkillDef>.AllDefs
                .Where(s => s.defName.Contains("Lightning"));
            
            foreach (var skill in lightningSkills)
            {
                if (!data.unlockedSkills.Contains(skill))
                    data.unlockedSkills.Add(skill);
            }
        }
    }

    public class FireTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationComp>();
            var data = comp?.cultivationData;
            if (data == null) return;

            var fireSkills = DefDatabase<CultivationSkillDef>.AllDefs
                .Where(s => s.defName.Contains("Fire"));
            
            foreach (var skill in fireSkills)
            {
                if (!data.unlockedSkills.Contains(skill))
                    data.unlockedSkills.Add(skill);
            }
        }
    }

    public class IceTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationComp>();
            var data = comp?.cultivationData;
            if (data == null) return;

            var iceSkills = DefDatabase<CultivationSkillDef>.AllDefs
                .Where(s => s.defName.Contains("Ice"));
            
            foreach (var skill in iceSkills)
            {
                if (!data.unlockedSkills.Contains(skill))
                    data.unlockedSkills.Add(skill);
            }
        }
    }

    public class EarthTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationComp>();
            var data = comp?.cultivationData;
            if (data == null) return;

            var earthSkills = DefDatabase<CultivationSkillDef>.AllDefs
                .Where(s => s.defName.Contains("Earth"));
            
            foreach (var skill in earthSkills)
            {
                if (!data.unlockedSkills.Contains(skill))
                    data.unlockedSkills.Add(skill);
            }
        }
    }
}
