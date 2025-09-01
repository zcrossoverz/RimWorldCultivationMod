using Verse;
using RimWorld;

/// <summary>
/// ✅ Task 1.1: DefOf registration for CultivationArtifactDef
/// Ensures RimWorld recognizes our custom artifact definitions
/// </summary>
[DefOf]
public static class CultivationArtifactDefOf
{
    // Static constructor to ensure DefDatabase initialization
    static CultivationArtifactDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(CultivationArtifactDefOf));
    }

    // Example artifact definitions - these will be defined in XML
    public static CultivationArtifactDef CultivationArtifact_IronSword;
    public static CultivationArtifactDef CultivationArtifact_ClothRobe;
    public static CultivationArtifactDef CultivationArtifact_SpiritBow;
    public static CultivationArtifactDef CultivationArtifact_DragonCrown;
    public static CultivationArtifactDef CultivationArtifact_ImmortalTalisman;
    
    // More artifact definitions will be added via XML files
}
