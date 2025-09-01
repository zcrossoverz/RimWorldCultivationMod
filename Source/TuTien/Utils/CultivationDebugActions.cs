using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using TuTien;
using TuTien.Core;

namespace TuTien.Utils
{
    /// <summary>
    /// Debug actions for testing cultivation and skill systems
    /// Call these methods from dev console or other debug interfaces
    /// </summary>
    public static class CultivationDebugActions
    {
        #region Cultivation Debug Actions
        
        public static void AddQi(Pawn pawn, float amount = 100f)
        {
            var cultivationComp = pawn.GetComp<CultivationComp>();
            if (cultivationComp?.cultivationData != null)
            {
                cultivationComp.cultivationData.currentQi += amount;
                cultivationComp.cultivationData.currentQi = UnityEngine.Mathf.Min(
                    cultivationComp.cultivationData.currentQi, 
                    cultivationComp.cultivationData.maxQi);
                
                Log.Message($"[TuTien Debug] Added {amount} qi to {pawn.Name.ToStringShort}");
            }
        }
        
        public static void AddCultivationPoints(Pawn pawn, float amount = 500f)
        {
            var cultivationComp = pawn.GetComp<CultivationComp>();
            if (cultivationComp?.cultivationData != null)
            {
                cultivationComp.cultivationData.cultivationPoints += amount;
                Log.Message($"[TuTien Debug] Added {amount} cultivation points to {pawn.Name.ToStringShort}");
            }
        }
        
        public static void ForceBreakthrough(Pawn pawn)
        {
            var cultivationComp = pawn.GetComp<CultivationComp>();
            if (cultivationComp?.cultivationData != null)
            {
                // Set cultivation points to required amount for breakthrough
                float requiredPoints = cultivationComp.cultivationData.GetRequiredCultivationPoints();
                cultivationComp.cultivationData.cultivationPoints = requiredPoints;
                
                Log.Message($"[TuTien Debug] Set {pawn.Name.ToStringShort} ready for breakthrough");
            }
        }
        
        #endregion
        
        #region Skill Debug Actions
        
