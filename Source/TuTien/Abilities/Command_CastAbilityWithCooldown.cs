using UnityEngine;
using Verse;
using RimWorld;
using System;

namespace TuTien.Abilities
{
    /// <summary>
    /// Command with cooldown overlay like cultivation techniques
    /// </summary>
    public class Command_CastAbilityWithCooldown : Command_Target
    {
        public CultivationAbility ability;
        public Action selfCastAction; // For self-cast abilities

        // Cooldown properties for visual display
        public int CooldownRemaining => ability?.CooldownRemaining ?? 0;
        public int CooldownMax => ability?.def?.cooldownTicks ?? 1;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            // Draw cooldown overlay
            if (CooldownRemaining > 0 && CooldownMax > 0)
            {
                float fillPercent = (float)CooldownRemaining / CooldownMax;
                var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
                
                // Draw cooldown overlay
                GUI.color = new Color(1f, 1f, 1f, 0.6f);
                GUI.DrawTexture(new Rect(rect.x, rect.y + (1f - fillPercent) * rect.height, rect.width, fillPercent * rect.height), BaseContent.WhiteTex);
                GUI.color = Color.white;

                // Draw cooldown text
                var cooldownText = $"{CooldownRemaining}";
                var textRect = new Rect(rect.x, rect.y + rect.height - 20f, rect.width, 20f);
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;
                GUI.color = Color.red;
                Widgets.Label(textRect, cooldownText);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }

            return result;
        }
    }
}
