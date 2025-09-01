using Verse;
using TuTien;

namespace TuTien.Abilities
{
    /// <summary>
    /// GameComponent to handle Qi regeneration for all pawns
    /// </summary>
    public class QiRegenerationGameComponent : GameComponent
    {
        private int lastQiTick;

        public QiRegenerationGameComponent(Game game) : base()
        {
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % 60 == 0) // Every 60 ticks (1 second)
            {
                RegenerateQiForAllPawns();
            }
        }

        private void RegenerateQiForAllPawns()
        {
            foreach (var map in Find.Maps)
            {
                foreach (var pawn in map.mapPawns.AllPawns)
                {
                    if (pawn.IsColonist || pawn.IsPrisonerOfColony)
                    {
                        var cultivationData = pawn.GetCultivationData();
                        if (cultivationData != null)
                        {
                            cultivationData.RegenerateQi();
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref lastQiTick, "lastQiTick");
        }
    }
}
