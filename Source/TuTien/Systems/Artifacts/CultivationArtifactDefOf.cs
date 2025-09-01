using RimWorld;

namespace TuTien
{
    /// <summary>
    /// DefOf accessor for CultivationArtifactDefs
    /// </summary>
    [DefOf]
    public static class CultivationArtifactDefOf
    {
        // Common Artifacts
        public static CultivationArtifactDef CultivationArtifact_IronSword;
        public static CultivationArtifactDef CultivationArtifact_ClothRobe;
        
        // Rare Artifacts  
        public static CultivationArtifactDef CultivationArtifact_SpiritBow;
        
        // Epic Artifacts
        public static CultivationArtifactDef CultivationArtifact_DragonCrown;
        
        // Legendary Artifacts
        public static CultivationArtifactDef CultivationArtifact_ImmortalTalisman;

        static CultivationArtifactDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CultivationArtifactDefOf));
        }
    }
}
