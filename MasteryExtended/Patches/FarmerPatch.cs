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
        internal static IEnumerable<CodeInstruction> LevelTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                FieldInfo farmerFarmingLevelInfo = AccessTools.Field(typeof(Farmer), nameof(Farmer.farmingLevel));
                MethodInfo farmerLevelNewInfo = AccessTools.Method(typeof(FarmerPatch), nameof(FarmerLevel));

                // From: (this.farmingLevel.Value + this.fishingLevel.Value + this.foragingLevel.Value + this.combatLevel.Value + this.miningLevel.Value + this.luckLevel.Value) / 2
                // To:   FarmerLevel(this, false, true);
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, farmerFarmingLevelInfo)
                    )
                    .ThrowIfNotMatch("Vanilla FarmerPatch.LevelTranspiler: IL code not found")
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

        internal static IEnumerable<CodeInstruction> gainExperienceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo masteryGainInfo = AccessTools.Method(typeof(FarmerPatch), nameof(ShouldGainMasteryExp));
                MethodInfo newMasteryAmountInfo = AccessTools.Method(typeof(FarmerPatch), nameof(newMasteryAmount));

                // From: if (this.Level >= 25)
                // To:   if (ShouldGainMasteryExp(this, which))
                matcher.Start()
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Stelem_Ref),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("Vanilla FarmerPatch.gainExperienceTranspiler: IL code 1 not found")
                    .Advance(3)
                    .RemoveInstructions(2)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, masteryGainInfo)
                    )
                    .SetOpcodeAndAdvance(OpCodes.Brfalse)
                ;

                // From: howMuch
                // To:   newAmount(howMuch)
                matcher
                    .MatchStartForward(new CodeMatch(OpCodes.Ldarg_2))
                    .ThrowIfNotMatch("Vanilla FarmerPatch.gainExperienceTranspiler: IL code 2 not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, newMasteryAmountInfo)
                    )
                ;

                // From: howMuch
                // To:   newAmount(howMuch)
                matcher
                    .MatchStartForward(new CodeMatch(OpCodes.Ldarg_2))
                    .ThrowIfNotMatch("Vanilla FarmerPatch.gainExperienceTranspiler: IL code 3 not found")
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

        internal static IEnumerable<CodeInstruction> getTitleTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo farmerLevelInfo = AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Level));
                MethodInfo farmerLevelNewInfo = AccessTools.Method(typeof(FarmerPatch), nameof(FarmerLevel));

                // From: int level = this.Level
                // To:   int level = FarmerLevel(this, true, true))
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Call, farmerLevelInfo)
                    )
                    .ThrowIfNotMatch("Vanilla FarmerPatch.getTitleTranspiler: IL code not found")
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

        internal static void GetAppliedMagneticRadiusPostfix(Farmer __instance, ref int __result)
        {
            if (!isFarmerAttractive(__instance)) return;
            __result += 128;
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
                    + (luck ? farmer.luckLevel.Value : 0)) / 2;
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

        internal static int newMasteryAmount(int howMuch, int which)
        {
            if (which is <= 0 or >= 5 && ModEntry.Config.BooksQuantity != BooksQuantityOption.None && GameStateQuery.CheckConditions($"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookUnlockMastery 1"))
            {
                howMuch *= 2;
            }
            return (int)(howMuch * (1 + ExtraMasteryExperienceMultiplier(which)));
        }

        internal static float ExtraMasteryExperienceMultiplier(int which, bool includeComplete = true)
        {
            float extraMultiplier = 0;
            string modID = ModEntry.ModManifest.UniqueID;

            if (ModEntry.Config.BooksQuantity == BooksQuantityOption.None) return extraMultiplier;

            if (includeComplete && GameStateQuery.CheckConditions($"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Complete_ID 1"))
            {
                extraMultiplier += 0.2f;
            }

            if (ModEntry.Config.BooksQuantity == BooksQuantityOption.Full)
            {
                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionCoopmaster_ID 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionAngler_ID 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionLumberjack_ID 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionBlacksmith_ID 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionBrute_ID 1"):
                        extraMultiplier += 0.25f;
                        break;
                }

                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionShepherd_ID 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionPirate_ID 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionTapper_ID 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionProspector_ID 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionDefender_ID 1"):
                        extraMultiplier += 0.25f;
                        break;
                }

                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionArtisan_ID 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionMariner_ID 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionBotanist_ID 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionExcavator_ID 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionAcrobat_ID 1"):
                        extraMultiplier += 0.25f;
                        break;
                }

                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionAgriculturist_ID 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionLuremaster_ID 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionTracker_ID 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionGemologist_ID 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_ProfessionDesperado_ID 1"):
                        extraMultiplier += 0.25f;
                        break;
                }
            }
            else if (ModEntry.Config.BooksQuantity == BooksQuantityOption.Lite)
            {
                switch (which)
                {
                    case 0 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_SkillFarming_ID 1"):
                    case 1 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_SkillFishing_ID 1"):
                    case 2 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_SkillForaging_ID 1"):
                    case 3 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_SkillMining_ID 1"):
                    case 4 when GameStateQuery.CheckConditions($"PLAYER_STAT Current {modID}_BookMastery_SkillCombat_ID 1"):
                        extraMultiplier += 0.5f;
                        break;
                }
            }

            return extraMultiplier;
        }

        internal static bool isFarmerAttractive(Farmer who)
        {
            return who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Attractive", out string value)
                && bool.Parse(value);
        }
    }
}
