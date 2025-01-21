using HarmonyLib;
using MasteryExtended.Menu.Pages;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class FarmerPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

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
            catch (Exception)
            {
                return instructions;
            }
        }

        /// <summary>Permite ganar maestría en la habilidad que tenga nivel mayor a 10.</summary>
        internal static IEnumerable<CodeInstruction> gainExperienceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo farmerLevelInfo = AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Level));
                FieldInfo experiencePointsInfo = AccessTools.Field(typeof(Farmer), nameof(Farmer.experiencePoints));
                MethodInfo masteryGainInfo = AccessTools.Method(typeof(FarmerPatch), nameof(ShouldGainMasteryExp));

                Label labelExpGain =
                    matcher.MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, experiencePointsInfo)
                    )
                    .ThrowIfNotMatch("Vanillla gainExperience: Label IL code not found")
                    .Labels[0]
                ;

                //from: if (this.Level >= 25)
                //to:   if (ShouldGainMasteryExp(this, which))
                matcher.Start()
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Call, farmerLevelInfo),
                        new CodeMatch(OpCodes.Ldc_I4_S)
                    )
                    .ThrowIfNotMatch("Vanillla gainExperience: IL code not found or already applied")
                    .Advance(1)
                    .RemoveInstructions(3)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, masteryGainInfo),
                        new CodeInstruction(OpCodes.Brfalse, labelExpGain)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception)
            {
                return instructions;
            }
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
            catch (Exception)
            {
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
    }
}
