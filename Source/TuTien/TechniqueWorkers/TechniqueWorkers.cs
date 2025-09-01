using System.Linq;
using Verse;
using RimWorld;
using TuTien.Core;
using TuTien.Systems.Registry;

namespace TuTien.TechniqueWorkers
{
    public class SwordQiTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationCompEnhanced>();
            var data = comp?.EnhancedData;
            if (data == null) return;

            var swordSkills = CultivationRegistry.AllSkillDefs
                .Where(s => s.defName.Contains("SwordQi"));
            
            foreach (var skill in swordSkills)
            {
                if (!data.HasSkill(skill))
                    data.AddSkill(skill);
            }
        }
    }

    public class LightningTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationCompEnhanced>();
            var data = comp?.EnhancedData;
            if (data == null) return;

            var lightningSkills = CultivationRegistry.AllSkillDefs
                .Where(s => s.defName.Contains("Lightning"));
            
            foreach (var skill in lightningSkills)
            {
                if (!data.HasSkill(skill))
                    data.AddSkill(skill);
            }
        }
    }

    public class FireTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationCompEnhanced>();
            var data = comp?.EnhancedData;
            if (data == null) return;

            var fireSkills = CultivationRegistry.AllSkillDefs
                .Where(s => s.defName.Contains("Fire"));
            
            foreach (var skill in fireSkills)
            {
                if (!data.HasSkill(skill))
                    data.AddSkill(skill);
            }
        }
    }

    public class IceTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationCompEnhanced>();
            var data = comp?.EnhancedData;
            if (data == null) return;

            var iceSkills = CultivationRegistry.AllSkillDefs
                .Where(s => s.defName.Contains("Ice"));
            
            foreach (var skill in iceSkills)
            {
                if (!data.HasSkill(skill))
                    data.AddSkill(skill);
            }
        }
    }

    public class EarthTechniqueWorker : CultivationTechniqueWorker
    {
        public override void Apply(Pawn pawn, CultivationTechniqueDef technique)
        {
            var comp = pawn.GetComp<CultivationCompEnhanced>();
            var data = comp?.EnhancedData;
            if (data == null) return;

            var earthSkills = CultivationRegistry.AllSkillDefs
                .Where(s => s.defName.Contains("Earth"));
            
            foreach (var skill in earthSkills)
            {
                if (!data.HasSkill(skill))
                    data.AddSkill(skill);
            }
        }
    }
}
