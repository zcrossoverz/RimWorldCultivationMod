using UnityEngine;
using Verse;
using RimWorld;

namespace TuTien.UI
{
    public class Command_CultivationSkill : Command_Action
    {
        public float cooldownPct = 0f;
        
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            
            // Draw cooldown overlay if on cooldown
            if (cooldownPct > 0f)
            {
                var rect = new Rect(topLeft.x, topLeft.y, 75f, 75f);
                DrawCooldownOverlay(rect, cooldownPct);
            }
            
            return result;
        }
        
        private void DrawCooldownOverlay(Rect rect, float pct)
        {
            // Similar to RimWorld's cooldown overlay with gray tint
            Material material = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.2f, 0.2f, 0.8f));
            
            // Draw cooldown overlay
            GenUI.DrawTextureWithMaterial(rect, TexUI.GrayTextBG, material);
            
            // Draw progress arc with subtle gray
            if (pct > 0.001f)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                Widgets.FillableBar(rect, 1f - pct, TexUI.FastFillTex, null, false);
                GUI.color = Color.white;
            }
        }
    }
}
