using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
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
            Log.Warning($"[TuTien] DrawCultivationTab called for {pawn.Name}");
            
            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null) 
            {
                Log.Warning($"[TuTien] No cultivation data for {pawn.Name}");
                return;
            }

            var data = comp.cultivationData;
            Log.Warning($"[TuTien] Found cultivation data, unlocked skills: {data.unlockedSkills.Count}");
            
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
            
            // Check if pawn is currently cultivating
            bool isCultivating = pawn.CurJob?.def == TuTienJobDefOf.MeditateCultivation || 
                               pawn.jobs?.curDriver is JobDriver_MeditateCultivation;
            
            // Set button color based on cultivation state
            if (isCultivating)
            {
                GUI.backgroundColor = Color.gray;
                GUI.enabled = false;
                Widgets.ButtonText(buttonRect, "Cultivating...");
                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = Color.green;
                if (Widgets.ButtonText(buttonRect, "Begin Cultivation"))
                {
                    StartCultivation(pawn);
                }
                GUI.backgroundColor = Color.white;
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
            Log.Warning($"[TuTien] DrawSkillsList called with {data.unlockedSkills.Count} skills");
            
            // Separate active and passive skills
            var activeSkills = data.unlockedSkills.Where(s => s.isActive).ToList();
            var passiveSkills = data.unlockedSkills.Where(s => !s.isActive).ToList();
            
            // Calculate required height 
            float skillHeight = 0f;
            if (activeSkills.Any())
            {
                skillHeight += 30f; // Header
                skillHeight += activeSkills.Count * 35f; // Skills
                skillHeight += 10f; // Gap
            }
            if (passiveSkills.Any())
            {
                skillHeight += 30f; // Header
                skillHeight += passiveSkills.Count * 45f; // More space for passive skills
            }
            
            var scrollRect = new Rect(0f, 0f, rect.width - 16f, skillHeight);
            Widgets.BeginScrollView(rect, ref skillScrollPosition, scrollRect);

            float y = 0f;
            
            // Draw Active Skills Section
            if (activeSkills.Any())
            {
                GUI.color = Color.cyan;
                Widgets.Label(new Rect(0f, y, scrollRect.width, 25f), "═══ ACTIVE SKILLS ═══");
                GUI.color = Color.white;
                y += 30f;
                
                foreach (var skill in activeSkills)
                {
                    var skillRect = new Rect(0f, y, scrollRect.width, 32f);
                    DrawActiveSkillEntry(skillRect, skill, data);
                    y += 35f;
                }
                y += 10f; // Gap between sections
            }
            
            // Draw Passive Skills Section
            if (passiveSkills.Any())
            {
                GUI.color = Color.yellow;
                Widgets.Label(new Rect(0f, y, scrollRect.width, 25f), "═══ PASSIVE SKILLS ═══");
                GUI.color = Color.white;
                y += 30f;
                
                foreach (var skill in passiveSkills)
                {
                    var skillRect = new Rect(0f, y, scrollRect.width, 42f); // More height for passive skills
                    DrawPassiveSkillEntry(skillRect, skill, data);
                    y += 45f; // More spacing
                }
            }

            Widgets.EndScrollView();
        }

        private static Vector2 skillScrollPosition = Vector2.zero;

        private static void DrawActiveSkillEntry(Rect rect, CultivationSkillDef skill, CultivationData data)
        {
            string skillName = skill.labelKey?.Translate() ?? skill.defName;
            
            // Active skill - draw as button with cooldown overlay
            bool hasEnoughQi = data.currentQi >= skill.qiCost;
            bool onCooldown = data.skillCooldowns.ContainsKey(skill.defName);
            bool canUse = data.CanUseSkill(skill);
            
            var buttonRect = new Rect(rect.x, rect.y, 120f, rect.height - 2f); // Make button smaller for more info space
            
            // Determine button color and state
            Color buttonColor;
            if (onCooldown)
                buttonColor = Color.red;
            else if (!hasEnoughQi)
                buttonColor = Color.gray;
            else
                buttonColor = Color.cyan;
            
            GUI.backgroundColor = buttonColor;
            
            string buttonText = $"⚡ {skillName}";
            bool clicked = Widgets.ButtonText(buttonRect, buttonText);
            
            // Draw cooldown overlay if on cooldown
            if (onCooldown)
            {
                int cooldownLeft = data.skillCooldowns[skill.defName];
                int totalCooldown = skill.cooldownHours * GenDate.TicksPerHour;
                float cooldownProgress = (float)cooldownLeft / totalCooldown;
                
                var cooldownRect = new Rect(buttonRect.x, buttonRect.y, buttonRect.width * cooldownProgress, buttonRect.height);
                Color overlayColor = new Color(0f, 0f, 0f, 0.7f);
                GUI.color = overlayColor;
                GUI.DrawTexture(cooldownRect, BaseContent.WhiteTex);
                GUI.color = Color.white;
                
                // Draw cooldown time
                string cooldownText;
                if (cooldownLeft >= GenDate.TicksPerHour)
                    cooldownText = $"{Mathf.CeilToInt(cooldownLeft / (float)GenDate.TicksPerHour)}h";
                else if (cooldownLeft >= 3600)
                    cooldownText = $"{Mathf.CeilToInt(cooldownLeft / 3600f)}m";
                else
                    cooldownText = $"{Mathf.CeilToInt(cooldownLeft / 60f)}s";
                
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;
                Widgets.Label(buttonRect, cooldownText);
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }
            
            GUI.backgroundColor = Color.white;
            
            if (clicked && canUse)
            {
                data.UseSkill(skill);
            }
            
            // Detailed skill info panel
            var infoRect = new Rect(rect.x + 125f, rect.y, rect.width - 125f, rect.height);
            
            // First line - basic info
            var line1Rect = new Rect(infoRect.x, infoRect.y, infoRect.width, 16f);
            string basicInfo = $"Qi Cost: {skill.qiCost} | Cooldown: {skill.cooldownHours}h";
            Widgets.Label(line1Rect, basicInfo);
            
            // Second line - status and description  
            var line2Rect = new Rect(infoRect.x, infoRect.y + 16f, infoRect.width, 16f);
            string statusInfo = "";
            
            if (onCooldown)
            {
                int cooldownLeft = data.skillCooldowns[skill.defName];
                int hoursLeft = Mathf.CeilToInt(cooldownLeft / (float)GenDate.TicksPerHour);
                statusInfo = $"On cooldown: {hoursLeft}h remaining";
                GUI.color = Color.red;
            }
            else if (!hasEnoughQi)
            {
                statusInfo = $"Need {skill.qiCost - data.currentQi:F0} more Qi";
                GUI.color = Color.yellow;
            }
            else
            {
                statusInfo = "Ready to use";
                GUI.color = Color.green;
            }
            
            Text.Font = GameFont.Tiny;
            Widgets.Label(line2Rect, statusInfo);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private static void DrawPassiveSkillEntry(Rect rect, CultivationSkillDef skill, CultivationData data)
        {
            string skillName = skill.labelKey?.Translate() ?? skill.defName;
            
            // Passive skill - show with different styling
            GUI.color = Color.yellow;
            var nameRect = new Rect(rect.x, rect.y, rect.width, 22f); // Increase height
            Widgets.Label(nameRect, $"🛡 {skillName} (Passive - Always Active)");
            
            // Description on second line with more spacing
            var descRect = new Rect(rect.x + 20f, rect.y + 22f, rect.width - 20f, 20f); // Move down more and increase height
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(descRect, skill.descriptionKey?.Translate() ?? "Passive enhancement");
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private static void StartCultivation(Pawn pawn)
        {
            // Create cultivation job at pawn's current position
            Job job = JobMaker.MakeJob(TuTienJobDefOf.MeditateCultivation, pawn.Position);
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
