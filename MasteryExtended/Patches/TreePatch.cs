using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class TreePatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/
        internal static IEnumerable<CodeInstruction> dayUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo checkGrowthStageInfo = AccessTools.Method(typeof(TreePatch), nameof(checkGrowthStage));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Dup)
                    )
                    .Advance(1)
                    .RemoveInstructions(8)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, checkGrowthStageInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(dayUpdateTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static IEnumerable<CodeInstruction> tickUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo checkExtraWoodInfo = AccessTools.Method(typeof(TreePatch), nameof(checkExtraWood));

                for (int i = 0; i < 2; i++)
                {
                    matcher
                        .MatchStartForward(
                            new CodeMatch(OpCodes.Conv_R8),
                            new CodeMatch(OpCodes.Mul),
                            new CodeMatch(OpCodes.Conv_I4)
                        )
                        .Insert(
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Call, checkExtraWoodInfo),
                            new CodeInstruction(OpCodes.Add)
                        )
                    ;

                }

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(dayUpdateTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static IEnumerable<CodeInstruction> performTreeFallTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo checkExtraWoodInfo = AccessTools.Method(typeof(TreePatch), nameof(checkExtraWood));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Conv_R8),
                        new CodeMatch(OpCodes.Mul),
                        new CodeMatch(OpCodes.Conv_I4)
                    )
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, checkExtraWoodInfo),
                        new CodeInstruction(OpCodes.Add)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(dayUpdateTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static bool IsGrowthBlockedByNearbyTreePrefix(Tree tree, ref bool __result)
        {
            if (IsFertilizedByWoodlander(tree))
            {
                __result = false;
                return false;
            }
            return true;
        }
        /***********
         * METHODS *
         ***********/
        internal static void checkGrowthStage(Tree tree)
        {
            if (tree.growthStage.Value >= 5 || !IsFertilizedByWoodlander(tree))
            {
                tree.growthStage.Value++;
            }
            else
            {
                tree.growthStage.Value = 5;
            }
        }
        internal static int checkExtraWood(Tree tree)
        {
            if (tree.growthStage.Value >= 5 && IsFertilizedByWoodlander(tree))
            {
                return 1;
            }
            return 0;
        }

        internal static bool IsFertilizedByWoodlander(Tree tree)
        {
            return tree.fertilized.Value
                && tree.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/TreeData/FertilizedBy", out string stringId)
                && long.TryParse(stringId, out long id)
                && Game1.GetPlayer(id) is Farmer who
                && who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Woodlander", out string value)
                && bool.Parse(value);
        }
    }
}
