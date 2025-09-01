using Verse;
using RimWorld.Planet;
using System.Collections.Generic;

namespace TuTien.Abilities
{
    /// <summary>
    /// World component to handle global Qi regeneration
    /// </summary>
    public class QiRegenerationWorldComponent : WorldComponent
    {
        private int tickCounter = 0;
        private const int QI_REGEN_INTERVAL = 60; // Every 1 second

        public QiRegenerationWorldComponent(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            
            tickCounter++;
            if (tickCounter >= QI_REGEN_INTERVAL)
            {
                tickCounter = 0;
                RegenerateQiForAllPawns();
            }
        }

        private void RegenerateQiForAllPawns()
        {
            // Regenerate Qi for player pawns
            if (Find.ColonistBar?.GetColonistsInOrder() != null)
            {
                foreach (var pawn in Find.ColonistBar.GetColonistsInOrder())
                {
                    if (pawn?.GetCultivationData() != null)
                    {
                        pawn.GetCultivationData().Tick();
                    }
                }
            }
        }
    }
}
