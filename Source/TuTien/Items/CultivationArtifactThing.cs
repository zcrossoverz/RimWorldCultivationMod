using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TuTien.Items
{
    /// <summary>
    /// Custom Thing class để hiển thị cultivation effects trong inspect string
    /// </summary>
    public class CultivationArtifactThing : ThingWithComps
    {
        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            
            // Tìm cultivation artifact extension
            var extension = this.def.GetModExtension<CultivationArtifactModExtension>();
            if (extension?.artifactDef != null)
            {
                var artifactDef = DefDatabase<CultivationArtifactDef>.GetNamed(extension.artifactDef, false);
                if (artifactDef != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("=== Cultivation Effects ===".Colorize(ColoredText.TipSectionTitleColor));
                    
                    // Hiển thị ELO range
                    stringBuilder.AppendLine($"Power Level: {artifactDef.eloRange.min:F0} - {artifactDef.eloRange.max:F0}");
                    
                    // Hiển thị artifact type
                    stringBuilder.AppendLine($"Type: {artifactDef.artifactType}".Colorize(UnityEngine.Color.cyan));
                    
                    // Hiển thị cultivation requirements
                    if (artifactDef.requiredRealmLevel > 0 || artifactDef.requiredStage > 1)
                    {
                        stringBuilder.AppendLine($"Requires: Realm {artifactDef.requiredRealmLevel}, Stage {artifactDef.requiredStage}".Colorize(UnityEngine.Color.yellow));
                    }
                    
                    // Hiển thị auto-attack abilities
                    if (artifactDef.hasAutoAttack)
                    {
                        stringBuilder.AppendLine($"Auto-Attack Range: {artifactDef.detectionRadius:F1}".Colorize(UnityEngine.Color.red));
                    }
                    
                    // Hiển thị Qi pool
                    if (artifactDef.qiPoolRange.max > 0)
                    {
                        stringBuilder.AppendLine($"Qi Pool: {artifactDef.qiPoolRange.min:F0} - {artifactDef.qiPoolRange.max:F0}".Colorize(UnityEngine.Color.green));
                    }
                    
                    // Hiển thị damage range
                    stringBuilder.AppendLine($"Damage: {artifactDef.damageRange.min:F0} - {artifactDef.damageRange.max:F0}".Colorize(UnityEngine.Color.white));
                    
                    // Hiển thị rarity
                    var rarityColor = GetRarityColor(artifactDef.rarity.ToString());
                    stringBuilder.AppendLine($"Rarity: {artifactDef.rarity}".Colorize(rarityColor));
                }
            }
            
            return stringBuilder.ToString().TrimEndNewlines();
        }
        
        private UnityEngine.Color GetEffectColor(string category)
        {
            return category?.ToLower() switch
            {
                "combat" => UnityEngine.Color.red,
                "defense" => UnityEngine.Color.blue,
                "movement" => UnityEngine.Color.green,
                "qi" => UnityEngine.Color.cyan,
                "support" => UnityEngine.Color.yellow,
                "cultivation" => UnityEngine.Color.magenta,
                "ability" => UnityEngine.Color.white,
                "temporary" => UnityEngine.Color.gray,
                _ => UnityEngine.Color.white
            };
        }
        
        private UnityEngine.Color GetRarityColor(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => UnityEngine.Color.white,
                "rare" => UnityEngine.Color.blue,
                "epic" => UnityEngine.Color.magenta,
                "legendary" => UnityEngine.Color.yellow,
                _ => UnityEngine.Color.white
            };
        }
    }
}
