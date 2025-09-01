using RimWorld;
using Verse;

/// <summary>
/// ✅ DefOf class for Cultivation Effects - helps RimWorld register our custom Def type
/// This ensures RimWorld properly recognizes CultivationEffectDef as a valid Def type
/// </summary>
[DefOf]
public static class CultivationEffectDefOf
{
    /// <summary>Static constructor to ensure DefOf is initialized</summary>
    static CultivationEffectDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(CultivationEffectDefOf));
    }
    
    // Example effect definitions (will be populated by RimWorld automatically)
    // These will be null if the corresponding XML definitions don't exist
    
    #pragma warning disable CS0649
    public static CultivationEffectDef CultivationEffect_IronBody;
    public static CultivationEffectDef CultivationEffect_SwiftSteps;
    public static CultivationEffectDef CultivationEffect_QiExpansion;
    #pragma warning restore CS0649
}
