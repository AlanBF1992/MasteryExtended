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
        public static IEnumerable<CodeInstruction> dayUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
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

        /***********
         * METHODS *
         ***********/
        internal static void checkGrowthStage(Tree tree)
        {
            if (tree.growthStage.Value >= 5
                || !tree.fertilized.Value
                || !tree.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/TreeData/FertilizedBy", out string stringId)
                || !long.TryParse(stringId, out long id)
                || Game1.GetPlayer(id) is not Farmer who
                || !who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Ranger", out string value)
                || !bool.Parse(value))
            {
                tree.growthStage.Value++;
            }
            else
            {
                tree.growthStage.Value = 5;
            }
        }
    }
}
