using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TuTien
{
    [StaticConstructorOnStartup]
    public class CultivationUI
    {
        private static readonly Texture2D QiBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 1f));
        private static readonly Texture2D QiBarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));
        private static readonly Texture2D CultivationBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.8f, 0.2f));

        public static void DrawCultivationTab(Rect rect, Pawn pawn)
        {
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null) return;

            var data = comp.cultivationData;
            var listing = new Listing_Standard();
            listing.Begin(rect);

            // Realm and Stage Info
            var talentDef = DefDatabase<TalentDef>.AllDefs
                .FirstOrDefault(t => t.talentLevel == data.talent);

            string realmName = data.currentRealm.ToString();
            string talentName = talentDef?.labelKey?.Translate() ?? data.talent.ToString();

            listing.Label($"Realm: {realmName} - Stage {data.currentStage}");
            listing.Label($"Talent: {talentName}");
            listing.Gap();

            // Main Progress Bar - Cultivation Points (Tu Vi)
            var cultRect = listing.GetRect(24f);
            float required = data.GetRequiredCultivationPoints();
            float progress = required > 0 ? data.cultivationPoints / required : 0f;
            string cultLabel = $"Tu Vi: {data.cultivationPoints:F1}/{required:F1} ({progress:P1})";
            DrawProgressBar(cultRect, progress, cultLabel, CultivationBarTex, QiBarBgTex);

            // Secondary Info - Qi (smaller text)
            listing.Label($"Qi: {data.currentQi:F1}/{data.maxQi:F1}");
            listing.Gap();

            // Show breakthrough info and button
            if (data.CanBreakthrough())
            {
                var breakthroughRect = listing.GetRect(35f);
                float breakthroughChance = data.GetBreakthroughChance();
                string breakthroughLabel = $"Breakthrough - {breakthroughChance:P0} chance";
                
                // Show detailed breakthrough info
                listing.Label($"✓ Ready for breakthrough! Success chance: {breakthroughChance:P1}");
                
                // Highlight button with gold color when ready
                GUI.backgroundColor = Color.yellow;
                if (Widgets.ButtonText(breakthroughRect, breakthroughLabel))
                {
                    AttemptBreakthrough(pawn);
                }
                GUI.backgroundColor = Color.white;
                listing.Gap();
            }
            else 
            {
                // Show how much more tu vi is needed
                float requiredPoints = data.GetRequiredCultivationPoints();
                float needed = requiredPoints - data.cultivationPoints;
                if (needed > 0)
                {
                    listing.Label($"Need {needed:F1} more Tu Vi to breakthrough (Total: {requiredPoints:F1})");
                }
                else
                {
                    listing.Label("Breakthrough available soon... (recovering from last attempt)");
                }
                listing.Gap();
            }

            // Skills Section
            listing.Label("Abilities:");
            var skillsRect = listing.GetRect(200f);
            DrawSkillsList(skillsRect, data);

            listing.Gap();

            // Action Buttons
            var buttonRect = listing.GetRect(30f);
            
            // Only show cultivate button
            if (Widgets.ButtonText(buttonRect, "Cultivate"))
            {
                StartCultivation(pawn);
            }

            listing.End();
        }

        private static void DrawProgressBar(Rect rect, float progress, string label, Texture2D fillTex, Texture2D bgTex)
        {
            Widgets.DrawBoxSolid(rect, Color.black);
            GUI.DrawTexture(rect, bgTex);
            
            var fillRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(progress), rect.height);
            GUI.DrawTexture(fillRect, fillTex);
            
            var labelRect = new Rect(rect.x + 4f, rect.y, rect.width - 8f, rect.height);
            Widgets.Label(labelRect, label);
        }

        private static void DrawSkillsList(Rect rect, CultivationData data)
        {
            var scrollRect = new Rect(0f, 0f, rect.width - 16f, data.unlockedSkills.Count * 30f);
            Widgets.BeginScrollView(rect, ref skillScrollPosition, scrollRect);

            float y = 0f;
            foreach (var skill in data.unlockedSkills)
            {
                var skillRect = new Rect(0f, y, scrollRect.width, 28f);
                DrawSkillEntry(skillRect, skill, data);
                y += 30f;
            }

            Widgets.EndScrollView();
        }

        private static Vector2 skillScrollPosition = Vector2.zero;

        private static void DrawSkillEntry(Rect rect, CultivationSkillDef skill, CultivationData data)
        {
            string skillName = skill.labelKey?.Translate() ?? skill.defName;
            string skillType = skill.isActive ? "Active" : "Passive";
            
            if (skill.isActive)
            {
                bool canUse = data.CanUseSkill(skill);
                GUI.enabled = canUse;
                
                var buttonRect = new Rect(rect.x, rect.y, 120f, rect.height);
                if (Widgets.ButtonText(buttonRect, skillName))
                {
                    data.UseSkill(skill);
                }
                
                var infoRect = new Rect(rect.x + 125f, rect.y, rect.width - 125f, rect.height);
                string info = $"Qi: {skill.qiCost}";
                
                if (data.skillCooldowns.ContainsKey(skill.defName))
                {
                    int remainingTicks = data.skillCooldowns[skill.defName];
                    int remainingHours = remainingTicks / GenDate.TicksPerHour;
                    info += $" | CD: {remainingHours}h";
                }
                
                Widgets.Label(infoRect, info);
                GUI.enabled = true;
            }
            else
            {
                Widgets.Label(rect, $"{skillName} (Passive)");
            }
        }

        private static void StartCultivation(Pawn pawn)
        {
            var job = JobMaker.MakeJob(JobDefOf.Meditate);
            job.playerForced = true;
            pawn.jobs.TryTakeOrderedJob(job);
        }

        private static void AttemptBreakthrough(Pawn pawn)
        {
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null) return;

            // Store old stats for comparison
            var oldRealm = comp.cultivationData.currentRealm;
            var oldStage = comp.cultivationData.currentStage;
            var oldStats = comp.cultivationData.GetCurrentStageStats();
            var oldSkills = comp.cultivationData.unlockedSkills.ToList();

            bool success = comp.cultivationData.AttemptBreakthrough();
            
            if (success)
            {
                // Get new stats after breakthrough
                var newStats = comp.cultivationData.GetCurrentStageStats();
                var newSkills = comp.cultivationData.unlockedSkills.Except(oldSkills).ToList();
                
                // Show detailed breakthrough dialog
                Find.WindowStack.Add(new UI.Dialog_BreakthroughResult(
                    pawn, true, oldRealm, oldStage, comp.cultivationData.currentRealm, comp.cultivationData.currentStage,
                    oldStats, newStats, newSkills));
            }
            else
            {
                // Show failure dialog
                Find.WindowStack.Add(new UI.Dialog_BreakthroughResult(
                    pawn, false, oldRealm, oldStage, oldRealm, oldStage,
                    oldStats, oldStats, null));
            }
        }
    }
}
