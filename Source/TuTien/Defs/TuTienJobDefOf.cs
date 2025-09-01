using RimWorld;
using Verse;

namespace TuTien
{
    [DefOf]
    public static class TuTienJobDefOf
    {
        public static JobDef MeditateCultivation;
        
        static TuTienJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TuTienJobDefOf));
        }
    }
}
