using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TuTien.Abilities
{
    public class AbilityEffect_CorpseRevival
    {
        public int qiCost = 200;
        public string puppetPawnKindDef = "TuTien_PuppetCorpse";
        public float healthMultiplier = 0.7f;
        public float speedMultiplier = 0.5f;
        
        public void Apply(CultivationAbilityDef abilityDef, Pawn caster, LocalTargetInfo target)
        {
            Log.Message($"[DEBUG] CorpseRevival.Apply - Target: {target}, Thing: {target.Thing}, Type: {target.Thing?.GetType()}");
            
            if (!(target.Thing is Pawn targetPawn))
            {
                Log.Message($"[DEBUG] Target is not Pawn! Thing type: {target.Thing?.GetType()}");
                Messages.Message("Must target a pawn for testing.", MessageTypeDefOf.RejectInput);
                return;
            }
            
            Log.Message($"[DEBUG] Pawn target found: {targetPawn.Name}");
            
            // Validate target
            if (!CanReviveCorpse(targetPawn, caster))
            {
                return;
            }
            
            // Test effect - just show message and apply simple effect
            Messages.Message($"{caster.Name} casts Khôi Lỗi Thuật on {targetPawn.Name}! (Testing)", MessageTypeDefOf.PositiveEvent);
            
            // Create some visual effects at target location
            CreateRevivalEffects(targetPawn.Position, targetPawn.Map);

            // Create visual effects first
            CreateRevivalEffects(targetPawn.Position, targetPawn.Map);
            
            // Actually create the puppet
            var puppet = CreatePuppetFromCorpse(targetPawn, caster);
            if (puppet != null)
            {
                // Spawn puppet near the target location
                IntVec3 spawnPos = targetPawn.Position;
                if (!spawnPos.Walkable(targetPawn.Map))
                {
                    // Find nearby walkable cell
                    if (!CellFinder.TryFindRandomReachableNearbyCell(targetPawn.Position, targetPawn.Map, 3, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out spawnPos))
                    {
                        spawnPos = targetPawn.Position; // fallback
                    }
                }
                
                GenSpawn.Spawn(puppet, spawnPos, targetPawn.Map);
                
                Messages.Message($"{caster.Name} successfully revived {targetPawn.Name} as a puppet!", MessageTypeDefOf.PositiveEvent);
                Log.Message($"[DEBUG] Puppet created and spawned at {spawnPos}");
            }
            else
            {
                Messages.Message("Failed to create puppet.", MessageTypeDefOf.RejectInput);
                Log.Error("[DEBUG] Failed to create puppet");
            }
            
            Log.Message($"[DEBUG] Ability executed successfully on {targetPawn.Name}");
        }

        private bool CanReviveCorpse(Pawn targetPawn, Pawn caster)
        {
             Log.Message($"[DEBUG] CanReviveCorpse - Target: {targetPawn.Name}");
            
            // For testing - just allow all pawns
            if (targetPawn.Dead)
            {
                Messages.Message("Cannot target dead pawns in test mode.", MessageTypeDefOf.RejectInput);
                return false;
            }
            
            // Don't target self
            if (targetPawn == caster)
            {
                Messages.Message("Cannot target yourself.", MessageTypeDefOf.RejectInput);
                return false;
            }
            
            Log.Message($"[DEBUG] CanReviveCorpse validation passed for {targetPawn.Name}");
            return true;
        }
        
        private Pawn CreatePuppetFromCorpse(Pawn originalPawn, Pawn caster)
        {
            try
            {
                // Use the original pawn's kind def for compatibility
                var puppetKindDef = originalPawn.kindDef;
                
                // Generate new pawn with fixed age to avoid generation issues
                var puppet = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    kind: puppetKindDef,
                    faction: caster.Faction,
                    context: PawnGenerationContext.NonPlayer,
                    tile: caster.Map.Tile,
                    forceGenerateNewPawn: true,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: false,
                    mustBeCapableOfViolence: false, // Allow non-violent puppets
                    colonistRelationChanceFactor: 0f,
                    forceAddFreeWarmLayerIfNeeded: true,
                    allowGay: true,
                    allowPregnant: false,
                    allowAddictions: false,
                    inhabitant: false,
                    certainlyBeenInCryptosleep: false,
                    forceRedressWorldPawnIfFormerColonist: false,
                    worldPawnFactionDoesntMatter: false,
                    biocodeWeaponChance: 0f,
                    biocodeApparelChance: 0f,
                    extraPawnForExtraRelationChance: null,
                    relationWithExtraPawnChanceFactor: 1f,
                    validatorPreGear: null,
                    validatorPostGear: null,
                    forcedTraits: null,
                    prohibitedTraits: null,
                    minChanceToRedressWorldPawn: null,
                    fixedBiologicalAge: 25f, // Fixed age to avoid generation issues
                    fixedChronologicalAge: 25f, // Fixed age to avoid generation issues  
                    fixedGender: originalPawn.gender
                ));
                
                // Copy some basic properties from original
                puppet.Name = originalPawn.Name;
                
                // Modify puppet stats
                ModifyPuppetStats(puppet, originalPawn, caster);
                
                return puppet;
            }
            catch (System.Exception e)
            {
                Log.Error($"Failed to create puppet from corpse: {e.Message}");
                return null;
            }
        }
        
        private void ModifyPuppetStats(Pawn puppet, Pawn original, Pawn caster)
        {
            // === ROBOT PUPPET MODIFICATIONS ===
            
            // 1. Reduce all skills to basic levels
            if (puppet.skills != null)
            {
                foreach (var skill in puppet.skills.skills)
                {
                    skill.Level = 3; // Basic competence, not completely useless
                    skill.passion = Passion.None;
                    skill.xpSinceLastLevel = 0f;
                    skill.xpSinceMidnight = 0f;
                }
            }
            
            // 2. Remove human traits, add robot-like traits
            if (puppet.story?.traits != null)
            {
                puppet.story.traits.allTraits.Clear();
                
                // Add robot-like traits
                var industriousTrait = DefDatabase<TraitDef>.GetNamedSilentFail("Industrious");
                if (industriousTrait != null)
                {
                    puppet.story.traits.GainTrait(new Trait(industriousTrait));
                }
                
                var steadfastTrait = DefDatabase<TraitDef>.GetNamedSilentFail("Steadfast");
                if (steadfastTrait != null)
                {
                    puppet.story.traits.GainTrait(new Trait(steadfastTrait));
                }
            }
            
            // 3. Disable/minimize all human needs
            if (puppet.needs != null)
            {
                // Set all needs to satisfied and freeze them
                if (puppet.needs.food != null)
                {
                    puppet.needs.food.CurLevel = 1.0f; // Always full
                }
                
                if (puppet.needs.rest != null)
                {
                    puppet.needs.rest.CurLevel = 1.0f; // Never tired
                }
                
                if (puppet.needs.mood != null)
                {
                    puppet.needs.mood.CurLevel = 0.5f; // Neutral, emotionless
                }
                
                // Remove recreation need if exists
                var recreationNeed = puppet.needs.TryGetNeed(NeedDefOf.Rest);
                if (recreationNeed != null)
                {
                    recreationNeed.CurLevel = 1.0f; // No recreation needed
                }
                
                // Remove comfort need
                var comfortNeed = puppet.needs.TryGetNeed(NeedDefOf.Indoors);
                if (comfortNeed != null)
                {
                    comfortNeed.CurLevel = 1.0f; // No comfort needed
                }
                
                // Remove beauty need
                var beautyNeed = puppet.needs.TryGetNeed(NeedDefOf.Food);
                if (beautyNeed != null)
                {
                    beautyNeed.CurLevel = 1f; // Indifferent to beauty
                }
            }
            
            // 4. Add puppet control hediff
            var puppetControl = HediffMaker.MakeHediff(HediffDefOf.PsychicBond, puppet);
            puppetControl.Severity = 1f;
            puppet.health.AddHediff(puppetControl);
            
            // 5. Set faction to caster's faction
            puppet.SetFaction(caster.Faction);
        
            
            Log.Message($"[DEBUG] Robot puppet created - no needs, basic skills, emotionless");
}
        
        private void CreateRevivalEffects(IntVec3 position, Map map)
        {
            // Dark energy visual effects
            for (int i = 0; i < 8; i++)
            {
                FleckMaker.ThrowSmoke(position.ToVector3(), map, 1.5f);
                FleckMaker.ThrowMicroSparks(position.ToVector3(), map);
            }
            
            // Purple/dark glow
            FleckMaker.ThrowLightningGlow(position.ToVector3(), map, 2f);
            
            // TODO: Sound effect when sound API is figured out
            // SoundDefOf.Psycast_Skip_Pulse.PlayOneShotOnCamera();
        }
    }
}
