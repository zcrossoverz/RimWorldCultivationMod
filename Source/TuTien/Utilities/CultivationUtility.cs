using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TuTien
{
    public static class CultivationUtility
    {
        public static CultivationComp GetCultivationComp(this Pawn pawn)
        {
            return pawn.GetComp<CultivationComp>();
        }

        public static CultivationData GetCultivationData(this Pawn pawn)
        {
            return pawn.GetCultivationComp()?.cultivationData;
        }

        public static bool IsCultivator(this Pawn pawn)
        {
            var data = pawn.GetCultivationData();
            return data != null && data.currentRealm > CultivationRealm.Mortal;
        }

        public static float GetCombatPowerModifier(this Pawn pawn)
        {
            var data = pawn.GetCultivationData();
            if (data == null) return 1f;

            float modifier = 1f;
            
            // Base realm bonuses
            switch (data.currentRealm)
            {
                case CultivationRealm.QiCondensation:
                    modifier += 0.1f + (data.currentStage - 1) * 0.05f;
                    break;
                case CultivationRealm.FoundationEstablishment:
                    modifier += 0.3f + (data.currentStage - 1) * 0.1f;
                    break;
                case CultivationRealm.GoldenCore:
                    modifier += 0.7f + (data.currentStage - 1) * 0.15f;
                    break;
            }

            // Talent modifier
            var talentDef = DefDatabase<TalentDef>.AllDefs
                .FirstOrDefault(t => t.talentLevel == data.talent);
            if (talentDef != null)
                modifier *= talentDef.buffMultiplier;

            return modifier;
        }

        public static void AddCultivationPoints(this Pawn pawn, float points)
        {
            var data = pawn.GetCultivationData();
            if (data != null)
            {
                var talentDef = DefDatabase<TalentDef>.AllDefs
                    .FirstOrDefault(t => t.talentLevel == data.talent);
                
                float multiplier = talentDef?.tuViGainRate ?? 1f;
                data.cultivationPoints += points * multiplier;
            }
        }

        public static string GetRealmStageLabel(this Pawn pawn)
        {
            var data = pawn.GetCultivationData();
            if (data == null) return "Mortal";

            string realmName = data.currentRealm.ToString();
            return $"{realmName} {data.currentStage}";
        }

        public static void TriggerBreakthroughEvent(Pawn pawn, bool success)
        {
            if (success)
            {
                // Positive effects
                var healingHediff = HediffMaker.MakeHediff(HediffDefOf.Malnutrition, pawn);
                healingHediff.Severity = 0.2f;
                pawn.health.AddHediff(healingHediff);

                Find.LetterStack.ReceiveLetter(
                    "Breakthrough Success",
                    $"{pawn.Name.ToStringShort} has successfully advanced in cultivation! Their power has increased significantly.",
                    LetterDefOf.PositiveEvent,
                    pawn
                );
            }
            else
            {
                // Negative effects - cultivation deviation
                var injuryHediff = HediffMaker.MakeHediff(HediffDefOf.Hypothermia, pawn);
                injuryHediff.Severity = 0.4f;
                pawn.health.AddHediff(injuryHediff);

                var moodHediff = HediffMaker.MakeHediff(HediffDefOf.Malnutrition, pawn);
                moodHediff.Severity = 0.3f;
                pawn.health.AddHediff(moodHediff);

                Find.LetterStack.ReceiveLetter(
                    "Cultivation Deviation",
                    $"{pawn.Name.ToStringShort} failed their breakthrough attempt and suffered cultivation deviation. They are injured and demoralized.",
                    LetterDefOf.NegativeEvent,
                    pawn
                );
            }
        }

        public static void SpawnWithRandomTechniques(Pawn pawn)
        {
            var comp = pawn.GetCultivationComp();
            if (comp?.cultivationData == null) return;

            var data = comp.cultivationData;
            
            // Determine technique spawn chance based on faction and pawn type
            float baseChance = 0.2f;
            
            if (pawn.Faction?.HostileTo(Faction.OfPlayer) == true)
                baseChance *= 1.5f; // Hostile NPCs more likely to have techniques
            
            if (pawn.kindDef.combatPower > 100f)
                baseChance *= 2f; // Strong pawns more likely to have techniques

            // Apply talent modifier
            var talentDef = DefDatabase<TalentDef>.AllDefs
                .FirstOrDefault(t => t.talentLevel == data.talent);
            
            if (talentDef != null)
                baseChance = talentDef.techniqueChance;

            if (Rand.Chance(baseChance))
            {
                data.GrantRandomTechnique();
                
                // Heaven Chosen can have multiple techniques
                if (data.talent == TalentLevel.HeavenChosen && Rand.Chance(0.3f))
                {
                    data.GrantRandomTechnique();
                }
            }
        }
    }
}
