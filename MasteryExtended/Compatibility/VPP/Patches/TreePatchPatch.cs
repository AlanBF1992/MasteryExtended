using HarmonyLib;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.VPP.Patches
{
    internal static class TreePatchPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;
        internal static readonly FieldInfo TalentGroveTending = AccessTools.Field("VanillaPlusProfessions.Constants:Talent_GroveTending");
        internal static readonly MethodInfo VPPCurrentPlayerHasTalent = AccessTools.Method("VanillaPlusProfessions.Utilities.TalentUtility:CurrentPlayerHasTalent");

        /***********
         * PATCHES *
         ***********/
        internal static IEnumerable<CodeInstruction> checkGrowthStageTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo checkGroveTendingTalentInfo = AccessTools.Method(typeof(TreePatchPatch), nameof(checkGroveTendingTalent));

                // From: growthStage.Value++
                // To:   checkGroveTendingTalent(this)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Dup)
                    )
                    .ThrowIfNotMatch("TreePatch.dayUpdateTranspiler: IL code not found")
                    .Advance(1)
                    .RemoveInstructions(8)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, checkGroveTendingTalentInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(checkGrowthStageTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * METHODS *
         ***********/

        private static void checkGroveTendingTalent(Tree tree)
        {
            if ((bool)VPPCurrentPlayerHasTalent.Invoke(null, [TalentGroveTending.GetValue(null), -1, null, true])!)
            {
                tree.growthStage.Value += 2;
            }
            else
            {
                tree.growthStage.Value++;
            }
        }
    }
}