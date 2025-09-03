using RimWorld;
using UnityEngine;
using Verse;

namespace TuTien
{
    public class CompProperties_QiStorage : CompProperties
    {
        public int maxQi = 1000;
        public int qiPerShot = 50;
        
        public CompProperties_QiStorage()
        {
            this.compClass = typeof(CompQiStorage);
        }
    }

    public class CompQiStorage : ThingComp
    {
        private int currentQi = 0;
        
        public CompProperties_QiStorage Props => (CompProperties_QiStorage)this.props;
        
        public int MaxQi => Props.maxQi;
        public int CurrentQi => currentQi;
        public int QiPerShot => Props.qiPerShot;
        
        public bool CanShoot => currentQi >= QiPerShot;
        public bool IsFull => currentQi >= MaxQi;
        
        public float QiPct => (float)currentQi / MaxQi;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            // Give some starting Qi for testing
            if (!respawningAfterLoad && currentQi == 0)
            {
                currentQi = QiPerShot * 5; // Start with enough for 5 shots
                Log.Message($"[TuTien] Turret spawned with {currentQi} starting Qi");
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentQi, "currentQi", 0);
        }

        public bool TryConsumeQi(int amount)
        {
            if (currentQi >= amount)
            {
                currentQi -= amount;
                return true;
            }
            return false;
        }

        public bool TryAddQi(int amount)
        {
            if (currentQi < MaxQi)
            {
                int actualAmount = Mathf.Min(amount, MaxQi - currentQi);
                currentQi += actualAmount;
                return actualAmount > 0;
            }
            return false;
        }

        public override string CompInspectStringExtra()
        {
            return $"Qi: {currentQi}/{MaxQi} ({QiPct:P0})";
        }
    }
}
