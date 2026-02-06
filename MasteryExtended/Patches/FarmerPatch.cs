using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class FarmerPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/

        /// <summary>Ya que el juego base nunca espera que luck sea mayor a 0, lo eliminamos, por si acaso</summary>
        internal static IEnumerable<CodeInstruction> LevelTranspiler (IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                FieldInfo farmerFarmingLevelInfo = AccessTools.Field(typeof(Farmer), nameof(Farmer.farmingLevel));
                MethodInfo farmerLevelNewInfo = AccessTools.Method(typeof(FarmerPatch), nameof(FarmerLevel));

                //from: (this.farmingLevel.Value + this.fishingLevel.Value + this.foragingLevel.Value + this.combatLevel.Value + this.miningLevel.Value + this.luckLevel.Value) / 2
                //to:   FarmerLevel(this, false, true);
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, farmerFarmingLevelInfo)
                    )
                    .ThrowIfNotMatch("Vanillla LevelTranspiler: IL code not found")
                    .Advance(1)
                    .RemoveInstructions(24)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Call, farmerLevelNewInfo)
                    )
                ;
                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(LevelTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /// <summary>Permite ganar maestría en la habilidad que tenga nivel mayor a 10.</summary>
        internal static IEnumerable<CodeInstruction> gainExperienceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo masteryGainInfo = AccessTools.Method(typeof(FarmerPatch), nameof(ShouldGainMasteryExp));
                MethodInfo newMasteryAmountInfo = AccessTools.Method(typeof(FarmerPatch), nameof(newMasteryAmount));

                //from: if (this.Level >= 25)
                //to:   if (ShouldGainMasteryExp(this, which))
                matcher.Start()
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Stelem_Ref),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("Vanillla gainExperience: IL code 1 not found")
                    .Advance(3)
                    .RemoveInstructions(2)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, masteryGainInfo)
                    )
                    .SetOpcodeAndAdvance(OpCodes.Brfalse)
                ;

                //from: howMuch
                //to:   newAmount(howMuch) 

                matcher
                    .MatchStartForward(new CodeMatch(OpCodes.Ldarg_2))
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, newMasteryAmountInfo)
                    )
                ;


                matcher
                    .MatchStartForward(new CodeMatch(OpCodes.Ldarg_2))
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, newMasteryAmountInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(gainExperienceTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static int newMasteryAmount(int howMuch, int which)
        {
            return (int)(howMuch * (1 + ExtraMasteryExperienceMultiplier(which)));
        }

        /// <summary>Arregla el título para si contar la suerte, en caso de que se agregue el mod, aunque permitiendo solo hasta lvl 30</summary>
        internal static IEnumerable<CodeInstruction> getTitleTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo farmerLevelInfo = AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Level));
                MethodInfo farmerLevelNewInfo = AccessTools.Method(typeof(FarmerPatch), nameof(FarmerLevel));

                //from: int level = this.Level
                //to:   int level = FarmerLevel(this, true, true))
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Call, farmerLevelInfo)
                    )
                    .ThrowIfNotMatch("Vanillla getTitle: IL code not found or already applied")
                    .Advance(1)
                    .RemoveInstruction()
                    .Insert(
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Call, farmerLevelNewInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(getTitleTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * METHODS *
         ***********/

        internal static int FarmerLevel(Farmer farmer, bool luck = false, bool restrict = false)
        {
            if (restrict)
            {
                return (Math.Min(farmer.farmingLevel.Value, 10)
                        + Math.Min(farmer.fishingLevel.Value, 10)
                        + Math.Min(farmer.foragingLevel.Value, 10)
                        + Math.Min(farmer.combatLevel.Value, 10)
                        + Math.Min(farmer.miningLevel.Value, 10)
                        + (luck ? Math.Min(farmer.luckLevel.Value, 10) : 0)) / 2;
            }
            return (farmer.farmingLevel.Value
                    + farmer.fishingLevel.Value
                    + farmer.foragingLevel.Value
                    + farmer.combatLevel.Value
                    + farmer.miningLevel.Value
                    + (luck? farmer.luckLevel.Value : 0)) / 2;
        }

        internal static bool ShouldGainMasteryExp(Farmer farmer, int which)
        {
            return which switch
            {
                0 => farmer.farmingLevel.Value >= 10,
                3 => farmer.miningLevel.Value >= 10,
                1 => farmer.fishingLevel.Value >= 10,
                2 => farmer.foragingLevel.Value >= 10,
                4 => farmer.combatLevel.Value >= 10,
                _ => false
            };
        }

        internal static float ExtraMasteryExperienceMultiplier(int which, bool includeComplete = true)
        {
            // Ugly AF, fix later?
            float extraMultiplier = 0;
            string modID = ModEntry.ModManifest.UniqueID;

            if (includeComplete && !ModEntry.Config.BooksQuantity.Equals("2") && GameStateQuery.CheckConditions($"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookCompleteMastery 1"))
            {
                extraMultiplier += 0.2f;
            }

            if (ModEntry.Config.BooksQuantity.Equals("0"))
            {
                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookCoopmasterMastery 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookAnglerMastery 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookLumberjackMastery 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookBlacksmithMastery 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookBruteMastery 1"):
                        extraMultiplier += 0.25f;
                        break;
                }

                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookShepherdMastery 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookPirateMastery 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookTapperMastery 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookProspectorMastery 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookDefenderMastery 1"):
                        extraMultiplier += 0.25f;
                        break;
                }

                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookArtisanMastery 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMarinerMastery 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookBotanistMastery 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookExcavatorMastery 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookAcrobatMastery 1"):
                        extraMultiplier += 0.25f;
                        break;
                }

                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookAgriculturistMastery 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookLuremasterMastery 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookTrackerMastery 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookGemologistMastery 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookDesperadoMastery 1"):
                        extraMultiplier += 0.25f;
                        break;
                }
            }
            else if (ModEntry.Config.BooksQuantity.Equals("1"))
            {
                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookFarmingMastery 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookFishingMastery 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookForagingMastery 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMiningMastery 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookCombatMastery 1"):
                        extraMultiplier += 0.5f;
                        break;
                }
            }

            return extraMultiplier;
        }
    }
}
