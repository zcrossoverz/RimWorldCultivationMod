using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TuTien
{
    public class ITab_Cultivation : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;
        private static readonly Vector2 WinSize = new Vector2(432f, 480f);

        public ITab_Cultivation()
        {
            size = WinSize;
            labelKey = "TuTien_Tab_Cultivation";
        }

        public override bool IsVisible => SelPawn?.RaceProps?.Humanlike == true;

        protected override void FillTab()
        {
            var pawn = SelPawn;
            if (pawn == null) return;

            var comp = pawn.GetComp<CultivationComp>();
            if (comp?.cultivationData == null)
            {
                var rect = new Rect(0f, 0f, size.x, size.y);
                Widgets.Label(rect, "No cultivation data available.");
                return;
            }

            var mainRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            CultivationUI.DrawCultivationTab(mainRect, pawn);
        }
    }
}
