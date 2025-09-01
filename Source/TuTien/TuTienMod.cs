using HarmonyLib;
using Verse;

namespace TuTien
{
    [StaticConstructorOnStartup]
    public class TuTienMod : Mod
    {
        public TuTienMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("tutien.basicpack");
            harmony.PatchAll();
            
            Log.Message("[Tu Tiên] Basic Pack loaded successfully!");
        }
    }
}
