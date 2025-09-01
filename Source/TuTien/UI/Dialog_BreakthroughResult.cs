using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TuTien.UI
{
    public class Dialog_BreakthroughResult : Window
    {
        private Pawn pawn;
        private bool success;
        private CultivationRealm oldRealm;
        private int oldStage;
        private CultivationRealm newRealm;
        private int newStage;
        private CultivationStageStats oldStats;
        private CultivationStageStats newStats;
        private System.Collections.Generic.List<CultivationSkillDef> newSkills;

        public Dialog_BreakthroughResult(Pawn pawn, bool success, CultivationRealm oldRealm, int oldStage, 
            CultivationRealm newRealm, int newStage, CultivationStageStats oldStats, CultivationStageStats newStats, 
            System.Collections.Generic.List<CultivationSkillDef> newSkills)
        {
            this.pawn = pawn;
            this.success = success;
            this.oldRealm = oldRealm;
            this.oldStage = oldStage;
            this.newRealm = newRealm;
            this.newStage = newStage;
            this.oldStats = oldStats;
            this.newStats = newStats;
            this.newSkills = newSkills ?? new System.Collections.Generic.List<CultivationSkillDef>();

            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = false;
            this.doCloseX = true;
        }

        public override Vector2 InitialSize => new Vector2(650f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            float y = 0f;
            
            // Title
            Text.Font = GameFont.Medium;
            string titleText = success ? "BREAKTHROUGH SUCCESS!" : "BREAKTHROUGH FAILED!";
            Color titleColor = success ? Color.green : Color.red;
            
            GUI.color = titleColor;
            Widgets.Label(new Rect(0f, y, inRect.width, 35f), titleText);
            GUI.color = Color.white;
            y += 45f;

            Text.Font = GameFont.Small;

            // Pawn info
            Widgets.Label(new Rect(0f, y, inRect.width, 25f), $"{pawn.Name.ToStringFull}");
            y += 30f;

            if (success)
            {
                // Success details
                Widgets.Label(new Rect(0f, y, inRect.width, 25f), $"Realm: {oldRealm} Stage {oldStage} → {newRealm} Stage {newStage}");
                y += 30f;

                // Stats comparison header
                GUI.color = Color.cyan;
                Widgets.Label(new Rect(0f, y, inRect.width, 25f), "── CULTIVATION IMPROVEMENTS ──");
                GUI.color = Color.green;
                y += 25f;

                // Core cultivation stats
                if (newStats.maxQi > oldStats.maxQi)
                {
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Max Qi: {oldStats.maxQi:F0} → {newStats.maxQi:F0} (+{newStats.maxQi - oldStats.maxQi:F0})");
                    y += 18f;
                }
                
                if (newStats.qiRegenRate > oldStats.qiRegenRate)
                {
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Qi Regen: {oldStats.qiRegenRate:F1}/s → {newStats.qiRegenRate:F1}/s (+{newStats.qiRegenRate - oldStats.qiRegenRate:F1}/s)");
                    y += 18f;
                }

                if (newStats.tuViGainMultiplier > oldStats.tuViGainMultiplier)
                {
                    float oldPercent = oldStats.tuViGainMultiplier * 100f;
                    float newPercent = newStats.tuViGainMultiplier * 100f;
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Tu Vi Gain: {oldPercent:F0}% → {newPercent:F0}% (+{newPercent - oldPercent:F0}%)");
                    y += 18f;
                }

                y += 5f;
                GUI.color = Color.yellow;
                Widgets.Label(new Rect(0f, y, inRect.width, 20f), "── COMBAT ENHANCEMENTS ──");
                GUI.color = Color.green;
                y += 20f;

                // Combat stats
                if (newStats.maxHpMultiplier > oldStats.maxHpMultiplier)
                {
                    float oldPercent = (oldStats.maxHpMultiplier - 1f) * 100f;
                    float newPercent = (newStats.maxHpMultiplier - 1f) * 100f;
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Max HP: +{oldPercent:F0}% → +{newPercent:F0}% (+{newPercent - oldPercent:F0}%)");
                    y += 18f;
                }

                if (newStats.meleeDamageMultiplier > oldStats.meleeDamageMultiplier)
                {
                    float oldPercent = (oldStats.meleeDamageMultiplier - 1f) * 100f;
                    float newPercent = (newStats.meleeDamageMultiplier - 1f) * 100f;
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Melee Damage: +{oldPercent:F0}% → +{newPercent:F0}% (+{newPercent - oldPercent:F0}%)");
                    y += 18f;
                }

                if (newStats.meleeHitChanceOffset > oldStats.meleeHitChanceOffset)
                {
                    float oldPercent = oldStats.meleeHitChanceOffset * 100f;
                    float newPercent = newStats.meleeHitChanceOffset * 100f;
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Melee Accuracy: +{oldPercent:F0}% → +{newPercent:F0}% (+{newPercent - oldPercent:F0}%)");
                    y += 18f;
                }

                if (newStats.armorRatingSharpMultiplier > oldStats.armorRatingSharpMultiplier)
                {
                    float oldPercent = (oldStats.armorRatingSharpMultiplier - 1f) * 100f;
                    float newPercent = (newStats.armorRatingSharpMultiplier - 1f) * 100f;
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Sharp Armor: +{oldPercent:F0}% → +{newPercent:F0}% (+{newPercent - oldPercent:F0}%)");
                    y += 18f;
                }

                y += 5f;
                GUI.color = Color.magenta;
                Widgets.Label(new Rect(0f, y, inRect.width, 20f), "── PHYSICAL IMPROVEMENTS ──");
                GUI.color = Color.green;
                y += 20f;

                // Physical stats
                if (newStats.moveSpeedOffset > oldStats.moveSpeedOffset)
                {
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Move Speed: +{oldStats.moveSpeedOffset:F1} → +{newStats.moveSpeedOffset:F1} (+{newStats.moveSpeedOffset - oldStats.moveSpeedOffset:F1})");
                    y += 18f;
                }

                if (newStats.workSpeedGlobalMultiplier > oldStats.workSpeedGlobalMultiplier)
                {
                    float oldPercent = (oldStats.workSpeedGlobalMultiplier - 1f) * 100f;
                    float newPercent = (newStats.workSpeedGlobalMultiplier - 1f) * 100f;
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Work Speed: +{oldPercent:F0}% → +{newPercent:F0}% (+{newPercent - oldPercent:F0}%)");
                    y += 18f;
                }

                if (newStats.injuryHealingFactorMultiplier > oldStats.injuryHealingFactorMultiplier)
                {
                    float oldPercent = (oldStats.injuryHealingFactorMultiplier - 1f) * 100f;
                    float newPercent = (newStats.injuryHealingFactorMultiplier - 1f) * 100f;
                    Widgets.Label(new Rect(0f, y, inRect.width, 18f), $"Healing Rate: +{oldPercent:F0}% → +{newPercent:F0}% (+{newPercent - oldPercent:F0}%)");
                    y += 18f;
                }

                // New skills
                if (newSkills.Any())
                {
                    y += 5f;
                    GUI.color = Color.yellow;
                    Widgets.Label(new Rect(0f, y, inRect.width, 25f), "── NEW SKILLS UNLOCKED ──");
                    y += 25f;

                    foreach (var skill in newSkills)
                    {
                        Widgets.Label(new Rect(20f, y, inRect.width - 20f, 18f), $"• {skill.label}");
                        y += 18f;
                    }
                    y += 10f;
                }

                GUI.color = Color.green;
                Widgets.Label(new Rect(0f, y, inRect.width, 25f), "Cultivation foundation greatly strengthened!");
                GUI.color = Color.white;
            }
            else
            {
                // Failure details
                GUI.color = Color.red;
                Widgets.Label(new Rect(0f, y, inRect.width, 25f), "Qi deviation occurred during breakthrough attempt!");
                y += 25f;
                
                Widgets.Label(new Rect(0f, y, inRect.width, 25f), "Cultivation progress lost and injuries sustained.");
                y += 25f;
                
                Widgets.Label(new Rect(0f, y, inRect.width, 25f), "Recovery time required before next attempt.");
                GUI.color = Color.white;
            }

            // Close button
            y = inRect.height - 35f;
            if (Widgets.ButtonText(new Rect(inRect.width - 120f, y, 100f, 30f), "Close"))
            {
                Close();
            }
        }
    }
}
