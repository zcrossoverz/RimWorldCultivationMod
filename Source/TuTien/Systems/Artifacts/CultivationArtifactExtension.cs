using Verse;
using RimWorld;

namespace TuTien
{
    /// <summary>
    /// ModExtension to link ThingDefs with CultivationArtifactDefs
    /// </summary>
    public class CultivationArtifactExtension : DefModExtension
    {
        /// <summary>
        /// The defName of the CultivationArtifactDef this ThingDef represents
        /// </summary>
        public string artifactDef;
        
        /// <summary>
        /// Get the linked CultivationArtifactDef
        /// </summary>
        public CultivationArtifactDef GetArtifactDef()
        {
            if (string.IsNullOrEmpty(artifactDef))
                return null;
                
            return DefDatabase<CultivationArtifactDef>.GetNamedSilentFail(artifactDef);
        }
    }
    
    /// <summary>
    /// CompProperties for CultivationArtifactComp
    /// </summary>
    public class CultivationArtifactCompProperties : CompProperties
    {
        public CultivationArtifactCompProperties()
        {
            compClass = typeof(CompQuality); // Temporary placeholder until CultivationArtifactComp loads
        }
    }
}
