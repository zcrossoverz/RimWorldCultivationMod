using HarmonyLib;
using Verse;
using TuTien.Utils;
using TuTien.Core;

namespace TuTien
{
    [StaticConstructorOnStartup]
    public class TuTienMod : Mod
    {
        public TuTienMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("tutien.basicpack");
            harmony.PatchAll();
            
            // Initialize event logging for testing
            CultivationEventLogger.Initialize();
            
            Log.Message("[Tu Tiên] Basic Pack loaded successfully!");
        }
    }
}