        public static void AwardSkillPoints(Pawn pawn, int amount = 10)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager != null)
            {
                skillManager.AwardSkillPoints(amount);
                Log.Message($"[TuTien Debug] Awarded {amount} skill points to {pawn.Name.ToStringShort}");
            }
        }
        
        public static void LearnRandomSkill(Pawn pawn)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager != null)
            {
                var availableSkills = skillManager.GetAvailableSkills().ToList();
                if (availableSkills.Count > 0)
                {
                    var skillToLearn = availableSkills.RandomElement();
                    skillManager.LearnSkill(skillToLearn.defName, paySkillPoints: false);
                    Log.Message($"[TuTien Debug] {pawn.Name.ToStringShort} learned {skillToLearn.LabelCap}");
                }
                else
                {
                    Log.Message($"[TuTien Debug] {pawn.Name.ToStringShort} has no available skills to learn");
                }
            }
        }
        
        public static void AddSkillExperience(Pawn pawn, float amount = 100f)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager != null)
            {
                var learnedSkills = skillManager.GetAllLearnedSkills().ToList();
                if (learnedSkills.Count > 0)
                {
                    var skillData = learnedSkills.RandomElement();
                    skillManager.AddSkillExperience(skillData.defName, amount);
                    
                    var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillData.defName);
                    Log.Message($"[TuTien Debug] Added {amount} experience to {pawn.Name.ToStringShort}'s {skillDef?.LabelCap ?? skillData.defName}");
                }
                else
                {
                    Log.Message($"[TuTien Debug] {pawn.Name.ToStringShort} has no learned skills");
                }
            }
        }
        
        public static void ResetSkillCooldowns(Pawn pawn)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager != null)
            {
                skillManager.ResetAllCooldowns();
                Log.Message($"[TuTien Debug] Reset all skill cooldowns for {pawn.Name.ToStringShort}");
            }
        }
        
        public static void UseRandomActiveSkill(Pawn pawn)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager != null)
            {
                var activeSkills = skillManager.GetAllLearnedSkills()
                    .Where(skillData =>
                    {
                        var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillData.defName);
                        return skillDef?.isActive == true && skillDef.CanUseSkill(pawn, out _);
                    })
                    .ToList();
                
                if (activeSkills.Count > 0)
                {
                    var skillToUse = activeSkills.RandomElement();
                    skillManager.UseSkill(skillToUse.defName);
                    
                    var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillToUse.defName);
                    Log.Message($"[TuTien Debug] {pawn.Name.ToStringShort} used {skillDef?.LabelCap ?? skillToUse.defName}");
                }
                else
                {
                    Log.Message($"[TuTien Debug] {pawn.Name.ToStringShort} has no usable active skills");
                }
            }
        }
        
        public static void ShowSkillSummary(Pawn pawn)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager != null)
            {
                var learnedSkills = skillManager.GetAllLearnedSkills().ToList();
                string summary = $"=== {pawn.Name.ToStringShort} Skill Summary ===\n";
                summary += $"Skill Points: {skillManager.skillPoints}\n";
                summary += $"Total Experience: {skillManager.totalExperience:F1}\n";
                summary += $"Learned Skills ({learnedSkills.Count}):\n";
                
                foreach (var skillData in learnedSkills.OrderByDescending(s => s.level))
                {
                    var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillData.defName);
                    string skillType = skillDef?.isActive == true ? "[Active]" : "[Passive]";
                    summary += $"  • {skillDef?.LabelCap ?? skillData.defName} {skillType} Lv.{skillData.level} (Exp: {skillData.experience:F1})\n";
                }
                
                Log.Message(summary);
            }
        }
        
        #endregion
        
        #region Event Testing
        
        public static void TestAllEvents()
        {
            var testPawn = Find.CurrentMap?.mapPawns?.FreeColonistsSpawned?.FirstOrDefault();
            if (testPawn == null)
            {
                Log.Warning("[TuTien Debug] No colonist found for testing");
                return;
            }
            
            // Test cultivation events
            CultivationEvents.TriggerQiChanged(testPawn, 50f, 100f);
            CultivationEvents.TriggerTuViChanged(testPawn, 100f, 200f);
            CultivationEvents.TriggerRealmChanged(testPawn, CultivationRealm.Mortal, CultivationRealm.QiCondensation);
            
            // Test skill events if pawn has skills
            var skillManager = testPawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager != null)
            {
                var learnedSkills = skillManager.GetAllLearnedSkills().ToList();
                if (learnedSkills.Count > 0)
                {
                    var skillData = learnedSkills.First();
                    var skillDef = DefDatabase<CultivationSkillDef>.GetNamedSilentFail(skillData.defName);
                    if (skillDef != null)
                    {
                        CultivationEvents.TriggerSkillUsed(testPawn, skillDef);
                    }
                }
            }
            
            Log.Message("[TuTien Debug] Triggered test events");
        }
        
        #endregion
        
        #region Quick Test Methods
        
        /// <summary>
        /// Quick test method to initialize a pawn with basic skills
        /// </summary>
        public static void QuickTestSkillSystem(Pawn pawn)
        {
            var skillManager = pawn.GetComp<CultivationComp>()?.GetSkillManager();
            if (skillManager == null)
            {
                Log.Warning($"[TuTien Debug] {pawn.Name.ToStringShort} has no skill manager");
                return;
            }
            
            // Award some skill points
            skillManager.AwardSkillPoints(20);
            
            // Learn a few basic skills
            var availableSkills = skillManager.GetAvailableSkills().Take(3);
            foreach (var skill in availableSkills)
            {
                skillManager.LearnSkill(skill.defName, paySkillPoints: false);
            }
            
            Log.Message($"[TuTien Debug] Quick test setup completed for {pawn.Name.ToStringShort}");
        }
        
        #endregion
    }
}
