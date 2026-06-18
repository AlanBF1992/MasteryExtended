using HarmonyLib;
using StardewModdingAPI;
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
                MethodInfo timesApplyFertilizerInfo = AccessTools.Method(typeof(TreePatchPatch), nameof(timesApplyFertilizer));

                // From: int baseGrowth = IsFertilizedByWoodlander(tree)? 3: 1
                // To:   int baseGrowth = (IsFertilizedByWoodlander(tree)? 3: 1) * timesApplyFertilizer()
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Stloc_0)
                    )
                    .ThrowIfNotMatch("TreePatchPatch.checkGrowthStageTranspiler: IL code not found")
                ;

                matcher.Opcode = OpCodes.Call;
                matcher.Operand = timesApplyFertilizerInfo;

                matcher
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Stloc_0)
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

        private static bool checkGroveTendingTalent()
        {
            return (bool)VPPCurrentPlayerHasTalent.Invoke(null, [TalentGroveTending.GetValue(null), -1, null, true])!;
        }

        private static int timesApplyFertilizer()
        {
            return checkGroveTendingTalent() ? 2 : 1;
        }
    }
}