using RimWorld;
using System.Collections.Generic;
using System.Linq;
using TuTien.Core;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TuTien
{
    public class Building_CultivationTurret : Building_TurretGun
    {
        private CompQiStorage qiComp;
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            qiComp = this.GetComp<CompQiStorage>();
        }

        public CompQiStorage QiComp => qiComp;

        protected override void Tick()
        {
            base.Tick();
            
            // Debug turret status every 5 seconds
            if (Find.TickManager.TicksGame % 300 == 0 && qiComp != null)
            {
                Log.Message($"[TuTien] Turret {this.LabelShort} - Qi: {qiComp.CurrentQi}/{qiComp.MaxQi}, CanShoot: {qiComp.CanShoot}, HasTarget: {this.CurrentTarget.IsValid}");
            }
            
            // Disable turret if no Qi
            if (qiComp != null && !qiComp.CanShoot && this.CurrentTarget.IsValid)
            {
                this.forcedTarget = LocalTargetInfo.Invalid;
            }
        }
        
        protected override bool CanSetForcedTarget => qiComp?.CanShoot ?? false;
        
        public override void OrderAttack(LocalTargetInfo targ)
        {
            if (qiComp != null && qiComp.CanShoot)
            {
                base.OrderAttack(targ);
            }
            else
            {
                Messages.Message("Pháo đài không có đủ Qi để bắn", this, MessageTypeDefOf.RejectInput);
            }
        }
        
        // Override to ensure turret can auto-target when it has Qi
        public override LocalTargetInfo CurrentTarget
        {
            get
            {
                if (qiComp != null && !qiComp.CanShoot)
                {
                    return LocalTargetInfo.Invalid;
                }
                return base.CurrentTarget;
            }
        }

        public override string GetInspectString()
        {
            string baseString = base.GetInspectString();
            // Don't add Qi info here since CompQiStorage already provides it
            return baseString;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            
            // Draw Qi bar
            if (qiComp != null && Find.CameraDriver.CurrentZoom <= CameraZoomRange.Middle)
            {
                Vector3 drawPos = drawLoc;
                drawPos.y += 0.05f;
                drawPos.z += 1.2f;
                
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = drawPos;
                r.size = new Vector2(0.8f, 0.15f);
                r.fillPercent = qiComp.QiPct;
                r.filledMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.cyan);
                r.unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.gray);
                r.margin = 0.05f;
                GenDraw.DrawFillableBar(r);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            
            // Add charge gizmo
            if (qiComp != null && !qiComp.IsFull)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Nạp Qi",
                    defaultDesc = "Ra lệnh cho người tu luyện đến nạp Qi cho pháo đài",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport", true),
                    action = delegate
                    {
                        // Find cultivators to charge
                        var cultivators = this.Map.mapPawns.FreeColonistsSpawned
                            .Where(p => {
                                var comp = p.TryGetComp<CultivationComp>();
                                return comp != null && comp.cultivationData != null && 
                                       comp.cultivationData.currentRealm != CultivationRealm.Mortal;
                            });
                        
                        if (cultivators.Any())
                        {
                            var bestCultivator = cultivators
                                .OrderByDescending(p => {
                                    var comp = p.TryGetComp<CultivationComp>();
                                    return comp?.cultivationData?.currentRealm ?? CultivationRealm.Mortal;
                                })
                                .First();
                            
                            Job job = JobMaker.MakeJob(TuTienDefOf.ChargeCultivationTurret, this);
                            bestCultivator.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            
                            Messages.Message($"{bestCultivator.LabelShort} đang đến nạp Qi cho pháo đài", this, MessageTypeDefOf.TaskCompletion);
                        }
                        else
                        {
                            Messages.Message("Không có người tu luyện nào có thể nạp Qi (cần ít nhất cấp Foundation)", this, MessageTypeDefOf.RejectInput);
                        }
                    }
                };
            }
        }
    }
}
