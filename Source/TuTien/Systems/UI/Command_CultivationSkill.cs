using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.UI
{
    public class Command_CultivationSkill : Command_Action
    {
        public float cooldownPct = 0f;
        public int cooldownTicksRemaining = 0;
        
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            
            // Draw cooldown overlay if on cooldown
            if (cooldownPct > 0f)
            {
                var rect = new Rect(topLeft.x, topLeft.y, 75f, 75f);
                DrawCooldownOverlay(rect, cooldownPct);
                
                // Draw cooldown time text
                if (cooldownTicksRemaining > 0)
                {
                    DrawCooldownText(rect, cooldownTicksRemaining);
                }
            }
            
            return result;
        }
        
        private void DrawCooldownOverlay(Rect rect, float pct)
        {
            // Dark overlay for cooldown
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = Color.white;
            
            // Draw remaining progress bar
            if (pct > 0.001f)
            {
                var remainingRect = new Rect(rect.x, rect.y + (1f - pct) * rect.height, rect.width, pct * rect.height);
                GUI.color = new Color(0.8f, 0.2f, 0.2f, 0.5f); // Red tint for cooldown
                GUI.DrawTexture(remainingRect, BaseContent.WhiteTex);
                GUI.color = Color.white;
            }
        }
        
        private void DrawCooldownText(Rect rect, int ticksRemaining)
        {
            string timeText;
            if (ticksRemaining >= GenDate.TicksPerHour)
            {
                float hours = ticksRemaining / (float)GenDate.TicksPerHour;
                timeText = $"{hours:F1}h";
            }
            else
            {
                float minutes = ticksRemaining / 3600f; // 3600 ticks = 1 minute
                timeText = $"{minutes:F0}m";
            }
            
            var textRect = new Rect(rect.x, rect.y + rect.height - 18f, rect.width, 18f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            GUI.color = Color.white;
            Widgets.Label(textRect, timeText);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }
    }
}
